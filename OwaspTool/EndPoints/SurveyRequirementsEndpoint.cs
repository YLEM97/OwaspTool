using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using OwaspTool.DAL;
using OwaspTool.Models.Database;
using OwaspTool.Services;
using System.Security.Claims;

namespace OwaspTool.EndPoints
{
    public static class SurveyRequirementsEndpoint
    {
        // DTO per binding del body JSON: esteso con Notes
        public record UpdateRequirementStatusDto(int Status, string? Notes);

        public static IEndpointRouteBuilder MapSurveyRequirementsEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/download-requirements/{userWebAppId:int}", static async (
                int userWebAppId,
                ISurveyRequirementsRepository repo,
                IUserWebAppRepository userWebAppRepo,
                IRequirementsPdfGeneratorService pdfGeneratorService,
                HttpContext http
            ) =>
            {
                var user = http.User;

                string? email =
                    user?.FindFirst(ClaimTypes.Email)?.Value ??
                    user?.FindFirst("email")?.Value;

                var requirements = await repo.GetRequirementsForUserWebAppIfOwnerAsync(userWebAppId, email);

                if (requirements == null) return Results.Unauthorized();

                var grouped = requirements
                    .GroupBy(r => r.Chapter!)
                    .ToDictionary(
                        g => g.Key,
                        g => g.GroupBy(r => r.Section!)
                              .ToDictionary(sg => sg.Key, sg => sg.ToList())
                    );

                var applicationName = userWebAppRepo.GetAppNameFromUserWebAppId(userWebAppId);

                var pdfBytes = pdfGeneratorService.CreatePdfV2(grouped, applicationName);

                return Results.File(
                    pdfBytes,
                    "application/pdf",
                    fileDownloadName: $"ASVS-{applicationName}.pdf"
                );
            });

            // Endpoint corretto: riceve DTO JSON nel body e usa il DbSet corretto (ASVSRequirementStatuses)
            app.MapPost("/api/userwebapp/{userWebAppId:int}/requirement/{requirementId:int}/status", async (
                int userWebAppId,
                int requirementId,
                UpdateRequirementStatusDto dto,
                HttpContext http,
                OwaspToolContext db
            ) =>
            {
                if (dto is null)
                    return Results.BadRequest("Request body is required.");

                if (dto.Status < 0 || dto.Status > 2)
                    return Results.BadRequest("Invalid status value.");

                // Verifica ownership
                var user = http.User;
                string? email =
                    user?.FindFirst(ClaimTypes.Email)?.Value ??
                    user?.FindFirst("email")?.Value;

                if (string.IsNullOrWhiteSpace(email))
                    return Results.Unauthorized();

                var owner = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (owner == null)
                    return Results.Unauthorized();

                var uwa = await db.UserWebApps.FirstOrDefaultAsync(u => u.UserWebAppID == userWebAppId);
                if (uwa == null || uwa.UserID != owner.UserID)
                    return Results.Unauthorized();

                // Check requirement exists
                var req = await db.ASVSRequirements.FirstOrDefaultAsync(r => r.ASVSRequirementID == requirementId);
                if (req == null)
                    return Results.NotFound("Requirement not found");

                // upsert sulla tabella ASVSRequirementStatuses (nota il DbSet corretto)
                var existing = await db.ASVSRequirementStatus
                    .FirstOrDefaultAsync(s => s.UserWebAppID == userWebAppId && s.ASVSRequirementID == requirementId);

                if (existing == null)
                {
                    var newEntry = new ASVSRequirementStatus
                    {
                        UserWebAppID = userWebAppId,
                        ASVSRequirementID = requirementId,
                        Status = dto.Status,
                        Notes = dto.Notes,
                        Modified = DateTime.UtcNow
                    };
                    db.ASVSRequirementStatus.Add(newEntry);
                }
                else
                {
                    existing.Status = dto.Status;
                    existing.Notes = dto.Notes;
                    existing.Modified = DateTime.UtcNow;
                    db.ASVSRequirementStatus.Update(existing);
                }

                await db.SaveChangesAsync();

                return Results.Ok();
            });

            return app;
        }
    }
}
