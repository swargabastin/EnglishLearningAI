using EnglishLearning.Application.DTOs;
using EnglishLearning.Application.Interfaces.Repositories;
using EnglishLearning.Application.Interfaces.Services;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Exceptions;

namespace EnglishLearning.Application.Services;

public class VocabularyService : IVocabularyService
{
    private readonly IVocabularyRepository _vocabularyRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public VocabularyService(
        IVocabularyRepository vocabularyRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _vocabularyRepository = vocabularyRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<VocabularyItemDto>> GetUserVocabularyAsync(CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();
        var items = await _vocabularyRepository.GetByUserIdAsync(currentUserId, cancellationToken);

        return items.Select(MapToDto).ToList();
    }

    public async Task<VocabularyItemDto> AddVocabularyAsync(AddVocabularyRequest request, CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();

        if (string.IsNullOrWhiteSpace(request.Word))
        {
            throw new ArgumentException("Word or phrase cannot be empty.", nameof(request.Word));
        }

        var vocabulary = new Vocabulary
        {
            Id = Guid.NewGuid(),
            UserId = currentUserId,
            Word = request.Word.Trim(),
            Meaning = (request.Meaning ?? string.Empty).Trim(),
            Example = (request.Example ?? string.Empty).Trim(),
            MasteryLevel = 1,
            ContextSessionId = request.ContextSessionId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _vocabularyRepository.AddAsync(vocabulary, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(vocabulary);
    }

    public async Task<VocabularyItemDto> UpdateMasteryAsync(Guid id, UpdateMasteryRequest request, CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();
        var item = await _vocabularyRepository.GetByIdAsync(id, cancellationToken);

        if (item == null)
        {
            throw new EntityNotFoundException(nameof(Vocabulary), id);
        }

        if (item.UserId != currentUserId)
        {
            throw new UnauthorizedAccessException("You are not authorized to update this vocabulary item.");
        }

        var clampedMastery = Math.Clamp(request.MasteryLevel, 1, 5);
        item.MasteryLevel = clampedMastery;
        item.UpdatedAt = DateTime.UtcNow;

        _vocabularyRepository.Update(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(item);
    }

    public async Task DeleteVocabularyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var currentUserId = GetCurrentUserId();
        var item = await _vocabularyRepository.GetByIdAsync(id, cancellationToken);

        if (item == null)
        {
            throw new EntityNotFoundException(nameof(Vocabulary), id);
        }

        if (item.UserId != currentUserId)
        {
            throw new UnauthorizedAccessException("You are not authorized to delete this vocabulary item.");
        }

        _vocabularyRepository.Delete(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private Guid GetCurrentUserId()
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            throw new UnauthorizedAccessException("User is not authenticated.");
        }
        return userId.Value;
    }

    private static VocabularyItemDto MapToDto(Vocabulary v) =>
        new(
            v.Id,
            v.Word,
            v.Meaning,
            v.Example,
            v.MasteryLevel,
            v.ContextSessionId,
            v.CreatedAt,
            v.UpdatedAt
        );
}
