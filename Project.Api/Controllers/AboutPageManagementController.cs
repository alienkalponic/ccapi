using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Project.Application.Common.Repository;
using Project.Domain.Dto.AboutPage;
using Project.Domain.Utility;
using Project.Infastructure.Data;
using Project.Infastructure.Service;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Project.Api.Controllers
{
    [Route("api/about-page")]
    [ApiController]
    public class AboutPageManagementController : ControllerBase
    {
        private readonly IUnitOfWork _unitofWork;
        private readonly ApiResponse apiResponse;
        private SqlParameter[] _paramObj;
        private Dictionary<string, object> _dictionaryObj;
        private readonly LogService _logService;
        private readonly IApiResponseService _responseService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationDbContext _db;
        private Dictionary<string, object> _dictionaryData;

        public AboutPageManagementController(
            IUnitOfWork unitofWork,
            LogService logService,
            IConfiguration configuration,
            IApiResponseService responseService,
            IWebHostEnvironment webHostEnvironment,
            ApplicationDbContext db)
        {
            _unitofWork = unitofWork;
            this.apiResponse = new();
            this._dictionaryObj = new Dictionary<string, object>();
            _logService = logService;
            _responseService = responseService;
            _webHostEnvironment = webHostEnvironment;
            _db = db;
        }

        #region :: About Page

        /***************************************
         * Title - Get All About Pages
         * Login Location - ADMIN PANEL
         * Procedure - Sp_Circle_AboutPageManagement
         * EXEC - OPERATION_ID = 4
         ***************************************/
        [Authorize]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("Get-all-about-page/{PageSize:long}/{PageNumber:long}", Name = "GetAllAboutPage")]
        public async Task<ActionResult<ApiResponse>> GetAllAboutPage(long PageSize, long PageNumber, string? Search = null)
        {
            try
            {
                _paramObj = new SqlParameter[]
                {
                    new SqlParameter("@OPERATION_ID", 4) { SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input },
                    new SqlParameter("@PageSize", PageSize) { SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input },
                    new SqlParameter("@PageNumber", PageNumber) { SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input },
                    new SqlParameter("@Search", (object?)Search ?? DBNull.Value) { SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input }
                };

                string responseDetails = await _unitofWork.aboutPageRepository
                    .CallStoreProcedure("Sp_Circle_AboutPageManagement", _paramObj);

                if (string.IsNullOrWhiteSpace(responseDetails))
                {
                    return _responseService.NotFound("No data returned from database.");
                }

                JObject JSONObj = JObject.Parse(responseDetails);

                if (JSONObj.ContainsKey("Status") && Convert.ToBoolean(JSONObj["Status"]))
                {
                    JArray responseArray = JSONObj["Response"] as JArray;
                    if (responseArray != null && responseArray.Count > 0)
                    {
                        int totalItems = (int?)JSONObj["TotalItems"] ?? 0;
                        int itemsPerPage = (int?)JSONObj["ItemsPerPage"] ?? (int)PageSize;
                        int currentPage = (int?)JSONObj["CurrentPage"] ?? (int)PageNumber;
                        int totalPageCount = (int?)JSONObj["TotalPageCount"] ?? 0;

                        return _responseService.PaginatedSuccess(
                            JsonConvert.SerializeObject(responseArray, Formatting.None),
                            totalItems,
                            itemsPerPage,
                            currentPage,
                            totalPageCount
                        );
                    }
                    else
                    {
                        return _responseService.NotFound("Response array is empty.");
                    }
                }
                else
                {
                    string errorMessage = JSONObj["Response"]?.ToString() ?? "An error occurred while fetching data.";
                    return _responseService.Error(errorMessage);
                }
            }
            catch (Exception ex)
            {
                return _responseService.Error(ex.Message);
            }
        }

        /***************************************
         * Title - Get About Page By Id
         * Login Location - ADMIN PANEL / PUBLIC
         * Procedure - Sp_Circle_AboutPageManagement
         * EXEC - OPERATION_ID = 5
         ***************************************/
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("Get-about-page-by-id/{AboutPageId:long}", Name = "GetAboutPageById")]
        public async Task<ActionResult<ApiResponse>> GetAboutPageById(long AboutPageId)
        {
            try
            {
                _paramObj = new SqlParameter[]
                {
                    new SqlParameter("@OPERATION_ID", 5),
                    new SqlParameter("@AboutPageId", AboutPageId)
                };

                string responseDetails = await _unitofWork.aboutPageRepository
                    .CallStoreProcedure("Sp_Circle_AboutPageManagement", _paramObj);

                JObject JSONObj = JObject.Parse(responseDetails);

                if (JSONObj.ContainsKey("Status") && Convert.ToBoolean(JSONObj["Status"]))
                {
                    if (JSONObj["Response"] != null)
                    {
                        return _responseService.Success(JsonConvert.SerializeObject(JSONObj["Response"]));
                    }

                    return _responseService.NotFound("About Page not found.");
                }

                return _responseService.Error((string)JSONObj["Response"]);
            }
            catch (Exception ex)
            {
                return _responseService.Error(ex.Message);
            }
        }

        /***************************************
         * Title - Create About Page Details
         * Login Location - ADMIN PANEL
         * Procedure - Sp_Circle_AboutPageManagement
         * EXEC - OPERATION_ID = 1
         ***************************************/
        [Authorize]
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("Create-about-page", Name = "CreateAboutPage")]
        public async Task<ActionResult<ApiResponse>> CreateAboutPage([FromForm] CreateAboutPageDto dto)
        {
            var rootPath = _webHostEnvironment.WebRootPath ?? Directory.GetCurrentDirectory();
            var newUploadedFiles = new List<string>();

            try
            {
                if (dto == null)
                {
                    return _responseService.Error("About Page details are required.");
                }

                string? bannerImageUrl = dto.AboutPage?.BannerImageUrl;

                // 1. Process Banner Image
                var bannerFile = dto.BannerFile ?? dto.File ?? dto.AboutPage?.BannerFile;
                if (bannerFile != null && bannerFile.Length > 0)
                {
                    if (!FileUploadHelper.IsValidFile(bannerFile))
                    {
                        return _responseService.Error("Invalid banner file format. Only images/videos allowed.");
                    }

                    var (fileUrl, _) = await FileUploadHelper.SaveFileAsync(bannerFile, rootPath, "about-pages");
                    bannerImageUrl = fileUrl;
                    newUploadedFiles.Add(fileUrl);
                }

                // 2. Process AboutDetails Images
                var aboutDetailsList = new List<object>();
                if (dto.AboutDetails != null && dto.AboutDetails.Count > 0)
                {
                    foreach (var detail in dto.AboutDetails)
                    {
                        string? detailImageUrl = detail.ImageUrl;
                        if (detail.ImageFile != null && detail.ImageFile.Length > 0)
                        {
                            if (!FileUploadHelper.IsValidFile(detail.ImageFile))
                            {
                                CleanupFiles(rootPath, newUploadedFiles);
                                return _responseService.Error($"Invalid image file for detail title '{detail.Title}'.");
                            }

                            var (fileUrl, _) = await FileUploadHelper.SaveFileAsync(detail.ImageFile, rootPath, "about-pages");
                            detailImageUrl = fileUrl;
                            newUploadedFiles.Add(fileUrl);
                        }

                        aboutDetailsList.Add(new
                        {
                            Title = detail.Title,
                            Description = detail.Description,
                            ImageUrl = detailImageUrl,
                            DisplayOrder = detail.DisplayOrder,
                            IsActive = detail.IsActive ?? true,
                            CreatedBy = detail.CreatedBy ?? dto.CreatedBy
                        });
                    }
                }

                // 3. Process AboutPerson Images
                var aboutPersonList = new List<object>();
                if (dto.AboutPerson != null && dto.AboutPerson.Count > 0)
                {
                    foreach (var person in dto.AboutPerson)
                    {
                        string? personImageUrl = person.ImageUrl;
                        if (person.ImageFile != null && person.ImageFile.Length > 0)
                        {
                            if (!FileUploadHelper.IsValidFile(person.ImageFile))
                            {
                                CleanupFiles(rootPath, newUploadedFiles);
                                return _responseService.Error($"Invalid image file for person '{person.PersonName}'.");
                            }

                            var (fileUrl, _) = await FileUploadHelper.SaveFileAsync(person.ImageFile, rootPath, "about-pages");
                            personImageUrl = fileUrl;
                            newUploadedFiles.Add(fileUrl);
                        }

                        aboutPersonList.Add(new
                        {
                            PersonName = person.PersonName,
                            FullDescription = person.FullDescription,
                            ImageUrl = personImageUrl,
                            DisplayOrder = person.DisplayOrder,
                            IsActive = person.IsActive ?? true,
                            CreatedBy = person.CreatedBy ?? dto.CreatedBy
                        });
                    }
                }

                // 4. Construct JSON for Stored Procedure
                var aboutPageObj = new
                {
                    AboutPage = new
                    {
                        PageTitle = dto.AboutPage?.PageTitle,
                        PageSlug = dto.AboutPage?.PageSlug,
                        HeroTitle = dto.AboutPage?.HeroTitle,
                        HeroSubtitle = dto.AboutPage?.HeroSubtitle,
                        HistoryTitle = dto.AboutPage?.HistoryTitle,
                        HistoryDescription = dto.AboutPage?.HistoryDescription,
                        MapTitle = dto.AboutPage?.MapTitle,
                        MapAddress = dto.AboutPage?.MapAddress,
                        Latitude = dto.AboutPage?.Latitude,
                        Longitude = dto.AboutPage?.Longitude,
                        BannerImageUrl = bannerImageUrl,
                        CreatedBy = dto.CreatedBy ?? dto.AboutPage?.CreatedBy,
                        IsActive=dto.AboutPage?.IsActive
                    },
                    AboutPageSection = dto.AboutPageSection ?? new List<AboutPageSectionDto>(),
                    AboutDetails = aboutDetailsList,
                    AboutPerson = aboutPersonList
                };

                _paramObj = new SqlParameter[]
                {
                    new SqlParameter("@OPERATION_ID", 1) { SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input },
                    new SqlParameter("@JSON", JsonConvert.SerializeObject(aboutPageObj)) { SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input }
                };

                string responseDetails = await _unitofWork.aboutPageRepository
                    .CallStoreProcedure("Sp_Circle_AboutPageManagement", _paramObj);

                JObject JSONObj = JObject.Parse(responseDetails);

                if (JSONObj.ContainsKey("Status") && Convert.ToBoolean(JSONObj["Status"]))
                {
                    return _responseService.Success(JsonConvert.SerializeObject(JSONObj["Response"]));
                }
                else
                {
                    CleanupFiles(rootPath, newUploadedFiles);
                    return _responseService.Error((string)JSONObj["Response"]);
                }
            }
            catch (Exception ex)
            {
                CleanupFiles(rootPath, newUploadedFiles);
                return _responseService.Error(ex.Message);
            }
        }

        /***************************************
         * Title - Update About Page Details
         * Login Location - ADMIN PANEL
         * Procedure - Sp_Circle_AboutPageManagement
         * EXEC - OPERATION_ID = 2
         ***************************************/
        [Authorize]
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("Update-about-page", Name = "UpdateAboutPage")]
        public async Task<ActionResult<ApiResponse>> UpdateAboutPage([FromForm] UpdateAboutPageDto dto)
        {
            var rootPath = _webHostEnvironment.WebRootPath ?? Directory.GetCurrentDirectory();
            var newUploadedFiles = new List<string>();
            var oldFilesToDelete = new List<string>();

            try
            {
                if (dto == null || dto.AboutPageId <= 0)
                {
                    return _responseService.Error("Valid AboutPageId and details are required.");
                }

                // 1. Fetch Existing About Page Record (OPERATION_ID = 5)
                var fetchParams = new SqlParameter[]
                {
                    new SqlParameter("@OPERATION_ID", 5),
                    new SqlParameter("@AboutPageId", dto.AboutPageId)
                };

                string existingResponse = await _unitofWork.aboutPageRepository
                    .CallStoreProcedure("Sp_Circle_AboutPageManagement", fetchParams);

                JObject existingJson = JObject.Parse(existingResponse);

                if (!existingJson.ContainsKey("Status") || !Convert.ToBoolean(existingJson["Status"]) || existingJson["Response"] == null)
                {
                    return _responseService.Error("About Page not found.");
                }

                var (oldBannerUrl, oldDetailsMap, oldPersonMap, oldDetailsList, oldPersonList) =
                    ExtractExistingImages(existingJson["Response"]!);

                // 2. Process Banner Image Update
                string? updatedBannerUrl = dto.AboutPage?.BannerImageUrl ?? oldBannerUrl;
                var bannerFile = dto.BannerFile ?? dto.File ?? dto.AboutPage?.BannerFile;

                if (bannerFile != null && bannerFile.Length > 0)
                {
                    if (!FileUploadHelper.IsValidFile(bannerFile))
                    {
                        return _responseService.Error("Invalid banner file format. Only images/videos allowed.");
                    }

                    var (fileUrl, _) = await FileUploadHelper.SaveFileAsync(bannerFile, rootPath, "about-pages");
                    updatedBannerUrl = fileUrl;
                    newUploadedFiles.Add(fileUrl);

                    if (!string.IsNullOrWhiteSpace(oldBannerUrl) && oldBannerUrl != fileUrl)
                    {
                        oldFilesToDelete.Add(oldBannerUrl);
                    }
                }

                // 3. Process AboutDetails Images Update
                var aboutDetailsListForJson = new List<object>();
                if (dto.AboutDetails != null && dto.AboutDetails.Count > 0)
                {
                    for (int i = 0; i < dto.AboutDetails.Count; i++)
                    {
                        var detail = dto.AboutDetails[i];
                        string? oldDetailImageUrl = null;

                        if (detail.AboutDetailsId.HasValue && detail.AboutDetailsId.Value > 0 && oldDetailsMap.ContainsKey(detail.AboutDetailsId.Value))
                        {
                            oldDetailImageUrl = oldDetailsMap[detail.AboutDetailsId.Value];
                        }
                        else if (i < oldDetailsList.Count)
                        {
                            oldDetailImageUrl = oldDetailsList[i];
                        }

                        string? detailImageUrl = detail.ImageUrl ?? oldDetailImageUrl;

                        if (detail.ImageFile != null && detail.ImageFile.Length > 0)
                        {
                            if (!FileUploadHelper.IsValidFile(detail.ImageFile))
                            {
                                CleanupFiles(rootPath, newUploadedFiles);
                                return _responseService.Error($"Invalid image file for detail title '{detail.Title}'.");
                            }

                            var (fileUrl, _) = await FileUploadHelper.SaveFileAsync(detail.ImageFile, rootPath, "about-pages");
                            detailImageUrl = fileUrl;
                            newUploadedFiles.Add(fileUrl);

                            if (!string.IsNullOrWhiteSpace(oldDetailImageUrl) && oldDetailImageUrl != fileUrl)
                            {
                                oldFilesToDelete.Add(oldDetailImageUrl);
                            }
                        }

                        aboutDetailsListForJson.Add(new
                        {
                            AboutDetailsId = detail.AboutDetailsId,
                            Title = detail.Title,
                            Description = detail.Description,
                            ImageUrl = detailImageUrl,
                            DisplayOrder = detail.DisplayOrder,
                            IsActive = detail.IsActive ?? true,
                            CreatedBy = detail.CreatedBy,
                            UpdatedBy = dto.UpdatedBy
                        });
                    }
                }

                // 4. Process AboutPerson Images Update
                var aboutPersonListForJson = new List<object>();
                if (dto.AboutPerson != null && dto.AboutPerson.Count > 0)
                {
                    for (int i = 0; i < dto.AboutPerson.Count; i++)
                    {
                        var person = dto.AboutPerson[i];
                        string? oldPersonImageUrl = null;

                        if (person.PersonId.HasValue && person.PersonId.Value > 0 && oldPersonMap.ContainsKey(person.PersonId.Value))
                        {
                            oldPersonImageUrl = oldPersonMap[person.PersonId.Value];
                        }
                        else if (i < oldPersonList.Count)
                        {
                            oldPersonImageUrl = oldPersonList[i];
                        }

                        string? personImageUrl = person.ImageUrl ?? oldPersonImageUrl;

                        if (person.ImageFile != null && person.ImageFile.Length > 0)
                        {
                            if (!FileUploadHelper.IsValidFile(person.ImageFile))
                            {
                                CleanupFiles(rootPath, newUploadedFiles);
                                return _responseService.Error($"Invalid image file for person '{person.PersonName}'.");
                            }

                            var (fileUrl, _) = await FileUploadHelper.SaveFileAsync(person.ImageFile, rootPath, "about-pages");
                            personImageUrl = fileUrl;
                            newUploadedFiles.Add(fileUrl);

                            if (!string.IsNullOrWhiteSpace(oldPersonImageUrl) && oldPersonImageUrl != fileUrl)
                            {
                                oldFilesToDelete.Add(oldPersonImageUrl);
                            }
                        }

                        aboutPersonListForJson.Add(new
                        {
                            PersonId = person.PersonId,
                            PersonName = person.PersonName,
                            FullDescription = person.FullDescription,
                            ImageUrl = personImageUrl,
                            DisplayOrder = person.DisplayOrder,
                            IsActive = person.IsActive ?? true,
                            CreatedBy = person.CreatedBy,
                            UpdatedBy = dto.UpdatedBy
                        });
                    }
                }

                // 5. Construct JSON for Stored Procedure
                var aboutPageObj = new
                {
                    AboutPage = new
                    {
                        AboutPageId = dto.AboutPageId,
                        PageTitle = dto.AboutPage?.PageTitle,
                        PageSlug = dto.AboutPage?.PageSlug,
                        HeroTitle = dto.AboutPage?.HeroTitle,
                        HeroSubtitle = dto.AboutPage?.HeroSubtitle,
                        HistoryTitle = dto.AboutPage?.HistoryTitle,
                        HistoryDescription = dto.AboutPage?.HistoryDescription,
                        MapTitle = dto.AboutPage?.MapTitle,
                        MapAddress = dto.AboutPage?.MapAddress,
                        Latitude = dto.AboutPage?.Latitude,
                        Longitude = dto.AboutPage?.Longitude,
                        BannerImageUrl = updatedBannerUrl,
                        UpdatedBy = dto.UpdatedBy ?? dto.AboutPage?.UpdatedBy,
                        IsActive=dto.AboutPage?.IsActive
                    },
                    AboutPageSection = dto.AboutPageSection ?? new List<AboutPageSectionDto>(),
                    AboutDetails = aboutDetailsListForJson,
                    AboutPerson = aboutPersonListForJson
                };

                _paramObj = new SqlParameter[]
                {
                    new SqlParameter("@OPERATION_ID", 2) { SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input },
                    new SqlParameter("@AboutPageId", dto.AboutPageId) { SqlDbType = SqlDbType.BigInt, Direction = ParameterDirection.Input },
                    new SqlParameter("@DeletedBy", dto.DeletedBy.HasValue ? (object)dto.DeletedBy.Value : DBNull.Value) { SqlDbType = SqlDbType.BigInt, Direction = ParameterDirection.Input },
                    new SqlParameter("@JSON", JsonConvert.SerializeObject(aboutPageObj)) { SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input }
                };

                string updateResponse = await _unitofWork.aboutPageRepository
                    .CallStoreProcedure("Sp_Circle_AboutPageManagement", _paramObj);

                JObject updateJson = JObject.Parse(updateResponse);

                // 6. Outcome Handling
                if (updateJson.ContainsKey("Status") && Convert.ToBoolean(updateJson["Status"]))
                {
                    // DB Success -> Delete replaced old physical files
                    CleanupFiles(rootPath, oldFilesToDelete);
                    return _responseService.Success(JsonConvert.SerializeObject(updateJson["Response"]));
                }
                else
                {
                    // DB Failure -> Rollback newly uploaded physical files
                    CleanupFiles(rootPath, newUploadedFiles);
                    return _responseService.Error((string)updateJson["Response"]);
                }
            }
            catch (Exception ex)
            {
                // Exception -> Rollback newly uploaded physical files
                CleanupFiles(rootPath, newUploadedFiles);
                return _responseService.Error(ex.Message);
            }
        }

        /***************************************
         * Title - Delete About Page
         * Login Location - ADMIN PANEL
         * Procedure - Sp_Circle_AboutPageManagement
         * EXEC - OPERATION_ID = 3
         ***************************************/
        [Authorize]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("Remove-about-page-by-id/{AboutPageId:long}", Name = "RemoveAboutPageById")]
        public async Task<ActionResult<ApiResponse>> RemoveAboutPageById(long AboutPageId, [FromQuery] long? DeletedBy = null)
        {
            var rootPath = _webHostEnvironment.WebRootPath ?? Directory.GetCurrentDirectory();

            try
            {
                // 1. Fetch existing About Page details to collect old physical file URLs
                var fetchParams = new SqlParameter[]
                {
                    new SqlParameter("@OPERATION_ID", 5),
                    new SqlParameter("@AboutPageId", AboutPageId)
                };

                string existingResponse = await _unitofWork.aboutPageRepository
                    .CallStoreProcedure("Sp_Circle_AboutPageManagement", fetchParams);

                JObject existingJson = JObject.Parse(existingResponse);

                var oldFilesToDelete = new List<string>();

                if (existingJson.ContainsKey("Status") && Convert.ToBoolean(existingJson["Status"]) && existingJson["Response"] != null)
                {
                    var (bannerUrl, detailsMap, personMap, detailsList, personList) =
                        ExtractExistingImages(existingJson["Response"]!);

                    if (!string.IsNullOrWhiteSpace(bannerUrl))
                        oldFilesToDelete.Add(bannerUrl);

                    foreach (var url in detailsMap.Values)
                        if (!string.IsNullOrWhiteSpace(url)) oldFilesToDelete.Add(url);

                    foreach (var url in detailsList)
                        if (!string.IsNullOrWhiteSpace(url) && !oldFilesToDelete.Contains(url)) oldFilesToDelete.Add(url);

                    foreach (var url in personMap.Values)
                        if (!string.IsNullOrWhiteSpace(url)) oldFilesToDelete.Add(url);

                    foreach (var url in personList)
                        if (!string.IsNullOrWhiteSpace(url) && !oldFilesToDelete.Contains(url)) oldFilesToDelete.Add(url);
                }

                // 2. Execute Stored Procedure for Soft-Delete (OPERATION_ID = 3)
                _paramObj = new SqlParameter[]
                {
                    new SqlParameter("@OPERATION_ID", 3),
                    new SqlParameter("@AboutPageId", AboutPageId),
                    new SqlParameter("@DeletedBy", DeletedBy.HasValue ? (object)DeletedBy.Value : DBNull.Value)
                };

                string responseDetails = await _unitofWork.aboutPageRepository
                    .CallStoreProcedure("Sp_Circle_AboutPageManagement", _paramObj);

                JObject JSONObj = JObject.Parse(responseDetails);

                if (JSONObj.ContainsKey("Status") && Convert.ToBoolean(JSONObj["Status"]))
                {
                    // 3. Soft-delete succeeded -> Delete physical files
                    CleanupFiles(rootPath, oldFilesToDelete);
                    return _responseService.Success((string)JSONObj["Response"]);
                }

                return _responseService.Error((string)JSONObj["Response"]);
            }
            catch (Exception ex)
            {
                return _responseService.Error(ex.Message);
            }
        }

        #endregion

        #region :: Private Helper Methods

        private (string? bannerUrl, Dictionary<long, string> detailsMap, Dictionary<long, string> personMap, List<string> detailsList, List<string> personList) ExtractExistingImages(JToken responseToken)
        {
            string? bannerUrl = null;
            var detailsMap = new Dictionary<long, string>();
            var personMap = new Dictionary<long, string>();
            var detailsList = new List<string>();
            var personList = new List<string>();

            JToken? root = responseToken;
            if (root is JArray arr && arr.Count > 0)
            {
                root = arr[0];
            }

            if (root == null)
                return (bannerUrl, detailsMap, personMap, detailsList, personList);

            // Extract Banner URL
            if (root["AboutPage"] != null)
            {
                var aboutPageToken = root["AboutPage"];
                if (aboutPageToken is JArray pageArr && pageArr.Count > 0)
                    bannerUrl = pageArr[0]["BannerImageUrl"]?.ToString();
                else
                    bannerUrl = aboutPageToken["BannerImageUrl"]?.ToString();
            }
            else if (root["BannerImageUrl"] != null)
            {
                bannerUrl = root["BannerImageUrl"]?.ToString();
            }

            // Extract AboutDetails Image URLs
            JToken? detailsToken = root["AboutDetails"];
            if (detailsToken is JArray detailsArr)
            {
                foreach (var item in detailsArr)
                {
                    long id = item["AboutDetailsId"] != null ? Convert.ToInt64(item["AboutDetailsId"]) : 0;
                    string? imgUrl = item["ImageUrl"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(imgUrl))
                    {
                        if (id > 0)
                        {
                            detailsMap[id] = imgUrl;
                        }
                        detailsList.Add(imgUrl);
                    }
                }
            }

            // Extract AboutPerson Image URLs
            JToken? personToken = root["AboutPerson"];
            if (personToken is JArray personArr)
            {
                foreach (var item in personArr)
                {
                    long id = item["PersonId"] != null ? Convert.ToInt64(item["PersonId"]) : 0;
                    string? imgUrl = item["ImageUrl"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(imgUrl))
                    {
                        if (id > 0)
                        {
                            personMap[id] = imgUrl;
                        }
                        personList.Add(imgUrl);
                    }
                }
            }

            return (bannerUrl, detailsMap, personMap, detailsList, personList);
        }

        private void CleanupFiles(string rootPath, IEnumerable<string?> fileUrls)
        {
            if (fileUrls == null) return;
            var distinctUrls = fileUrls.Where(u => !string.IsNullOrWhiteSpace(u)).Distinct();
            foreach (var fileUrl in distinctUrls)
            {
                FileUploadHelper.DeleteFile(rootPath, fileUrl);
            }
        }

        #endregion
    }
}
