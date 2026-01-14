using Microsoft.EntityFrameworkCore;
using OwaspTool.DAL;
using OwaspTool.DTOs;
using OwaspTool.Models.Database;
using System.Security.Claims;

public interface IUserWebAppRepository
{
    Task<List<UserWebAppDTO>> GetAllByUserAsync(string email);
    string GetAppNameFromUserWebAppId(int userWebAppId);
    Task AddAsync(string email, string name, int levelId);
    Task DeleteAsync(int userWebAppId, string email);
    Task<List<Level>> GetAllLevelsAsync();
    Task<bool> IsSurveyCompletedAsync(int userWebAppId);
}
public class UserWebAppRepository : IUserWebAppRepository
{
    public UserWebAppRepository(OwaspToolContext context)
    {
        _context = context;
    }

    public async Task<List<UserWebAppDTO>> GetAllByUserAsync(string email)
    {
        var userId = await _context.Users
            .Where(u => u.Email == email)
            .Select(u => u.UserID)
            .FirstOrDefaultAsync();

        return await _context.UserWebApps
            .Where(uwa => uwa.UserID == userId)
            .Include(uwa => uwa.WebApplication)
            .Include(uwa => uwa.Level)
            .Select(uwa => new UserWebAppDTO
            {
                UserWebAppID = uwa.UserWebAppID,
                WebAppID = uwa.WebApplicationID,
                Name = uwa.WebApplication != null ? uwa.WebApplication.Name : string.Empty,
                LevelID = uwa.LevelID,
                LevelAcronym = uwa.Level != null ? uwa.Level.Acronym : string.Empty,
                LevelLabel = uwa.Level != null ? uwa.Level.Label : string.Empty
            })
            .ToListAsync();
    }

    public async Task AddAsync(string email, string name, int levelId)
    {
        var userId = await _context.Users
            .Where(u => u.Email == email)
            .Select(u => u.UserID)
            .FirstOrDefaultAsync();

        var webApp = new OwaspTool.Models.Database.WebApplication
        {
            Name = name
        };
        _context.WebApplications.Add(webApp);
        await _context.SaveChangesAsync();

        var userWebApp = new UserWebApp
        {
            UserID = userId,
            WebApplicationID = webApp.WebApplicationID,
            LevelID = levelId
        };
        _context.UserWebApps.Add(userWebApp);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int userWebAppId, string email)
    {
        var userId = await _context.Users
            .Where(u => u.Email == email)
            .Select(u => u.UserID)
            .FirstOrDefaultAsync();
        // Trova la riga nella tabella UserWebApp
        var entity = await _context.UserWebApps
            .FirstOrDefaultAsync(uwa => uwa.UserWebAppID == userWebAppId && uwa.UserID == userId);

        if (entity == null)
            return;

        var webAppId = entity.WebApplicationID;

        // Use a transaction to ensure all related deletes are performed atomically
        using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            // Load survey instances related to this UserWebApp
            var instances = await _context.SurveyInstances
                .Where(si => si.UserWebAppID == userWebAppId)
                .Select(si => si.SurveyInstanceID)
                .ToListAsync();

            if (instances.Any())
            {
                // Delete GivenAnswer rows linked to these instances
                var givenAnswers = await _context.GivenAnswers
                    .Where(ga => instances.Contains(ga.SurveyInstanceID))
                    .ToListAsync();
                if (givenAnswers.Any())
                {
                    _context.GivenAnswers.RemoveRange(givenAnswers);
                    await _context.SaveChangesAsync();
                }

                // Delete SurveyCategoryStatus rows linked to these instances
                var catStatuses = await _context.SurveyCategoryStatuses
                    .Where(scs => instances.Contains(scs.SurveyInstanceID))
                    .ToListAsync();
                if (catStatuses.Any())
                {
                    _context.SurveyCategoryStatuses.RemoveRange(catStatuses);
                    await _context.SaveChangesAsync();
                }

                // Delete SurveyInstance rows
                var surveyInstances = await _context.SurveyInstances
                    .Where(si => instances.Contains(si.SurveyInstanceID))
                    .ToListAsync();
                if (surveyInstances.Any())
                {
                    _context.SurveyInstances.RemoveRange(surveyInstances);
                    await _context.SaveChangesAsync();
                }
            }

            // Finally remove the UserWebApp relation
            _context.UserWebApps.Remove(entity);
            await _context.SaveChangesAsync();

            // If no other UserWebApp references this WebApplication, remove it too
            bool isUsedByOthers = await _context.UserWebApps
                .AnyAsync(uwa => uwa.WebApplicationID == webAppId);

            if (!isUsedByOthers)
            {
                var webApp = await _context.WebApplications
                    .FirstOrDefaultAsync(wa => wa.WebApplicationID == webAppId);

                if (webApp != null)
                {
                    _context.WebApplications.Remove(webApp);
                    await _context.SaveChangesAsync();
                }
            }

            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
    public async Task<List<Level>> GetAllLevelsAsync()
    {
        return await _context.Levels.ToListAsync();
    }
    public async Task<bool> IsSurveyCompletedAsync(int userWebAppId)
    {
        // prendi l'istanza attiva (EndDate == null) o l'ultima
        var instance = await _context.SurveyInstances
            .Include(si => si.SurveyCategoryStatuses)
            .Where(si => si.UserWebAppID == userWebAppId && si.EndDate == null)
            .OrderByDescending(si => si.StartDate)
            .FirstOrDefaultAsync();

        if (instance == null)
        {
            instance = await _context.SurveyInstances
                .Include(si => si.SurveyCategoryStatuses)
                .Where(si => si.UserWebAppID == userWebAppId)
                .OrderByDescending(si => si.StartDate)
                .FirstOrDefaultAsync();
        }

        if (instance == null)
            return false;

        var statuses = instance.SurveyCategoryStatuses;
        // consider complete if ci sono status e tutti hanno StatusID == 3
        return statuses != null && statuses.Any() && statuses.All(s => s.StatusID == 3);
    }

    public string GetAppNameFromUserWebAppId(int userWebAppId) 
    {
        var appName = (from uwa in _context.UserWebApps
                       join wa in _context.WebApplications on uwa.WebApplicationID equals wa.WebApplicationID
                       where uwa.UserWebAppID == userWebAppId
                       select wa.Name).FirstOrDefault();
        return appName ?? string.Empty;
    }

    protected OwaspToolContext _context;
}