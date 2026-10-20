using EnglishLearning.Application.Common.Models;
using EnglishLearning.Application.DTOs;
using EnglishLearning.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearning.Api.Controllers.V1;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class VocabularyController : ControllerBase
{
    private readonly IVocabularyService _vocabularyService;

    public VocabularyController(IVocabularyService vocabularyService)
    {
        _vocabularyService = vocabularyService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<VocabularyItemDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetVocabulary(CancellationToken cancellationToken)
    {
        var items = await _vocabularyService.GetUserVocabularyAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<VocabularyItemDto>>.Ok(items));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<VocabularyItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddVocabulary([FromBody] AddVocabularyRequest request, CancellationToken cancellationToken)
    {
        var item = await _vocabularyService.AddVocabularyAsync(request, cancellationToken);
        return Ok(ApiResponse<VocabularyItemDto>.Ok(item, "Vocabulary word saved successfully."));
    }

    [HttpPut("{id:guid}/mastery")]
    [ProducesResponseType(typeof(ApiResponse<VocabularyItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateMastery(Guid id, [FromBody] UpdateMasteryRequest request, CancellationToken cancellationToken)
    {
        var item = await _vocabularyService.UpdateMasteryAsync(id, request, cancellationToken);
        return Ok(ApiResponse<VocabularyItemDto>.Ok(item, "Mastery updated."));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteVocabulary(Guid id, CancellationToken cancellationToken)
    {
        await _vocabularyService.DeleteVocabularyAsync(id, cancellationToken);
        return Ok(ApiResponse<bool>.Ok(true, "Vocabulary word removed."));
    }
}
