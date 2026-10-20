using EnglishLearning.Application.DTOs;

namespace EnglishLearning.Application.Interfaces.Services;

public interface IProgressService
{
    Task<DashboardResponseDto> GetDashboardAsync(CancellationToken cancellationToken = default);

    Task<PaginatedListDto<HistoryItemDto>> GetHistoryAsync(
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default
    );

    Task<SessionDetailDto> GetSessionDetailAsync(
        Guid sessionId,
        CancellationToken cancellationToken = default
    );
}
