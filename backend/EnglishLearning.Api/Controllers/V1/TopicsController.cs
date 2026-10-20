using EnglishLearning.Application.Common.Models;
using EnglishLearning.Application.DTOs;
using EnglishLearning.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearning.Api.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
public class TopicsController : ControllerBase
{
    private readonly ITopicService _topicService;

    public TopicsController(ITopicService topicService)
    {
        _topicService = topicService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TopicDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var topics = await _topicService.GetAllActiveTopicsAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<TopicDto>>.Ok(topics));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<TopicDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var topic = await _topicService.GetTopicByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<TopicDto>.Ok(topic));
    }
}
