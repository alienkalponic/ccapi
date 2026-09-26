using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Project.Application.Common.Repository;
using Project.Domain.Dto.CourseManagement;
using Project.Domain.Utility;
using Project.Infastructure.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Project.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CourseManagementController : ControllerBase
    {
        private readonly IUnitOfWork _unitofWork;
        private readonly LogService _logService;
        private readonly IApiResponseService _responseService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationDbContext _db;

        public CourseManagementController(
            IUnitOfWork unitofWork,
            LogService logService,
            IConfiguration configuration,
            IApiResponseService responseService,
            IWebHostEnvironment webHostEnvironment,
            ApplicationDbContext db
        )
        {
            _unitofWork = unitofWork;
            _logService = logService;
            _responseService = responseService;
            _webHostEnvironment = webHostEnvironment;
            _db = db;
        }

        #region :: Helper Execution Method ::

        private async Task<ActionResult<ApiResponse>> ExecuteSpAsync(int operationId, object? jsonObject = null, List<SqlParameter>? extraParams = null)
        {
            try
            {
                var parameters = new List<SqlParameter>
                {
                    new SqlParameter("@OPERATION_ID", operationId) { SqlDbType = SqlDbType.Int, Direction = ParameterDirection.Input }
                };

                if (jsonObject != null)
                {
                    parameters.Add(new SqlParameter("@JSON", JsonConvert.SerializeObject(jsonObject)) { SqlDbType = SqlDbType.NVarChar, Direction = ParameterDirection.Input });
                }

                if (extraParams != null && extraParams.Count > 0)
                {
                    parameters.AddRange(extraParams);
                }

                string responseDetails = await _unitofWork.courseManagementRepository.CallStoreProcedure("Sp_Circle_CourseManagement", parameters.ToArray());

                if (string.IsNullOrWhiteSpace(responseDetails))
                {
                    return _responseService.NotFound("No data returned from database.");
                }

                JObject jsonObj = JObject.Parse(responseDetails);

                if (jsonObj.ContainsKey("Status") && Convert.ToBoolean(jsonObj["Status"]))
                {
                    if (jsonObj["TotalItems"] != null || jsonObj["totalitems"] != null)
                    {
                        JToken responseData = jsonObj["Response"];
                        int totalItems = (int?)(jsonObj["TotalItems"] ?? jsonObj["totalitems"]) ?? 0;
                        int itemsPerPage = (int?)(jsonObj["ItemsPerPage"] ?? jsonObj["itemsperpage"]) ?? 10;
                        int currentPage = (int?)(jsonObj["CurrentPage"] ?? jsonObj["currentpage"]) ?? 1;
                        int totalPageCount = (int?)(jsonObj["TotalPageCount"] ?? jsonObj["totalpagecount"]) ?? 1;

                        return _responseService.PaginatedSuccess(
                            responseData != null ? JsonConvert.SerializeObject(responseData) : "[]",
                            totalItems,
                            itemsPerPage,
                            currentPage,
                            totalPageCount
                        );
                    }
                    else
                    {
                        JToken resToken = jsonObj["Response"];
                        if (resToken != null)
                        {
                            return _responseService.Success(JsonConvert.SerializeObject(resToken));
                        }
                        return _responseService.Success(jsonObj);
                    }
                }
                else
                {
                    string errorMessage = jsonObj["Response"]?.ToString() ?? "An error occurred while processing your request.";
                    return _responseService.Error(errorMessage);
                }
            }
            catch (Exception ex)
            {
                _logService.LogCustom(ex.Message, $"CourseManagement_Op_{operationId}");
                return _responseService.Error(ex.Message);
            }
        }

        #endregion

        #region :: A. COURSE APIs ::

        /***************************************
         * Title - Create Course
         * Route - POST api/CourseManagement/create-course
         * Procedure - Sp_Circle_CourseManagement (@OPERATION_ID = 1)
         ***************************************/
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("create-course", Name = "CreateCourse")]
        public async Task<ActionResult<ApiResponse>> CreateCourse([FromBody] CreateCourseRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return _responseService.Error("Invalid request model.");
            }
            return await ExecuteSpAsync(1, new { Course = dto });
        }

        /***************************************
         * Title - Update Course
         * Route - PUT api/CourseManagement/update-course
         * Procedure - Sp_Circle_CourseManagement (@OPERATION_ID = 2)
         ***************************************/
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("update-course", Name = "UpdateCourse")]
        public async Task<ActionResult<ApiResponse>> UpdateCourse([FromBody] UpdateCourseRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return _responseService.Error("Invalid request model.");
            }
            var extraParams = new List<SqlParameter>
            {
                new SqlParameter("@CourseId", dto.CourseId) { SqlDbType = SqlDbType.BigInt }
            };
            return await ExecuteSpAsync(2, new { Course = dto }, extraParams);
        }

        /***************************************
         * Title - Delete / Deactivate Course
         * Route - DELETE api/CourseManagement/delete-course/{courseId}
         * Procedure - Sp_Circle_CourseManagement (@OPERATION_ID = 3)
         ***************************************/
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("delete-course/{courseId:long}", Name = "DeleteCourse")]
        public async Task<ActionResult<ApiResponse>> DeleteCourse(long courseId)
        {
            var extraParams = new List<SqlParameter>
            {
                new SqlParameter("@CourseId", courseId) { SqlDbType = SqlDbType.BigInt }
            };
            return await ExecuteSpAsync(3, null, extraParams);
        }

        /***************************************
         * Title - Get All Courses
         * Route - GET api/CourseManagement/get-all-course
         * Procedure - Sp_Circle_CourseManagement (@OPERATION_ID = 4)
         ***************************************/
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("get-all-course", Name = "GetAllCourse")]
        public async Task<ActionResult<ApiResponse>> GetAllCourse(
            [FromQuery] long PageSize = 10,
            [FromQuery] long PageNumber = 1,
            [FromQuery] string? Search = null)
        {
            var extraParams = new List<SqlParameter>
            {
                new SqlParameter("@PageSize", PageSize) { SqlDbType = SqlDbType.Int },
                new SqlParameter("@PageNumber", PageNumber) { SqlDbType = SqlDbType.Int },
                new SqlParameter("@Search", (object?)Search ?? DBNull.Value) { SqlDbType = SqlDbType.NVarChar }
            };
            return await ExecuteSpAsync(4, null, extraParams);
        }

        /***************************************
         * Title - Get Course By ID
         * Route - GET api/CourseManagement/get-course/{courseId}
         * Procedure - Sp_Circle_CourseManagement (@OPERATION_ID = 5)
         ***************************************/
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("get-course/{courseId:long}", Name = "GetCourseById")]
        public async Task<ActionResult<ApiResponse>> GetCourseById(long courseId)
        {
            var extraParams = new List<SqlParameter>
            {
                new SqlParameter("@CourseId", courseId) { SqlDbType = SqlDbType.BigInt }
            };
            return await ExecuteSpAsync(5, null, extraParams);
        }

        #endregion

        #region :: B. COURSE BATCH APIs ::

        /***************************************
         * Title - Create Course Batch
         * Route - POST api/CourseManagement/create-course-batch
         * Procedure - Sp_Circle_CourseManagement (@OPERATION_ID = 6)
         ***************************************/
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("create-course-batch", Name = "CreateCourseBatch")]
        public async Task<ActionResult<ApiResponse>> CreateCourseBatch([FromBody] CreateCourseBatchRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return _responseService.Error("Invalid request model.");
            }
            if (dto.AdvanceAmount > dto.TotalCourseFee || dto.CancellationRetentionAmount > dto.TotalCourseFee)
            {
                return _responseService.Error("Advance or Cancellation Retention amount cannot exceed Total Course Fee.");
            }
            return await ExecuteSpAsync(6, new { CourseBatch = dto });
        }

        /***************************************
         * Title - Update Course Batch
         * Route - PUT api/CourseManagement/update-course-batch
         * Procedure - Sp_Circle_CourseManagement (@OPERATION_ID = 7)
         ***************************************/
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("update-course-batch", Name = "UpdateCourseBatch")]
        public async Task<ActionResult<ApiResponse>> UpdateCourseBatch([FromBody] UpdateCourseBatchRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return _responseService.Error("Invalid request model.");
            }
            if (dto.AdvanceAmount > dto.TotalCourseFee || dto.CancellationRetentionAmount > dto.TotalCourseFee)
            {
                return _responseService.Error("Advance or Cancellation Retention amount cannot exceed Total Course Fee.");
            }
            var extraParams = new List<SqlParameter>
            {
                new SqlParameter("@CourseBatchId", dto.CourseBatchId) { SqlDbType = SqlDbType.BigInt }
            };
            return await ExecuteSpAsync(7, new { CourseBatch = dto }, extraParams);
        }

        /***************************************
         * Title - Delete Course Batch
         * Route - DELETE api/CourseManagement/delete-course-batch/{courseBatchId}
         * Procedure - Sp_Circle_CourseManagement (@OPERATION_ID = 8)
         ***************************************/
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("delete-course-batch/{courseBatchId:long}", Name = "DeleteCourseBatch")]
        public async Task<ActionResult<ApiResponse>> DeleteCourseBatch(long courseBatchId)
        {
            var extraParams = new List<SqlParameter>
            {
                new SqlParameter("@CourseBatchId", courseBatchId) { SqlDbType = SqlDbType.BigInt }
            };
            return await ExecuteSpAsync(8, null, extraParams);
        }

        /***************************************
         * Title - Get All Course Batches
         * Route - GET api/CourseManagement/get-all-course-batch
         * Procedure - Sp_Circle_CourseManagement (@OPERATION_ID = 9)
         ***************************************/
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("get-all-course-batch", Name = "GetAllCourseBatch")]
        public async Task<ActionResult<ApiResponse>> GetAllCourseBatch(
            [FromQuery] long PageSize = 10,
            [FromQuery] long PageNumber = 1,
            [FromQuery] string? Search = null)
        {
            var extraParams = new List<SqlParameter>
            {
                new SqlParameter("@PageSize", PageSize) { SqlDbType = SqlDbType.Int },
                new SqlParameter("@PageNumber", PageNumber) { SqlDbType = SqlDbType.Int },
                new SqlParameter("@Search", (object?)Search ?? DBNull.Value) { SqlDbType = SqlDbType.NVarChar }
            };
            return await ExecuteSpAsync(9, null, extraParams);
        }

        /***************************************
         * Title - Get Course Batch By ID
         * Route - GET api/CourseManagement/get-course-batch/{courseBatchId}
         * Procedure - Sp_Circle_CourseManagement (@OPERATION_ID = 10)
         ***************************************/
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("get-course-batch/{courseBatchId:long}", Name = "GetCourseBatchById")]
        public async Task<ActionResult<ApiResponse>> GetCourseBatchById(long courseBatchId)
        {
            var extraParams = new List<SqlParameter>
            {
                new SqlParameter("@CourseBatchId", courseBatchId) { SqlDbType = SqlDbType.BigInt }
            };
            return await ExecuteSpAsync(10, null, extraParams);
        }

        #endregion

        #region :: C. COURSE ENROLLMENT APIs ::

        /***************************************
         * Title - Create Course Enrollment (Person + Enrollment + Payment)
         * Route - POST api/CourseManagement/create-enrollment
         * Procedure - Sp_Circle_CourseManagement (@OPERATION_ID = 11)
         ***************************************/
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("create-enrollment", Name = "CreateEnrollment")]
        public async Task<ActionResult<ApiResponse>> CreateEnrollment([FromBody] CreateCourseEnrollmentRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return _responseService.Error("Invalid request model.");
            }

            var personObj = dto.Person ?? (object)new
            {
                dto.FullName,
                dto.PhoneNumber,
                dto.Email,
                dto.Gender,
                dto.DateOfBirth,
                dto.FatherMotherName,
                dto.Address,
                dto.Profession,
                dto.MotherTongue,
                dto.Height,
                dto.Weight,
                dto.BloodGroup,
                dto.FoodHabit,
                dto.PhysicalProblem
            };

            var enrollmentObj = new
            {
                dto.CourseBatchId,
                dto.RegistrationNumber,
                dto.ReferencePerson,
                dto.FormSubmitted,
                dto.Status,
                dto.Remarks
            };

            var paymentObj = new
            {
                PaymentDate = dto.PaymentDate ?? DateTime.Now,
                Amount = dto.InitialPaymentAmount ?? 0,
                dto.PaymentMode,
                dto.PaymentReceiver,
                dto.TransactionReference,
                dto.Remarks
            };

            var jsonPayload = new
            {
                Person = personObj,
                Enrollment = enrollmentObj,
                Payment = paymentObj
            };

            return await ExecuteSpAsync(11, jsonPayload);
        }

        /***************************************
         * Title - Update Course Enrollment, Person & Payment
         * Route - PUT api/CourseManagement/update-enrollment
         * Procedure - Sp_Circle_CourseManagement (@OPERATION_ID = 12)
         ***************************************/
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("update-enrollment", Name = "UpdateEnrollment")]
        public async Task<ActionResult<ApiResponse>> UpdateEnrollment([FromBody] UpdateCourseEnrollmentRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return _responseService.Error("Invalid request model.");
            }

            var personObj = dto.Person ?? (object)new
            {
                dto.FullName,
                dto.PhoneNumber,
                dto.Email,
                dto.Gender,
                dto.DateOfBirth,
                dto.FatherMotherName,
                dto.Address,
                dto.Profession,
                dto.MotherTongue,
                dto.Height,
                dto.Weight,
                dto.BloodGroup,
                dto.FoodHabit,
                dto.PhysicalProblem
            };

            var enrollmentObj = new
            {
                dto.CourseBatchId,
                dto.RegistrationNumber,
                dto.ReferencePerson,
                dto.FormSubmitted,
                Status=dto.EnrollmentStatus,
                dto.Remarks
            };

            object? paymentObj = null;
            if (dto.Payment != null && dto.Payment.PaymentId.HasValue && dto.Payment.PaymentId.Value > 0)
            {
                paymentObj = dto.Payment;
            }
            else if (dto.PaymentId.HasValue && dto.PaymentId.Value > 0)
            {
                paymentObj = new
                {
                    PaymentId = dto.PaymentId.Value,
                    dto.PaymentDate,
                    Amount = dto.PaymentAmount,
                    dto.PaymentMode,
                    dto.PaymentReceiver,
                    dto.TransactionReference,
                    Remarks = dto.PaymentRemarks ?? dto.Remarks
                };
            }
            else
            {
                paymentObj = new
                {
                    PaymentDate = dto.PaymentDate ?? DateTime.Now,
                    Amount = dto.PaymentAmount ?? 0,
                    dto.PaymentMode,
                    dto.PaymentReceiver,
                    dto.TransactionReference,
                    dto.Remarks
                };
            }

            var jsonPayload = paymentObj != null ? (object)new
            {
                Person = personObj,
                Enrollment = enrollmentObj,
                Payment = paymentObj
            } : new
            {
                Person = personObj,
                Enrollment = enrollmentObj
            };

            var extraParams = new List<SqlParameter>
            {
                new SqlParameter("@EnrollmentId", dto.EnrollmentId) { SqlDbType = SqlDbType.BigInt }
            };

            return await ExecuteSpAsync(12, jsonPayload, extraParams);
        }

        /***************************************
         * Title - Cancel Enrollment & Auto Calculate Refund
         * Route - POST api/CourseManagement/cancel-enrollment
         * Procedure - Sp_Circle_CourseManagement (@OPERATION_ID = 13)
         ***************************************/
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("cancel-enrollment", Name = "CancelEnrollment")]
        public async Task<ActionResult<ApiResponse>> CancelEnrollment([FromBody] CancelEnrollmentRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return _responseService.Error("Invalid request model.");
            }

            var jsonPayload = new
            {
                Refund = new
                {
                    dto.RefundMode,
                    dto.Reason,
                    dto.TransactionReference
                }
            };

            var extraParams = new List<SqlParameter>
            {
                new SqlParameter("@EnrollmentId", dto.EnrollmentId) { SqlDbType = SqlDbType.BigInt }
            };

            return await ExecuteSpAsync(13, jsonPayload, extraParams);
        }

        /***************************************
         * Title - Get All Course Enrollments
         * Route - GET api/CourseManagement/get-all-enrollment
         * Procedure - Sp_Circle_CourseManagement (@OPERATION_ID = 14)
         ***************************************/
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("get-all-enrollment", Name = "GetAllEnrollment")]
        public async Task<ActionResult<ApiResponse>> GetAllEnrollment(
            [FromQuery] long PageSize = 10,
            [FromQuery] long PageNumber = 1,
            [FromQuery] string? Search = null,
            [FromQuery] int? CourseYear = null,
            [FromQuery] long? CourseId = null)
        {
            var extraParams = new List<SqlParameter>
            {
                new SqlParameter("@PageSize", PageSize) { SqlDbType = SqlDbType.Int },
                new SqlParameter("@PageNumber", PageNumber) { SqlDbType = SqlDbType.Int },
                new SqlParameter("@Search", (object?)Search ?? DBNull.Value) { SqlDbType = SqlDbType.NVarChar },
                new SqlParameter("@CourseYear", (object?)CourseYear ?? DBNull.Value) { SqlDbType = SqlDbType.Int },
                new SqlParameter("@CourseId", (object?)CourseId ?? DBNull.Value) { SqlDbType = SqlDbType.BigInt }
            };
            return await ExecuteSpAsync(14, null, extraParams);
        }

        /***************************************
         * Title - Get Course Enrollment By ID (Detailed)
         * Route - GET api/CourseManagement/get-enrollment/{id}
         * Procedure - Sp_Circle_CourseManagement (@OPERATION_ID = 15)
         ***************************************/
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("get-enrollment/{id:long}", Name = "GetEnrollmentById")]
        public async Task<ActionResult<ApiResponse>> GetEnrollmentById(long id)
        {
            var extraParams = new List<SqlParameter>
            {
                new SqlParameter("@EnrollmentId", id) { SqlDbType = SqlDbType.BigInt }
            };
            return await ExecuteSpAsync(15, null, extraParams);
        }

        /***************************************
         * Title - Get Complete Enrollment Details
         * Route - GET api/CourseManagement/get-enrollment-details/{id}
         * Procedure - Sp_Circle_CourseManagement (@OPERATION_ID = 15)
         ***************************************/
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("get-enrollment-details/{id:long}", Name = "GetEnrollmentDetails")]
        public async Task<ActionResult<ApiResponse>> GetEnrollmentDetails(long id)
        {
            var extraParams = new List<SqlParameter>
            {
                new SqlParameter("@EnrollmentId", id) { SqlDbType = SqlDbType.BigInt }
            };
            return await ExecuteSpAsync(15, null, extraParams);
        }

        #endregion

        #region :: D. PAYMENT APIs ::

        /***************************************
         * Title - Add Course Payment
         * Route - POST api/CourseManagement/add-payment
         * Procedure - Sp_Circle_CourseManagement (@OPERATION_ID = 16)
         ***************************************/
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("add-payment", Name = "AddPayment")]
        public async Task<ActionResult<ApiResponse>> AddPayment([FromBody] AddCoursePaymentRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return _responseService.Error("Invalid request model.");
            }
            if (dto.Amount <= 0)
            {
                return _responseService.Error("Payment Amount must be greater than zero.");
            }

            var jsonPayload = new
            {
                Payment = new
                {
                    dto.PaymentDate,
                    dto.Amount,
                    dto.PaymentMode,
                    dto.PaymentReceiver,
                    dto.TransactionReference,
                    dto.Remarks
                }
            };

            var extraParams = new List<SqlParameter>
            {
                new SqlParameter("@EnrollmentId", dto.EnrollmentId) { SqlDbType = SqlDbType.BigInt }
            };

            return await ExecuteSpAsync(16, jsonPayload, extraParams);
        }

        /***************************************
         * Title - Update Course Payment
         * Route - PUT api/CourseManagement/update-payment
         * Procedure - Sp_Circle_CourseManagement (@OPERATION_ID = 31)
         ***************************************/
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("update-payment", Name = "UpdatePayment")]
        public async Task<ActionResult<ApiResponse>> UpdatePayment([FromBody] UpdateCoursePaymentRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return _responseService.Error("Invalid request model.");
            }

            var jsonPayload = new
            {
                Payment = new
                {
                    dto.PaymentDate,
                    dto.Amount,
                    dto.PaymentMode,
                    dto.PaymentReceiver,
                    dto.TransactionReference,
                    dto.Remarks
                }
            };

            var extraParams = new List<SqlParameter>
            {
                new SqlParameter("@PaymentId", dto.PaymentId) { SqlDbType = SqlDbType.BigInt }
            };

            return await ExecuteSpAsync(31, jsonPayload, extraParams);
        }

        /***************************************
         * Title - Get Enrollment Payments
         * Route - GET api/CourseManagement/get-enrollment-payments/{enrollmentId}
         * Procedure - Sp_Circle_CourseManagement (@OPERATION_ID = 17)
         ***************************************/
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("get-enrollment-payments/{enrollmentId:long}", Name = "GetEnrollmentPayments")]
        public async Task<ActionResult<ApiResponse>> GetEnrollmentPayments(long enrollmentId)
        {
            var extraParams = new List<SqlParameter>
            {
                new SqlParameter("@EnrollmentId", enrollmentId) { SqlDbType = SqlDbType.BigInt }
            };
            return await ExecuteSpAsync(17, null, extraParams);
        }

        /***************************************
         * Title - Get All Payments
         * Route - GET api/CourseManagement/get-all-payment
         * Procedure - Sp_Circle_CourseManagement (@OPERATION_ID = 32)
         ***************************************/
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("get-all-payment", Name = "GetAllPayment")]
        public async Task<ActionResult<ApiResponse>> GetAllPayment(
            [FromQuery] long PageSize = 10,
            [FromQuery] long PageNumber = 1,
            [FromQuery] string? Search = null,
            [FromQuery] long? CourseId = null,
            [FromQuery] int? CourseYear = null,
            [FromQuery] string? PaymentMode = null,
            [FromQuery] DateTime? DateFrom = null,
            [FromQuery] DateTime? DateTo = null)
        {
            var extraParams = new List<SqlParameter>
            {
                new SqlParameter("@PageSize", PageSize) { SqlDbType = SqlDbType.Int },
                new SqlParameter("@PageNumber", PageNumber) { SqlDbType = SqlDbType.Int },
                new SqlParameter("@Search", (object?)Search ?? DBNull.Value) { SqlDbType = SqlDbType.NVarChar },
                new SqlParameter("@CourseId", (object?)CourseId ?? DBNull.Value) { SqlDbType = SqlDbType.BigInt },
                new SqlParameter("@CourseYear", (object?)CourseYear ?? DBNull.Value) { SqlDbType = SqlDbType.Int },
                new SqlParameter("@PaymentMode", (object?)PaymentMode ?? DBNull.Value) { SqlDbType = SqlDbType.VarChar },
                new SqlParameter("@DateFrom", (object?)DateFrom ?? DBNull.Value) { SqlDbType = SqlDbType.Date },
                new SqlParameter("@DateTo", (object?)DateTo ?? DBNull.Value) { SqlDbType = SqlDbType.Date }
            };
            return await ExecuteSpAsync(32, null, extraParams);
        }

        #endregion

        #region :: E. REFUND APIs ::

        /***************************************
         * Title - Add Course Refund
         * Route - POST api/CourseManagement/add-refund
         * Procedure - Sp_Circle_CourseManagement (@OPERATION_ID = 18)
         ***************************************/
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("add-refund", Name = "AddRefund")]
        public async Task<ActionResult<ApiResponse>> AddRefund([FromBody] AddCourseRefundRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return _responseService.Error("Invalid request model.");
            }

            var jsonPayload = new
            {
                Refund = new
                {
                    dto.RefundDate,
                    dto.RefundAmount,
                    dto.RefundMode,
                    dto.Reason,
                    dto.TransactionReference
                }
            };

            var extraParams = new List<SqlParameter>
            {
                new SqlParameter("@EnrollmentId", dto.EnrollmentId) { SqlDbType = SqlDbType.BigInt }
            };

            return await ExecuteSpAsync(18, jsonPayload, extraParams);
        }

        /***************************************
         * Title - Get Enrollment Refunds
         * Route - GET api/CourseManagement/get-enrollment-refunds/{enrollmentId}
         * Procedure - Sp_Circle_CourseManagement (@OPERATION_ID = 19)
         ***************************************/
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("get-enrollment-refunds/{enrollmentId:long}", Name = "GetEnrollmentRefunds")]
        public async Task<ActionResult<ApiResponse>> GetEnrollmentRefunds(long enrollmentId)
        {
            var extraParams = new List<SqlParameter>
            {
                new SqlParameter("@EnrollmentId", enrollmentId) { SqlDbType = SqlDbType.BigInt }
            };
            return await ExecuteSpAsync(19, null, extraParams);
        }

        /***************************************
         * Title - Get All Refunds
         * Route - GET api/CourseManagement/get-all-refund
         * Procedure - Sp_Circle_CourseManagement (@OPERATION_ID = 19)
         ***************************************/
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("get-all-refund", Name = "GetAllRefund")]
        public async Task<ActionResult<ApiResponse>> GetAllRefund([FromQuery] long? EnrollmentId = null)
        {
            var extraParams = new List<SqlParameter>
            {
                new SqlParameter("@EnrollmentId", (object?)EnrollmentId ?? DBNull.Value) { SqlDbType = SqlDbType.BigInt }
            };
            return await ExecuteSpAsync(19, null, extraParams);
        }

        #endregion

        #region :: F. COURSE OFFICIAL APIs ::

        /***************************************
         * Title - Add Course Official
         * Route - POST api/CourseManagement/add-course-official
         * Procedure - Sp_Circle_CourseManagement (@OPERATION_ID = 20)
         ***************************************/
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("add-course-official", Name = "AddCourseOfficial")]
        public async Task<ActionResult<ApiResponse>> AddCourseOfficial([FromBody] AddCourseOfficialRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return _responseService.Error("Invalid request model.");
            }
            return await ExecuteSpAsync(20, new { CourseOfficial = dto });
        }

        /***************************************
         * Title - Update Course Official
         * Route - PUT api/CourseManagement/update-course-official
         * Procedure - Sp_Circle_CourseManagement (@OPERATION_ID = 21)
         ***************************************/
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("update-course-official", Name = "UpdateCourseOfficial")]
        public async Task<ActionResult<ApiResponse>> UpdateCourseOfficial([FromBody] UpdateCourseOfficialRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return _responseService.Error("Invalid request model.");
            }
            var extraParams = new List<SqlParameter>
            {
                new SqlParameter("@CourseOfficialId", dto.CourseOfficialId) { SqlDbType = SqlDbType.BigInt }
            };
            return await ExecuteSpAsync(21, new { CourseOfficial = dto }, extraParams);
        }

        /***************************************
         * Title - Update Official Payment Status
         * Route - PUT api/CourseManagement/update-course-official-payment-status
         * Procedure - Sp_Circle_CourseManagement (@OPERATION_ID = 21)
         ***************************************/
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("update-course-official-payment-status", Name = "UpdateCourseOfficialPaymentStatus")]
        public async Task<ActionResult<ApiResponse>> UpdateCourseOfficialPaymentStatus([FromBody] UpdateOfficialPaymentStatusDto dto)
        {
            if (!ModelState.IsValid)
            {
                return _responseService.Error("Invalid request model.");
            }
            var extraParams = new List<SqlParameter>
            {
                new SqlParameter("@CourseOfficialId", dto.CourseOfficialId) { SqlDbType = SqlDbType.BigInt }
            };
            var jsonPayload = new
            {
                CourseOfficial = new
                {
                    dto.PaymentStatus,
                    dto.Remarks
                }
            };
            return await ExecuteSpAsync(21, jsonPayload, extraParams);
        }

        /***************************************
         * Title - Delete Course Official
         * Route - DELETE api/CourseManagement/delete-course-official/{id}
         * Procedure - Sp_Circle_CourseManagement (@OPERATION_ID = 22)
         ***************************************/
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("delete-course-official/{id:long}", Name = "DeleteCourseOfficial")]
        public async Task<ActionResult<ApiResponse>> DeleteCourseOfficial(long id)
        {
            var extraParams = new List<SqlParameter>
            {
                new SqlParameter("@CourseOfficialId", id) { SqlDbType = SqlDbType.BigInt }
            };
            return await ExecuteSpAsync(22, null, extraParams);
        }

        /***************************************
         * Title - Get All Course Officials
         * Route - GET api/CourseManagement/get-all-course-official
         * Procedure - Sp_Circle_CourseManagement (@OPERATION_ID = 23)
         ***************************************/
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("get-all-course-official", Name = "GetAllCourseOfficial")]
        public async Task<ActionResult<ApiResponse>> GetAllCourseOfficial([FromQuery] long? CourseBatchId = null)
        {
            var extraParams = new List<SqlParameter>
            {
                new SqlParameter("@CourseBatchId", (object?)CourseBatchId ?? DBNull.Value) { SqlDbType = SqlDbType.BigInt }
            };
            return await ExecuteSpAsync(23, null, extraParams);
        }

        /***************************************
         * Title - Get Course Official By ID
         * Route - GET api/CourseManagement/get-course-official/{id}
         * Procedure - Sp_Circle_CourseManagement (@OPERATION_ID = 23)
         ***************************************/
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("get-course-official/{id:long}", Name = "GetCourseOfficialById")]
        public async Task<ActionResult<ApiResponse>> GetCourseOfficialById(long id)
        {
            var extraParams = new List<SqlParameter>
            {
                new SqlParameter("@CourseOfficialId", id) { SqlDbType = SqlDbType.BigInt }
            };
            return await ExecuteSpAsync(33, null, extraParams);
        }

        #endregion

        #region :: G. COURSE EXPENSE APIs ::

        /***************************************
         * Title - Add Course Expense
         * Route - POST api/CourseManagement/add-course-expense
         * Procedure - Sp_Circle_CourseManagement (@OPERATION_ID = 24)
         ***************************************/
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("add-course-expense", Name = "AddCourseExpense")]
        public async Task<ActionResult<ApiResponse>> AddCourseExpense([FromBody] AddCourseExpenseRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return _responseService.Error("Invalid request model.");
            }
            return await ExecuteSpAsync(24, new { CourseExpense = dto });
        }

        /***************************************
         * Title - Update Course Expense
         * Route - PUT api/CourseManagement/update-course-expense
         * Procedure - Sp_Circle_CourseManagement (@OPERATION_ID = 25)
         ***************************************/
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("update-course-expense", Name = "UpdateCourseExpense")]
        public async Task<ActionResult<ApiResponse>> UpdateCourseExpense([FromBody] UpdateCourseExpenseRequestDto dto)
        {
            if (!ModelState.IsValid)
            {
                return _responseService.Error("Invalid request model.");
            }
            var extraParams = new List<SqlParameter>
            {
                new SqlParameter("@ExpenseId", dto.ExpenseId) { SqlDbType = SqlDbType.BigInt }
            };
            return await ExecuteSpAsync(25, new { CourseExpense = dto }, extraParams);
        }

        /***************************************
         * Title - Delete Course Expense
         * Route - DELETE api/CourseManagement/delete-course-expense/{expenseId}
         * Procedure - Sp_Circle_CourseManagement (@OPERATION_ID = 26)
         ***************************************/
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Route("delete-course-expense/{expenseId:long}", Name = "DeleteCourseExpense")]
        public async Task<ActionResult<ApiResponse>> DeleteCourseExpense(long expenseId)
        {
            var extraParams = new List<SqlParameter>
            {
                new SqlParameter("@ExpenseId", expenseId) { SqlDbType = SqlDbType.BigInt }
            };
            return await ExecuteSpAsync(26, null, extraParams);
        }

        /***************************************
         * Title - Get All Course Expenses
         * Route - GET api/CourseManagement/get-all-course-expense
         * Procedure - Sp_Circle_CourseManagement (@OPERATION_ID = 27)
         ***************************************/
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("get-all-course-expense", Name = "GetAllCourseExpense")]
        public async Task<ActionResult<ApiResponse>> GetAllCourseExpense([FromQuery] long? CourseBatchId = null)
        {
            var extraParams = new List<SqlParameter>
            {
                new SqlParameter("@CourseBatchId", (object?)CourseBatchId ?? DBNull.Value) { SqlDbType = SqlDbType.BigInt }
            };
            return await ExecuteSpAsync(27, null, extraParams);
        }

        /***************************************
         * Title - Get Course Expense By ID
         * Route - GET api/CourseManagement/get-course-expense/{expenseId}
         * Procedure - Sp_Circle_CourseManagement (@OPERATION_ID = 27)
         ***************************************/
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("get-course-expense/{expenseId:long}", Name = "GetCourseExpenseById")]
        public async Task<ActionResult<ApiResponse>> GetCourseExpenseById(long expenseId)
        {
            var extraParams = new List<SqlParameter>
            {
                new SqlParameter("@CourseBatchId", expenseId) { SqlDbType = SqlDbType.BigInt }
            };
            return await ExecuteSpAsync(27, null, extraParams);
        }

        #endregion

        #region :: H. COURSE DASHBOARD & REPORTS APIs ::

        /***************************************
         * Title - Get Course Dashboard Summary
         * Route - GET api/CourseManagement/get-course-dashboard
         * Procedure - Sp_Circle_CourseManagement (@OPERATION_ID = 28)
         ***************************************/
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("get-course-dashboard", Name = "GetCourseDashboard")]
        public async Task<ActionResult<ApiResponse>> GetCourseDashboard([FromQuery] long? CourseBatchId = null)
        {
            var extraParams = new List<SqlParameter>
            {
                new SqlParameter("@CourseBatchId", (object?)CourseBatchId ?? DBNull.Value) { SqlDbType = SqlDbType.BigInt }
            };
            return await ExecuteSpAsync(28, null, extraParams);
        }

        /***************************************
         * Title - Get Year-Wise Course Summary
         * Route - GET api/CourseManagement/get-year-wise-course-summary
         * Procedure - Sp_Circle_CourseManagement (@OPERATION_ID = 29)
         ***************************************/
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("get-year-wise-course-summary", Name = "GetYearWiseCourseSummary")]
        public async Task<ActionResult<ApiResponse>> GetYearWiseCourseSummary([FromQuery] int? CourseYear = null)
        {
            var extraParams = new List<SqlParameter>
            {
                new SqlParameter("@CourseYear", (object?)CourseYear ?? DBNull.Value) { SqlDbType = SqlDbType.Int }
            };
            return await ExecuteSpAsync(29, null, extraParams);
        }

        /***************************************
         * Title - Get Course Summary By Segment / Course Wise Summary
         * Route - GET api/CourseManagement/get-course-summary-by-segment
         * Procedure - Sp_Circle_CourseManagement (@OPERATION_ID = 30)
         ***************************************/
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("get-course-summary-by-segment", Name = "GetCourseSummaryBySegment")]
        public async Task<ActionResult<ApiResponse>> GetCourseSummaryBySegment([FromQuery] int? CourseYear = null)
        {
            var extraParams = new List<SqlParameter>
            {
                new SqlParameter("@CourseYear", (object?)CourseYear ?? DBNull.Value) { SqlDbType = SqlDbType.Int }
            };
            return await ExecuteSpAsync(30, null, extraParams);
        }

        /***************************************
         * Title - Get Course Financial Report
         * Route - GET api/CourseManagement/get-course-financial-report
         * Procedure - Sp_Circle_CourseManagement (@OPERATION_ID = 28)
         ***************************************/
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Route("get-course-financial-report", Name = "GetCourseFinancialReport")]
        public async Task<ActionResult<ApiResponse>> GetCourseFinancialReport([FromQuery] long? CourseBatchId = null)
        {
            var extraParams = new List<SqlParameter>
            {
                new SqlParameter("@CourseBatchId", (object?)CourseBatchId ?? DBNull.Value) { SqlDbType = SqlDbType.BigInt }
            };
            return await ExecuteSpAsync(28, null, extraParams);
        }

        #endregion
    }
}
