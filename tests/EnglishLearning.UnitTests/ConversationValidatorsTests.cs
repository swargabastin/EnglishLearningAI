using EnglishLearning.Application.DTOs;
using EnglishLearning.Application.Validators;
using FluentAssertions;
using Xunit;

namespace EnglishLearning.UnitTests;

public class ConversationValidatorsTests
{
    private readonly StartConversationRequestValidator _startValidator = new();
    private readonly SendMessageRequestValidator _messageValidator = new();

    [Fact]
    public void StartValidator_WithValidRequest_ShouldPass()
    {
        var request = new StartConversationRequest(Guid.NewGuid(), "B2");
        var result = _startValidator.Validate(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void StartValidator_WithEmptyTopicId_ShouldFail()
    {
        var request = new StartConversationRequest(Guid.Empty, "B2");
        var result = _startValidator.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "TopicId");
    }

    [Fact]
    public void StartValidator_WithInvalidLevel_ShouldFail()
    {
        var request = new StartConversationRequest(Guid.NewGuid(), "InvalidLevel");
        var result = _startValidator.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "EnglishLevel");
    }

    [Fact]
    public void SendMessageValidator_WithValidMessage_ShouldPass()
    {
        var request = new SendMessageRequest("Hello Emma, I'm ready to practice!");
        var result = _messageValidator.Validate(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void SendMessageValidator_WithEmptyMessage_ShouldFail()
    {
        var request = new SendMessageRequest("");
        var result = _messageValidator.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Message");
    }

    [Fact]
    public void SendMessageValidator_WithExcessiveLength_ShouldFail()
    {
        var request = new SendMessageRequest(new string('a', 2500));
        var result = _messageValidator.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Message");
    }
}
