using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi.Models;

namespace TechQuiz.Api.OpenApi;

/// <summary>
/// Adds a JWT bearer security scheme to the generated OpenAPI document so the Scalar UI
/// exposes an "Authorize" control and attaches <c>Authorization: Bearer &lt;token&gt;</c> to
/// requests. The requirement is applied globally for simplicity; the anonymous auth
/// endpoints still work without a token despite showing a lock in the UI.
/// </summary>
public sealed class BearerSecuritySchemeTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var scheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Paste the access token from /api/auth/login. The 'Bearer ' prefix is added automatically.",
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes["Bearer"] = scheme;

        document.SecurityRequirements.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Id = "Bearer", Type = ReferenceType.SecurityScheme },
            }] = [],
        });

        return Task.CompletedTask;
    }
}
