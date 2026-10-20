using EnglishLearning.Application.DTOs;
using EnglishLearning.Application.Interfaces.Repositories;
using EnglishLearning.Application.Services;
using EnglishLearning.Application.Validators;
using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.Exceptions;
using FluentAssertions;
using Moq;
using Xunit;

namespace EnglishLearning.UnitTests;

public class TopicServiceTests
{
    private readonly Mock<ITopicRepository> _mockRepo;
    private readonly TopicService _service;

    public TopicServiceTests()
    {
        _mockRepo = new Mock<ITopicRepository>();
        _service = new TopicService(_mockRepo.Object);
    }

    [Fact]
    public async Task GetAllActiveTopicsAsync_ShouldReturn_MappedTopicDtos()
    {
        // Arrange
        var topics = new List<Topic>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Daily Conversation",
                Description = "Everyday talk",
                RecommendedDifficulty = EnglishProficiencyLevel.A1,
                IconKey = "MessageCircle",
                IsActive = true,
                SortOrder = 1
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Job Interview",
                Description = "Interview preparation",
                RecommendedDifficulty = EnglishProficiencyLevel.B2,
                IconKey = "Briefcase",
                IsActive = true,
                SortOrder = 2
            }
        };

        _mockRepo.Setup(r => r.GetAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(topics);

        // Act
        var result = await _service.GetAllActiveTopicsAsync();

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Daily Conversation");
        result[0].Difficulty.Should().Be("A1");
        result[1].Name.Should().Be("Job Interview");
        result[1].Difficulty.Should().Be("B2");
    }

    [Fact]
    public async Task GetTopicByIdAsync_WithValidId_ShouldReturn_TopicDto()
    {
        // Arrange
        var topicId = Guid.NewGuid();
        var topic = new Topic
        {
            Id = topicId,
            Name = "Travel",
            Description = "Travel scenarios",
            RecommendedDifficulty = EnglishProficiencyLevel.B1,
            IconKey = "Compass",
            IsActive = true
        };

        _mockRepo.Setup(r => r.GetByIdAsync(topicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(topic);

        // Act
        var result = await _service.GetTopicByIdAsync(topicId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(topicId);
        result.Name.Should().Be("Travel");
        result.Difficulty.Should().Be("B1");
    }

    [Fact]
    public async Task GetTopicByIdAsync_WithInvalidId_ShouldThrow_EntityNotFoundException()
    {
        // Arrange
        var topicId = Guid.NewGuid();
        _mockRepo.Setup(r => r.GetByIdAsync(topicId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Topic?)null);

        // Act
        var act = () => _service.GetTopicByIdAsync(topicId);

        // Assert
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public void TopicDtoValidator_ShouldValidate_RequiredFields()
    {
        // Arrange
        var validator = new TopicDtoValidator();
        var invalidDto = new TopicDto(Guid.NewGuid(), "", "", "", "icon", true, 1);

        // Act
        var result = validator.Validate(invalidDto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
        result.Errors.Should().Contain(e => e.PropertyName == "Difficulty");
    }
}
