using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Project.Application.Common.Repository;
using Project.Domain.Dto.Banner;
using Project.Domain.Model;
using Project.Domain.Utility;
using System.Data;

namespace Project.Api.Controllers
{
    [Route("api/content")]
    [ApiController]
    public class ContentManagementController : ControllerBase
    {
        private readonly IUnitOfWork _unitofWork;
        private readonly ApiResponse apiResponse;
        private SqlParameter[] _paramObj;
        private Dictionary<string, object> _dictionaryObj;
        private readonly LogService _logService;
        private readonly IApiResponseService _responseService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private Dictionary<string, object> _dictionaryData;

        public ContentManagementController(IUnitOfWork unitofWork, LogService logService, IConfiguration configuration, IApiResponseService responseService, IWebHostEnvironment webHostEnvironment)
        {
            _unitofWork = unitofWork;
            this.apiResponse = new();
            this._dictionaryObj = new Dictionary<string, object>();
            _logService = logService;
            _responseService = responseService;
            _webHostEnvironment = webHostEnvironment;
        }

        #region::Banner
        /***************************************
          * Title - Get All Banner Details 
          * Login Location - ADMIN PANEL
          * Procedure - Sp_Circle_ContentManagement
          * EXEC - EXEC [dbo].[Sp_Circle_ContentManagement] @OPERATION_ID=3
          ***************************************/
        //[Authorize]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("Get-all-banner/{PageSize:long}/{PageNumber:long}", Name = "GetAllBanner")]
        public async Task<ActionResult<ApiResponse>> GetAllBanner(long PageSize, long PageNumber, string Search = null)
        {

            _paramObj = new SqlParameter[]
                {

                        new SqlParameter("@OPERATION_ID",3) {SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input },
                        new SqlParameter("@PageSize",PageSize) {SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input },
                        new SqlParameter("@PageNumber",PageNumber) {SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input },
                        new SqlParameter("@Search",Search) {SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input },


                };

            string responseDetails = await _unitofWork.bannerRepository.CallStoreProcedure("Sp_Circle_ContentManagement", _paramObj);


            JObject JSONObj = JObject.Parse(responseDetails);
            if (JSONObj.ContainsKey("Status") && Convert.ToBoolean(JSONObj["Status"]))
            {

                JArray responseArray = (JArray)JSONObj["Response"]!;
                if (responseArray != null && responseArray.Count > 0)
                {
                    return _responseService.PaginatedSuccess(
                        JsonConvert.SerializeObject(responseArray, Formatting.None),
                        (int)JSONObj["TotalItems"]!,
                        (int)JSONObj["ItemsPerPage"]!,
                        (int)JSONObj["CurrentPage"]!,
                        (int)JSONObj["TotalPageCount"]!
                    );

                }
                else
                {

                    return _responseService.NotFound("Response array is empty.");
                }



            }
            else
            {

                return _responseService.Error((string)JSONObj["Response"]);
            }


        }

        /***************************************
         * Title - Get Banner By Id
         * Login Location - ADMIN PANEL
         * EXEC - OPERATION_ID = 4
         ***************************************/
        [Authorize]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("Get-banner-by-id/{BannerId:long}", Name = "GetBannerById")]
        public async Task<ActionResult<ApiResponse>> GetBannerById(long BannerId)
        {
            _paramObj = new SqlParameter[]
            {
        new SqlParameter("@OPERATION_ID", 4),
        new SqlParameter("@BannerId", BannerId)
            };

            string responseDetails = await _unitofWork.bannerRepository
                .CallStoreProcedure("Sp_Circle_ContentManagement", _paramObj);

            JObject JSONObj = JObject.Parse(responseDetails);

            if (JSONObj.ContainsKey("Status") && Convert.ToBoolean(JSONObj["Status"]))
            {
                if (JSONObj["Response"] != null)
                    return _responseService.Success(JsonConvert.SerializeObject(JSONObj["Response"]));

                return _responseService.NotFound("Banner not found.");
            }

            return _responseService.Error((string)JSONObj["Response"]);
        }

        /***************************************
          * Title - Create Banner Details 
          * Login Location - ADMIN PANEL
          * Procedure - Sp_Circle_ContentManagement
          * EXEC - EXEC [dbo].[Sp_Circle_ContentManagement] @OPERATION_ID=1
          ***************************************/
        //[Authorize]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("Create-banner", Name = "CreateBanner")]
        public async Task<ActionResult<ApiResponse>> CreateBanner([FromForm] CreateBannerDto dto)
        {
            if (dto.File == null || dto.File.Length == 0)
                return _responseService.Error("File is required");

            if (!FileUploadHelper.IsValidFile(dto.File))
                return _responseService.Error("Only image/video allowed");

            (string fileUrl, string fileName) =
     await FileUploadHelper.SaveFileAsync(dto.File, _webHostEnvironment.WebRootPath,"banners");


            string? mobileUrl = null;

            if (dto.MobileFile != null)
            {
                if (!FileUploadHelper.IsImage(dto.MobileFile))
                {
                    FileUploadHelper.DeleteFile(_webHostEnvironment.WebRootPath, fileUrl);
                    FileUploadHelper.DeleteFile(_webHostEnvironment.WebRootPath, mobileUrl!);
                    return _responseService.Error("Mobile file must be image");
                }
                    

                var mobileResult =
                    await FileUploadHelper.SaveFileAsync(dto.MobileFile, _webHostEnvironment.WebRootPath, "banners");

                mobileUrl = mobileResult.fileUrl;
            }
            else
            {
                // fallback → same image
                mobileUrl = fileUrl;
            }

            var bannerObj = new[]
            {
        new {
            Title = dto.Title,
            Caption = dto.Caption,
            ImageUrl = fileUrl,
            ImageName = fileName,
            MobileImageUrl = mobileUrl,
            DisplayOrder = dto.DisplayOrder,
            IsActive = true
        }
    };

            _paramObj = new SqlParameter[]
            {
        new("@OPERATION_ID", 1),
        new("@JSON", JsonConvert.SerializeObject(bannerObj))
            };

            string response = await _unitofWork.bannerRepository
                .CallStoreProcedure("Sp_Circle_ContentManagement", _paramObj);

            JObject JSONObj = JObject.Parse(response);

            if (Convert.ToBoolean(JSONObj["Status"]))
            {
                return _responseService.Success((string)JSONObj["Response"]);
            }
            else
            {
                FileUploadHelper.DeleteFile(_webHostEnvironment.WebRootPath, fileUrl);
                FileUploadHelper.DeleteFile(_webHostEnvironment.WebRootPath, mobileUrl);
                return _responseService.Error((string)JSONObj["Response"]);
            }


                
        }


        /***************************************
          * Title - Update Banner Details 
          * Login Location - ADMIN PANEL
          * Procedure - Sp_Circle_ContentManagement
          * EXEC - EXEC [dbo].[Sp_Circle_ContentManagement] @OPERATION_ID=2
          ***************************************/
        [Authorize]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("Update-banner", Name = "UpdateBanner")]
        public async Task<ActionResult<ApiResponse>> UpdateBanner([FromForm] UpdateBannerDto dto)
        {
            string? newFileUrl = null;
            string? newFileName = null;
            string? newMobileUrl = null;

            string? oldFileUrl = null;
            string? oldMobileUrl = null;
            string? oldFileName = null;
            try
            {
                /* -------------------------------------------------
                   1️⃣ FETCH EXISTING BANNER (VERY IMPORTANT)
                --------------------------------------------------*/

                var fetchParams = new SqlParameter[]
                {
            new("@OPERATION_ID", 4),
            new("@BannerId", dto.BannerId)
                };

                string existingBannerResponse = await _unitofWork.bannerRepository
                    .CallStoreProcedure("Sp_Circle_ContentManagement", fetchParams);

                JObject existingJson = JObject.Parse(existingBannerResponse);
                

                if (!Convert.ToBoolean(existingJson["Status"]))
                    return _responseService.Error("Banner not found");

                JArray responseArray = (JArray)existingJson["Response"]!;
                if (responseArray != null && responseArray.Count > 0)
                {
                    oldFileUrl = responseArray[0]["ImageUrl"]?.ToString();
                    oldMobileUrl = responseArray[0]["MobileImageUrl"]?.ToString();
                    oldFileName= responseArray[0]["ImageName"]?.ToString();
                }
                

                /* -------------------------------------------------
                   2️⃣ VALIDATE & SAVE NEW MAIN FILE
                --------------------------------------------------*/

                if (dto.File != null)
                {
                    if (!FileUploadHelper.IsValidFile(dto.File))
                        return _responseService.Error("Only image/video allowed");

                    (newFileUrl, newFileName) =
                        await FileUploadHelper.SaveFileAsync(dto.File, _webHostEnvironment.WebRootPath, "banners");
                }

                /* -------------------------------------------------
                   3️⃣ VALIDATE & SAVE NEW MOBILE FILE
                --------------------------------------------------*/

                if (dto.MobileFile != null)
                {
                    if (!FileUploadHelper.IsImage(dto.MobileFile))
                    {
                        FileUploadHelper.DeleteFile(_webHostEnvironment.WebRootPath, newFileUrl);
                        return _responseService.Error("Mobile file must be image");
                    }

                    var mobileResult =
                        await FileUploadHelper.SaveFileAsync(dto.MobileFile, _webHostEnvironment.WebRootPath, "banners");

                    newMobileUrl = mobileResult.fileUrl;
                }

                /* -------------------------------------------------
                   4️⃣ BUILD JSON OBJECT FOR SP
                --------------------------------------------------*/

                var bannerObj = new[]
                {
            new {
                BannerId = dto.BannerId,
                Title = dto.Title,
                Caption = dto.Caption,
                ImageUrl =dto.File==null?oldFileUrl: newFileUrl,          // NULL হলে SP ignore করবে
                ImageName =dto.File==null?oldFileName: newFileName,
                MobileImageUrl =dto.File==null?oldMobileUrl: newMobileUrl,
                DisplayOrder = dto.DisplayOrder,
                IsActive = dto.IsActive
            }
        };

                var updateParams = new SqlParameter[]
                {
            new("@OPERATION_ID", 2),
            new("@JSON", JsonConvert.SerializeObject(bannerObj))
                };

                string updateResponse = await _unitofWork.bannerRepository
                    .CallStoreProcedure("Sp_Circle_ContentManagement", updateParams);

                JObject updateJson = JObject.Parse(updateResponse);

                /* -------------------------------------------------
                   5️⃣ IF DB FAILS → DELETE NEW FILES
                --------------------------------------------------*/

                if (!Convert.ToBoolean(updateJson["Status"]))
                {
                    FileUploadHelper.DeleteFile(_webHostEnvironment.WebRootPath, newFileUrl);
                    FileUploadHelper.DeleteFile(_webHostEnvironment.WebRootPath, newMobileUrl);

                    return _responseService.Error((string)updateJson["Response"]);
                }

                /* -------------------------------------------------
                   6️⃣ DB SUCCESS → DELETE OLD FILES
                --------------------------------------------------*/

                if (dto.File != null)
                    FileUploadHelper.DeleteFile(_webHostEnvironment.WebRootPath, oldFileUrl);

                if (dto.MobileFile != null)
                    FileUploadHelper.DeleteFile(_webHostEnvironment.WebRootPath, oldMobileUrl);

                return _responseService.Success((string)updateJson["Response"]);
            }
            catch (Exception ex)
            {
                /* -------------------------------------------------
                   7️⃣ EXCEPTION → CLEANUP NEW FILES
                --------------------------------------------------*/

                FileUploadHelper.DeleteFile(_webHostEnvironment.WebRootPath, newFileUrl);
                FileUploadHelper.DeleteFile(_webHostEnvironment.WebRootPath, newMobileUrl);

                return _responseService.Error(ex.Message);
            }
        }

        /***************************************
         * Title - Get Banner By Id
         * Login Location - ADMIN PANEL
         * EXEC - OPERATION_ID = 4
         ***************************************/
        [Authorize]
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("delete-banner-by-id/{BannerId:long}", Name = "DeleteBannerById")]
        public async Task<ActionResult<ApiResponse>> DeleteBannerById(long BannerId)
        {
            string? oldFileUrl = null;
            string? oldMobileUrl = null;
            var fetchParams = new SqlParameter[]
                {
                    new("@OPERATION_ID", 4),
                    new("@BannerId", BannerId)
                };

            string existingBannerResponse = await _unitofWork.bannerRepository
                .CallStoreProcedure("Sp_Circle_ContentManagement", fetchParams);

            JObject existingJson = JObject.Parse(existingBannerResponse);
            _paramObj = new SqlParameter[]
            {
                new SqlParameter("@OPERATION_ID", 5),
                new SqlParameter("@BannerId", BannerId)
            };

            string responseDetails = await _unitofWork.bannerRepository
                .CallStoreProcedure("Sp_Circle_ContentManagement", _paramObj);

            JObject JSONObj = JObject.Parse(responseDetails);

            if (JSONObj.ContainsKey("Status") && Convert.ToBoolean(JSONObj["Status"]))
            {
                JArray responseArray = (JArray)existingJson["Response"]!;
                if (responseArray != null && responseArray.Count > 0)
                {
                    oldFileUrl = responseArray[0]["ImageUrl"]?.ToString();
                    oldMobileUrl = responseArray[0]["MobileImageUrl"]?.ToString();

                    if (oldFileUrl != null)
                        FileUploadHelper.DeleteFile(_webHostEnvironment.WebRootPath, oldFileUrl);

                    if (oldMobileUrl != null)
                        FileUploadHelper.DeleteFile(_webHostEnvironment.WebRootPath, oldMobileUrl);

                }
                if (JSONObj["Response"] != null)
                    return _responseService.Success(JsonConvert.SerializeObject(JSONObj["Response"]));

                return _responseService.NotFound("Banner not found.");
            }

            return _responseService.Error((string)JSONObj["Response"]);
        }


        #endregion
    }
}
