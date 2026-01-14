using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OwaspTool.DAL;
using OwaspTool.Models.Database;
using OwaspTool.Services;          
using System.Security.Claims;

namespace OwaspTool.EndPoints
{
    public static class SurveyRequirementsEndpoint
    {
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
                // Recupero email utente loggato
                var user = http.User;

                string? email =
                    user?.FindFirst(ClaimTypes.Email)?.Value ??
                    user?.FindFirst("email")?.Value;

                var requirements = await repo.GetRequirementsForUserWebAppIfOwnerAsync(userWebAppId, email);

                //if (requirements == null)
                //    return Results.Unauthorized();

                //if (!requirements.Any())
                //    return Results.NotFound("No requirements available.");

                //var applicationName = userWebAppRepo.GetAppNameFromUserWebAppId(userWebAppId);

                //var pdfBytes = pdfGeneratorService.CreatePdf(requirements, applicationName);

                //return Results.File(
                //    pdfBytes,
                //    "application/pdf",
                //    fileDownloadName: "asvs-requirements.pdf"
                //);

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

            return app;
        }
    }
}
