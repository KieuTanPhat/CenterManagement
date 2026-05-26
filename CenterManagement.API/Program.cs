using CenterManagement.API.Services;
using CenterManagement.DataAccess.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;



var builder = WebApplication.CreateBuilder(args);



// ===================== CONTROLLERS =====================
builder.Services.AddControllers();

// ===================== DB CONTEXT =====================
builder.Services.AddDbContext<CenterManagementDBContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});

// ===================== SERVICES =====================
builder.Services.AddSingleton<TokenService>();

// ===================== AUTH =====================
builder.Services.AddAuthorization();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["AppSettings:Jwt:Issuer"],
        ValidAudience = builder.Configuration["AppSettings:Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Jwt:Key"]!))
    };
});

var app = builder.Build();

// ===================== PIPELINE =====================
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

