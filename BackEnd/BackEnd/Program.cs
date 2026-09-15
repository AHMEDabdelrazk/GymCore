using System.Text;
using GymCore.API.Data;
using GymCore.API.Models;
using GymCore.API.Repositories;
using GymCore.API.Repositories.Interfaces;
using GymCore.API.Services;
using GymCore.API.Services.BackgroundJobs;
using GymCore.API.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Controllers & JSON Options
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

// Multi-Tenant Provider
builder.Services.AddScoped<ITenantProvider, TenantProvider>();

// Database Context (Supports In-Memory for portable evaluation and SQL Server for production)
var useInMemory = builder.Configuration.GetValue<bool>("UseInMemoryDatabase");
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<GymDbContext>((serviceProvider, options) =>
{
    var tenantProvider = serviceProvider.GetService<ITenantProvider>();
    if (useInMemory || string.IsNullOrEmpty(connectionString))
    {
        options.UseInMemoryDatabase("GymCoreEnterpriseDb");
    }
    else
    {
        options.UseSqlServer(connectionString);
    }
});

// Identity Configuration
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = false;
        options.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<GymDbContext>()
    .AddDefaultTokenProviders();

// Core Services Registration
builder.Services.AddScoped<IPlanService, PlanService>();
builder.Services.AddScoped<IPlanRepository, PlanRepository>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IClassBookingService, ClassBookingService>();
builder.Services.AddScoped<ICheckInService, CheckInService>();
builder.Services.AddScoped<IPaymentGatewayService, StripePaymentGatewayService>();

// Background Worker for automated subscription lifecycle audits
builder.Services.AddHostedService<SubscriptionLifecycleWorker>();

// AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "GymCoreSuperSecretKey2026VeryStrongProductionGradeEntropyKey!";
builder.Services
    .AddAuthentication(options =>
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
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "GymCore",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "GymCoreUsers",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// Swagger OpenAPI with Bearer Token and Tenant Header support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GymCore Enterprise Operations API",
        Version = "v2.0",
        Description = "Enterprise Gym & Fitness Club Operations Platform with Multi-Tenancy, Class Concurrency, Stripe Webhooks, and Audit Logging."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
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
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Seed Database automatically on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<GymDbContext>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    if (useInMemory)
    {
        context.Database.EnsureCreated();
    }
    else
    {
        try
        {
            context.Database.Migrate();
        }
        catch
        {
            // If SQL server is unavailable, ensure created on in-memory fallback
            context.Database.EnsureCreated();
        }
    }

    await DbInitializer.SeedAsync(context, userManager, roleManager);
}

app.UseDeveloperExceptionPage();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "GymCore Enterprise API v2.0");
});

app.UseCors("ReactPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();