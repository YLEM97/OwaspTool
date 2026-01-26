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

    // Nuovo: salva/upsert lo stato di implementazione per una coppia (UserWebApp, Requirement)
    Task<bool> SaveRequirementStatusAsync(int userWebAppId, int requirementId, int status, string? userEmail, string? notes = null, string? aiNotes = null);

    // Batch save: riduce round-trips al DB per sezioni con molti requisiti
    // ora accetta per ogni requirement anche le notes e aiNotes
    Task<bool> SaveRequirementStatusesAsync(int userWebAppId, Dictionary<int, (int Status, string? Notes, string? AiNotes)> statuses, string? userEmail);
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
        Console.WriteLine($"[1] Starting GetRequirementsForSurveyInstanceAsync for {surveyInstanceId}");

        var surveyInstance = await _context.SurveyInstances
            .Include(si => si.UserWebApp)
                .ThenInclude(uwa => uwa.Level)
            .FirstOrDefaultAsync(si => si.SurveyInstanceID == surveyInstanceId);

        Console.WriteLine($"[2] Loaded survey instance");

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

        Console.WriteLine($"[3] Loaded {answerOptionIds.Count} answer options");

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

        Console.WriteLine($"[4] Loaded {matches.Count} matches");

        var reqs = matches
            .Where(ar => ar.ASVSReqLevel != null && ar.ASVSReqLevel.LevelID == levelId && (ar.ASVSReqLevel.Active ?? true))
            .Select(ar => ar.ASVSReqLevel.ASVSRequirement!)
            .Where(r => r != null)
            .GroupBy(r => r.ASVSRequirementID)
            .Select(g => g.First())
            .ToList();

        Console.WriteLine($"[5] Filtered to {reqs.Count} requirements");

        // Carica gli stati salvati (includendo notes e aiNotes)
        Dictionary<int, ASVSRequirementStatus> statusMap = new Dictionary<int, ASVSRequirementStatus>();
        if (surveyInstance.UserWebApp != null)
        {
            var userWebAppId = surveyInstance.UserWebApp.UserWebAppID;

            var statuses = await _context.ASVSRequirementStatus
                .Where(s => s.UserWebAppID == userWebAppId)
                .ToListAsync();

            statusMap = statuses.ToDictionary(s => s.ASVSRequirementID, s => s);
        }

        var dtos = reqs.Select(r =>
        {
            var dto = new RequirementDTO(r);
            if (statusMap.TryGetValue(dto.ASVSRequirementID, out var st))
            {
                dto.ImplementationStatus = st.Status;
                dto.Notes = st.Notes;
                dto.AiNotes = st.AiNotes;
            }
            else
            {
                dto.ImplementationStatus = null;
                dto.Notes = null;
                dto.AiNotes = null;
            }
            return dto;
        }).ToList();

        Console.WriteLine($"[8] Created {dtos.Count} DTOs - DONE");

        return dtos;
    }



    public async Task<List<RequirementDTO>> GetRequirementsForUserWebAppAsync(int userWebAppId)
    {
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
            return new List<RequirementDTO>();

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

    // Nuovo metodo: salva/upsert lo status in batch (riduce round-trips)
    public async Task<bool> SaveRequirementStatusesAsync(int userWebAppId, Dictionary<int, (int Status, string? Notes, string? AiNotes)> statuses, string? userEmail)
    {
        if (string.IsNullOrWhiteSpace(userEmail) || statuses == null || !statuses.Any())
            return false;

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
        if (user == null)
            return false;

        var uwa = await _context.UserWebApps.FirstOrDefaultAsync(u => u.UserWebAppID == userWebAppId);
        if (uwa == null || uwa.UserID != user.UserID)
            return false; // not owner or not found

        var reqIds = statuses.Keys.ToList();

        // Carica gli status esistenti solo per gli ID richiesti
        var existingStatuses = await _context.ASVSRequirementStatus
            .Where(s => s.UserWebAppID == userWebAppId && reqIds.Contains(s.ASVSRequirementID))
            .ToDictionaryAsync(s => s.ASVSRequirementID, s => s);

        foreach (var kv in statuses)
        {
            var reqId = kv.Key;
            var statusValue = kv.Value.Status;
            var notesValue = kv.Value.Notes;
            var aiNotesValue = kv.Value.AiNotes;

            if (existingStatuses.TryGetValue(reqId, out var existing))
            {
                existing.Status = statusValue;
                existing.Notes = notesValue;
                existing.AiNotes = aiNotesValue;
                existing.Modified = DateTime.UtcNow;
                _context.ASVSRequirementStatus.Update(existing);
            }
            else
            {
                _context.ASVSRequirementStatus.Add(new ASVSRequirementStatus
                {
                    UserWebAppID = userWebAppId,
                    ASVSRequirementID = reqId,
                    Status = statusValue,
                    Notes = notesValue,
                    AiNotes = aiNotesValue,
                    Modified = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync();
        return true;
    }

    // Manteniamo il metodo singolo per compatibilità
    public async Task<bool> SaveRequirementStatusAsync(int userWebAppId, int requirementId, int status, string? userEmail, string? notes = null, string? aiNotes = null)
    {
        if (string.IsNullOrWhiteSpace(userEmail))
            return false;

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
        if (user == null)
            return false;

        var uwa = await _context.UserWebApps.FirstOrDefaultAsync(u => u.UserWebAppID == userWebAppId);
        if (uwa == null || uwa.UserID != user.UserID)
            return false; // not owner or not found

        var req = await _context.ASVSRequirements.FirstOrDefaultAsync(r => r.ASVSRequirementID == requirementId);
        if (req == null)
            return false;

        var existing = await _context.ASVSRequirementStatus
            .FirstOrDefaultAsync(s => s.UserWebAppID == userWebAppId && s.ASVSRequirementID == requirementId);

        if (existing == null)
        {
            var newEntry = new ASVSRequirementStatus
            {
                UserWebAppID = userWebAppId,
                ASVSRequirementID = requirementId,
                Status = status,
                Notes = notes,
                AiNotes = aiNotes,
                Modified = DateTime.UtcNow
            };
            _context.ASVSRequirementStatus.Add(newEntry);
        }
        else
        {
            existing.Status = status;
            existing.Notes = notes;
            existing.AiNotes = aiNotes;
            existing.Modified = DateTime.UtcNow;
            _context.ASVSRequirementStatus.Update(existing);
        }

        await _context.SaveChangesAsync();
        return true;
    }
}
