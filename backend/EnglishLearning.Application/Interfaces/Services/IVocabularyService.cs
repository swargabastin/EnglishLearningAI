using EnglishLearning.Application.DTOs;

namespace EnglishLearning.Application.Interfaces.Services;

public interface IVocabularyService
{
    Task<IReadOnlyList<VocabularyItemDto>> GetUserVocabularyAsync(CancellationToken cancellationToken = default);
    Task<VocabularyItemDto> AddVocabularyAsync(AddVocabularyRequest request, CancellationToken cancellationToken = default);
    Task<VocabularyItemDto> UpdateMasteryAsync(Guid id, UpdateMasteryRequest request, CancellationToken cancellationToken = default);
    Task DeleteVocabularyAsync(Guid id, CancellationToken cancellationToken = default);
}
