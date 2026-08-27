using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RequestLifeCycle.data;
using RequestLifeCycle.services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<IRequestOfferService, RequestOfferService>();
builder.Services.AddAuthorization();
builder.Services.AddScoped<IServiceRequestService, ServiceRequestService>();
builder.Services.AddControllers();
builder.Services.AddAuthentication(
    JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer =
                    builder.Configuration["AppSettings:Issuer"],

                ValidAudience =
                    builder.Configuration["AppSettings:Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            builder.Configuration[
                                "AppSettings:SecretKey"]!
                        )
                    )
            };
    });
builder.Services.AddDbContext<AppDbContext>(Option =>
Option.UseMySQL(
        builder.Configuration.GetConnectionString(
            "DefaultConnection")!
    ));
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
