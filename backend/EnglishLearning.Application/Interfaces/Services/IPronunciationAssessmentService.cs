using EnglishLearning.Application.DTOs;

namespace EnglishLearning.Application.Interfaces.Services;

public interface IPronunciationAssessmentService
{
    PronunciationAssessmentDto EvaluatePronunciation(string spokenText, string? targetPhrase = null);
}
