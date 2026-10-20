using EnglishLearning.Application.DTOs;
using EnglishLearning.Infrastructure.Ai.Prompts;
using FluentAssertions;
using Xunit;

namespace EnglishLearning.UnitTests;

public class TutorPromptBuilderTests
{
    [Theory]
    [InlineData("A1", "Beginner")]
    [InlineData("A2", "Elementary")]
    [InlineData("B1", "Intermediate")]
    [InlineData("B2", "Upper Intermediate")]
    [InlineData("C1", "Advanced")]
    [InlineData("C2", "Proficient")]
    public void BuildSystemPrompt_ShouldInclude_CefrSpecificInstructions(string level, string expectedKeyword)
    {
        // Arrange
        var request = new AiConversationRequest(
            "Job Interview",
            "Practice behavioral interview questions",
            level,
            Array.Empty<AiMessageContext>()
        );

        // Act
        var prompt = TutorPromptBuilder.BuildSystemPrompt(request);

        // Assert
        prompt.Should().Contain("Emma");
        prompt.Should().Contain("Job Interview");
        prompt.Should().Contain(level);
        prompt.Should().Contain(expectedKeyword);
        prompt.Should().Contain("CORRECTION BEHAVIOR");
        prompt.Should().Contain("SAFETY AND CONVERSATION BOUNDARIES");
    }
}
