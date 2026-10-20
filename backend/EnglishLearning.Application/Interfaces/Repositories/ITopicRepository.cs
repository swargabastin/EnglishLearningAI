using EnglishLearning.Domain.Entities;

namespace EnglishLearning.Application.Interfaces.Repositories;

public interface ITopicRepository
{
    Task<Topic?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Topic>> GetAllActiveAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Topic topic, CancellationToken cancellationToken = default);
    void Update(Topic topic);
}
