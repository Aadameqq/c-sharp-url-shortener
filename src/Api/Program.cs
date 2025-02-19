using Api.Auth;
using Api.Options;
using Core;
using Infrastructure;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureOptions();
builder.Services.ConfigureInfrastructureDependencies();
builder.Services.ConfigureCoreDependencies();

builder.Services.AddControllers(options =>
{
    options.ModelBinderProviders.Insert(0, new AuthorizedUserBinderProvider());
    options.ModelBinderProviders.Insert(1, new AccessManagerBinderProvider());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
        }
    );

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                Array.Empty<string>()
            },
        }
    );
    options.SwaggerDoc(
        "docs",
        new OpenApiInfo
        {
            Title = "Survey App API",
            Description = "Application programming interface",
        }
    );
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "api-docs/{documentName}/swagger.json";
    });
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/api-docs/docs/swagger.json", "Docs");
        c.RoutePrefix = "api-docs";
    });
}

app.UseMiddleware<JwtMiddleware>();
app.MapControllers();

app.Run();
