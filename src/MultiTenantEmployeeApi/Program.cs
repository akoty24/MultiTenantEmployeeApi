using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MultiTenantEmployeeApi.Common;
using MultiTenantEmployeeApi.Common.Behaviors;
using MultiTenantEmployeeApi.Common.Middleware;
using MultiTenantEmployeeApi.Data;
using MultiTenantEmployeeApi.Tenancy;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // return model binding errors (bad json, wrong enum value...) using the same envelope
        options.InvalidModelStateResponseFactory = context =>
        {
            var details = context.ModelState
                .Where(x => x.Value!.Errors.Count > 0)
                .ToDictionary(x => x.Key, x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

            var error = new ApiError("ValidationFailed", "The request is invalid", details);
            return new BadRequestObjectResult(ApiResponse<object>.Fail(error));
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Tenant", new OpenApiSecurityScheme
    {
        Name = TenantMiddleware.TenantHeader,
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Description = "Tenant id, e.g. 11111111-1111-1111-1111-111111111111"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Tenant" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ITenantProvider, TenantProvider>();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

var app = builder.Build();

// apply migrations on startup so "docker compose up" is enough to run the project
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<TenantMiddleware>();

app.MapControllers();

app.Run();

// needed for WebApplicationFactory in the integration tests
public partial class Program
{
}
