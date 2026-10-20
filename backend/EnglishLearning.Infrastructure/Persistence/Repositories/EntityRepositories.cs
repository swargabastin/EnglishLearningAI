using EnglishLearning.Application.Interfaces.Repositories;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearning.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower(), cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Users.AddAsync(user, cancellationToken);
    }

    public void Update(User user)
    {
        _context.Users.Update(user);
    }
}

public class ConversationSessionRepository : IConversationSessionRepository
{
    private readonly AppDbContext _context;

    public ConversationSessionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ConversationSession?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ConversationSessions
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<ConversationSession?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ConversationSessions
            .Include(s => s.Topic)
            .Include(s => s.Messages)
            .Include(s => s.Assessment)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ConversationSession>> GetByUserIdAsync(Guid userId, int limit = 20, CancellationToken cancellationToken = default)
    {
        return await _context.ConversationSessions
            .AsNoTracking()
            .Include(s => s.Topic)
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.StartedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<ConversationSession> Items, int TotalCount)> GetPaginatedHistoryByUserIdAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ConversationSessions
            .AsNoTracking()
            .Include(s => s.Topic)
            .Include(s => s.Assessment)
            .Include(s => s.Messages)
            .Where(s => s.UserId == userId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(s => s.StartedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<ConversationSession>> GetCompletedSessionsWithAssessmentsByUserIdAsync(
        Guid userId,
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        return await _context.ConversationSessions
            .AsNoTracking()
            .Include(s => s.Topic)
            .Include(s => s.Assessment)
            .Where(s => s.UserId == userId && s.Status == SessionStatus.Completed)
            .OrderByDescending(s => s.StartedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ConversationSession session, CancellationToken cancellationToken = default)
    {
        await _context.ConversationSessions.AddAsync(session, cancellationToken);
    }

    public async Task AddMessageAsync(ConversationMessage message, CancellationToken cancellationToken = default)
    {
        await _context.ConversationMessages.AddAsync(message, cancellationToken);
    }

    public void Update(ConversationSession session)
    {
        _context.ConversationSessions.Update(session);
    }
}

public class AssessmentRepository : IAssessmentRepository
{
    private readonly AppDbContext _context;

    public AssessmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Assessment?> GetBySessionIdAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        return await _context.Assessments
            .FirstOrDefaultAsync(a => a.SessionId == sessionId, cancellationToken);
    }

    public async Task AddAsync(Assessment assessment, CancellationToken cancellationToken = default)
    {
        await _context.Assessments.AddAsync(assessment, cancellationToken);
    }
}

public class VocabularyRepository : IVocabularyRepository
{
    private readonly AppDbContext _context;

    public VocabularyRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Vocabulary?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Vocabulary
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Vocabulary>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Vocabulary
            .AsNoTracking()
            .Where(v => v.UserId == userId)
            .OrderBy(v => v.Word)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Vocabulary vocabulary, CancellationToken cancellationToken = default)
    {
        await _context.Vocabulary.AddAsync(vocabulary, cancellationToken);
    }

    public void Update(Vocabulary vocabulary)
    {
        _context.Vocabulary.Update(vocabulary);
    }

    public void Delete(Vocabulary vocabulary)
    {
        _context.Vocabulary.Remove(vocabulary);
    }
}
