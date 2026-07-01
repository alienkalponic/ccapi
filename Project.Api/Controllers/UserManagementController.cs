using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Project.Application.Common.Repository;
using Project.Domain.Dto.ActivityRegistration;
using Project.Domain.Dto.Banner;
using Project.Domain.Utility;
using Project.Infastructure.Data;
using System.Data;

namespace Project.Api.Controllers
{
    [Route("api/usermanagement")]
    [ApiController]
    public class UserManagementController : ControllerBase
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

        public UserManagementController(IUnitOfWork unitofWork,
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

        #region::Activity Registration

        /***************************************
          * Title - Get All Activity Registration Details 
          * Login Location -Admin PANEL
          * Procedure - Sp_Circle_ContentManagement
          * EXEC - EXEC [dbo].[Sp_Circle_UsersManagement] @OPERATION_ID=2
          ***************************************/
        [Authorize]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("Get-all-activityregistration/{PageSize:long}/{PageNumber:long}", Name = "GetAllActivityRegistration")]
        public async Task<ActionResult<ApiResponse>> GetAllActivityRegistration(long PageSize, long PageNumber, string Search = null)
        {

            _paramObj = new SqlParameter[]
                {

                        new SqlParameter("@OPERATION_ID",2) {SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input },
                        new SqlParameter("@PageSize",PageSize) {SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input },
                        new SqlParameter("@PageNumber",PageNumber) {SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input },
                        new SqlParameter("@Search",Search) {SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input },


                };

            string responseDetails = await _unitofWork.bannerRepository.CallStoreProcedure("Sp_Circle_UsersManagement", _paramObj);


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
         * Title - Get Activity Registration By Id
         * Login Location - ADMIN PANEL
         * EXEC - OPERATION_ID = 3
         ***************************************/
        [Authorize]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("Get-activityregistration-by-id/{RegistrationId:long}", Name = "GetActivityRegistrationById")]
        public async Task<ActionResult<ApiResponse>> GetBannerById(long RegistrationId)
        {
            _paramObj = new SqlParameter[]
            {
        new SqlParameter("@OPERATION_ID", 3),
        new SqlParameter("@BannerId", RegistrationId)
            };

            string responseDetails = await _unitofWork.bannerRepository
                .CallStoreProcedure("Sp_Circle_UsersManagement", _paramObj);

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
          * Title - Create ActivityRegistration Details 
          * Login Location - ADMIN PANEL
          * Procedure - Sp_Circle_UsersManagement
          * EXEC - EXEC [dbo].[Sp_Circle_UsersManagement] @OPERATION_ID=1
          ***************************************/
        //[Authorize]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("Create-activityregistration", Name = "CreateActivityRegistration")]
        public async Task<ActionResult<ApiResponse>> CreateActivityRegistration([FromForm] ActivityRegistrationCreateDto dto)
        {
            

            var bannerObj = new[]
            {
                new {
                    fullname = dto.FullName,
                    emailaddress = dto.EmailAddress,
                    phonenumber = dto.PhoneNumber,
                    activityid = dto.ActivityId,
                    activityname = dto.ActivityName,
                    messagenotes = dto.MessageNotes,
                    IsActive = true
                }
            };

            _paramObj = new SqlParameter[]
            {
                new("@OPERATION_ID", 1),
                new("@JSON", JsonConvert.SerializeObject(bannerObj))
            };

            string response = await _unitofWork.bannerRepository
                .CallStoreProcedure("Sp_Circle_UsersManagement", _paramObj);

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

        #endregion
    }
}
