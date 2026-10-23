using EnglishLearning.Application.Services;
using FluentAssertions;
using Xunit;

namespace EnglishLearning.UnitTests;

public class PronunciationAssessmentServiceTests
{
    private readonly PronunciationAssessmentService _service = new();

    [Fact]
    public void EvaluatePronunciation_WhenEmpty_ReturnsZeroScore()
    {
        var result = _service.EvaluatePronunciation("");
        result.OverallScore.Should().Be(0);
        result.Words.Should().BeEmpty();
    }

    [Fact]
    public void EvaluatePronunciation_WithValidSpeech_ReturnsColorCodedWordsAndPhonemes()
    {
        var result = _service.EvaluatePronunciation("I am practicing English today");

        result.OverallScore.Should().BeGreaterThan(70);
        result.AccuracyScore.Should().BeGreaterThan(70);
        result.Words.Should().HaveCount(5);

        var firstWord = result.Words[0];
        firstWord.Phonemes.Should().NotBeEmpty();
        firstWord.Phonemes.All(p => p.ColorHex.StartsWith("#")).Should().BeTrue();
    }

    [Fact]
    public void EvaluatePronunciation_WithTargetPhraseMismatch_PenalizesSubstitutedWords()
    {
        var accurateResult = _service.EvaluatePronunciation(
            spokenText: "I think this is great",
            targetPhrase: "I think this is great"
        );

        var mismatchedResult = _service.EvaluatePronunciation(
            spokenText: "I sink dis is grate",
            targetPhrase: "I think this is great"
        );

        accurateResult.OverallScore.Should().BeGreaterThan(mismatchedResult.OverallScore);
        mismatchedResult.Feedback.Should().NotBeNullOrWhiteSpace();
    }
}
