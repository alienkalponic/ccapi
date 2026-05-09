using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Project.Api;
using Project.Infastructure;
using Project.Infastructure.Middleware;
using Project.Domain.Utility;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddInfrastructureService(builder.Configuration);

var key = builder.Configuration.GetValue<string>("ApiSettings:Secret");

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 2147483647; // Set the maximum request body size in bytes (2GB in this example)
});

builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 2147483647;
});


builder.Services.Configure<FormOptions>(x =>
{
    x.MultipartBodyLengthLimit = 2147483647; // In case of multipart
});

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
        ValidateIssuer = false,
        ValidateAudience = false,
    };
});

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        builder =>
        {
            //builder.WithOrigins("http://localhost:3000", "http://localhost:5173") // Replace with your React app's URL
            //       .AllowAnyHeader()
            //       .AllowAnyMethod();
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.Use(async (context, next) =>
{
    await next();
    if (context.Response.StatusCode == 403)
    {
        var logger = context.RequestServices.GetRequiredService<LogService>();
        logger.LogCustom($"403 Forbidden detected for {context.Request.Path}", "Diagnostic");
    }
});


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionHandlingMiddleware>();
// Use CORS policy
app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
