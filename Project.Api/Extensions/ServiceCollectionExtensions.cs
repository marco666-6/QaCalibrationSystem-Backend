using System.Data;
using System.Text;
using Dapper;
using FluentValidation;
using Project.Application.Common;
using Project.Application.DTOs;
using Project.Application.Interfaces;
using Project.Application.Services;
using Project.Domain.Entities;
using Project.Infrastructure.Data;
using Project.Infrastructure.Mapping;
using Project.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Project.Api.Extensions;


public static class ServiceCollectionExtensions
{


    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services)
    {
        DapperTypeMapRegistrar.Register(
            typeof(Employee),
            typeof(User),
            typeof(PasswordResetToken),
            typeof(DefaultLocation),
            typeof(Section),
            typeof(Position),
            typeof(Equipment)
        );

        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
        SqlMapper.AddTypeHandler(new TimeOnlyTypeHandler());

        services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IDefaultLocationRepository, DefaultLocationRepository>();
        services.AddScoped<ISectionRepository, SectionRepository>();
        services.AddScoped<IPositionRepository, PositionRepository>();
        services.AddScoped<IEquipmentRepository, EquipmentRepository>();
        return services;
    }

    public static IServiceCollection AddApplication(
        this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IDefaultLocationService, DefaultLocationService>();
        services.AddScoped<ISectionService, SectionService>();
        services.AddScoped<IPositionService, PositionService>();
        services.AddScoped<IEquipmentService, EquipmentService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        
        services.AddValidatorsFromAssemblyContaining<CreateUserRequestValidator>();
        return services;
    }

    public static IServiceCollection AddEmailNotifications(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services;
    }

    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("JwtSettings section is missing from appsettings.json.");

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddAuthentication(options =>
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
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddAuthorization();

        return services;
    }

    public class DateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
    {
        public override void SetValue(IDbDataParameter parameter, DateOnly value)
            => parameter.Value = value.ToDateTime(TimeOnly.MinValue);

        public override DateOnly Parse(object value)
            => DateOnly.FromDateTime((DateTime)value);
    }


    public sealed class TimeOnlyTypeHandler : SqlMapper.TypeHandler<TimeOnly>
    {
        public override void SetValue(IDbDataParameter parameter, TimeOnly value)
        {
            parameter.DbType = DbType.Time;
            parameter.Value = value.ToTimeSpan();
        }

        public override TimeOnly Parse(object value)
        {
            if (value is TimeSpan timeSpan)
                return TimeOnly.FromTimeSpan(timeSpan);

            if (value is DateTime dateTime)
                return TimeOnly.FromDateTime(dateTime);

            throw new DataException($"Cannot convert {value.GetType()} to TimeOnly");
        }
    }

    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Calibration Management System API",
                Version = "v1",
                Description = "Calibration Management System REST API — .NET 9 + Dapper + SQL Server",
                Contact = new OpenApiContact
                {
                    Name = "Calibration Team",
                    Email = "calibration@team.com"
                }
            });

            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
                options.IncludeXmlComments(xmlPath);
        });

        return services;
    }
}
