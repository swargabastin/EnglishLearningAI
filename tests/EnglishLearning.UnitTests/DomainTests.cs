using EnglishLearning.Domain.Entities;
using EnglishLearning.Domain.Enums;
using EnglishLearning.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace EnglishLearning.UnitTests;

public class DomainTests
{
    [Fact]
    public void User_ShouldInitialize_WithDefaultValues()
    {
        var user = new User
        {
            Name = "Jane Doe",
            Email = "jane.doe@example.com",
            PasswordHash = "hashedpassword",
            EnglishLevel = EnglishProficiencyLevel.B2,
            LearningGoal = LearningGoal.WorkplaceEnglish
        };

        user.Id.Should().NotBeEmpty();
        user.Name.Should().Be("Jane Doe");
        user.Email.Should().Be("jane.doe@example.com");
        user.EnglishLevel.Should().Be(EnglishProficiencyLevel.B2);
        user.LearningGoal.Should().Be(LearningGoal.WorkplaceEnglish);
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        user.ConversationSessions.Should().NotBeNull();
        user.VocabularyItems.Should().NotBeNull();
    }

    [Fact]
    public void Topic_ShouldInitialize_WithActiveStateAndOrder()
    {
        var topic = new Topic
        {
            Name = "Job Interview",
            Description = "Practice interview scenarios",
            RecommendedDifficulty = EnglishProficiencyLevel.B2,
            IconKey = "Briefcase",
            SortOrder = 2
        };

        topic.Id.Should().NotBeEmpty();
        topic.IsActive.Should().BeTrue();
        topic.Name.Should().Be("Job Interview");
        topic.RecommendedDifficulty.Should().Be(EnglishProficiencyLevel.B2);
    }

    [Fact]
    public void ConversationSession_ShouldEstablish_InitialStatusAndRelationships()
    {
        var userId = Guid.NewGuid();
        var topicId = Guid.NewGuid();

        var session = new ConversationSession
        {
            UserId = userId,
            TopicId = topicId,
            EnglishLevel = EnglishProficiencyLevel.B1,
            Status = SessionStatus.Active
        };

        session.Id.Should().NotBeEmpty();
        session.UserId.Should().Be(userId);
        session.TopicId.Should().Be(topicId);
        session.Status.Should().Be(SessionStatus.Active);
        session.DurationSeconds.Should().Be(0);
        session.Messages.Should().NotBeNull();
    }

    [Fact]
    public void ConversationMessage_ShouldTrack_RoleAndSequence()
    {
        var sessionId = Guid.NewGuid();
        var message = new ConversationMessage
        {
            SessionId = sessionId,
            Role = SpeakerRole.Assistant,
            MessageText = "Tell me about your previous work experience.",
            SequenceNumber = 1
        };

        message.Id.Should().NotBeEmpty();
        message.SessionId.Should().Be(sessionId);
        message.Role.Should().Be(SpeakerRole.Assistant);
        message.SequenceNumber.Should().Be(1);
    }

    [Fact]
    public void Assessment_ShouldStore_ScoresAndJsonData()
    {
        var sessionId = Guid.NewGuid();
        var assessment = new Assessment
        {
            SessionId = sessionId,
            GrammarScore = 80.5m,
            VocabularyScore = 85.0m,
            FluencyScore = 78.0m,
            OverallScore = 81.2m,
            Feedback = "Strong conversational fluency with minor tense inconsistencies."
        };

        assessment.Id.Should().NotBeEmpty();
        assessment.SessionId.Should().Be(sessionId);
        assessment.OverallScore.Should().Be(81.2m);
        assessment.StrengthsJson.Should().Be("[]");
    }

    [Fact]
    public void EntityNotFoundException_ShouldContain_EntityNameAndKey()
    {
        var key = Guid.NewGuid();
        var ex = new EntityNotFoundException("Topic", key);

        ex.EntityName.Should().Be("Topic");
        ex.Key.Should().Be(key);
        ex.Message.Should().Contain("Topic");
        ex.Message.Should().Contain(key.ToString());
    }
}
