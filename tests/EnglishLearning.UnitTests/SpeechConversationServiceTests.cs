using System.Text;
using EnglishLearning.Application.DTOs;
using EnglishLearning.Application.Interfaces.Infrastructure;
using EnglishLearning.Application.Interfaces.Services;
using EnglishLearning.Application.Services;
using EnglishLearning.Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EnglishLearning.UnitTests;

public class SpeechConversationServiceTests
{
    private readonly Mock<ISpeechToTextService> _sttMock;
    private readonly Mock<ITextToSpeechService> _ttsMock;
    private readonly Mock<IConversationService> _conversationServiceMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<ILogger<SpeechConversationService>> _loggerMock;
    private readonly SpeechConversationService _service;

    private readonly Guid _sessionId = Guid.NewGuid();

    public SpeechConversationServiceTests()
    {
        _sttMock = new Mock<ISpeechToTextService>();
        _ttsMock = new Mock<ITextToSpeechService>();
        _conversationServiceMock = new Mock<IConversationService>();
        _configMock = new Mock<IConfiguration>();
        _loggerMock = new Mock<ILogger<SpeechConversationService>>();

        _configMock.Setup(c => c["AiSettings:MaxAudioSizeBytes"]).Returns("10485760");

        _service = new SpeechConversationService(
            _sttMock.Object,
            _ttsMock.Object,
            _conversationServiceMock.Object,
            _configMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task ProcessSpeechAsync_WithValidAudio_ShouldSucceedWithSttAiAndTts()
    {
        // Arrange
        var audioBytes = Encoding.UTF8.GetBytes("RIFF...WAVEfmt...mock_audio_content");
        using var audioStream = new MemoryStream(audioBytes);

        var transcript = "I would like to practice speaking English today.";
        _sttMock.Setup(s => s.TranscribeAsync(It.IsAny<Stream>(), "audio/webm", "en-US", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SpeechRecognitionResult(transcript, 0.98));

        var sessionDto = new ConversationSessionDto(
            _sessionId, Guid.NewGuid(), Guid.NewGuid(), "Job Interview", "B1",
            DateTime.UtcNow, null, 10, null, "Active", new List<ConversationMessageDto>()
        );
        var userMsgDto = new ConversationMessageDto(Guid.NewGuid(), _sessionId, "user", transcript, null, 2, DateTime.UtcNow);
        var aiMsgDto = new ConversationMessageDto(Guid.NewGuid(), _sessionId, "assistant", "That sounds great! What topic would you like to explore?", null, 3, DateTime.UtcNow);
        var sendResult = new SendMessageResponseDto(sessionDto, userMsgDto, aiMsgDto);

        _conversationServiceMock.Setup(c => c.SendMessageAsync(_sessionId, It.Is<SendMessageRequest>(r => r.Message == transcript), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sendResult);

        var syntheticAudio = new byte[] { 0xFF, 0xFB, 0x90, 0x64, 0x01, 0x02 };
        _ttsMock.Setup(t => t.SynthesizeAsync(aiMsgDto.MessageText, It.IsAny<VoiceOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TextToSpeechResult(syntheticAudio, "audio/mpeg"));

        // Act
        var result = await _service.ProcessSpeechAsync(_sessionId, audioStream, "speech.webm", "audio/webm", audioBytes.Length);

        // Assert
        result.Should().NotBeNull();
        result.RecognizedText.Should().Be(transcript);
        result.UserMessage.MessageText.Should().Be(transcript);
        result.AiMessage.MessageText.Should().Be(aiMsgDto.MessageText);
        result.AudioBase64.Should().Be(Convert.ToBase64String(syntheticAudio));
        result.AudioContentType.Should().Be("audio/mpeg");

        _sttMock.Verify(s => s.TranscribeAsync(It.IsAny<Stream>(), "audio/webm", "en-US", It.IsAny<CancellationToken>()), Times.Once);
        _conversationServiceMock.Verify(c => c.SendMessageAsync(_sessionId, It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _ttsMock.Verify(t => t.SynthesizeAsync(aiMsgDto.MessageText, It.IsAny<VoiceOptions>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessSpeechAsync_WithEmptyAudio_ShouldThrow_InvalidAudioException()
    {
        // Arrange
        using var emptyStream = new MemoryStream();

        // Act
        var act = () => _service.ProcessSpeechAsync(_sessionId, emptyStream, "empty.webm", "audio/webm", 0);

        // Assert
        await act.Should().ThrowAsync<InvalidAudioException>()
            .WithMessage("*empty*");
    }

    [Fact]
    public async Task ProcessSpeechAsync_WithOversizedAudio_ShouldThrow_InvalidAudioException()
    {
        // Arrange
        var largeSize = 15 * 1024 * 1024; // 15MB > 10MB limit
        using var stream = new MemoryStream(new byte[100]);

        // Act
        var act = () => _service.ProcessSpeechAsync(_sessionId, stream, "large.wav", "audio/wav", largeSize);

        // Assert
        await act.Should().ThrowAsync<InvalidAudioException>()
            .WithMessage("*limit*");
    }

    [Fact]
    public async Task ProcessSpeechAsync_WithUnsupportedFormat_ShouldThrow_InvalidAudioException()
    {
        // Arrange
        var bytes = new byte[100];
        using var stream = new MemoryStream(bytes);

        // Act
        var act = () => _service.ProcessSpeechAsync(_sessionId, stream, "document.pdf", "application/pdf", bytes.Length);

        // Assert
        await act.Should().ThrowAsync<InvalidAudioException>()
            .WithMessage("*Unsupported*");
    }

    [Fact]
    public async Task ProcessSpeechAsync_WhenTtsFails_ShouldStillReturn_RecognizedTextAndAiMessage()
    {
        // Arrange
        var audioBytes = new byte[500];
        using var audioStream = new MemoryStream(audioBytes);

        var transcript = "Hello Emma.";
        _sttMock.Setup(s => s.TranscribeAsync(It.IsAny<Stream>(), "audio/wav", "en-US", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SpeechRecognitionResult(transcript, 0.95));

        var sessionDto = new ConversationSessionDto(
            _sessionId, Guid.NewGuid(), Guid.NewGuid(), "Daily Conversation", "A1",
            DateTime.UtcNow, null, 5, null, "Active", new List<ConversationMessageDto>()
        );
        var userMsgDto = new ConversationMessageDto(Guid.NewGuid(), _sessionId, "user", transcript, null, 2, DateTime.UtcNow);
        var aiMsgDto = new ConversationMessageDto(Guid.NewGuid(), _sessionId, "assistant", "Hello! How are you?", null, 3, DateTime.UtcNow);
        var sendResult = new SendMessageResponseDto(sessionDto, userMsgDto, aiMsgDto);

        _conversationServiceMock.Setup(c => c.SendMessageAsync(_sessionId, It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sendResult);

        // TTS throws exception
        _ttsMock.Setup(t => t.SynthesizeAsync(It.IsAny<string>(), It.IsAny<VoiceOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AiServiceException("TTS service temporarily down."));

        // Act
        var result = await _service.ProcessSpeechAsync(_sessionId, audioStream, "audio.wav", "audio/wav", audioBytes.Length);

        // Assert: Partial success behavior preserves conversation
        result.Should().NotBeNull();
        result.RecognizedText.Should().Be(transcript);
        result.AiMessage.MessageText.Should().Be("Hello! How are you?");
        result.AudioBase64.Should().BeNull(); // Audio is null because TTS failed gracefully
    }
}
