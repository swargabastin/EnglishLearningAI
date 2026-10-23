namespace EnglishLearning.Application.Interfaces.Infrastructure;

public record SpeechRecognitionResult(
    string Transcript,
    double Confidence = 1.0,
    TimeSpan? Duration = null
);

public interface ISpeechToTextService
{
    Task<SpeechRecognitionResult> TranscribeAsync(
        Stream audioStream,
        string contentType,
        string languageCode = "en-US",
        CancellationToken cancellationToken = default
    );

    Task<SpeechRecognitionResult> TranscribeAsync(
        Stream audioStream,
        string contentType,
        string languageCode,
        string? promptHint,
        CancellationToken cancellationToken = default
    );
}
