using System.Security.Claims;
using System.Text;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.OpenApi.Models;
    using Serilog;
    using SORMS.API.Configs;
    using SORMS.API.Data;
    using SORMS.API.Interfaces;
    using SORMS.API.Services;



    var builder = WebApplication.CreateBuilder(args);

    var configuration = builder.Configuration;


    // 1. Load cấu hình từ appsettings.json
    builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("JwtConfig"));
    builder.Services.Configure<EmailConfig>(builder.Configuration.GetSection("EmailConfig"));

    // 2. Cấu hình DbContext
    builder.Services.AddDbContext<SormsDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    // 3. Đăng ký các service
    builder.Services.AddScoped<IEmailService, EmailService>();

    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IResidentService, ResidentService>();
    builder.Services.AddScoped<IBillingService, BillingService>();
    builder.Services.AddScoped<ICheckInService, CheckInService>();
    builder.Services.AddScoped<INotificationService, NotificationService>();
    builder.Services.AddScoped<IReportService, ReportService>();
    builder.Services.AddScoped<IServiceRequestService, ServiceRequestService>();
    builder.Services.AddScoped<IRoomService, RoomService>();
    builder.Services.AddScoped<IStaffService, StaffService>();
    builder.Services.AddControllers()
        .AddJsonOptions(opts =>
        {
            opts.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        });

// ========== 4. JWT AUTHENTICATION ==========
var jwtIssuer = configuration["Jwt:Issuer"];
    var jwtAudience = configuration["Jwt:Audience"];
    var jwtKey = builder.Configuration["JwtConfig:key"];

    if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
    {
        throw new InvalidOperationException(
            "JWT Key must be configured in appsettings.json and be at least 32 characters long");
    }

    var key = Encoding.ASCII.GetBytes(jwtKey);

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set to true in production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,

        // ---- Thêm 2 dòng sau để ASP.NET đọc role/name đúng ----
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.NameIdentifier
    };

    // Event handlers (giữ nguyên, có thể bổ sung logging claims để debug)
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            // Log tất cả claim (hữu ích khi debug)
            var claims = string.Join(", ", context.Principal?.Claims.Select(c => $"{c.Type}:{c.Value}") ?? Array.Empty<string>());
            Log.Information("Token validated for user: {user}. Claims: {claims}",
                context.Principal?.Identity?.Name, claims);
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Log.Warning("Authentication failed: {exception}", context.Exception?.Message);
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            Log.Warning("Challenge issued");
            return Task.CompletedTask;
        }
    };
});

// 5. Cấu hình Swagger có bảo mật
builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "SORMS API", Version = "v1" });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "Nhập token dạng: Bearer {token}",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] {}
            }
        });
    });

    // 6. Cấu hình CORS nếu cần
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });

    builder.Services.AddControllers();
    builder.Services.AddAuthorization();

    var app = builder.Build();

    // 7. Middleware pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseCors("AllowAll");
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
