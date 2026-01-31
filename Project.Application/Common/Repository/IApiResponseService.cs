using Microsoft.AspNetCore.Mvc;
using Project.Domain.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.Common.Repository
{
    public interface IApiResponseService
    {
        ActionResult<ApiResponse> Success(object response, HttpStatusCode statusCode = HttpStatusCode.OK);
        ActionResult<ApiResponse> Success_false(object response, HttpStatusCode statusCode = HttpStatusCode.OK);
        ActionResult<ApiResponse> PaginatedSuccess(object response, int totalItems, int itemsPerPage, int currentPage, int totalPages, HttpStatusCode statusCode = HttpStatusCode.OK);
        ActionResult<ApiResponse> Error(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest);
        ActionResult<ApiResponse> NotFound(string message);
        ActionResult<ApiResponse> BadRequest(string message);
        ActionResult<ApiResponse> NotFound1(string message);
    }
}
