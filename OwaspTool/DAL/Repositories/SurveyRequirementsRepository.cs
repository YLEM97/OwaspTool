using Microsoft.EntityFrameworkCore;
using OwaspTool.DAL;
using OwaspTool.Models.Database;
using OwaspTool.DTOs;
using System.Linq;

public interface ISurveyRequirementsRepository
{
    Task<List<RequirementDTO>> GetRequirementsForSurveyInstanceAsync(int surveyInstanceId);
    Task<List<RequirementDTO>> GetRequirementsForUserWebAppAsync(int userWebAppId);
    Task<List<RequirementDTO>?> GetRequirementsForUserWebAppIfOwnerAsync(int userWebAppId, string? userEmail);
}

public class SurveyRequirementsRepository : ISurveyRequirementsRepository
{
    private readonly OwaspToolContext _context;

    public SurveyRequirementsRepository(OwaspToolContext context)
    {
        _context = context;
    }

    public async Task<List<RequirementDTO>> GetRequirementsForSurveyInstanceAsync(int surveyInstanceId)
    {
        var surveyInstance = await _context.SurveyInstances
            .Include(si => si.UserWebApp)
                .ThenInclude(uwa => uwa.Level)
            .FirstOrDefaultAsync(si => si.SurveyInstanceID == surveyInstanceId);

        if (surveyInstance == null)
            return new List<RequirementDTO>();

        var levelId = surveyInstance.UserWebApp?.LevelID ?? 0;
        if (levelId == 0)
            return new List<RequirementDTO>();

        var answerOptionIds = await _context.GivenAnswers
            .Where(ga => ga.SurveyInstanceID == surveyInstanceId && ga.AnswerOptionID != null)
            .Select(ga => ga.AnswerOptionID!.Value)
            .Distinct()
            .ToListAsync();

        if (!answerOptionIds.Any())
            return new List<RequirementDTO>();

        var matches = await _context.ASVSReqAnswers
            .Where(ar => answerOptionIds.Contains(ar.AnswerOptionID))
            .Include(ar => ar.ASVSReqLevel)
                .ThenInclude(rl => rl.ASVSRequirement!)
                    .ThenInclude(r => r.Chapter)
            .Include(ar => ar.ASVSReqLevel)
                .ThenInclude(rl => rl.ASVSRequirement!)
                    .ThenInclude(r => r.Section)
            .OrderBy(ar => ar.DisplayOrder)
            .ToListAsync();

        var reqs = matches
            .Where(ar => ar.ASVSReqLevel != null && ar.ASVSReqLevel.LevelID == levelId && (ar.ASVSReqLevel.Active ?? true))
            .Select(ar => ar.ASVSReqLevel.ASVSRequirement!)
            .Where(r => r != null)
            .GroupBy(r => r.ASVSRequirementID)
            .Select(g => g.First())
            .ToList();

        var dtos = reqs.Select(r => new RequirementDTO(r)).ToList();
        return dtos;
    }

    public async Task<List<RequirementDTO>> GetRequirementsForUserWebAppAsync(int userWebAppId)
    {
        // Try to find an active survey instance for this UserWebApp (EndDate == null)
        var surveyInstance = await _context.SurveyInstances
            .Include(si => si.UserWebApp)
                .ThenInclude(uwa => uwa.Level)
            .Include(si => si.SurveyCategoryStatuses)
            .Where(si => si.UserWebAppID == userWebAppId && si.EndDate == null)
            .OrderByDescending(si => si.StartDate)
            .FirstOrDefaultAsync();

        // If none active, try most recent instance
        if (surveyInstance == null)
        {
            surveyInstance = await _context.SurveyInstances
                .Include(si => si.UserWebApp)
                    .ThenInclude(uwa => uwa.Level)
                .Include(si => si.SurveyCategoryStatuses)
                .Where(si => si.UserWebAppID == userWebAppId)
                .OrderByDescending(si => si.StartDate)
                .FirstOrDefaultAsync();
        }

        if (surveyInstance == null)
            return new List<RequirementDTO>();

        // Show requirements only if all category statuses for this instance are completed (StatusID == 3)
        var statuses = surveyInstance.SurveyCategoryStatuses;
        if (statuses == null || !statuses.Any() || !statuses.All(s => s.StatusID == 3))
        {
            return new List<RequirementDTO>();
        }

        return await GetRequirementsForSurveyInstanceAsync(surveyInstance.SurveyInstanceID);
    }

    public async Task<List<RequirementDTO>?> GetRequirementsForUserWebAppIfOwnerAsync(int userWebAppId, string? userEmail)
    {
        if (string.IsNullOrWhiteSpace(userEmail))
            return null; // no authenticated email -> not owner

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
        if (user == null)
            return null;

        // find a survey instance (prefer active) for this UserWebApp
        var surveyInstance = await _context.SurveyInstances
            .Include(si => si.UserWebApp)
                .ThenInclude(uwa => uwa.Level)
            .Include(si => si.SurveyCategoryStatuses)
            .Where(si => si.UserWebAppID == userWebAppId && si.EndDate == null)
            .OrderByDescending(si => si.StartDate)
            .FirstOrDefaultAsync();

        if (surveyInstance == null)
        {
            surveyInstance = await _context.SurveyInstances
                .Include(si => si.UserWebApp)
                    .ThenInclude(uwa => uwa.Level)
                .Include(si => si.SurveyCategoryStatuses)
                .Where(si => si.UserWebAppID == userWebAppId)
                .OrderByDescending(si => si.StartDate)
                .FirstOrDefaultAsync();
        }

        if (surveyInstance == null)
            return null;

        // Check ownership
        if (surveyInstance.UserWebApp == null || surveyInstance.UserWebApp.UserID != user.UserID)
            return null;

        // Now use existing logic: only show if all categories completed
        var statuses = surveyInstance.SurveyCategoryStatuses;
        if (statuses == null || !statuses.Any() || !statuses.All(s => s.StatusID == 3))
        {
            return new List<RequirementDTO>();
        }

        return await GetRequirementsForSurveyInstanceAsync(surveyInstance.SurveyInstanceID);
    }
}
