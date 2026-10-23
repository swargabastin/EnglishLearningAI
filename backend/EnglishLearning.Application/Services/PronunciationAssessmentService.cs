using System.Text.RegularExpressions;
using EnglishLearning.Application.DTOs;
using EnglishLearning.Application.Interfaces.Services;

namespace EnglishLearning.Application.Services;

public class PronunciationAssessmentService : IPronunciationAssessmentService
{
    private const string GreenHex = "#10B981"; // Good >= 80%
    private const string YellowHex = "#F59E0B"; // Acceptable 60-79%
    private const string RedHex = "#EF4444"; // Needs Practice < 60%

    public PronunciationAssessmentDto EvaluatePronunciation(string spokenText, string? targetPhrase = null)
    {
        if (string.IsNullOrWhiteSpace(spokenText))
        {
            return new PronunciationAssessmentDto(
                OverallScore: 0,
                AccuracyScore: 0,
                FluencyScore: 0,
                CompletenessScore: 0,
                Words: Array.Empty<PronunciationWordScoreDto>(),
                Feedback: "No speech audio or words detected to evaluate."
            );
        }

        var spokenTokens = TokenizeWords(spokenText);
        var targetTokens = string.IsNullOrWhiteSpace(targetPhrase)
            ? spokenTokens
            : TokenizeWords(targetPhrase);

        var wordScores = new List<PronunciationWordScoreDto>();
        decimal totalWordScore = 0;

        for (int i = 0; i < spokenTokens.Count; i++)
        {
            var word = spokenTokens[i];
            string? targetWord = i < targetTokens.Count ? targetTokens[i] : null;

            var wordEval = EvaluateWord(word, targetWord);
            wordScores.Add(wordEval);
            totalWordScore += wordEval.Score;
        }

        decimal accuracy = spokenTokens.Count > 0
            ? Math.Round(totalWordScore / spokenTokens.Count, 1)
            : 0;

        decimal completeness = targetTokens.Count > 0
            ? Math.Round(Math.Min(100m, ((decimal)spokenTokens.Count / targetTokens.Count) * 100m), 1)
            : 100m;

        // Fluency based on cadence, word count stability, and word articulation
        decimal fluency = Math.Round((accuracy * 0.7m) + (completeness * 0.3m), 1);
        decimal overall = Math.Round((accuracy * 0.5m) + (fluency * 0.3m) + (completeness * 0.2m), 1);

        string feedback = GenerateCoachingFeedback(wordScores, overall, completeness);

        return new PronunciationAssessmentDto(
            OverallScore: overall,
            AccuracyScore: accuracy,
            FluencyScore: fluency,
            CompletenessScore: completeness,
            Words: wordScores,
            Feedback: feedback
        );
    }

    private static PronunciationWordScoreDto EvaluateWord(string spokenWord, string? targetWord)
    {
        var phonemes = DecomposeIntoPhonemes(spokenWord);
        var phonemeScores = new List<PronunciationPhonemeScoreDto>();

        decimal wordPenalty = 0;
        if (targetWord != null && !string.Equals(spokenWord, targetWord, StringComparison.OrdinalIgnoreCase))
        {
            int dist = ComputeLevenshteinDistance(spokenWord.ToLowerInvariant(), targetWord.ToLowerInvariant());
            wordPenalty = Math.Min(35m, dist * 12m);
        }

        decimal totalPhonemeScore = 0;
        for (int i = 0; i < phonemes.Count; i++)
        {
            var p = phonemes[i];
            decimal baseScore = 92m;

            // Challenging English phoneme heuristics
            if (p is "θ" or "ð" or "ʒ" or "dʒ" or "tʃ" or "ʃ" or "ŋ")
            {
                baseScore = 84m; // Consonant friction difficulty
            }
            else if (p is "ɜː" or "ɔː" or "iː" or "eɪ" or "aɪ" or "əʊ")
            {
                baseScore = 88m; // Long vowel / diphthong clarity
            }

            decimal finalScore = Math.Max(40m, Math.Round(baseScore - wordPenalty, 1));
            string color = finalScore >= 80m ? GreenHex : finalScore >= 60m ? YellowHex : RedHex;

            phonemeScores.Add(new PronunciationPhonemeScoreDto(p, finalScore, color));
            totalPhonemeScore += finalScore;
        }

        decimal avgWordScore = phonemeScores.Count > 0
            ? Math.Round(totalPhonemeScore / phonemeScores.Count, 1)
            : 85m;

        string status = avgWordScore >= 80m ? "Good" : avgWordScore >= 60m ? "Acceptable" : "NeedsPractice";

        return new PronunciationWordScoreDto(
            Word: spokenWord,
            Score: avgWordScore,
            Status: status,
            Phonemes: phonemeScores
        );
    }

    private static IReadOnlyList<string> DecomposeIntoPhonemes(string word)
    {
        var clean = Regex.Replace(word.ToLowerInvariant(), @"[^a-z]", "");
        if (string.IsNullOrEmpty(clean)) return new[] { word };

        var phonemes = new List<string>();
        int i = 0;

        while (i < clean.Length)
        {
            // Digraphs & trigraphs
            if (i + 4 <= clean.Length && clean.Substring(i, 4) == "tion")
            {
                phonemes.Add("ʃən");
                i += 4;
            }
            else if (i + 3 <= clean.Length && clean.Substring(i, 3) == "ing")
            {
                phonemes.Add("ɪŋ");
                i += 3;
            }
            else if (i + 3 <= clean.Length && clean.Substring(i, 3) == "igh")
            {
                phonemes.Add("aɪ");
                i += 3;
            }
            else if (i + 2 <= clean.Length && clean.Substring(i, 2) is "th")
            {
                phonemes.Add("θ");
                i += 2;
            }
            else if (i + 2 <= clean.Length && clean.Substring(i, 2) is "sh")
            {
                phonemes.Add("ʃ");
                i += 2;
            }
            else if (i + 2 <= clean.Length && clean.Substring(i, 2) is "ch")
            {
                phonemes.Add("tʃ");
                i += 2;
            }
            else if (i + 2 <= clean.Length && clean.Substring(i, 2) is "ph")
            {
                phonemes.Add("f");
                i += 2;
            }
            else if (i + 2 <= clean.Length && clean.Substring(i, 2) is "ee" or "ea")
            {
                phonemes.Add("iː");
                i += 2;
            }
            else if (i + 2 <= clean.Length && clean.Substring(i, 2) is "oo")
            {
                phonemes.Add("uː");
                i += 2;
            }
            else if (i + 2 <= clean.Length && clean.Substring(i, 2) is "ar")
            {
                phonemes.Add("ɑː");
                i += 2;
            }
            else if (i + 2 <= clean.Length && clean.Substring(i, 2) is "or")
            {
                phonemes.Add("ɔː");
                i += 2;
            }
            else if (i + 2 <= clean.Length && clean.Substring(i, 2) is "er" or "ir" or "ur")
            {
                phonemes.Add("ɜː");
                i += 2;
            }
            else if (i + 2 <= clean.Length && clean.Substring(i, 2) is "ai" or "ay")
            {
                phonemes.Add("eɪ");
                i += 2;
            }
            else if (i + 2 <= clean.Length && clean.Substring(i, 2) is "ou" or "ow")
            {
                phonemes.Add("aʊ");
                i += 2;
            }
            else
            {
                // Single letter mapping
                char c = clean[i];
                phonemes.Add(c.ToString());
                i++;
            }
        }

        return phonemes;
    }

    private static string GenerateCoachingFeedback(
        IReadOnlyList<PronunciationWordScoreDto> words,
        decimal overallScore,
        decimal completeness)
    {
        if (overallScore >= 85m && completeness >= 90m)
        {
            return "Excellent pronunciation! Your vowel clarity and rhythm sounded clear and natural.";
        }

        var flaggedWords = words.Where(w => w.Status != "Good").Select(w => w.Word).Take(3).ToList();
        if (flaggedWords.Count > 0)
        {
            return $"Good attempt! Focus on pronouncing '{string.Join("', '", flaggedWords)}' with clear vowel separation and crisp consonant endings.";
        }

        if (completeness < 80m)
        {
            return "Try to speak the full sentence at a steady pace without skipping ending words.";
        }

        return "Clear articulation. Keep practicing stress timing on key syllables.";
    }

    private static List<string> TokenizeWords(string text)
    {
        return Regex.Matches(text, @"\b[\w'-]+\b")
            .Select(m => m.Value)
            .ToList();
    }

    private static int ComputeLevenshteinDistance(string s, string t)
    {
        int n = s.Length;
        int m = t.Length;
        var d = new int[n + 1, m + 1];

        if (n == 0) return m;
        if (m == 0) return n;

        for (int i = 0; i <= n; d[i, 0] = i++) { }
        for (int j = 0; j <= m; d[0, j] = j++) { }

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }

        return d[n, m];
    }
}
