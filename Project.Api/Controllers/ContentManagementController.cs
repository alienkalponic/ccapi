using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Project.Application.Common.Repository;
using Project.Domain.Dto.AchievementDetails;
using Project.Domain.Dto.ActivityDetails;
using Project.Domain.Dto.Banner;
using Project.Domain.Dto.ClubActivity;
using Project.Domain.Dto.ClubDescription;
using Project.Domain.Dto.Gallery;
using Project.Domain.Model;
using Project.Domain.Utility;
using Project.Infastructure.Data;
using Project.Infastructure.Service;
using System.Data;
using System.Linq;
using IODirectory = System.IO.Directory;
using IOFile = System.IO.File;

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
        private readonly ApplicationDbContext _db;
        private Dictionary<string, object> _dictionaryData;

        public ContentManagementController(
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
        [Authorize]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("Create-banner", Name = "CreateBanner")]
        public async Task<ActionResult<ApiResponse>> CreateBanner([FromForm] CreateBannerDto dto)
        {
            var rootPath = _webHostEnvironment.WebRootPath
               ?? Path.Combine(Directory.GetCurrentDirectory());
            if (dto.File == null || dto.File.Length == 0)
                return _responseService.Error("File is required");

            if (!FileUploadHelper.IsValidFile(dto.File))
                return _responseService.Error("Only image/video allowed");

            (string fileUrl, string fileName) =
     await FileUploadHelper.SaveFileAsync(dto.File, _webHostEnvironment.WebRootPath, "banners");


            string? mobileUrl = null;

            if (dto.MobileFile != null)
            {
                if (!FileUploadHelper.IsImage(dto.MobileFile))
                {
                    FileUploadHelper.DeleteFile(rootPath, fileUrl);
                    FileUploadHelper.DeleteFile(rootPath, mobileUrl!);
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
                FileUploadHelper.DeleteFile(rootPath, fileUrl);
                FileUploadHelper.DeleteFile(rootPath, mobileUrl);
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
        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("Update-banner", Name = "UpdateBanner")]
        public async Task<ActionResult<ApiResponse>> UpdateBanner([FromForm] UpdateBannerDto dto)
        {
            _logService.LogCustom("Reached UpdateBanner", "Diagnostic");
            string? newFileUrl = null;
            string? newFileName = null;
            string? newMobileUrl = null;

            string? oldFileUrl = null;
            string? oldMobileUrl = null;
            string? oldFileName = null;
            var rootPath = _webHostEnvironment.WebRootPath ?? Directory.GetCurrentDirectory();
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
                    oldFileName = responseArray[0]["ImageName"]?.ToString();
                }


                /* -------------------------------------------------
                   2️⃣ VALIDATE & SAVE NEW MAIN FILE
                --------------------------------------------------*/

                if (dto.File != null)
                {
                    if (!FileUploadHelper.IsValidFile(dto.File))
                        return _responseService.Error("Only image/video allowed");

                    (newFileUrl, newFileName) =
                        await FileUploadHelper.SaveFileAsync(dto.File, rootPath, "banners");
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
                        await FileUploadHelper.SaveFileAsync(dto.MobileFile, rootPath, "banners");

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

                    FileUploadHelper.DeleteFile(rootPath, newFileUrl);
                    FileUploadHelper.DeleteFile(rootPath, newMobileUrl);

                    return _responseService.Error((string)updateJson["Response"]);
                }

                /* -------------------------------------------------
                   6️⃣ DB SUCCESS → DELETE OLD FILES
                --------------------------------------------------*/

                if (dto.File != null)
                    FileUploadHelper.DeleteFile(rootPath, oldFileUrl);

                if (dto.MobileFile != null)
                    FileUploadHelper.DeleteFile(rootPath, oldMobileUrl);

                return _responseService.Success((string)updateJson["Response"]);
            }
            catch (Exception ex)
            {
                /* -------------------------------------------------
                   7️⃣ EXCEPTION → CLEANUP NEW FILES
                --------------------------------------------------*/

                FileUploadHelper.DeleteFile(rootPath, newFileUrl);
                FileUploadHelper.DeleteFile(rootPath, newMobileUrl);

                return _responseService.Error(ex.Message);
            }
        }

        /***************************************
         * Title - Get Banner By Id
         * Login Location - ADMIN PANEL
         * EXEC - OPERATION_ID = 4
         ***************************************/
        [Authorize]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("remove-banner-by-id/{BannerId:long}", Name = "RemoveBannerById")]
        public async Task<ActionResult<ApiResponse>> RemoveBannerById(long BannerId)
        {
            var rootPath = _webHostEnvironment.WebRootPath
               ?? Path.Combine(Directory.GetCurrentDirectory());
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
                        FileUploadHelper.DeleteFile(rootPath, oldFileUrl);

                    if (oldMobileUrl != null)
                        FileUploadHelper.DeleteFile(rootPath, oldMobileUrl);

                }
                if (JSONObj["Response"] != null)
                    return _responseService.Success(JsonConvert.SerializeObject(JSONObj["Response"]));

                return _responseService.NotFound("Banner not found.");
            }

            return _responseService.Error((string)JSONObj["Response"]);
        }


        #endregion

        #region::Club Description

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("Get-all-club-description/{PageSize:long}/{PageNumber:long}")]
        public async Task<ActionResult<ApiResponse>> GetAllClubDescription(long PageSize, long PageNumber, string? Search = null)
        {
            var paramObj = new SqlParameter[]
            {
                new("@OPERATION_ID", 10),
                new("@PageSize", PageSize),
                new("@PageNumber", PageNumber),
                new("@Search", Search ?? (object)DBNull.Value)
            };

            string responseDetails = await _unitofWork.bannerRepository
                .CallStoreProcedure("Sp_Circle_ContentManagement", paramObj);

            JObject JSONObj = JObject.Parse(responseDetails);

            if (Convert.ToBoolean(JSONObj["Status"]))
            {
                return _responseService.Success(JsonConvert.SerializeObject(JSONObj["Response"]));
            }

            return _responseService.Error((string)JSONObj["Response"]);
        }

        [Authorize]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("Get-club-description-by-id/{ClubDescriptionId:long}")]
        public async Task<ActionResult<ApiResponse>> GetClubDescriptionById(long ClubDescriptionId)
        {
            var paramObj = new SqlParameter[]
            {
                new("@OPERATION_ID", 9),
                new("@ClubDescriptionId", ClubDescriptionId)
            };

            string responseDetails = await _unitofWork.bannerRepository
                .CallStoreProcedure("Sp_Circle_ContentManagement", paramObj);

            JObject JSONObj = JObject.Parse(responseDetails);

            if (Convert.ToBoolean(JSONObj["Status"]))
            {
                return _responseService.Success(JsonConvert.SerializeObject(JSONObj["Response"]));
            }

            return _responseService.Error((string)JSONObj["Response"]);
        }

        [Authorize]
        [HttpPost]
        [Route("Create-club-description")]
        public async Task<ActionResult<ApiResponse>> CreateClubDescription([FromForm] CreateClubDescriptionDto dto)
        {
            var rootPath = _webHostEnvironment.WebRootPath ?? Directory.GetCurrentDirectory();

            if (string.IsNullOrWhiteSpace(dto.Title))
                return _responseService.Error("Title is required");

            List<dynamic>? imagesMeta = null;

            if (!string.IsNullOrWhiteSpace(dto.ImagesJson))
            {
                imagesMeta = JsonConvert.DeserializeObject<List<dynamic>>(dto.ImagesJson);
            }

            var uploadedImages = new List<object>();

            if (dto.Files != null && dto.Files.Count > 0)
            {
                for (int i = 0; i < dto.Files.Count; i++)
                {
                    var file = dto.Files[i];

                    if (!FileUploadHelper.IsImage(file))
                        return _responseService.Error($"File {file.FileName} is not a valid image");

                    var (fileUrl, fileName) =
                        await FileUploadHelper.SaveFileAsync(file, rootPath, "club");

                    uploadedImages.Add(new
                    {
                        Title = fileName.ToString(),
                        DisplayOrder = i + 1,
                        ImageUrl = fileUrl,
                        ImageName = fileName
                    });
                }
            }

            var jsonObject = new[]
            {
                new
                {
                    dto.Title,
                    dto.Description,
                    dto.DisplayOrder,
                    dto.IsActive,
                    Images = uploadedImages
                }
            };

            var parameters = new SqlParameter[]
            {
        new("@OPERATION_ID", 6),
        new("@JSON", JsonConvert.SerializeObject(jsonObject))
            };

            var response = await _unitofWork.clubDescriptionRepository
                .CallStoreProcedure("Sp_Circle_ContentManagement", parameters);

            var json = JObject.Parse(response);

            if (!Convert.ToBoolean(json["Status"]))
            {
                foreach (var img in uploadedImages)
                    FileUploadHelper.DeleteFile(rootPath, (string)img.GetType().GetProperty("ImageUrl")!.GetValue(img)!);

                return _responseService.Error((string)json["Response"]!);
            }

            return _responseService.Success((string)json["Response"]!);
        }

        [Authorize]
        [HttpPost]
        [Route("Update-club-description")]
        public async Task<ActionResult<ApiResponse>> UpdateClubDescription([FromForm] UpdateClubDescriptionRequestDto dto)
        {
            if (dto.ClubDescriptionId <= 0)
                return _responseService.Error("Invalid ClubDescriptionId");

            var webRootPath = _webHostEnvironment.WebRootPath ?? Directory.GetCurrentDirectory();

            // Track file operations so we can roll back safely if the DB transaction fails.
            var movedToTrash = new List<(string OriginalFullPath, string TrashFullPath)>();
            var newlyCreatedFullPaths = new List<string>();

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var club = await _db.ClubDescription
                    .FirstOrDefaultAsync(x => x.ClubDescriptionId == dto.ClubDescriptionId && !x.IsDeleted);

                if (club == null)
                    return _responseService.NotFound("Club description not found");

                // Update only the fields the client actually sent.
                if (dto.Title != null) club.Title = dto.Title;
                if (dto.Description != null) club.Description = dto.Description;
                if (dto.DisplayOrder.HasValue) club.DisplayOrder = dto.DisplayOrder.Value;
                if (dto.IsActive.HasValue) club.IsActive = dto.IsActive.Value;
                club.UpdatedAt = DateTime.UtcNow;

                var existingImages = await _db.ClubDescriptionImage
                    .Where(x => x.ClubDescriptionId == dto.ClubDescriptionId && !x.IsDeleted)
                    .ToListAsync();

                await DeleteClubDescriptionImagesAsync(
                    webRootPath,
                    existingImages,
                    dto.DeletedImageIds ?? new List<long>(),
                    movedToTrash);

                await AddClubDescriptionImagesAsync(
                    webRootPath,
                    dto.ClubDescriptionId,
                    dto.NewImages ?? new List<IFormFile>(),
                    existingImages,
                    newlyCreatedFullPaths);

                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                // After commit: permanently delete the trashed files (best-effort).
                PermanentlyDeleteTrashedFiles(movedToTrash);

                return _responseService.Success("Club description updated successfully.");
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();

                // Roll back file operations best-effort so disk stays consistent with DB rollback.
                RestoreMovedFiles(movedToTrash);
                DeleteNewFiles(newlyCreatedFullPaths);

                return _responseService.Error("Failed to update club description. " + ex.Message);
            }
        }

        private async Task DeleteClubDescriptionImagesAsync(
            string webRootPath,
            List<ClubDescriptionImage> existingImages,
            List<long> deletedImageIds,
            List<(string OriginalFullPath, string TrashFullPath)> movedToTrash)
        {
            var deleteSet = deletedImageIds
                .Where(x => x > 0)
                .Distinct()
                .ToHashSet();

            if (deleteSet.Count == 0)
                return;

            var existingIds = existingImages.Select(x => x.ClubDescriptionImageId).ToHashSet();
            var invalidIds = deleteSet.Except(existingIds).ToList();
            if (invalidIds.Count > 0)
                throw new InvalidOperationException($"One or more images do not belong to this club description: {string.Join(",", invalidIds)}");

            var imagesToDelete = existingImages.Where(x => deleteSet.Contains(x.ClubDescriptionImageId)).ToList();

            // Move files to a trash folder first. If the DB transaction rolls back, we can move them back.
            foreach (var img in imagesToDelete)
            {
                var originalFullPath = TryResolveFullPath(webRootPath, img.ImageUrl);
                if (string.IsNullOrWhiteSpace(originalFullPath))
                    continue;

                if (!IOFile.Exists(originalFullPath))
                    continue;

                var originalDir = Path.GetDirectoryName(originalFullPath)!;
                var batchTrashDir = Path.Combine(originalDir, ".trash", Guid.NewGuid().ToString("N"));
                IODirectory.CreateDirectory(batchTrashDir);

                var trashFullPath = Path.Combine(batchTrashDir, Path.GetFileName(originalFullPath));

                // Ensure destination is unique.
                if (IOFile.Exists(trashFullPath))
                    trashFullPath = Path.Combine(batchTrashDir, $"{Guid.NewGuid():N}_{Path.GetFileName(originalFullPath)}");

                IOFile.Move(originalFullPath, trashFullPath);
                movedToTrash.Add((originalFullPath, trashFullPath));
            }

            _db.ClubDescriptionImage.RemoveRange(imagesToDelete);
        }

        private async Task AddClubDescriptionImagesAsync(
            string webRootPath,
            long clubDescriptionId,
            List<IFormFile> newImages,
            List<ClubDescriptionImage> existingImages,
            List<string> newlyCreatedFullPaths)
        {
            if (newImages.Count == 0)
                return;

            var uploadDir = Path.Combine(webRootPath, "uploads", "clubDescriptionImages");
            IODirectory.CreateDirectory(uploadDir);

            var nextDisplayOrder = existingImages.Count == 0 ? 1 : existingImages.Max(x => x.DisplayOrder) + 1;

            foreach (var file in newImages)
            {
                if (file == null || file.Length == 0)
                    continue;

                if (!FileUploadHelper.IsImage(file))
                    throw new InvalidOperationException($"Invalid image file: {file.FileName}");

                var extension = Path.GetExtension(file.FileName);
                if (string.IsNullOrWhiteSpace(extension))
                    extension = ".bin";

                var newFileName = $"{Guid.NewGuid():N}{extension}";
                var fullPath = Path.Combine(uploadDir, newFileName);

                await using (var stream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    await file.CopyToAsync(stream);
                }

                newlyCreatedFullPaths.Add(fullPath);

                var imageUrl = $"/uploads/clubDescriptionImages/{newFileName}";

                await _db.ClubDescriptionImage.AddAsync(new ClubDescriptionImage
                {
                    ClubDescriptionId = clubDescriptionId,
                    Title = newFileName,
                    ImageName = newFileName,
                    ImageUrl = imageUrl,
                    DisplayOrder = nextDisplayOrder++,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        private static string? TryResolveFullPath(string webRootPath, string? fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
                return null;

            // If full URL comes in, extract just the path portion.
            if (Uri.TryCreate(fileUrl, UriKind.Absolute, out var uri))
                fileUrl = uri.AbsolutePath;

            var cleanUrl = fileUrl
                .Replace("/", Path.DirectorySeparatorChar.ToString())
                .TrimStart(Path.DirectorySeparatorChar);

            return Path.Combine(webRootPath, cleanUrl);
        }

        private static void PermanentlyDeleteTrashedFiles(List<(string OriginalFullPath, string TrashFullPath)> movedToTrash)
        {
            foreach (var (_, trashFullPath) in movedToTrash)
            {
                try
                {
                    if (IOFile.Exists(trashFullPath))
                        IOFile.Delete(trashFullPath);

                    var parentDir = Path.GetDirectoryName(trashFullPath);
                    if (!string.IsNullOrWhiteSpace(parentDir) && IODirectory.Exists(parentDir))
                    {
                        // Clean up empty trash directories.
                        if (!IODirectory.EnumerateFileSystemEntries(parentDir).Any())
                            IODirectory.Delete(parentDir, recursive: false);
                    }
                }
                catch
                {
                    // Best-effort cleanup: at this point DB is committed and file is already out of the live folder.
                }
            }
        }

        private static void RestoreMovedFiles(List<(string OriginalFullPath, string TrashFullPath)> movedToTrash)
        {
            foreach (var (originalFullPath, trashFullPath) in movedToTrash)
            {
                try
                {
                    if (!IOFile.Exists(trashFullPath))
                        continue;

                    var originalDir = Path.GetDirectoryName(originalFullPath);
                    if (!string.IsNullOrWhiteSpace(originalDir))
                        IODirectory.CreateDirectory(originalDir);

                    // If something was recreated at the original path, do not overwrite it.
                    if (!IOFile.Exists(originalFullPath))
                        IOFile.Move(trashFullPath, originalFullPath);
                }
                catch
                {
                    // Best-effort restore.
                }
            }
        }

        private static void DeleteNewFiles(List<string> newlyCreatedFullPaths)
        {
            foreach (var fullPath in newlyCreatedFullPaths)
            {
                try
                {
                    if (IOFile.Exists(fullPath))
                        IOFile.Delete(fullPath);
                }
                catch
                {
                    // Best-effort cleanup.
                }
            }
        }

        [Authorize]
        [HttpPost]
        [Route("remove-club-description-by-id/{ClubDescriptionId:long}")]
        public async Task<ActionResult<ApiResponse>> RemoveClubDescription(long ClubDescriptionId)
        {


            var rootPath = _webHostEnvironment.WebRootPath
                ?? Path.Combine(Directory.GetCurrentDirectory());
            string? oldFileUrl = null;
            string? oldMobileUrl = null;
            var fetchParams = new SqlParameter[]
                {
                    new("@OPERATION_ID", 9),
                    new("@ClubDescriptionId", ClubDescriptionId)
                };

            string existingDescriptionResponse = await _unitofWork.clubDescriptionImageRepository
                .CallStoreProcedure("Sp_Circle_ContentManagement", fetchParams);

            JObject existingJson = JObject.Parse(existingDescriptionResponse);
            var paramObj = new SqlParameter[]
            {
                new("@OPERATION_ID", 8),
                new("@ClubDescriptionId", ClubDescriptionId)
            };

            string responseDetails = await _unitofWork.clubDescriptionImageRepository
                .CallStoreProcedure("Sp_Circle_ContentManagement", paramObj);

            JObject JSONObj = JObject.Parse(responseDetails);

            if (JSONObj.ContainsKey("Status") && Convert.ToBoolean(JSONObj["Status"]))
            {
                JArray responseArray = (JArray)existingJson["Response"]!;
                if (responseArray != null && responseArray.Count > 0)
                {
                    var firstItem = responseArray[0];

                    // Images array ধরলাম
                    JArray imagesArray = (JArray)firstItem["Images"];

                    if (imagesArray != null && imagesArray.Count > 0)
                    {
                        foreach (var image in imagesArray)
                        {
                            var imageUrl = image["ImageUrl"]?.ToString();

                            if (!string.IsNullOrEmpty(imageUrl))
                            {
                                FileUploadHelper.DeleteFile(rootPath, imageUrl);
                            }
                        }
                    }
                }

                // response return
                if (existingJson["Response"] != null)
                    return _responseService.Success(JsonConvert.SerializeObject(existingJson["Response"]));

                return _responseService.NotFound("Data not found.");
            }

            return _responseService.Error((string)JSONObj["Response"]);
        }

        #endregion

        #region::ClubActivities

        [HttpGet]
        [Route("get-all-activities/{PageSize:long}/{PageNumber:long}")]
        public async Task<ActionResult<ApiResponse>> GetAllActivities(long PageSize, long PageNumber, string Search = null)
        {
            _paramObj = new SqlParameter[]
            {
                new("@OPERATION_ID", 13),
                new("@PageSize", PageSize),
                new("@PageNumber", PageNumber),
                new("@Search", Search)
            };

            string response = await _unitofWork.bannerRepository
                .CallStoreProcedure("Sp_Circle_ContentManagement", _paramObj);

            JObject json = JObject.Parse(response);

            if (Convert.ToBoolean(json["Status"]))
            {
                JArray arr = (JArray)json["Response"]!;

                return _responseService.PaginatedSuccess(
                    JsonConvert.SerializeObject(arr, Formatting.None),
                    (int)json["TotalItems"]!,
                    (int)json["ItemsPerPage"]!,
                    (int)json["CurrentPage"]!,
                    (int)json["TotalPageCount"]!
                );
            }

            return _responseService.Error((string)json["Response"]);
        }

        [Authorize]
        [HttpGet]
        [Route("get-activity-by-id/{ActivityId:long}")]
        public async Task<ActionResult<ApiResponse>> GetActivityById(long ActivityId)
        {
            _paramObj = new SqlParameter[]
            {
                new("@OPERATION_ID", 14),
                new("@ActivityId", ActivityId)
            };

            string response = await _unitofWork.bannerRepository
                .CallStoreProcedure("Sp_Circle_ContentManagement", _paramObj);

            JObject json = JObject.Parse(response);

            if (Convert.ToBoolean(json["Status"]))
                return _responseService.Success(JsonConvert.SerializeObject(json["Response"]));

            return _responseService.Error((string)json["Response"]);
        }

        [Authorize]
        [HttpPost]
        [Route("create-activity")]
        public async Task<ActionResult<ApiResponse>> CreateActivity([FromForm] CreateActivityDto dto)
        {
            var rootPath = _webHostEnvironment.WebRootPath ?? Directory.GetCurrentDirectory();

            if (dto.Image == null)
                return _responseService.Error("Image required");

            if (!FileUploadHelper.IsImage(dto.Image))
                return _responseService.Error("Only image allowed");

            var (fileUrl, fileName) =
                await FileUploadHelper.SaveFileAsync(dto.Image, rootPath, "activities");

            var obj = new[]
            {
                new {
                    Title = dto.Title,
                    SubTitle = dto.SubTitle,
                    Description = dto.Description,
                    ImageUrl = fileUrl,
                    ImageName = fileName,
                    RedirectUrl = dto.RedirectUrl,
                    DisplayOrder = dto.DisplayOrder,
                    IsActive = true
                }
            };

            _paramObj = new SqlParameter[]
            {
            new("@OPERATION_ID", 11),
            new("@JSON", JsonConvert.SerializeObject(obj))
            };

            string response = await _unitofWork.bannerRepository
                .CallStoreProcedure("Sp_Circle_ContentManagement", _paramObj);

            JObject json = JObject.Parse(response);

            if (Convert.ToBoolean(json["Status"]))
                return _responseService.Success((string)json["Response"]);

            FileUploadHelper.DeleteFile(rootPath, fileUrl);
            return _responseService.Error((string)json["Response"]);
        }

        [Authorize]
        [HttpPost]
        [Route("update-activity")]
        public async Task<ActionResult<ApiResponse>> UpdateActivity([FromForm] UpdateActivityDto dto)
        {
            string? newFileUrl = null;
            string? newFileName = null;

            string? oldFileUrl = null;
            string? oldFileName = null;

            var rootPath = _webHostEnvironment.WebRootPath ?? Directory.GetCurrentDirectory();

            try
            {
                /* 1️⃣ FETCH OLD DATA */
                var fetchParams = new SqlParameter[]
                {
                new("@OPERATION_ID", 14),
                new("@ActivityId", dto.ActivityId)
                };

                string existing = await _unitofWork.bannerRepository
                    .CallStoreProcedure("Sp_Circle_ContentManagement", fetchParams);

                JObject existingJson = JObject.Parse(existing);

                if (!Convert.ToBoolean(existingJson["Status"]))
                    return _responseService.Error("Not found");

                JArray arr = (JArray)existingJson["Response"]!;
                oldFileUrl = arr[0]["ImageUrl"]?.ToString();
                oldFileName = arr[0]["ImageName"]?.ToString();

                /* 2️⃣ NEW FILE */
                if (dto.Image != null)
                {
                    if (!FileUploadHelper.IsImage(dto.Image))
                        return _responseService.Error("Only image allowed");

                    (newFileUrl, newFileName) =
                        await FileUploadHelper.SaveFileAsync(dto.Image, rootPath, "activities");
                }

                /* 3️⃣ BUILD JSON */
                var obj = new[]
                {
                    new {
                        ActivityId = dto.ActivityId,
                        Title = dto.Title,
                        SubTitle = dto.SubTitle,
                        Description = dto.Description,
                        ImageUrl = dto.Image == null ? oldFileUrl : newFileUrl,
                        ImageName = dto.Image == null ? oldFileName : newFileName,
                        RedirectUrl = dto.RedirectUrl,
                        DisplayOrder = dto.DisplayOrder,
                        IsActive = dto.IsActive
                    }
                };

                var param = new SqlParameter[]
                {
                    new("@OPERATION_ID", 12),
                    new("@JSON", JsonConvert.SerializeObject(obj))
                };

                string update = await _unitofWork.bannerRepository
                    .CallStoreProcedure("Sp_Circle_ContentManagement", param);

                JObject json = JObject.Parse(update);

                if (!Convert.ToBoolean(json["Status"]))
                {
                    FileUploadHelper.DeleteFile(rootPath, newFileUrl);
                    return _responseService.Error((string)json["Response"]);
                }

                if (dto.Image != null)
                    FileUploadHelper.DeleteFile(rootPath, oldFileUrl);

                return _responseService.Success((string)json["Response"]);
            }
            catch (Exception ex)
            {
                FileUploadHelper.DeleteFile(rootPath, newFileUrl);
                return _responseService.Error(ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("remove-activity-by-id/{ActivityId:long}")]
        public async Task<ActionResult<ApiResponse>> DeleteActivity(long ActivityId)
        {
            var rootPath = _webHostEnvironment.WebRootPath
               ?? Path.Combine(Directory.GetCurrentDirectory());
            string? oldFileUrl = null;
            string? oldMobileUrl = null;
            var fetchParams = new SqlParameter[]
                {
                    new("@OPERATION_ID", 14),
                    new("@ActivityId", ActivityId)
                };

            string existingBannerResponse = await _unitofWork.bannerRepository
                .CallStoreProcedure("Sp_Circle_ContentManagement", fetchParams);

            JObject existingJson = JObject.Parse(existingBannerResponse);
            _paramObj = new SqlParameter[]
            {
                new SqlParameter("@OPERATION_ID", 15),
                new SqlParameter("@ActivityId", ActivityId)
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
                        FileUploadHelper.DeleteFile(rootPath, oldFileUrl);

                    if (oldMobileUrl != null)
                        FileUploadHelper.DeleteFile(rootPath, oldMobileUrl);

                }
                if (JSONObj["Response"] != null)
                    return _responseService.Success(JsonConvert.SerializeObject(JSONObj["Response"]));

                return _responseService.NotFound("Banner not found.");
            }

            return _responseService.Error((string)JSONObj["Response"]);



        }
        #endregion

        #region::Gallery

        /***************************************
         * Title - Get All Gallery
         * OPERATION_ID = 3
         ***************************************/

        [HttpGet]
        [Route("Get-all-gallery/{PageSize:long}/{PageNumber:long}", Name = "GetAllGallery")]
        public async Task<ActionResult<ApiResponse>> GetAllGallery(long PageSize, long PageNumber, string Search = null)
        {
            _paramObj = new SqlParameter[]
            {
        new SqlParameter("@OPERATION_ID",18),
        new SqlParameter("@PageSize",PageSize),
        new SqlParameter("@PageNumber",PageNumber),
        new SqlParameter("@Search",Search ?? (object)DBNull.Value),
            };

            string responseDetails = await _unitofWork.bannerRepository
                .CallStoreProcedure("Sp_Circle_ContentManagement", _paramObj);

            JObject JSONObj = JObject.Parse(responseDetails);

            if (Convert.ToBoolean(JSONObj["Status"]))
            {
                JArray responseArray = (JArray)JSONObj["Response"]!;

                if (responseArray != null && responseArray.Count > 0)
                {
                    return _responseService.PaginatedSuccess(
                        JsonConvert.SerializeObject(responseArray),
                        (int)JSONObj["TotalItems"]!,
                        (int)JSONObj["ItemsPerPage"]!,
                        (int)JSONObj["CurrentPage"]!,
                        (int)JSONObj["TotalPageCount"]!
                    );
                }

                return _responseService.NotFound("No gallery data found");
            }

            return _responseService.Error((string)JSONObj["Response"]);
        }

        /***************************************
         * Title - Get Gallery By Id
         * OPERATION_ID = 4
         ***************************************/
        [Authorize]
        [HttpGet]
        [Route("Get-gallery-by-id/{GalleryId:long}")]
        public async Task<ActionResult<ApiResponse>> GetGalleryById(long GalleryId)
        {
            _paramObj = new SqlParameter[]
            {
        new SqlParameter("@OPERATION_ID", 19),
        new SqlParameter("@GalleryItemsId", GalleryId)
            };

            string response = await _unitofWork.bannerRepository
                .CallStoreProcedure("Sp_Circle_ContentManagement", _paramObj);

            JObject JSONObj = JObject.Parse(response);

            if (Convert.ToBoolean(JSONObj["Status"]))
            {
                return _responseService.Success(JsonConvert.SerializeObject(JSONObj["Response"]));
            }

            return _responseService.Error((string)JSONObj["Response"]);
        }

        /***************************************
         * Title - Create Gallery
         * OPERATION_ID = 1
         ***************************************/
        [Authorize]
        [HttpPost]
        [Route("Create-gallery")]
        public async Task<ActionResult<ApiResponse>> CreateGallery([FromForm] CreateGalleryDto dto)
        {
            var rootPath = _webHostEnvironment.WebRootPath ?? Directory.GetCurrentDirectory();

            if (dto.File == null || dto.File.Length == 0)
                return _responseService.Error("Image is required");

            if (!FileUploadHelper.IsImage(dto.File))
                return _responseService.Error("Only image allowed");

            var (fileUrl, fileName) = await FileUploadHelper
                .SaveFileAsync(dto.File, rootPath, "gallery");

            var galleryObj = new[]
            {
                new
                {
                    Title = dto.Title,
                    SubTitle = dto.SubTitle,
                    ExpeditionYear = dto.ExpeditionYear,
                    ImageUrl = fileUrl,
                    ImageName = fileName,
                    DisplayOrder = dto.DisplayOrder,
                    IsActive = true
                }
            };

            _paramObj = new SqlParameter[]
            {
                new("@OPERATION_ID",16),
                new("@JSON", JsonConvert.SerializeObject(galleryObj))
            };

            string response = await _unitofWork.bannerRepository
                .CallStoreProcedure("Sp_Circle_ContentManagement", _paramObj);

            JObject JSONObj = JObject.Parse(response);

            if (Convert.ToBoolean(JSONObj["Status"]))
            {
                return _responseService.Success((string)JSONObj["Response"]);
            }

            FileUploadHelper.DeleteFile(rootPath, fileUrl);
            return _responseService.Error((string)JSONObj["Response"]);
        }

        /***************************************
         * Title - Update Gallery
         * OPERATION_ID = 2
         ***************************************/
        [Authorize]
        [HttpPost]
        [Route("Update-gallery")]
        public async Task<ActionResult<ApiResponse>> UpdateGallery([FromForm] UpdateGalleryDto dto)
        {
            string? newFileUrl = null;
            string? newFileName = null;
            string? oldFileUrl = null;
            string? oldFileName = null;

            var rootPath = _webHostEnvironment.WebRootPath ?? Directory.GetCurrentDirectory();

            try
            {
                // 1️⃣ Get existing
                var fetchParams = new SqlParameter[]
                {
            new("@OPERATION_ID", 19),
            new("@GalleryItemsId", dto.GalleryItemsId)
                };

                string existingResponse = await _unitofWork.bannerRepository
                    .CallStoreProcedure("Sp_Circle_ContentManagement", fetchParams);

                JObject existingJson = JObject.Parse(existingResponse);

                if (!Convert.ToBoolean(existingJson["Status"]))
                    return _responseService.Error("Gallery not found");

                JArray arr = (JArray)existingJson["Response"]!;
                oldFileUrl = arr[0]["ImageUrl"]?.ToString();
                oldFileName = arr[0]["ImageName"]?.ToString();

                // 2️⃣ Upload new image if exists
                if (dto.File != null)
                {
                    if (!FileUploadHelper.IsImage(dto.File))
                        return _responseService.Error("Only image allowed");

                    (newFileUrl, newFileName) = await FileUploadHelper
                        .SaveFileAsync(dto.File, rootPath, "gallery");
                }

                var galleryObj = new[]
                {
                    new
                    {
                        GalleryItemsId = dto.GalleryItemsId,
                        Title = dto.Title,
                        SubTitle = dto.SubTitle,
                        ExpeditionYear = dto.ExpeditionYear,
                        ImageUrl = dto.File == null ? oldFileUrl : newFileUrl,
                        ImageName = dto.File == null ? oldFileName : newFileName,
                        DisplayOrder = dto.DisplayOrder,
                        IsActive = dto.IsActive
                    }
                };

                var updateParams = new SqlParameter[]
                {
                    new("@OPERATION_ID",17),
                    new("@JSON", JsonConvert.SerializeObject(galleryObj))
                };

                string updateResponse = await _unitofWork.bannerRepository
                    .CallStoreProcedure("Sp_Circle_ContentManagement", updateParams);

                JObject updateJson = JObject.Parse(updateResponse);

                if (!Convert.ToBoolean(updateJson["Status"]))
                {
                    FileUploadHelper.DeleteFile(rootPath, newFileUrl);
                    return _responseService.Error((string)updateJson["Response"]);
                }

                // delete old file
                if (dto.File != null)
                    FileUploadHelper.DeleteFile(rootPath, oldFileUrl);

                return _responseService.Success((string)updateJson["Response"]);
            }
            catch (Exception ex)
            {
                FileUploadHelper.DeleteFile(rootPath, newFileUrl);
                return _responseService.Error(ex.Message);
            }
        }

        /***************************************
         * Title - Delete Gallery
         * OPERATION_ID = 5
         ***************************************/
        [Authorize]
        [HttpPost]
        [Route("remove-gallery-by-id/{GalleryId:long}")]
        public async Task<ActionResult<ApiResponse>> removeeteGallery(long GalleryId)
        {
            var rootPath = _webHostEnvironment.WebRootPath ?? Directory.GetCurrentDirectory();

            string? oldFileUrl = null;

            // fetch existing
            var fetchParams = new SqlParameter[]
            {
                new("@OPERATION_ID", 19),
                new("@GalleryItemsId", GalleryId)
            };

            string existingResponse = await _unitofWork.bannerRepository
                .CallStoreProcedure("Sp_Circle_ContentManagement", fetchParams);

            JObject existingJson = JObject.Parse(existingResponse);

            var deleteParams = new SqlParameter[]
            {
                new("@OPERATION_ID", 20),
                new("@GalleryItemsId", GalleryId)
            };

            string response = await _unitofWork.bannerRepository
                .CallStoreProcedure("Sp_Circle_ContentManagement", deleteParams);

            JObject JSONObj = JObject.Parse(response);

            if (Convert.ToBoolean(JSONObj["Status"]))
            {
                JArray arr = (JArray)existingJson["Response"]!;
                oldFileUrl = arr[0]["ImageUrl"]?.ToString();

                if (oldFileUrl != null)
                    FileUploadHelper.DeleteFile(rootPath, oldFileUrl);

                return _responseService.Success((string)JSONObj["Response"]);
            }

            return _responseService.Error((string)JSONObj["Response"]);
        }


        #endregion

        #region::Activity Details

        /***************************************
         * Title - Get All Activity Details
         * Procedure - Sp_Circle_ContentManagement
         * OPERATION_ID = 23
         ***************************************/
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("Get-all-activity-details/{PageSize:long}/{PageNumber:long}", Name = "GetAllActivityDetails")]
        public async Task<ActionResult<ApiResponse>> GetAllActivityDetails(
            long PageSize,
            long PageNumber,
            string Search = null)
        {
            try
            {
                _paramObj = new SqlParameter[]
                {
            new SqlParameter("@OPERATION_ID",23)
            {
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Input
            },

            new SqlParameter("@PageSize",PageSize)
            {
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Input
            },

            new SqlParameter("@PageNumber",PageNumber)
            {
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Input
            },

            new SqlParameter("@Search",Search ?? (object)DBNull.Value)
            {
                SqlDbType = SqlDbType.NVarChar,
                Direction = ParameterDirection.Input
            }
                };

                string responseDetails =
                    await _unitofWork.bannerRepository
                    .CallStoreProcedure("Sp_Circle_ContentManagement", _paramObj);

                JObject JSONObj = JObject.Parse(responseDetails);

                if (JSONObj.ContainsKey("Status") &&
                    Convert.ToBoolean(JSONObj["Status"]))
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

                    return _responseService.NotFound("No activity details found.");
                }

                return _responseService.Error((string)JSONObj["Response"]);
            }
            catch (Exception ex)
            {
                _logService.LogCustom(ex.Message, "GetAllActivityDetails");
                return _responseService.Error(ex.Message);
            }
        }


        /***************************************
         * Title - Get Activity Details By Id
         * OPERATION_ID = 24
         ***************************************/
        [Authorize]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("Get-activity-details-by-id/{ActivitieDetailsId:long}", Name = "GetActivityDetailsById")]
        public async Task<ActionResult<ApiResponse>> GetActivityDetailsById(long ActivitieDetailsId)
        {
            try
            {
                _paramObj = new SqlParameter[]
                {
            new SqlParameter("@OPERATION_ID",24),
            new SqlParameter("@ActivitieDetailsId",ActivitieDetailsId)
                };

                string responseDetails =
                    await _unitofWork.bannerRepository
                    .CallStoreProcedure("Sp_Circle_ContentManagement", _paramObj);

                JObject JSONObj = JObject.Parse(responseDetails);

                if (JSONObj.ContainsKey("Status") &&
                    Convert.ToBoolean(JSONObj["Status"]))
                {
                    if (JSONObj["Response"] != null)
                        return _responseService.Success(
                            JsonConvert.SerializeObject(JSONObj["Response"]));

                    return _responseService.NotFound("Activity details not found.");
                }

                return _responseService.Error((string)JSONObj["Response"]);
            }
            catch (Exception ex)
            {
                _logService.LogCustom(ex.Message, "GetActivityDetailsById");
                return _responseService.Error(ex.Message);
            }
        }


        /***************************************
         * Title - Create Activity Details
         * OPERATION_ID = 21
         ***************************************/
        [Authorize]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("Create-activity-details", Name = "CreateActivityDetails")]
        public async Task<ActionResult<ApiResponse>> CreateActivityDetails(
            [FromForm] CreateActivityDetailsDto dto)
        {
            try
            {
                if (dto == null)
                    return _responseService.Error("Request body is required.");

                if (dto.ActivityId <= 0)
                    return _responseService.Error("ActivityId is required.");

                if (string.IsNullOrWhiteSpace(dto.Title))
                    return _responseService.Error("Title is required.");

                if (dto.DisplayOrder <= 0)
                    return _responseService.Error("DisplayOrder is required.");

                List<object> imageList = new();

                if (dto.Images != null && dto.Images.Count > 0)
                {
                    short priority = 1;

                    foreach (var file in dto.Images)
                    {
                        if (!FileUploadHelper.IsImage(file))
                            return _responseService.Error("Only image files allowed.");

                        var uploadResult =
                            await FileUploadHelper.SaveFileAsync(
                                file,
                                _webHostEnvironment.WebRootPath,
                                "activity-details");

                        imageList.Add(new
                        {
                            ActivitieDetailsImageName = uploadResult.fileName,
                            ImagePath1 = uploadResult.fileUrl,
                            DisplayPriority = priority
                        });

                        priority++;
                    }
                }

                var activityObj = new[]
                {
            new
            {
                ActivityId = dto.ActivityId,
                Title = dto.Title,
                SubTitle = dto.SubTitle,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Location = dto.Location,
                Duration = dto.Duration,
                Fee = dto.Fee,
                DisplayOrder = dto.DisplayOrder,
                IsActive = true,
                Images = imageList
            }
        };

                _paramObj = new SqlParameter[]
                {
            new SqlParameter("@OPERATION_ID",21),
            new SqlParameter("@JSON",
                JsonConvert.SerializeObject(activityObj))
                };

                string responseDetails =
                    await _unitofWork.bannerRepository
                    .CallStoreProcedure("Sp_Circle_ContentManagement", _paramObj);

                JObject JSONObj = JObject.Parse(responseDetails);

                if (Convert.ToBoolean(JSONObj["Status"]))
                {
                    return _responseService.Success(
                        (string)JSONObj["Response"]);
                }

                return _responseService.Error(
                    (string)JSONObj["Response"]);
            }
            catch (Exception ex)
            {
                _logService.LogCustom(ex.Message, "CreateActivityDetails");
                return _responseService.Error(ex.Message);
            }
        }


        /***************************************
         * Title - Update Activity Details
         * OPERATION_ID = 22
         ***************************************/
        [Authorize]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("Update-activity-details", Name = "UpdateActivityDetails")]
        public async Task<ActionResult<ApiResponse>> UpdateActivityDetails(
            [FromForm] UpdateActivityDetailsDto dto)
        {

            try
            {
                if (dto == null)
                    return _responseService.Error("Request body is required.");

                if (dto.ActivitieDetailsId <= 0)
                    return _responseService.Error("ActivitieDetailsId is required.");

                if (dto.ActivityId <= 0)
                    return _responseService.Error("ActivityId is required.");

                if (string.IsNullOrWhiteSpace(dto.Title))
                    return _responseService.Error("Title is required.");



                var webRootPath = _webHostEnvironment.WebRootPath ?? Directory.GetCurrentDirectory();

                // Track file operations so we can roll back safely if the DB transaction fails.
                var movedToTrash = new List<(string OriginalFullPath, string TrashFullPath)>();
                var newlyCreatedFullPaths = new List<string>();


                var fetchParams = new SqlParameter[]
                {
            new SqlParameter("@OPERATION_ID",24),
            new SqlParameter("@ActivitieDetailsId",dto.ActivitieDetailsId)
                };

                string existingResponse =
                    await _unitofWork.bannerRepository
                    .CallStoreProcedure("Sp_Circle_ContentManagement", fetchParams);

                JObject existingJson = JObject.Parse(existingResponse);

                if (!Convert.ToBoolean(existingJson["Status"]))
                    return _responseService.Error("Activity details not found.");

                JArray existingData =
                    (JArray)existingJson["Response"]!;

                List<object> imageList = new();
                var existingImages = await _unitofWork.activitieDetailsImageRepository.GetAllAsync(x => x.ActivitieDetailsId == dto.ActivitieDetailsId && x.IsDeleted == false);
                await DeleteaCTIVITYImagesAsync(existingImages.ToList(), dto.DeletedImageIds ?? new List<long>());

                await AddActivityDetailsImagesAsync(dto.ActivitieDetailsId, dto.NewImages ?? new List<IFormFile>(), existingImages.ToList());

                var activityObj = new[]
                {
                    new
                    {
                        ActivitieDetailsId = dto.ActivitieDetailsId,
                        ActivityId = dto.ActivityId,
                        Title = dto.Title,
                        SubTitle = dto.SubTitle,
                        Description = dto.Description,
                        StartDate = dto.StartDate,
                        EndDate = dto.EndDate,
                        Location = dto.Location,
                        Duration = dto.Duration,
                        Fee = dto.Fee,
                        DisplayOrder = dto.DisplayOrder,
                        IsActive = dto.IsActive,
                        Images = imageList
                    }
                };

                _paramObj = new SqlParameter[]
                {
                    new SqlParameter("@OPERATION_ID",22),
                    new SqlParameter("@JSON",
                    JsonConvert.SerializeObject(activityObj))
                };

                string responseDetails =
                    await _unitofWork.bannerRepository
                    .CallStoreProcedure("Sp_Circle_ContentManagement", _paramObj);

                JObject JSONObj = JObject.Parse(responseDetails);

                if (Convert.ToBoolean(JSONObj["Status"]))
                {
                    return _responseService.Success(
                        (string)JSONObj["Response"]);
                }

                return _responseService.Error(
                    (string)JSONObj["Response"]);
            }
            catch (Exception ex)
            {
                _logService.LogCustom(ex.Message, "UpdateActivityDetails");
                return _responseService.Error(ex.Message);
            }
        }

        private async Task DeleteaCTIVITYImagesAsync(
            List<ActivitieDetailsImage> existingImages,
            List<long> deletedImageIds
            )
        {
            var rootPath = _webHostEnvironment.WebRootPath ?? Directory.GetCurrentDirectory();
            var deleteSet = deletedImageIds
                .Where(x => x > 0)
                .Distinct()
                .ToHashSet();

            if (deleteSet.Count == 0)
                return;

            var existingIds = existingImages.Select(x => x.ActivitieDetailsImageId).ToHashSet();
            var invalidIds = deleteSet.Where(x => !existingIds.Contains(x)).ToList();

            if (invalidIds.Count > 0)
                throw new InvalidOperationException($"One or more images do not belong to this club description: {string.Join(",", invalidIds)}");

            var imagesToDelete = existingImages.Where(x => deleteSet.Contains(x.ActivitieDetailsImageId ?? 0)).ToList();

            // Move files to a trash folder first. If the DB transaction rolls back, we can move them back.
            foreach (var img in imagesToDelete)
            {

                FileUploadHelper.DeleteFile(rootPath, img.ImagePath1);

            }

            await _unitofWork.activitieDetailsImageRepository
                .RemoveRangeAsync(imagesToDelete);
        }

        private async Task AddActivityDetailsImagesAsync(
            long ActivityDetailsId,
            List<IFormFile> newImages,
            List<ActivitieDetailsImage> existingImages)
        {
            string? newFileUrl = null;
            string? newFileName = null;
            if (newImages.Count == 0)
                return;

            var rootPath = _webHostEnvironment.WebRootPath ?? Directory.GetCurrentDirectory();

            var nextDisplayOrder = existingImages.Count == 0 ? 1 : existingImages.Max(x => x.DisplayPriority) + 1;

            foreach (var file in newImages)
            {
                (newFileUrl, newFileName) = await FileUploadHelper.SaveFileAsync(file, rootPath, "activity-details");

                await _unitofWork.activitieDetailsImageRepository.AddAsync(new ActivitieDetailsImage
                {
                    ActivitieDetailsId = ActivityDetailsId,
                    ActivitieDetailsImageName = newFileName,
                    ImagePath1 = newFileUrl,
                    DisplayPriority = Convert.ToInt16(nextDisplayOrder++),
                    IsActive = true,
                    IsDeleted = false,
                    UpdatedDate = DateTime.UtcNow
                });
            }
        }

        /***************************************
         * Title - Remove Activity Details
         * OPERATION_ID = 25
         ***************************************/
        [Authorize]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("remove-activity-details-by-id/{ActivitieDetailsId:long}",
            Name = "RemoveActivityDetailsById")]
        public async Task<ActionResult<ApiResponse>> RemoveActivityDetailsById(
            long ActivitieDetailsId)
        {
            var rootPath = _webHostEnvironment.WebRootPath ?? Directory.GetCurrentDirectory();
            try
            {
                var existingImages = await _unitofWork.activitieDetailsImageRepository.GetAllAsync(x => x.ActivitieDetailsId == ActivitieDetailsId && x.IsDeleted == false);

                foreach (var img in existingImages)
                {

                    FileUploadHelper.DeleteFile(rootPath, img.ImagePath1);

                }


                _paramObj = new SqlParameter[]
                {
            new SqlParameter("@OPERATION_ID",25),
            new SqlParameter("@ActivitieDetailsId",ActivitieDetailsId)
                };

                string responseDetails =
                    await _unitofWork.bannerRepository
                    .CallStoreProcedure("Sp_Circle_ContentManagement", _paramObj);

                JObject JSONObj = JObject.Parse(responseDetails);

                if (JSONObj.ContainsKey("Status") &&
                    Convert.ToBoolean(JSONObj["Status"]))
                {
                    return _responseService.Success(
                        JsonConvert.SerializeObject(JSONObj["Response"]));
                }

                return _responseService.Error(
                    (string)JSONObj["Response"]);
            }
            catch (Exception ex)
            {
                _logService.LogCustom(ex.Message, "RemoveActivityDetailsById");
                return _responseService.Error(ex.Message);
            }
        }


        /***************************************
         * Title - Get Activity Details By ActivityId
         * OPERATION_ID = 27
         ***************************************/
        [Authorize]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("Get-activity-details-by-activitie-id/{ActivitieId:long}", Name = "GetActivityDetailsByActivityId")]
        public async Task<ActionResult<ApiResponse>> GetActivityDetailsByActivityId(long ActivitieId)
        {
            try
            {
                _paramObj = new SqlParameter[]
                {
            new SqlParameter("@OPERATION_ID",27),
            new SqlParameter("@ActivityId",ActivitieId)
                };

                string responseDetails =
                    await _unitofWork.bannerRepository
                    .CallStoreProcedure("Sp_Circle_ContentManagement", _paramObj);

                JObject JSONObj = JObject.Parse(responseDetails);

                if (JSONObj.ContainsKey("Status") &&
                    Convert.ToBoolean(JSONObj["Status"]))
                {
                    if (JSONObj["Response"] != null)
                        return _responseService.Success(
                            JsonConvert.SerializeObject(JSONObj["Response"]));

                    return _responseService.NotFound("Activity details not found.");
                } 
                else
                {
                    return _responseService.NotFound("Activity details not found.");
                }


                
            }
            catch (Exception ex)
            {
                _logService.LogCustom(ex.Message, "GetActivityDetailsById");
                return _responseService.Error(ex.Message);
            }
        }

        #endregion

        #region::Achievement Details & Gallery

        [HttpGet]
        [Route("get-all-achievement/{PageSize:long}/{PageNumber:long}")]
        public async Task<ActionResult<ApiResponse>> GetAllAchievement(long PageSize, long PageNumber, string Search = null)
        {
            _paramObj = new SqlParameter[]
            {
                new("@OPERATION_ID", 32),
                new("@PageSize", PageSize),
                new("@PageNumber", PageNumber),
                new("@Search", Search)
            };

            string response = await _unitofWork.bannerRepository
                .CallStoreProcedure("Sp_Circle_ContentManagement", _paramObj);

            if (string.IsNullOrWhiteSpace(response))
            {
                return _responseService.Error("No data returned from database.");
            }

            JObject json = JObject.Parse(response);

            bool status = json["Status"]?.Value<bool>() ?? false;

            if (status)
            {
                JArray arr = json["Response"] as JArray ?? new JArray();

                int totalItems = json["TotalItems"]?.Value<int>() ?? 0;
                int itemsPerPage = json["ItemsPerPage"]?.Value<int>() ?? 0;
                int currentPage = json["CurrentPage"]?.Value<int>() ?? 0;
                int totalPageCount = json["TotalPageCount"]?.Value<int>() ?? 0;

                return _responseService.PaginatedSuccess(
                    JsonConvert.SerializeObject(arr, Formatting.None),
                    totalItems,
                    itemsPerPage,
                    currentPage,
                    totalPageCount
                );
            }

                return _responseService.Error((string)json["Response"]);
        }

        [Authorize]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("Create-achievement", Name = "CreateAchievement")]
        public async Task<ActionResult<ApiResponse>> CreateAchievement([FromForm] AchievementDetailsCreateDto dto)
        {
            

            var achievementObj = new[]
            {
                new {
                    Title = dto.Title,
                    GalleryItemsId = dto.GalleryItemsId,
                    SubTitle = dto.SubTitle,
                    IsActive = true
                }
            };

            _paramObj = new SqlParameter[]
            {
                new("@OPERATION_ID", 28),
                new("@JSON", JsonConvert.SerializeObject(achievementObj))
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
                return _responseService.Error((string)JSONObj["Response"]);
            }



        }

        [Authorize]
        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("Update-achievement", Name = "UpdateAchievement")]
        public async Task<ActionResult<ApiResponse>> UpdateAchievement([FromForm] AchievementDetailsUpdateDto dto)
        {
            _logService.LogCustom("Reached UpdateBanner", "Diagnostic");
            
            try
            {


                var achievementObj = new[]
                {
            new {
                AchievementDetailsId = dto.AchievementDetailsId,
                Title = dto.Title,
                GalleryItemsId = dto.GalleryItemsId,
                SubTitle = dto.SubTitle,
                IsActive = dto.IsActive
            }
        };

                var updateParams = new SqlParameter[]
                {
            new("@OPERATION_ID", 29),
            new("@JSON", JsonConvert.SerializeObject(achievementObj))
                };

                string updateResponse = await _unitofWork.bannerRepository
                    .CallStoreProcedure("Sp_Circle_ContentManagement", updateParams);

                JObject updateJson = JObject.Parse(updateResponse);


                if (!Convert.ToBoolean(updateJson["Status"]))
                {


                    return _responseService.Error((string)updateJson["Response"]);
                }

                

                return _responseService.Success((string)updateJson["Response"]);
            }
            catch (Exception ex)
            {
                return _responseService.Error(ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("create-achievement-gallery")]
        public async Task<ActionResult<ApiResponse>> CreateAchievementGalley([FromForm] AchievementDetailsGalleryCreateDto dto)
        {
            var rootPath = _webHostEnvironment.WebRootPath ?? Directory.GetCurrentDirectory();

            if (dto.Image == null)
                return _responseService.Error("Image required");

            if (!FileUploadHelper.IsImage(dto.Image))
                return _responseService.Error("Only image allowed");

            var (fileUrl, fileName) =
                await FileUploadHelper.SaveFileAsync(dto.Image, rootPath, "achievement");

            var obj = new[]
            {
                new {
                    AchievementDetailsId=dto.AchievementDetailsId,
                    Title = dto.Title,
                    Description = dto.Description,
                    ImagePath1 = fileUrl,
                    DisplayOrder = dto.DisplayOrder,
                    IsActive = true
                }
            };

            _paramObj = new SqlParameter[]
            {
            new("@OPERATION_ID", 30),
            new("@JSON", JsonConvert.SerializeObject(obj))
            };

            string response = await _unitofWork.bannerRepository
                .CallStoreProcedure("Sp_Circle_ContentManagement", _paramObj);

            JObject json = JObject.Parse(response);

            if (Convert.ToBoolean(json["Status"]))
                return _responseService.Success((string)json["Response"]);

            FileUploadHelper.DeleteFile(rootPath, fileUrl);
            return _responseService.Error((string)json["Response"]);
        }

        [Authorize]
        [HttpPost]
        [Route("update-achievement-gallery")]
        public async Task<ActionResult<ApiResponse>> UpdateAchievementGallery([FromForm] AchievementDetailsGalleryUpdateDto dto)
        {
            try
            {
                var rootPath = _webHostEnvironment.WebRootPath ?? Directory.GetCurrentDirectory();
                var existing = await _unitofWork.achievementDetailsGalleryRepository.GetAsync(x => x.AchievementDetailsGalleryId == dto.AchievementDetailsGalleryId, tracked: false);
                if (existing == null)
                {
                    return _responseService.Error("Achievement Gallery details not found.");
                }

                string fileUrl = existing.ImagePath1;

                if (dto.Image != null)
                {
                    if (!FileUploadHelper.IsImage(dto.Image))
                        return _responseService.Error("Only image allowed");

                    var (newUrl, fileName) = await FileUploadHelper.SaveFileAsync(dto.Image, rootPath, "achievement");
                    
                    // delete old file if it exists
                    if (!string.IsNullOrEmpty(existing.ImagePath1))
                    {
                        FileUploadHelper.DeleteFile(rootPath, existing.ImagePath1);
                    }
                    fileUrl = newUrl;
                }

                var obj = new[]
                {
                    new {
                        AchievementDetailsGalleryId = dto.AchievementDetailsGalleryId,
                        AchievementDetailsId = dto.AchievementDetailsId,
                        Title = dto.Title,
                        Description = dto.Description,
                        ImagePath1 = fileUrl,
                        DisplayOrder = dto.DisplayOrder,
                        IsActive = dto.IsActive
                    }
                };

                _paramObj = new SqlParameter[]
                {
                    new("@OPERATION_ID", 31),
                    new("@JSON", JsonConvert.SerializeObject(obj))
                };

                string response = await _unitofWork.bannerRepository
                    .CallStoreProcedure("Sp_Circle_ContentManagement", _paramObj);

                JObject json = JObject.Parse(response);

                if (Convert.ToBoolean(json["Status"]))
                    return _responseService.Success((string)json["Response"]);

                return _responseService.Error((string)json["Response"]);
            }
            catch (Exception ex)
            {
                return _responseService.Error(ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("delete-achievement-gallery-by-id/{id:long}")]
        public async Task<ActionResult<ApiResponse>> DeleteAchievementGallery(long id)
        {
            try
            {
                var rootPath = _webHostEnvironment.WebRootPath ?? Directory.GetCurrentDirectory();
                var existing = await _unitofWork.achievementDetailsGalleryRepository.GetAsync(x => x.AchievementDetailsGalleryId == id, tracked: false);
                if (existing == null)
                {
                    return _responseService.Error("Achievement Gallery details not found.");
                }
                _paramObj = new SqlParameter[]
                {
                    new("@OPERATION_ID", 33),
                    new("@AchievementDetailsGalleryId", id)
                };

                string response = await _unitofWork.bannerRepository
                    .CallStoreProcedure("Sp_Circle_ContentManagement", _paramObj);

                JObject json = JObject.Parse(response);

                if (Convert.ToBoolean(json["Status"]))
                {
                    if (!string.IsNullOrEmpty(existing.ImagePath1))
                    {
                        FileUploadHelper.DeleteFile(rootPath, existing.ImagePath1);
                    }
                    return _responseService.Success((string)json["Response"]);
                }

                return _responseService.Error((string)json["Response"]);
            }
            catch (Exception ex)
            {
                return _responseService.Error(ex.Message);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("get-achievement-gallery-by-achievement-id/{id:long}")]
        public async Task<ActionResult<ApiResponse>> GetAchievementGalleryByAchievementId(long id)
        {
            try
            {
                _paramObj = new SqlParameter[]
                {
                    new("@OPERATION_ID", 34),
                    new("@AchievementDetailsId", id)
                };

                string response = await _unitofWork.bannerRepository
                    .CallStoreProcedure("Sp_Circle_ContentManagement", _paramObj);

                JObject json = JObject.Parse(response);

                if (Convert.ToBoolean(json["Status"]))
                {
                    return _responseService.Success(JsonConvert.SerializeObject(json["Response"]));
                }

                return _responseService.Error((string)json["Response"]);
            }
            catch (Exception ex)
            {
                return _responseService.Error(ex.Message);
            }
        }

        [Authorize]
        [HttpPost]
        [Route("delete-achievement-by-id/{id:long}")]
        public async Task<ActionResult<ApiResponse>> DeleteAchievementDetails(long id)
        {
            try
            {
                var rootPath = _webHostEnvironment.WebRootPath ?? Directory.GetCurrentDirectory();
                
                // Fetch all child gallery items first to delete their physical files
                var galleryItems = await _unitofWork.achievementDetailsGalleryRepository.GetAllAsync(x => x.AchievementDetailsId == id && x.IsDeleted == false);
                
                _paramObj = new SqlParameter[]
                {
                    new("@OPERATION_ID", 35),
                    new("@AchievementDetailsId", id)
                };

                string response = await _unitofWork.bannerRepository
                    .CallStoreProcedure("Sp_Circle_ContentManagement", _paramObj);

                JObject json = JObject.Parse(response);

                if (Convert.ToBoolean(json["Status"]))
                {
                    // Physically delete child gallery files
                    foreach (var item in galleryItems)
                    {
                        if (!string.IsNullOrEmpty(item.ImagePath1))
                        {
                            FileUploadHelper.DeleteFile(rootPath, item.ImagePath1);
                        }
                    }
                    return _responseService.Success((string)json["Response"]);
                }

                return _responseService.Error((string)json["Response"]);
            }
            catch (Exception ex)
            {
                return _responseService.Error(ex.Message);
            }
        }

        [Authorize]
        [HttpGet]
        [Route("get-achievement-by-id/{id:long}")]
        public async Task<ActionResult<ApiResponse>> GetAchievementDetailsById(long id)
        {
            try
            {
                _paramObj = new SqlParameter[]
                {
                    new("@OPERATION_ID", 36),
                    new("@AchievementDetailsId", id)
                };

                string response = await _unitofWork.bannerRepository
                    .CallStoreProcedure("Sp_Circle_ContentManagement", _paramObj);

                JObject json = JObject.Parse(response);

                if (Convert.ToBoolean(json["Status"]))
                {
                    return _responseService.Success(JsonConvert.SerializeObject(json["Response"]));
                }

                return _responseService.Error((string)json["Response"]);
            }
            catch (Exception ex)
            {
                return _responseService.Error(ex.Message);
            }
        }

        #endregion
    }
}
