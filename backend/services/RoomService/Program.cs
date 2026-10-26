//using System.Text;
//using Microsoft.AspNetCore.Authentication.JwtBearer;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Tokens;
//using Microsoft.OpenApi.Models;
//using RoomService.Data;
//using RoomService.Services;
//using Microsoft.Extensions.Configuration;

//var builder = WebApplication.CreateBuilder(args);

//// DB
//builder.Services.AddDbContext<RoomDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//// Controllers
//builder.Services.AddControllers();

//// JWT
//var jwt = builder.Configuration.GetSection("Jwt");
//var key = Encoding.UTF8.GetBytes(jwt["Key"]!);

//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        options.RequireHttpsMetadata = false;
//        options.TokenValidationParameters = new TokenValidationParameters
//        {
//            ValidateIssuer = true,
//            ValidIssuer = jwt["Issuer"],
//            ValidateAudience = true,
//            ValidAudience = jwt["Audience"],
//            ValidateIssuerSigningKey = true,
//            IssuerSigningKey = new SymmetricSecurityKey(key),
//            ValidateLifetime = true
//        };
//    });

//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("RequireAdmin", p => p.RequireRole("Admin"));
//    options.AddPolicy("RequireDoctor", p => p.RequireRole("Doctor"));
//    options.AddPolicy("RequireReceptionist", p => p.RequireRole("Receptionist"));
//});

//// Swagger
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RoomService", Version = "v1" });
//    var jwtSecurityScheme = new OpenApiSecurityScheme
//    {
//        Scheme = "Bearer",
//        BearerFormat = "JWT",
//        Name = "Authorization",
//        In = ParameterLocation.Header,
//        Type = SecuritySchemeType.Http,
//        Description = "Enter JWT Bearer token only",
//        Reference = new OpenApiReference { Id = JwtBearerDefaults.AuthenticationScheme, Type = ReferenceType.SecurityScheme }
//    };
//    c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
//    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { jwtSecurityScheme, new string[] { } } });
//});

//// Http clients
//builder.Services.AddHttpContextAccessor();
////builder.Services.AddHttpClient<PatientClient>(client =>
////{
////    client.BaseAddress = new Uri(builder.Configuration["PatientService:BaseUrl"] ?? "http://patient-service:5233");
////});
////builder.Services.AddHttpClient<UserClient>(client =>
////{
////    client.BaseAddress = new Uri(builder.Configuration["UserService:BaseUrl"] ?? "http://user-service:5107");
////});
//builder.Services.AddHttpClient<PatientClient>(client =>
//{
//    var url = builder.Configuration["PatientService:BaseUrl"] ?? "http://patient-service:8080";
//    client.BaseAddress = new Uri(url);
//    Console.WriteLine($"[HttpClient] PatientClient BaseAddress = {client.BaseAddress}");
//});

//builder.Services.AddHttpClient<UserClient>(client =>
//{
//    var url = builder.Configuration["UserService:BaseUrl"] ?? "http://user-service:8080";
//    client.BaseAddress = new Uri(url);
//    Console.WriteLine($"[HttpClient] UserClient BaseAddress = {client.BaseAddress}");
//});

//// Capacity service
//builder.Services.AddScoped<ICapacityService, CapacityService>();

//var app = builder.Build();
//// Print environment
//Console.WriteLine($"[Env] ASPNETCORE_ENVIRONMENT = {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");

//// Print config keys
//var patientUrl = builder.Configuration["PatientService:BaseUrl"];
//var userUrl = builder.Configuration["UserService:BaseUrl"];
//Console.WriteLine($"[Config] PatientService:BaseUrl = {patientUrl}");
//Console.WriteLine($"[Config] UserService:BaseUrl    = {userUrl}");

//// Optional: dump full config view
//if (builder.Configuration is IConfigurationRoot root)
//{
//    Console.WriteLine("[Config] DebugView:");
//    Console.WriteLine(root.GetDebugView());
//}

//app.UseSwagger();
//app.UseSwaggerUI();

//app.UseRouting();
//app.UseAuthentication();
//app.UseAuthorization();

//app.MapControllers();

//// Migrate + seed
//using (var scope = app.Services.CreateScope())
//{
//    var db = scope.ServiceProvider.GetRequiredService<RoomDbContext>();
//    db.Database.Migrate();

//    if (!db.Rooms.Any() && !db.WardBeds.Any())
//    {
//        db.Rooms.AddRange(
//            new RoomService.Models.Room { RoomNumber = "AC-101", Category = RoomService.Models.RoomCategory.AC },
//            new RoomService.Models.Room { RoomNumber = "AC-102", Category = RoomService.Models.RoomCategory.AC },
//            new RoomService.Models.Room { RoomNumber = "NAC-201", Category = RoomService.Models.RoomCategory.NON_AC },
//            new RoomService.Models.Room { RoomNumber = "NAC-202", Category = RoomService.Models.RoomCategory.NON_AC }
//        );
//        db.WardBeds.AddRange(
//            new RoomService.Models.WardBed { WardName = "GW-A", BedNumber = "A-01" },
//            new RoomService.Models.WardBed { WardName = "GW-A", BedNumber = "A-02" },
//            new RoomService.Models.WardBed { WardName = "GW-B", BedNumber = "B-01" }
//        );
//        db.SaveChanges();
//        Console.WriteLine("RoomService: Seeded sample rooms and ward beds.");
//    }
//}

//app.Run();
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RoomService.Data;
using RoomService.Services;

var builder = WebApplication.CreateBuilder(args);

// DB
builder.Services.AddDbContext<RoomDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Controllers
builder.Services.AddControllers();

// JWT
var jwt = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwt["Key"]!);

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
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", p => p.RequireRole("Admin"));
    options.AddPolicy("RequireDoctor", p => p.RequireRole("Doctor"));
    options.AddPolicy("RequireReceptionist", p => p.RequireRole("Receptionist"));
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RoomService", Version = "v1" });
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Enter JWT Bearer token only",
        Reference = new OpenApiReference { Id = JwtBearerDefaults.AuthenticationScheme, Type = ReferenceType.SecurityScheme }
    };
    c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { jwtSecurityScheme, Array.Empty<string>() } });
});

// Http clients
builder.Services.AddHttpContextAccessor();

// PatientClient
builder.Services.AddHttpClient<PatientClient>(client =>
{
    var url = builder.Configuration["PatientService:BaseUrl"] ?? "http://localhost:5233";
    client.BaseAddress = new Uri(url);
    Console.WriteLine($"[HttpClient] PatientClient BaseAddress = {client.BaseAddress}");
});



// UserClient
builder.Services.AddHttpClient<UserClient>(client =>
{
    var url = builder.Configuration["UserService:BaseUrl"] ?? "http://localhost:5107";
    client.BaseAddress = new Uri(url);
    Console.WriteLine($"[HttpClient] UserClient BaseAddress = {client.BaseAddress}");
});

// Capacity service
builder.Services.AddScoped<ICapacityService, CapacityService>();
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

// Optional: quick config debug
Console.WriteLine($"[Env] ASPNETCORE_ENVIRONMENT = {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");
Console.WriteLine($"[Config] PatientService:BaseUrl = {builder.Configuration["PatientService:BaseUrl"]}");
Console.WriteLine($"[Config] UserService:BaseUrl    = {builder.Configuration["UserService:BaseUrl"]}");

// Swagger
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAngular");
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Migrate + seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RoomDbContext>();
    db.Database.Migrate();

    if (!db.Rooms.Any() && !db.WardBeds.Any())
    {
        db.Rooms.AddRange(
            new RoomService.Models.Room { RoomNumber = "AC-101", Category = RoomService.Models.RoomCategory.AC },
            new RoomService.Models.Room { RoomNumber = "AC-102", Category = RoomService.Models.RoomCategory.AC },
            new RoomService.Models.Room { RoomNumber = "NAC-201", Category = RoomService.Models.RoomCategory.NON_AC },
            new RoomService.Models.Room { RoomNumber = "NAC-202", Category = RoomService.Models.RoomCategory.NON_AC }
        );
        db.WardBeds.AddRange(
            new RoomService.Models.WardBed { WardName = "GW-A", BedNumber = "A-01" },
            new RoomService.Models.WardBed { WardName = "GW-A", BedNumber = "A-02" },
            new RoomService.Models.WardBed { WardName = "GW-B", BedNumber = "B-01" }
        );
        db.SaveChanges();
        Console.WriteLine("RoomService: Seeded sample rooms and ward beds.");
    }
}

app.Run();
