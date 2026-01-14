using Microsoft.EntityFrameworkCore;
using OwaspTool.DAL;
using OwaspTool.Models.Database;
using System.Linq;

public interface ISurveyRepository
{
    Task<Survey?> GetDefaultSurveyAsync();
    Task<UserWebApp?> GetUserWebAppAsync(int userWebAppId);
    Task<UserWebApp?> GetUserWebAppIfOwnedByEmailAsync(int userWebAppId, string email);
    Task<SurveyInstance> CreateOrGetSurveyInstanceAsync(int userWebAppId, int surveyId);
    Task<List<GivenAnswer>> GetGivenAnswersAsync(int surveyInstanceId);

    // Supporta più risposte per domanda
    Task SaveCategoryAnswersAsync(
        int surveyInstanceId,
        int categoryId,
        Dictionary<int, List<int>> selectedOptionsByQuestionId,
        Dictionary<int, string?> otherTextByQuestionId,
        int statusId);

    // Radio (singola risposta)
    Task SaveSingleAnswerAsync(int surveyInstanceId, int questionId, int? answerOptionId);
    // Checklist (multiple risposte)
    Task SaveMultipleAnswersAsync(int surveyInstanceId, int questionId, List<int> answerOptionIds);
    Task DeleteCategoryAnswersAsync(int surveyInstanceId, int categoryId);
    Task<List<SurveyCategoryStatus>> GetCategoryStatusesAsync(int surveyInstanceId);
    Task SetCategoryStatusAsync(int surveyInstanceId, int categoryId, int statusId);

}

public class SurveyRepository : ISurveyRepository
{
    private readonly OwaspToolContext _context;

    public SurveyRepository(OwaspToolContext context)
    {
        _context = context;
    }

    public async Task<Survey?> GetDefaultSurveyAsync()
    {
        return await _context.Surveys
            .Include(s => s.Categories.OrderBy(c => c.DisplayOrder))
                .ThenInclude(c => c.Questions.OrderBy(q => q.DisplayOrder))
                    .ThenInclude(q => q.AnswerOptionQuestions.OrderBy(a => a.DisplayOrder))
                        .ThenInclude(a => a.Answer)
            .FirstOrDefaultAsync();
    }

    public async Task<UserWebApp?> GetUserWebAppAsync(int userWebAppId)
    {
        return await _context.UserWebApps
            .Include(uwa => uwa.WebApplication)
            .Include(uwa => uwa.Level)
            .FirstOrDefaultAsync(uwa => uwa.UserWebAppID == userWebAppId);
    }

    public async Task<UserWebApp?> GetUserWebAppIfOwnedByEmailAsync(int userWebAppId, string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var userId = await _context.Users
            .Where(u => u.Email == email)
            .Select(u => u.UserID)
            .FirstOrDefaultAsync();

        if (userId == Guid.Empty)
            return null;

        return await _context.UserWebApps
            .Include(uwa => uwa.WebApplication)
            .Include(uwa => uwa.Level)
            .FirstOrDefaultAsync(uwa => uwa.UserWebAppID == userWebAppId && uwa.UserID == userId);
    }

    public async Task<SurveyInstance> CreateOrGetSurveyInstanceAsync(int userWebAppId, int surveyId)
    {
        var instance = await _context.SurveyInstances
            .FirstOrDefaultAsync(si => si.UserWebAppID == userWebAppId && si.SurveyID == surveyId && si.EndDate == null);

        if (instance != null)
            return instance;

        var newInstance = new SurveyInstance
        {
            UserWebAppID = userWebAppId,
            SurveyID = surveyId,
            StartDate = DateTime.UtcNow
        };
        _context.SurveyInstances.Add(newInstance);
        await _context.SaveChangesAsync();
        return newInstance;
    }

    public async Task<List<GivenAnswer>> GetGivenAnswersAsync(int surveyInstanceId)
    {
        return await _context.GivenAnswers
            .Include(ga => ga.AnswerOption)
                .ThenInclude(ao => ao.Answer)
            .Where(ga => ga.SurveyInstanceID == surveyInstanceId)
            .ToListAsync();
    }

    public async Task SaveCategoryAnswersAsync(
        int surveyInstanceId,
        int categoryId,
        Dictionary<int, List<int>> selectedOptionsByQuestionId,
        Dictionary<int, string?> otherTextByQuestionId,
        int statusId)
    {
        var questionIds = await _context.Questions
            .Where(q => q.CategoryID == categoryId)
            .Select(q => q.QuestionID)
            .ToListAsync();

        var existing = await _context.GivenAnswers
            .Include(ga => ga.AnswerOption)
            .Where(ga => ga.SurveyInstanceID == surveyInstanceId
                      && ga.AnswerOption != null
                      && questionIds.Contains(ga.AnswerOption.QuestionID))
            .ToListAsync();

        if (existing.Any())
        {
            _context.GivenAnswers.RemoveRange(existing);
            await _context.SaveChangesAsync();
        }

        var toAdd = new List<GivenAnswer>();

        foreach (var qid in questionIds)
        {
            if (selectedOptionsByQuestionId.TryGetValue(qid, out var optIds) && optIds != null && optIds.Any())
            {
                foreach (var optId in optIds)
                {
                    var ga = new GivenAnswer
                    {
                        SurveyInstanceID = surveyInstanceId,
                        AnswerOptionID = optId,
                        OtherText = otherTextByQuestionId != null
                                    && otherTextByQuestionId.TryGetValue(qid, out var ot) ? ot : null,
                        Modified = DateTime.UtcNow,
                        Date = DateOnly.FromDateTime(DateTime.UtcNow)
                    };
                    toAdd.Add(ga);
                }
            }
        }

        if (toAdd.Any())
        {
            _context.GivenAnswers.AddRange(toAdd);
            await _context.SaveChangesAsync();
        }

        int validStatusId = statusId;
        var statusExists = await _context.Statuses.AnyAsync(s => s.StatusID == validStatusId);
        if (!statusExists)
        {
            var fallback = await _context.Statuses
                .OrderBy(s => s.StatusID)
                .Select(s => s.StatusID)
                .FirstOrDefaultAsync();

            if (fallback == 0)
            {
                throw new InvalidOperationException("Nessun record nella tabella 'Status'. Crea almeno uno status valido prima di salvare il questionario.");
            }
            validStatusId = fallback;
        }

        var scs = await _context.SurveyCategoryStatuses
            .FirstOrDefaultAsync(s => s.SurveyInstanceID == surveyInstanceId && s.CategoryID == categoryId);

        if (scs == null)
        {
            scs = new SurveyCategoryStatus
            {
                SurveyInstanceID = surveyInstanceId,
                CategoryID = categoryId,
                StatusID = validStatusId,
                LastSavedAt = DateTime.UtcNow
            };
            _context.SurveyCategoryStatuses.Add(scs);
        }
        else
        {
            scs.StatusID = validStatusId;
            scs.LastSavedAt = DateTime.UtcNow;
            _context.SurveyCategoryStatuses.Update(scs);
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException dbex)
        {
            throw new InvalidOperationException($"Errore salvataggio SurveyCategoryStatus (StatusID usato: {validStatusId}). Controlla che lo Status esista. Dettaglio: {dbex.Message}", dbex);
        }
    }

    public async Task SaveSingleAnswerAsync(int surveyInstanceId, int questionId, int? answerOptionId)
    {
        if (answerOptionId.HasValue)
        {
            var ao = await _context.AnswerOptions
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.AnswerOptionID == answerOptionId.Value);

            if (ao == null)
                throw new InvalidOperationException($"AnswerOptionId {answerOptionId.Value} non trovato.");

            if (ao.QuestionID != questionId)
                throw new InvalidOperationException(
                    $"AnswerOptionId {answerOptionId.Value} non appartiene alla Question {questionId} (found QuestionID: {ao.QuestionID})."
                );
        }

        var existing = await _context.GivenAnswers
            .Include(ga => ga.AnswerOption)
            .FirstOrDefaultAsync(ga =>
                ga.SurveyInstanceID == surveyInstanceId &&
                ga.AnswerOption != null &&
                ga.AnswerOption.QuestionID == questionId
            );

        if (!answerOptionId.HasValue)
        {
            if (existing != null)
            {
                _context.GivenAnswers.Remove(existing);
                await _context.SaveChangesAsync();
            }
            return;
        }

        if (existing != null)
        {
            existing.AnswerOptionID = answerOptionId.Value;
            existing.Modified = DateTime.UtcNow;
            existing.Date = DateOnly.FromDateTime(DateTime.UtcNow);
            _context.GivenAnswers.Update(existing);
        }
        else
        {
            var ga = new GivenAnswer
            {
                SurveyInstanceID = surveyInstanceId,
                AnswerOptionID = answerOptionId.Value,
                Modified = DateTime.UtcNow,
                Date = DateOnly.FromDateTime(DateTime.UtcNow)
            };
            _context.GivenAnswers.Add(ga);
        }

        await _context.SaveChangesAsync();
    }

    public async Task SaveMultipleAnswersAsync(int surveyInstanceId, int questionId, List<int> answerOptionIds)
    {
        var existing = await _context.GivenAnswers
            .Include(ga => ga.AnswerOption)
            .Where(ga => ga.SurveyInstanceID == surveyInstanceId &&
                         ga.AnswerOption != null &&
                         ga.AnswerOption.QuestionID == questionId)
            .ToListAsync();

        var existingIds = existing.Select(e => e.AnswerOptionID).ToHashSet();
        var newIds = answerOptionIds?.ToHashSet() ?? new HashSet<int>();

        var toRemove = existing.Where(e => !newIds.Contains((int)e.AnswerOptionID)).ToList();
        if (toRemove.Any())
            _context.GivenAnswers.RemoveRange(toRemove);

        var toAddIds = newIds.Except(existingIds.Cast<int>()).ToList();
        foreach (var id in toAddIds)
        {
            var ga = new GivenAnswer
            {
                SurveyInstanceID = surveyInstanceId,
                AnswerOptionID = id,
                Modified = DateTime.UtcNow,
                Date = DateOnly.FromDateTime(DateTime.UtcNow)
            };
            _context.GivenAnswers.Add(ga);
        }

        await _context.SaveChangesAsync();
    }

    public async Task DeleteCategoryAnswersAsync(int surveyInstanceId, int categoryId)
    {
        var answers = await _context.GivenAnswers
            .Where(a => a.SurveyInstanceID == surveyInstanceId &&
                        a.AnswerOption.Question.CategoryID == categoryId)
            .ToListAsync();

        _context.GivenAnswers.RemoveRange(answers);
        await _context.SaveChangesAsync();
    }
    public async Task<List<SurveyCategoryStatus>> GetCategoryStatusesAsync(int surveyInstanceId)
    {
        return await _context.SurveyCategoryStatuses
            .Where(s => s.SurveyInstanceID == surveyInstanceId)
            .ToListAsync();
    }

    public async Task SetCategoryStatusAsync(int surveyInstanceId, int categoryId, int statusId)
    {
        var scs = await _context.SurveyCategoryStatuses
            .FirstOrDefaultAsync(s => s.SurveyInstanceID == surveyInstanceId && s.CategoryID == categoryId);

        if (scs == null)
        {
            scs = new SurveyCategoryStatus
            {
                SurveyInstanceID = surveyInstanceId,
                CategoryID = categoryId,
                StatusID = statusId,
                LastSavedAt = DateTime.UtcNow
            };
            _context.SurveyCategoryStatuses.Add(scs);
        }
        else
        {
            scs.StatusID = statusId;
            scs.LastSavedAt = DateTime.UtcNow;
            _context.SurveyCategoryStatuses.Update(scs);
        }

        await _context.SaveChangesAsync();
    }


}
