using EnglishLearning.Application.Interfaces.Repositories;
using EnglishLearning.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearning.Infrastructure.Persistence.Repositories;

public class TopicRepository : ITopicRepository
{
    private readonly AppDbContext _context;

    public TopicRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Topic?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Topics
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Topic>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Topics
            .AsNoTracking()
            .Where(t => t.IsActive)
            .OrderBy(t => t.SortOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Topic topic, CancellationToken cancellationToken = default)
    {
        await _context.Topics.AddAsync(topic, cancellationToken);
    }

    public void Update(Topic topic)
    {
        _context.Topics.Update(topic);
    }
}
