using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Project.Application.Common.Repository;
using Project.Domain.Utility;
using Project.Infastructure.Data;

namespace Project.Api.Controllers
{
    [Route("api/uimanagement")]
    [ApiController]
    public class UIManagementController : ControllerBase
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

        public UIManagementController(
            IUnitOfWork unitofWork,
            LogService logService,
            IConfiguration configuration,
            IApiResponseService responseService,
            IWebHostEnvironment webHostEnvironment,
            ApplicationDbContext db
            )
        {
            _unitofWork = unitofWork;
            this.apiResponse = new();
            this._dictionaryObj = new Dictionary<string, object>();
            _logService = logService;
            _responseService = responseService;
            _webHostEnvironment = webHostEnvironment;
            _db = db;
        }

        #region:: UI Gallery Management

        /***************************************
         * Title - Get UI Gallery Page Details
         * Login Location - UI / PUBLIC
         * Procedure - Sp_Circle_GalleryPageManagement
         * EXEC - OPERATION_ID = 1
         ***************************************/
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("Get-gallery-page-details", Name = "GetGalleryPageDetails")]
        public async Task<ActionResult<ApiResponse>> GetGalleryPageDetails()
        {
            try
            {
                _paramObj = new SqlParameter[]
                {
                    new SqlParameter("@OPERATION_ID", 1) { SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input }
                };

                string responseDetails = await _unitofWork.achievementDetailsRepository
                    .CallStoreProcedure("Sp_Circle_GalleryPageManagement", _paramObj);

                if (string.IsNullOrWhiteSpace(responseDetails))
                {
                    return _responseService.NotFound("No data returned from database.");
                }

                JObject JSONObj = JObject.Parse(responseDetails);

                if (JSONObj.ContainsKey("Status") && Convert.ToBoolean(JSONObj["Status"]))
                {
                    if (JSONObj["Response"] != null)
                    {
                        return _responseService.Success(JsonConvert.SerializeObject(JSONObj["Response"]));
                    }

                    return _responseService.NotFound("Gallery data not found.");
                }
                else
                {
                    string errorMessage = JSONObj["Response"]?.ToString() ?? "Data not found.";
                    return _responseService.NotFound(errorMessage);
                }
            }
            catch (Exception ex)
            {
                _logService.LogCustom(ex.Message, "GetGalleryPageDetails");
                return _responseService.Error(ex.Message);
            }
        }

        #endregion
    }
}
