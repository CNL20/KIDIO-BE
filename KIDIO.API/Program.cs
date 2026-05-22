using KIDIO.API.Middleware;
using KIDIO.Business.Interfaces;
using KIDIO.Business.Services;
using KIDIO.Common;
using KIDIO.Data;
using KIDIO.Data.Entities;
using KIDIO.Data.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// =========================
// SETTINGS
// =========================

var jwtSettings = builder.Configuration
    .GetSection("JwtSettings")
    .Get<JwtSettings>()!;

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<GoogleOAuthSettings>(
    builder.Configuration.GetSection("GoogleOAuth"));
builder.Services.Configure<AdminSettings>(
    builder.Configuration.GetSection("AdminSettings"));
builder.Services.Configure<FacebookOAuthSettings>(
    builder.Configuration.GetSection("FacebookOAuth"));
builder.Services.Configure<AISettings>(
    builder.Configuration.GetSection("AISettings"));

// =========================
// DATABASE
// =========================

builder.Services.AddDbContext<KidioDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// =========================
// REPOSITORY & UNIT OF WORK
// =========================

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// =========================
// BUSINESS SERVICES
// =========================

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IChildService, ChildService>();
builder.Services.AddScoped<ITopicService, TopicService>();
builder.Services.AddScoped<ILessonService, LessonService>();
builder.Services.AddScoped<IVocabularyService, VocabularyService>();
builder.Services.AddScoped<IProgressService, ProgressService>();
// =========================
// JWT AUTHENTICATION
// =========================
var isDev = builder.Environment.IsDevelopment();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer      = jwtSettings.Issuer,
            ValidAudience    = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),

            // ClockSkew = TimeSpan.Zero nghĩa là không cho phép lệch giờ
            // Nếu máy dev lệch giờ thì tăng lên TimeSpan.FromMinutes(5)
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        // Let JwtBearer read the Authorization header and handle token extraction by default.
        // If you need to implement custom parsing, validate the candidate token first
        // (e.g. check it contains two '.' characters and doesn't contain control chars)
        // before assigning `context.Token = token`. For requests without a JWT, call
        // `context.NoResult()` to stop further processing.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var header = context.Request.Headers["Authorization"].ToString();
                if (header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    context.Token = header["Bearer ".Length..];
                }
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// =========================
// SWAGGER
// =========================

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "KIDIO API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Bearer",
        Type = SecuritySchemeType.Http,     // ← đúng
        Scheme = "bearer",                    // ← lowercase
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Chỉ nhập token, không cần gõ 'Bearer'"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// =========================
// CONTROLLERS
// =========================

builder.Services.AddControllers();
builder.Services.AddHttpClient();

// =========================
// BUILD APP
// =========================

var app = builder.Build();

// =========================
// MIDDLEWARE PIPELINE — THỨ TỰ QUAN TRỌNG
// =========================

// 1. Bắt exception toàn app — phải đứng đầu
app.UseMiddleware<ExceptionMiddleware>();

// 2. HTTPS redirect
app.UseHttpsRedirection();

// 3. Dev tools
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<KidioDbContext>();
    await db.Database.MigrateAsync();

    app.UseSwagger();
    app.UseSwaggerUI();
}

// 4. Auth — đúng thứ tự: Authentication trước, Authorization sau
app.UseAuthentication();
app.UseAuthorization();

// 5. Controllers
app.MapControllers();

app.Run();