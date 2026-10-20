using EnglishLearning.Application.DTOs;

namespace EnglishLearning.Application.Interfaces.Services;

public interface ITopicService
{
    Task<IReadOnlyList<TopicDto>> GetAllActiveTopicsAsync(CancellationToken cancellationToken = default);
    Task<TopicDto> GetTopicByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
