using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Project.Application.Common.Repository;
using Project.Domain.Utility;
using Project.Infastructure.Data;
using System.Data;

namespace Project.Api.Controllers
{
    [Route("api/settings")]
    [ApiController]
    public class SettingsManagementController : ControllerBase
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

        public SettingsManagementController(
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

        /***************************************
         * Title - Get All Category Details
         * Login Location - Admin / PUBLIC
         * Procedure - Linq
         ***************************************/
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("Get-all-category-details", Name = "GetAllCategoryDetails")]
        public async Task<ActionResult<ApiResponse>> GetAllCategoryDetails()
        {
            try
            {
                var category = await _unitofWork.categoryRepository.GetAllAsync(x=>x.IsDeleted==false);
                if (category == null || !category.Any())
                {
                    return _responseService.NotFound("No category data found.");
                }

                return _responseService.Success(category);

                
            }
            catch (Exception ex)
            {
                _logService.LogCustom(ex.Message, "GetAllCategoryDetails");
                return _responseService.Error(ex.Message);
            }
        }
    }
}
