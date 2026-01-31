using Microsoft.AspNetCore.Mvc;
using Project.Application.Common.Repository;
using Project.Domain.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Project.Infastructure.Service
{
    public class ApiResponseService : IApiResponseService
    {
        public ActionResult<ApiResponse> Success(object response, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            return new ObjectResult(new ApiResponse
            {
                StatusCode = statusCode,
                Success = true,
                Response = response
            })
            { StatusCode = (int)statusCode };
        }

        public ActionResult<ApiResponse> Success_false(object response, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            return new ObjectResult(new ApiResponse
            {
                StatusCode = statusCode,
                Success = false,
                Response = response
            })
            { StatusCode = (int)statusCode };
        }

        public ActionResult<ApiResponse> PaginatedSuccess(object response, int totalItems, int itemsPerPage, int currentPage, int totalPages, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            return new ObjectResult(new ApiResponse
            {
                StatusCode = statusCode,
                Success = true,
                Response = response,
                TotalItem = totalItems,
                ItemsPerPage = itemsPerPage,
                CurrentPage = currentPage,
                TotalPageCount = totalPages
            })
            { StatusCode = (int)statusCode };
        }
        public ActionResult<ApiResponse> Error(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
        {
            return new ObjectResult(new ApiResponse
            {
                StatusCode = statusCode,
                Success = false,
                ErrorMassage = new List<string> { message }
            })
            { StatusCode = (int)statusCode };
        }

        public ActionResult<ApiResponse> NotFound(string message)
        {
            return new ObjectResult(new ApiResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Success = false,
                Response = message
            })
            { StatusCode = (int)HttpStatusCode.NotFound };
        }

        public ActionResult<ApiResponse> BadRequest(string message)
        {
            return new ObjectResult(new ApiResponse
            {
                StatusCode = HttpStatusCode.BadRequest,
                Success = false,
                ErrorMassage = new List<string> { message }
            })
            { StatusCode = (int)HttpStatusCode.BadRequest };
        }
        public ActionResult<ApiResponse> NotFound1(string message)
        {
            return new ObjectResult(new ApiResponse
            {
                StatusCode = HttpStatusCode.NotFound,
                Success = false,
                ErrorMassage = new List<string> { message }
            })
            { StatusCode = (int)HttpStatusCode.NotFound };
        }
    }
}
