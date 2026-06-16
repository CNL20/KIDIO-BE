namespace KIDIO.Business.Services;

/// <summary>
/// Generator for pronunciation feedback with detailed suggestions
/// </summary>
public static class PronunciationFeedbackGenerator
{
    private const int ExcellentThreshold = 85;
    private const int GoodThreshold = 70;
    private const int FairThreshold = 50;

    /// <summary>
    /// Generate detailed feedback based on scores
    /// </summary>
    public static string GenerateFeedback(
        int overallScore,
        int accuracyScore,
        int fluencyScore,
        int completenessScore)
    {
        var feedbackParts = new List<string>();

        // Overall assessment
        var overallAssessment = GetOverallAssessment(overallScore);
        feedbackParts.Add(overallAssessment);

        // Detailed feedback by component
        var accuracyFeedback = GetAccuracyFeedback(accuracyScore);
        if (!string.IsNullOrEmpty(accuracyFeedback))
            feedbackParts.Add(accuracyFeedback);

        var fluencyFeedback = GetFluencyFeedback(fluencyScore);
        if (!string.IsNullOrEmpty(fluencyFeedback))
            feedbackParts.Add(fluencyFeedback);

        var completenessFeedback = GetCompletenessFeedback(completenessScore);
        if (!string.IsNullOrEmpty(completenessFeedback))
            feedbackParts.Add(completenessFeedback);

        // Actionable suggestions
        var suggestions = GetSuggestions(accuracyScore, fluencyScore, completenessScore);
        if (suggestions.Count > 0)
        {
            feedbackParts.Add("Tips to improve:");
            feedbackParts.AddRange(suggestions.Select((s, i) => $"{i + 1}. {s}"));
        }

        return string.Join(" ", feedbackParts);
    }

    private static string GetOverallAssessment(int overallScore)
    {
        return overallScore switch
        {
            >= 90 => "🌟 Excellent! Your pronunciation is near-native. Keep up this outstanding performance!",
            >= 80 => "👏 Great job! Your pronunciation is very good with clear articulation.",
            >= 70 => "✓ Good! Your pronunciation is understandable, but there's room for improvement.",
            >= 60 => "⚠️ Fair pronunciation. Focus on clarity and try again.",
            >= 50 => "Practice needed. Listen to the reference pronunciation and try speaking more carefully.",
            _ => "Your pronunciation needs more practice. Try speaking slower and focus on each syllable."
        };
    }

    private static string GetAccuracyFeedback(int accuracyScore)
    {
        return accuracyScore switch
        {
            >= ExcellentThreshold => "Accuracy is excellent - you pronounced the sounds correctly.",
            >= GoodThreshold => $"Accuracy score: {accuracyScore}/100. Most sounds are correct; work on weak consonants.",
            >= FairThreshold => $"Accuracy: {accuracyScore}/100. Some sounds need adjustment - focus on vowels and final consonants.",
            _ => $"Accuracy: {accuracyScore}/100. Several sounds need improvement. Practice individual phonemes."
        };
    }

    private static string GetFluencyFeedback(int fluencyScore)
    {
        return fluencyScore switch
        {
            >= ExcellentThreshold => "Fluency is excellent - smooth and natural rhythm.",
            >= GoodThreshold => $"Fluency: {fluencyScore}/100. Good pacing; avoid rushing through difficult words.",
            >= FairThreshold => $"Fluency: {fluencyScore}/100. Work on rhythm and intonation patterns.",
            _ => $"Fluency: {fluencyScore}/100. Speak more slowly and evenly to improve flow."
        };
    }

    private static string GetCompletenessFeedback(int completenessScore)
    {
        return completenessScore switch
        {
            >= ExcellentThreshold => "Completeness is excellent - you pronounced all sounds in the word.",
            >= GoodThreshold => $"Completeness: {completenessScore}/100. You covered most sounds; don't skip syllables.",
            >= FairThreshold => $"Completeness: {completenessScore}/100. Some sounds or syllables were skipped.",
            _ => $"Completeness: {completenessScore}/100. Pronounce every syllable clearly."
        };
    }

    private static List<string> GetSuggestions(int accuracyScore, int fluencyScore, int completenessScore)
    {
        var suggestions = new List<string>();

        // Accuracy suggestions
        if (accuracyScore < GoodThreshold)
        {
            suggestions.Add("Listen to native speakers pronounce the word multiple times");
            suggestions.Add("Break the word into syllables and practice each one");
        }

        // Fluency suggestions
        if (fluencyScore < GoodThreshold)
        {
            suggestions.Add("Slow down - speak at a steady, comfortable pace");
            suggestions.Add("Work on stress and intonation in the word");
        }

        // Completeness suggestions
        if (completenessScore < GoodThreshold)
        {
            suggestions.Add("Don't skip any syllables - pronounce every part clearly");
            suggestions.Add("Record yourself and compare with the reference pronunciation");
        }

        // General improvement if all scores are low
        if (accuracyScore < FairThreshold && fluencyScore < FairThreshold)
        {
            suggestions.Clear();
            suggestions.Add("Start with a simpler pronunciation - focus on individual sounds");
            suggestions.Add("Use a mirror to see your mouth movements");
            suggestions.Add("Listen and repeat slowly without rush");
        }

        return suggestions;
    }
}
