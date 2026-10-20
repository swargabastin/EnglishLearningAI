namespace EnglishLearning.Application.Interfaces.Infrastructure;

public record VoiceOptions(
    string Voice = "alloy",
    float Speed = 1.0f,
    string Format = "audio/mpeg"
);

public record TextToSpeechResult(
    byte[] AudioData,
    string ContentType,
    TimeSpan? Duration = null
);

public interface ITextToSpeechService
{
    Task<TextToSpeechResult> SynthesizeAsync(
        string text,
        VoiceOptions? options = null,
        CancellationToken cancellationToken = default
    );
}
