using AutoMapper;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Project.Application.Common.Repository;
using Project.Domain.Dto.Login;
using Project.Domain.Model;
using Project.Domain.Utility;
using Project.Infastructure.Service;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Project.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IUnitOfWork _unitofWork;
        //private readonly ApiResponse apiResponse;
        private string secretKey;
        private SqlParameter[] _paramObj;
        private string connectionString;
        private readonly IMapper _mapper;
        private readonly LogService _logService;
        private Dictionary<string, object> _dictionaryObj;
        private readonly IApiResponseService _responseService;


        public AuthenticationController(IUnitOfWork unitofWork, IMapper mapper, LogService logService, IApiResponseService responseService)
        {
            _unitofWork = unitofWork;
            //this.apiResponse = new();
            secretKey = "This is Used to sign and Verify JWT Token";
            _mapper = mapper;
            this._dictionaryObj = new Dictionary<string, object>();
            _logService = logService;
            _responseService = responseService;
        }

        [HttpPost("login")]

        public async Task<ActionResult<ApiResponse>> Login([FromBody] LoginRequestDTO loginRequestDTO)
        {
            Encryption eobj = new Encryption();



            if (string.IsNullOrEmpty(loginRequestDTO.UserName) || string.IsNullOrEmpty(loginRequestDTO.Password))
            {
                return _responseService.BadRequest("Email or Password is incorrect");
            }

            loginRequestDTO.Password = eobj.Encrypt_final(loginRequestDTO.Password);

            _paramObj = new SqlParameter[]
            {
                //EXEC [dbo].[Sp_Artemis_USER_LOGIN_ACTIVITY] @OPERATION_ID=1,@EMAIL_ID ='tmladmin1',@PASSWORD='abc@123'
                new SqlParameter("@OPERATION_ID",1) {SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input },
                new SqlParameter("@EMAIL_ID",loginRequestDTO.UserName) {SqlDbType = SqlDbType.NVarChar, Direction= ParameterDirection.Input },
                new SqlParameter("@PASSWORD",loginRequestDTO.Password) {SqlDbType =SqlDbType.NVarChar, Direction= ParameterDirection.Input }
            };

            string loginDetails = await _unitofWork.contactRepository.CallStoreProcedure("Sp_Circle_LOGIN_ACTIVITY", _paramObj);

            _dictionaryObj = new Dictionary<string, object>();
            _dictionaryObj = Data.Deserialize(loginDetails, typeof(Dictionary<string, object>));
            JObject JSONObj = JObject.Parse(loginDetails);

            if (JSONObj.ContainsKey("Status") && Convert.ToBoolean(JSONObj["Status"]))
            {

                LoginUserDetails LuD = new LoginUserDetails();

                LuD.ContactId = Convert.ToInt32((string)JSONObj["ContactId"]);
                LuD.FirstName = (string)JSONObj["FirstName"];
                LuD.LastName = (string)JSONObj["LastName"];
                LuD.RoleId = Convert.ToInt32((string)JSONObj["RoleId"]);
                LuD.RoleName = (string)JSONObj["RoleName"];


                var tokenHandeler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(secretKey);

                var tokenDescription = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[] {
                    new Claim(ClaimTypes.Name, JSONObj["FirstName"]+" "+JSONObj["LastName"]),
                    new Claim(ClaimTypes.Role, (string)JSONObj["RoleId"]),
                    new Claim(ClaimTypes.Email, (string)JSONObj["UserEmail"]),
                    new Claim(ClaimTypes.NameIdentifier,(string)JSONObj["ContactId"])

                }),
                    Expires = DateTime.UtcNow.AddDays(1),
                    SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHandeler.CreateToken(tokenDescription);

                LoginResponseDto loginResponseDto = new LoginResponseDto()
                {
                    AccessToken = tokenHandeler.WriteToken(token),
                    UserDetails = LuD,
                };

                if (loginResponseDto.UserDetails == null || string.IsNullOrEmpty(loginResponseDto.AccessToken))
                {


                    return _responseService.BadRequest("Email or Password is incorrect");
                }


                return _responseService.Success(loginResponseDto);

            }
            else
            {
                LoginUserDetails LuD = _mapper.Map<LoginUserDetails>(loginDetails);
                LoginResponseDto loginResponseDto = new LoginResponseDto()
                {
                    AccessToken = "",
                    UserDetails = LuD,
                };

                if (loginResponseDto.UserDetails == null || string.IsNullOrEmpty(loginResponseDto.AccessToken))
                {

                    return _responseService.BadRequest("Email or Password is incorrect");

                }

                return _responseService.Success(loginResponseDto);

            }

        }

        [HttpGet("generate-new-token/{UserId}", Name = "IsUnauthNewTokenGenerate")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> IsUnauthNewTokenGenerate(long UserId)
        {


            if (UserId == 0)
            {
                return _responseService.BadRequest("UserId can not be null or 0");
            }
            //LoginResponceDTO loginResponceDTO = new();

            Contact User = await _unitofWork.contactRepository.GetAsync(s => s.ContactId == UserId);
            var userType = (await _unitofWork.roleRepository.GetAsync(Su => Su.RoleId == User.RoleId)).RoleName;

            if (User == null)
            {

                return _responseService.BadRequest("No User Found");
            }


            LoginUserDetails LuD = new LoginUserDetails();

            LuD.ContactId = User.ContactId;
            LuD.FirstName = User.FirstName;
            LuD.LastName = User.LastName;
            LuD.RoleId = User.RoleId;
            LuD.RoleName = userType;

            var tokenHandeler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);

            var tokenDescription = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] {
                    new Claim(ClaimTypes.Name, User.FirstName+" "+User.LastName),
                    new Claim(ClaimTypes.Role, User.RoleId.ToString()),
                    new Claim(ClaimTypes.Email, User.EmailAddress),
                    new Claim(ClaimTypes.NameIdentifier,User.ContactId.ToString()),


                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandeler.CreateToken(tokenDescription);

            LoginResponseDto loginResponseDto = new LoginResponseDto()
            {
                AccessToken = tokenHandeler.WriteToken(token),
                UserDetails = LuD,
            };

            if (loginResponseDto.UserDetails == null || string.IsNullOrEmpty(loginResponseDto.AccessToken))
            {

                return _responseService.BadRequest("Email or Password is incorrect");
            }

            return _responseService.Success(loginResponseDto);


        }

        //[HttpGet]
        //[ProducesResponseType(StatusCodes.Status403Forbidden)]
        //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[Route("data-test", Name = "DataTest")]

        //public async Task<ActionResult<ApiResponse>> DataTest()
        //{
        //    _paramObj = new SqlParameter[]
        //        {

        //                new SqlParameter("@OPERATION_ID",100) {SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input },
        //                new SqlParameter("@Keyby",1243) {SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input },


        //        };

        //    string responseDetails = await _unitofWork.usersRepository.CallStoreProcedure("Sp_IOT_MASTER_MANAGEMENT", _paramObj);


        //    JObject JSONObj = JObject.Parse(responseDetails);
        //    if (JSONObj.ContainsKey("Status") && Convert.ToBoolean(JSONObj["Status"]))
        //    {
        //        return _responseService.Success(responseDetails);
        //    }
        //    else
        //    {

        //        return _responseService.BadRequest((string)JSONObj["Response"]);
        //    }

        //}

        //[HttpGet]
        //[ProducesResponseType(StatusCodes.Status403Forbidden)]
        //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[Route("data-test-update/{key:long}/{trigger}/{value}/{env}", Name = "DataTestUpdate")]

        //public async Task<ActionResult<ApiResponse>> DataTestUpdate(long key, string trigger, string value, string env)
        //{
        //    try
        //    {
        //        _paramObj = new SqlParameter[]
        //            {

        //                new SqlParameter("@OPERATION_ID",101) {SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input },
        //                new SqlParameter("@VALUE",value) {SqlDbType = SqlDbType.NChar, Direction = ParameterDirection.Input },
        //                new SqlParameter("@TRIGGER",trigger) {SqlDbType = SqlDbType.Bit, Direction = ParameterDirection.Input },
        //                new SqlParameter("@Keyby",key) {SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input },
        //                new SqlParameter("@Env",env) {SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input },


        //            };

        //        string responseDetails = await _unitofWork.usersRepository.CallStoreProcedure("Sp_IOT_MASTER_MANAGEMENT", _paramObj);


        //        JObject JSONObj = JObject.Parse(responseDetails);
        //        if (JSONObj.ContainsKey("Status") && Convert.ToBoolean(JSONObj["Status"]))
        //        {


        //            apiResponse.Response = responseDetails;


        //            apiResponse.StatusCode = (System.Net.HttpStatusCode)HttpStatusCode.OK;
        //            apiResponse.Success = true;


        //        }
        //        else
        //        {
        //            apiResponse.StatusCode = (System.Net.HttpStatusCode)HttpStatusCode.NotFound;
        //            apiResponse.Success = false;
        //            apiResponse.ErrorMassage.Add((string)JSONObj["Response"]);
        //            return BadRequest(apiResponse);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logService.LogExceptionError(ex);
        //        _logService.LogCustom(ex.ToString(), "TestFile");
        //        apiResponse.Success = false;
        //        apiResponse.ErrorMassage = new List<string>() { ex.ToString() };
        //    }
        //    return apiResponse;
        //}

        //[HttpGet]
        //[ProducesResponseType(StatusCodes.Status403Forbidden)]
        //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[Route("get-water-tank", Name = "GetWaterTank")]

        //public async Task<ActionResult<ApiResponse>> GetWaterTank()
        //{
        //    try
        //    {
        //        _paramObj = new SqlParameter[]
        //            {

        //                new SqlParameter("@OPERATION_ID",102) {SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input },
        //                new SqlParameter("@Keyby",1243) {SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input },


        //            };

        //        string responseDetails = await _unitofWork.usersRepository.CallStoreProcedure("Sp_IOT_MASTER_MANAGEMENT", _paramObj);


        //        JObject JSONObj = JObject.Parse(responseDetails);
        //        if (JSONObj.ContainsKey("Status") && Convert.ToBoolean(JSONObj["Status"]))
        //        {


        //            apiResponse.Response = responseDetails;


        //            apiResponse.StatusCode = (System.Net.HttpStatusCode)HttpStatusCode.OK;
        //            apiResponse.Success = true;


        //        }
        //        else
        //        {
        //            apiResponse.StatusCode = (System.Net.HttpStatusCode)HttpStatusCode.NotFound;
        //            apiResponse.Success = false;
        //            apiResponse.ErrorMassage.Add((string)JSONObj["Response"]);
        //            return BadRequest(apiResponse);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logService.LogExceptionError(ex);
        //        _logService.LogCustom(ex.ToString(), "TestFile");
        //        apiResponse.Success = false;
        //        apiResponse.ErrorMassage = new List<string>() { ex.ToString() };
        //    }
        //    return apiResponse;
        //}

        //[HttpGet]
        //[ProducesResponseType(StatusCodes.Status403Forbidden)]
        //[ProducesResponseType(StatusCodes.Status401Unauthorized)]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[Route("water-tank-update/{key:long}", Name = "WaterTankUpdate")]

        //public async Task<ActionResult<ApiResponse>> WaterTankUpdate(long key, string? percentage, bool? switchtrigger)
        //{
        //    try
        //    {
        //        _paramObj = new SqlParameter[]
        //            {

        //                new SqlParameter("@OPERATION_ID",103) {SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input },
        //                new SqlParameter("@percentage",percentage) {SqlDbType = SqlDbType.NChar, Direction = ParameterDirection.Input },
        //                new SqlParameter("@triggerSwitch",switchtrigger) {SqlDbType = SqlDbType.Bit, Direction = ParameterDirection.Input },
        //                new SqlParameter("@Keyby",key) {SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input },



        //            };

        //        string responseDetails = await _unitofWork.usersRepository.CallStoreProcedure("Sp_IOT_MASTER_MANAGEMENT", _paramObj);


        //        JObject JSONObj = JObject.Parse(responseDetails);
        //        if (JSONObj.ContainsKey("Status") && Convert.ToBoolean(JSONObj["Status"]))
        //        {


        //            apiResponse.Response = responseDetails;


        //            apiResponse.StatusCode = (System.Net.HttpStatusCode)HttpStatusCode.OK;
        //            apiResponse.Success = true;


        //        }
        //        else
        //        {
        //            apiResponse.StatusCode = (System.Net.HttpStatusCode)HttpStatusCode.NotFound;
        //            apiResponse.Success = false;
        //            apiResponse.ErrorMassage.Add((string)JSONObj["Response"]);
        //            return BadRequest(apiResponse);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logService.LogExceptionError(ex);
        //        _logService.LogCustom(ex.ToString(), "TestFile");
        //        apiResponse.Success = false;
        //        apiResponse.ErrorMassage = new List<string>() { ex.ToString() };
        //    }
        //    return apiResponse;
        //}

        /***************************************
          * Title - Update Banner Details 
          * Login Location - ADMIN PANEL
          * Procedure - Sp_Circle_ContentManagement
          * EXEC - EXEC [dbo].[Sp_Circle_ContentManagement] @OPERATION_ID=2
          ***************************************/
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("Update-tank/{percent:long}", Name = "UpdateTank")]
        public async Task<ActionResult<ApiResponse>> UpdateTank(long percent)
        {

            try
            {



                var updateParams = new SqlParameter[]
                {
            new("@OPERATION_ID", 3),
            new("@Percent", percent)
                };

                string updateResponse = await _unitofWork.bannerRepository
                    .CallStoreProcedure("Sp_Circle_AdminManagement", updateParams);

                JObject updateJson = JObject.Parse(updateResponse);



                return _responseService.Success((string)updateJson["Response"]);
            }
            catch (Exception ex)
            {


                return _responseService.Error(ex.Message);
            }
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Route("Get-tank", Name = "GetTank")]
        public async Task<ActionResult<ApiResponse>> GetTank()
        {
            _paramObj = new SqlParameter[]
            {
                new SqlParameter("@OPERATION_ID", 4),

            };

            string responseDetails = await _unitofWork.bannerRepository
                .CallStoreProcedure("Sp_Circle_AdminManagement", _paramObj);

            JObject JSONObj = JObject.Parse(responseDetails);

            if (JSONObj.ContainsKey("Status") && Convert.ToBoolean(JSONObj["Status"]))
            {
                if (JSONObj["Response"] != null)
                    return _responseService.Success(JsonConvert.SerializeObject(JSONObj["Response"]));

                return _responseService.NotFound("Banner not found.");
            }

            return _responseService.Error((string)JSONObj["Response"]);
        }
    }
}
