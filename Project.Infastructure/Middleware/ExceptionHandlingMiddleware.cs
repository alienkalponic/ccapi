using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Hosting;
using System.Text.Json;
using Project.Domain.Utility;

namespace Project.Infastructure.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly LogService _logger;
        //private readonly IHostEnvironment _env;
        private readonly ApiResponse apiResponse;

        public ExceptionHandlingMiddleware(RequestDelegate next, LogService logger)
        {
            _next = next;
            _logger = logger;
            //_env = env;
            this.apiResponse = new();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context); // proceed to next middleware
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "An unhandled exception occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var statusCode = ex switch
            {
                UnauthorizedAccessException => HttpStatusCode.Unauthorized, // 401
                ArgumentException => HttpStatusCode.BadRequest, // 400
                KeyNotFoundException => HttpStatusCode.NotFound, // 404
                InvalidOperationException => HttpStatusCode.Conflict, // 409
                NotImplementedException => HttpStatusCode.NotImplemented, // 501
                TimeoutException => HttpStatusCode.RequestTimeout, // 408
                AccessViolationException => HttpStatusCode.Forbidden, // 403
                FormatException => HttpStatusCode.UnprocessableEntity, // 422
                OperationCanceledException => HttpStatusCode.RequestTimeout, // 408
                DivideByZeroException => HttpStatusCode.InternalServerError, // 500 (Internal logic error)                
                _ => HttpStatusCode.InternalServerError // Default fallback
            };
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = Convert.ToInt32(statusCode);

            //var errorDetails = _env.IsDevelopment()
            //    ? new
            //    {
            //        Success = false,
            //        StatusCode = context.Response.StatusCode,
            //        Message = ex.Message,
            //        ExceptionType = ex.GetType().Name,
            //        StackTrace = ex.StackTrace,
            //        Source = ex.Source,
            //        Path = context.Request.Path,
            //        Method = context.Request.Method,
            //        TraceId = context.TraceIdentifier
            //    }
            //    : new
            //    {
            //        //StatusCode = context.Response.StatusCode,
            //        //Message = "An unexpected error occurred. Please try again later.",
            //        //ExceptionType = "",
            //        //StackTrace = "",
            //        //Source = "",
            //        //Path = context.Request.Path,
            //        //Method = context.Request.Method,
            //        //TraceId = context.TraceIdentifier
            //        Success = false,
            //        StatusCode = context.Response.StatusCode,
            //        Message = ex.Message,
            //        ExceptionType = ex.GetType().Name,
            //        StackTrace = ex.StackTrace,
            //        Source = ex.Source,
            //        Path = context.Request.Path,
            //        Method = context.Request.Method,
            //        TraceId = context.TraceIdentifier
            //    };

            var errorDetails = new
            {
                Success = false,
                StatusCode = context.Response.StatusCode,
                Message = ex.Message,
                ExceptionType = ex.GetType().Name,
                StackTrace = ex.StackTrace,
                Source = ex.Source,
                Path = context.Request.Path,
                Method = context.Request.Method,
                TraceId = context.TraceIdentifier
            };

            var json = JsonSerializer.Serialize(errorDetails, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            _logger.LogCustom(json, "ExceptionError");


            await context.Response.WriteAsync(json);
        }
    }
}
