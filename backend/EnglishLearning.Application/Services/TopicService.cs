using EnglishLearning.Application.DTOs;
using EnglishLearning.Application.Interfaces.Repositories;
using EnglishLearning.Application.Interfaces.Services;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Exceptions;

namespace EnglishLearning.Application.Services;

public class TopicService : ITopicService
{
    private readonly ITopicRepository _topicRepository;

    public TopicService(ITopicRepository topicRepository)
    {
        _topicRepository = topicRepository;
    }

    public async Task<IReadOnlyList<TopicDto>> GetAllActiveTopicsAsync(CancellationToken cancellationToken = default)
    {
        var topics = await _topicRepository.GetAllActiveAsync(cancellationToken);

        return topics.Select(MapToDto).ToList();
    }

    public async Task<TopicDto> GetTopicByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var topic = await _topicRepository.GetByIdAsync(id, cancellationToken);
        if (topic == null || !topic.IsActive)
        {
            throw new EntityNotFoundException(nameof(Topic), id);
        }

        return MapToDto(topic);
    }

    private static TopicDto MapToDto(Topic topic) =>
        new(
            topic.Id,
            topic.Name,
            topic.Description,
            topic.RecommendedDifficulty.ToString(),
            topic.IconKey,
            topic.IsActive,
            topic.SortOrder
        );
}
