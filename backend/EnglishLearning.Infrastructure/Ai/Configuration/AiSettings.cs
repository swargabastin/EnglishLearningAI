namespace EnglishLearning.Infrastructure.Ai.Configuration;

public class AiSettings
{
    public const string SectionName = "AiSettings";

    public string Provider { get; set; } = "OpenAI";
    public string Model { get; set; } = "gpt-4o-mini";
    public string SttModel { get; set; } = "whisper-1";
    public string TtsModel { get; set; } = "tts-1";
    public string TtsVoice { get; set; } = "nova";
    public string? ApiKey { get; set; }
    public int MaxTokens { get; set; } = 500;
    public double Temperature { get; set; } = 0.7;
    public int MaxContextMessages { get; set; } = 10;
    public int TimeoutSeconds { get; set; } = 30;
    public int SttTimeoutSeconds { get; set; } = 25;
    public int TtsTimeoutSeconds { get; set; } = 25;
    public long MaxAudioSizeBytes { get; set; } = 10 * 1024 * 1024; // 10 MB
}
