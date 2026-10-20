using EnglishLearning.Domain.Entities;

namespace EnglishLearning.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    void Update(User user);
}

public interface IConversationSessionRepository
{
    Task<ConversationSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ConversationSession?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ConversationSession>> GetByUserIdAsync(Guid userId, int limit = 20, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<ConversationSession> Items, int TotalCount)> GetPaginatedHistoryByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ConversationSession>> GetCompletedSessionsWithAssessmentsByUserIdAsync(Guid userId, int limit = 50, CancellationToken cancellationToken = default);
    Task AddAsync(ConversationSession session, CancellationToken cancellationToken = default);
    Task AddMessageAsync(ConversationMessage message, CancellationToken cancellationToken = default);
    void Update(ConversationSession session);
}

public interface IAssessmentRepository
{
    Task<Assessment?> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task AddAsync(Assessment assessment, CancellationToken cancellationToken = default);
}

public interface IVocabularyRepository
{
    Task<Vocabulary?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Vocabulary>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Vocabulary vocabulary, CancellationToken cancellationToken = default);
    void Update(Vocabulary vocabulary);
    void Delete(Vocabulary vocabulary);
}

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
