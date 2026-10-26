using System.Text;
using AuthService.Data;
using AuthService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add configuration
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddControllers();

// JWT Authentication
var jwt = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwt["Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
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
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireDoctor", policy => policy.RequireRole("Doctor"));
    options.AddPolicy("RequireReceptionist", policy => policy.RequireRole("Receptionist"));
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "AuthService", Version = "v1" });
    // Add JWT auth to swagger
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
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Database migrate + seed on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    db.Database.Migrate();

    // seed admin if no users
    if (!db.Users.Any())
    {
        // create admin: username=admin password=Admin@123 roles=Admin
        using var hmac = new System.Security.Cryptography.HMACSHA512();
        var salt = hmac.Key;
        var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes("Admin@123"));
        db.Users.Add(new AuthService.Models.User
        {
            Username = "admin",
            PasswordHash = hash,
            PasswordSalt = salt,
            Roles = "Admin"
        });
        db.SaveChanges();
        Console.WriteLine("Seeded admin user (admin/Admin@123)");
    }
}


app.Run();
