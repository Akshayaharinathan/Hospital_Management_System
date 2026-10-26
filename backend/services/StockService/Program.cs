//using System.Text;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Tokens;
//using Microsoft.OpenApi.Models;
//using StockService.Data;

//var builder = WebApplication.CreateBuilder(args);

//// -------------------- EF Core: SQL Server --------------------
//builder.Services.AddDbContext<StockDbContext>(opts =>
//{
//    var cs = builder.Configuration.GetConnectionString("DefaultConnection");
//    if (string.IsNullOrWhiteSpace(cs))
//        throw new InvalidOperationException("Connection string 'DefaultConnection' is missing or empty.");
//    opts.UseSqlServer(cs);
//});

//// -------------------- Authentication (JWT, raw token) --------------------
//builder.Services
//    .AddAuthentication(options =>
//    {
//        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//    })
//    .AddJwtBearer(options =>
//    {
//        options.RequireHttpsMetadata = false;     // set true in production behind HTTPS
//        options.SaveToken = true;

//        options.TokenValidationParameters = new TokenValidationParameters

//        {
//            ValidateIssuer = true,
//            ValidIssuer = builder.Configuration["Jwt:Issuer"],

//            ValidateAudience = true,
//            ValidAudience = builder.Configuration["Jwt:Audience"],

//            ValidateLifetime = true,

//            ValidateIssuerSigningKey = true,
//            IssuerSigningKey = new SymmetricSecurityKey(
//                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
//            ),

//            // If your tokens use a custom role claim (e.g., "roles"), uncomment:
//            // RoleClaimType = "roles"
//        };

//        // Read token directly from Authorization header (no "Bearer " prefix)
//        options.Events = new JwtBearerEvents
//        {
//            OnMessageReceived = context =>
//            {
//                var raw = context.Request.Headers["Authorization"].FirstOrDefault();
//                if (!string.IsNullOrWhiteSpace(raw))
//                {
//                    context.Token = raw.Trim(); // use raw token value
//                }
//                return Task.CompletedTask;
//            }
//        };
//    });

//// -------------------- Authorization Policies --------------------
//builder.Services.AddAuthorization(options =>
//{
//    // Combined policy used by Stock endpoints (Admin OR Pharmacist)
//    //options.AddPolicy("Pharmacist,Admin",
//    //    policy => policy.RequireRole("Pharmacist", "Admin"));

//    // Keep other role policies if needed elsewhere
//    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));
//    options.AddPolicy("RequireDoctor", policy => policy.RequireRole("Doctor"));
//    options.AddPolicy("RequireReceptionist", policy => policy.RequireRole("Receptionist"));
//    options.AddPolicy("RequirePharmacist", policy => policy.RequireRole("Pharmacist"));
//});

//// -------------------- MVC & Swagger --------------------
//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();

//// Swagger: add security definition for RAW token (no "Bearer " prefix)
//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo
//    {
//        Title = "StockService",
//        Version = "v1",
//        Description = "Hospital Stock Service API"
//    });

//    // ApiKey scheme on "Authorization" header lets you paste raw token
//    var securityScheme = new OpenApiSecurityScheme
//    {
//        Name = "Authorization",
//        Description = "Paste your JWT token (RAW token, NO 'Bearer ' prefix).",
//        In = ParameterLocation.Header,
//        Type = SecuritySchemeType.ApiKey,
//        Reference = new OpenApiReference
//        {
//            Type = ReferenceType.SecurityScheme,
//            Id = "Authorization"
//        }
//    };

//    c.AddSecurityDefinition("Authorization", securityScheme);
//    c.AddSecurityRequirement(new OpenApiSecurityRequirement
//    {
//        { securityScheme, Array.Empty<string>() }
//    });
//});
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowAngular", policy =>
//        policy
//            .WithOrigins("http://localhost:4200", "https://localhost:4200") // allow both http/https dev server
//            .AllowAnyHeader()
//            .AllowAnyMethod()
//            .AllowCredentials() // only if you use cookies/auth
//    );
//});

//var app = builder.Build();

//// -------------------- DB Migrations (auto) --------------------
//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<StockDbContext>();
//    db.Database.Migrate(); // creates/updates schema on startup (visible in SSMS)
//}

//// -------------------- Middleware Pipeline --------------------
//app.UseSwagger();
//app.UseSwaggerUI();
//app.UseCors("AllowAngular");
//app.UseAuthentication();
//app.UseAuthorization();

//app.MapControllers();

//app.Run();
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using StockService.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<StockDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var jwt = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwt["Key"]);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwt["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateLifetime = true,
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", p => p.RequireRole("Admin"));
    options.AddPolicy("RequireDoctor", p => p.RequireRole("Doctor"));
    options.AddPolicy("RequireReceptionist", p => p.RequireRole("Receptionist"));
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "StockService", Version = "v1" });
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Reference = new OpenApiReference { Id = JwtBearerDefaults.AuthenticationScheme, Type = ReferenceType.SecurityScheme }
    };
    c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { jwtSecurityScheme, new string[] { } } });
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy
            .WithOrigins("http://localhost:4200", "https://localhost:4200") // allow both http/https dev server
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials() // only if you use cookies/auth
    );
});

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StockDbContext>();
    db.Database.Migrate();
}

app.Run();