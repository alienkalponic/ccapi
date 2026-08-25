using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Project.Application.Common.Repository;
using Project.Domain.Dto.ActivityRegistration;
using Project.Domain.Utility;
using Project.Infastructure.Data;
using System.Data;

namespace Project.Api.Controllers
{
    [Route("api/ActivityInterestRegistration")]
    [ApiController]
    public class ActivityInterestRegistrationController : ControllerBase
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

        public ActivityInterestRegistrationController(IUnitOfWork unitofWork,
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

        /***************************************
         * Title - Create Activity Interest Registration
         * Login Location - PUBLIC WEBSITE
         * Procedure - Sp_ActivityInterestRegistration
         * EXEC - EXEC [dbo].[Sp_ActivityInterestRegistration] @OPERATION_ID=1
         ***************************************/
        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("Create-activity-interest", Name = "CreateActivityInterest")]
        public async Task<ActionResult<ApiResponse>> CreateActivityInterest(
            [FromBody] CreateActivityInterestRegistrationDto dto)
        {
            try
            {
                var interestObj = new[]
                {
            new
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                ClubActivityId = dto.ClubActivityId,
                Message = dto.Message,
                IsActive = true
            }
        };
                var json = JsonConvert.SerializeObject(interestObj);
                _paramObj = new SqlParameter[]
                {
            new SqlParameter("@OPERATION_ID", 1)
            {
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Input
            },

            new SqlParameter(
                "@JSON",
                JsonConvert.SerializeObject(interestObj))
            {
                SqlDbType = SqlDbType.NVarChar,
                Direction = ParameterDirection.Input
            }
                };

                string responseDetails =
                    await _unitofWork.bannerRepository
                    .CallStoreProcedure(
                        "Sp_Circle_ActivityInterestRegistration",
                        _paramObj);

                JObject JSONObj = JObject.Parse(responseDetails);

                if (JSONObj.ContainsKey("Status") &&
                    Convert.ToBoolean(JSONObj["Status"]))
                {
                    return _responseService.Success(
                        (string)JSONObj["Response"]);
                }

                return _responseService.Error(
                    (string)JSONObj["Response"]);
            }
            catch (Exception ex)
            {
                return _responseService.Error(ex.Message);
            }
        }


        /***************************************
         * Title - Get All Activity Interest Registration
         * Login Location - ADMIN PANEL
         * Procedure - Sp_ActivityInterestRegistration
         * EXEC - EXEC [dbo].[Sp_ActivityInterestRegistration] @OPERATION_ID=3
         ***************************************/
        [Authorize]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("Get-all-activity-interest/{PageSize:long}/{PageNumber:long}",
            Name = "GetAllActivityInterest")]
        public async Task<ActionResult<ApiResponse>> GetAllActivityInterest(
            long PageSize,
            long PageNumber,
            string Search = null,
            long? ClubActivityId = null,
            bool? IsActive = null)
        {
            try
            {
                _paramObj = new SqlParameter[]
                {
            new SqlParameter("@OPERATION_ID", 3)
            {
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Input
            },

            new SqlParameter("@PageSize", PageSize)
            {
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Input
            },

            new SqlParameter("@PageNumber", PageNumber)
            {
                SqlDbType = SqlDbType.Int,
                Direction = ParameterDirection.Input
            },

            new SqlParameter("@Search",
                (object?)Search ?? DBNull.Value)
            {
                SqlDbType = SqlDbType.NVarChar,
                Direction = ParameterDirection.Input
            },

            new SqlParameter("@ClubActivityId",
                (object?)ClubActivityId ?? DBNull.Value)
            {
                SqlDbType = SqlDbType.BigInt,
                Direction = ParameterDirection.Input
            },

            new SqlParameter("@IsActive",
                (object?)IsActive ?? DBNull.Value)
            {
                SqlDbType = SqlDbType.Bit,
                Direction = ParameterDirection.Input
            }
                };

                string responseDetails =
                    await _unitofWork.bannerRepository
                    .CallStoreProcedure(
                        "Sp_ActivityInterestRegistration",
                        _paramObj);

                JObject JSONObj = JObject.Parse(responseDetails);

                if (JSONObj.ContainsKey("Status") &&
                    Convert.ToBoolean(JSONObj["Status"]))
                {
                    JArray responseArray =
                        (JArray)JSONObj["Response"]!;

                    if (responseArray != null &&
                        responseArray.Count > 0)
                    {
                        return _responseService.PaginatedSuccess(
                            JsonConvert.SerializeObject(
                                responseArray,
                                Formatting.None),

                            (int)JSONObj["TotalItems"]!,
                            (int)JSONObj["ItemsPerPage"]!,
                            (int)JSONObj["CurrentPage"]!,
                            (int)JSONObj["TotalPageCount"]!
                        );
                    }

                    return _responseService.NotFound(
                        "Response array is empty.");
                }
                else
                {
                    return _responseService.Error(
                        (string)JSONObj["Response"]);
                }
            }
            catch (Exception ex)
            {
                return _responseService.Error(ex.Message);
            }
        }

        /***************************************
         * Title - Get Activity Interest By Id
         * Login Location - ADMIN PANEL
         * Procedure - Sp_ActivityInterestRegistration
         * EXEC - OPERATION_ID = 4
         ***************************************/
        [Authorize]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("Get-activity-interest-by-id/{InterestId:long}",
            Name = "GetActivityInterestById")]
        public async Task<ActionResult<ApiResponse>> GetActivityInterestById(
            long InterestId)
        {
            try
            {
                _paramObj = new SqlParameter[]
                {
            new SqlParameter("@OPERATION_ID", 4),

            new SqlParameter("@InterestId", InterestId)
                };

                string responseDetails =
                    await _unitofWork.bannerRepository
                    .CallStoreProcedure(
                        "Sp_ActivityInterestRegistration",
                        _paramObj);

                JObject JSONObj =
                    JObject.Parse(responseDetails);

                if (JSONObj.ContainsKey("Status") &&
                    Convert.ToBoolean(JSONObj["Status"]))
                {
                    if (JSONObj["Response"] != null)
                    {
                        return _responseService.Success(
                            JsonConvert.SerializeObject(
                                JSONObj["Response"]));
                    }

                    return _responseService.NotFound(
                        "Activity interest not found.");
                }

                return _responseService.Error(
                    (string)JSONObj["Response"]);
            }
            catch (Exception ex)
            {
                return _responseService.Error(ex.Message);
            }
        }

        /***************************************
         * Title - Update Activity Interest Registration
         * Login Location - ADMIN PANEL
         * Procedure - Sp_ActivityInterestRegistration
         * EXEC - EXEC [dbo].[Sp_ActivityInterestRegistration] @OPERATION_ID=2
         ***************************************/
        [Authorize]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("Update-activity-interest",
            Name = "UpdateActivityInterest")]
        public async Task<ActionResult<ApiResponse>> UpdateActivityInterest(
            [FromBody] UpdateActivityInterestRegistrationDto dto)
        {
            try
            {
                var interestObj = new[]
                {
            new
            {
                InterestId = dto.InterestId,
                FullName = dto.FullName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                ClubActivityId = dto.ClubActivityId,
                Message = dto.Message,
                IsActive = dto.IsActive
            }
        };

                _paramObj = new SqlParameter[]
                {
            new SqlParameter("@OPERATION_ID", 2),

            new SqlParameter(
                "@JSON",
                JsonConvert.SerializeObject(interestObj))
            {
                SqlDbType = SqlDbType.NVarChar,
                Direction = ParameterDirection.Input
            }
                };

                string responseDetails =
                    await _unitofWork.bannerRepository
                    .CallStoreProcedure(
                        "Sp_ActivityInterestRegistration",
                        _paramObj);

                JObject JSONObj =
                    JObject.Parse(responseDetails);

                if (JSONObj.ContainsKey("Status") &&
                    Convert.ToBoolean(JSONObj["Status"]))
                {
                    return _responseService.Success(
                        (string)JSONObj["Response"]);
                }

                return _responseService.Error(
                    (string)JSONObj["Response"]);
            }
            catch (Exception ex)
            {
                return _responseService.Error(ex.Message);
            }
        }

        /***************************************
         * Title - Remove Activity Interest Registration
         * Login Location - ADMIN PANEL
         * Procedure - Sp_ActivityInterestRegistration
         * EXEC - EXEC [dbo].[Sp_ActivityInterestRegistration] @OPERATION_ID=5
         ***************************************/
        [Authorize]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("remove-activity-interest-by-id/{InterestId:long}",
            Name = "RemoveActivityInterestById")]
        public async Task<ActionResult<ApiResponse>> RemoveActivityInterestById(
            long InterestId)
        {
            try
            {
                _paramObj = new SqlParameter[]
                {
            new SqlParameter("@OPERATION_ID", 5),

            new SqlParameter("@InterestId", InterestId)
                };

                string responseDetails =
                    await _unitofWork.bannerRepository
                    .CallStoreProcedure(
                        "Sp_ActivityInterestRegistration",
                        _paramObj);

                JObject JSONObj =
                    JObject.Parse(responseDetails);

                if (JSONObj.ContainsKey("Status") &&
                    Convert.ToBoolean(JSONObj["Status"]))
                {
                    if (JSONObj["Response"] != null)
                    {
                        return _responseService.Success(
                            JsonConvert.SerializeObject(
                                JSONObj["Response"]));
                    }

                    return _responseService.NotFound(
                        "Activity interest not found.");
                }

                return _responseService.Error(
                    (string)JSONObj["Response"]);
            }
            catch (Exception ex)
            {
                return _responseService.Error(ex.Message);
            }
        }
    }
}
