using Application.LanguagePractice.DTOs;
using Application.LanguagePractice.Interfaces;
using Application.LanguagePractice.Queries;
using Application.Shared.Interfaces;

namespace Application.LanguagePractice.Handlers;

public class GetActiveLanguageStatsQueryHandler : IQueryHandler<GetActiveLanguageStatsQuery, LanguageStatsResponse>
{
    private readonly ILanguageLearnerQueryService _languageLearnerQueryService;

    public GetActiveLanguageStatsQueryHandler(ILanguageLearnerQueryService languageLearnerQueryService)
    {
        _languageLearnerQueryService = languageLearnerQueryService;
    }

    public async Task<LanguageStatsResponse> HandleAsync(GetActiveLanguageStatsQuery query, CancellationToken cancellationToken = default)
    {
        var languageCode = await _languageLearnerQueryService.GetUserLanguageAsync(query.UserId, cancellationToken);
        if (languageCode == null)
            return new LanguageStatsResponse("", 0, 0);

        var languageStats = await _languageLearnerQueryService.GetLanguageStatsAsync(query.UserId, languageCode.Value, cancellationToken);
        // Include today
        var daysPractised = (DateTime.UtcNow - languageStats.StartedAt).Days + 1;
        return new LanguageStatsResponse(languageStats.DisplayName, languageStats.UniqueLemmas, daysPractised);
    }
}
