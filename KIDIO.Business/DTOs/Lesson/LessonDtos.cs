namespace KIDIO.Business.DTOs.Lesson;

public record CreateLessonRequest(
    string Title,
    string? Description,
    string LessonType,      // "Story" | "Dialogue" | "VideoShort" | "PronunciationDrill"
    string Difficulty,      // "Beginner" | "Elementary" | "PreIntermediate"
    string SkillFocus,      // "Listening" | "Speaking" | "Vocabulary" | "Pronunciation"
    int DurationSeconds,
    string? ThumbnailUrl,
    string? AudioUrl,
    string? VideoUrl,
    string? ContentJson,
    int OrderIndex,
    Guid TopicId
);

public record UpdateLessonRequest(
    string Title,
    string? Description,
    string LessonType,
    string Difficulty,
    string SkillFocus,
    int DurationSeconds,
    string? ThumbnailUrl,
    string? AudioUrl,
    string? VideoUrl,
    string? ContentJson,
    int OrderIndex,
    bool IsPublished
);

public record LessonResponse(
    Guid Id,
    string Title,
    string? Description,
    string LessonType,
    string Difficulty,
    string SkillFocus,
    int DurationSeconds,
    string? ThumbnailUrl,
    string? AudioUrl,
    string? VideoUrl,
    string? ContentJson,
    int OrderIndex,
    bool IsPublished,
    Guid TopicId,
    string TopicName,
    int TotalVocabularies,
    DateTime CreatedAt
);

public record LessonSummaryResponse(
    Guid Id,
    string Title,
    string LessonType,
    string Difficulty,
    string SkillFocus,
    int DurationSeconds,
    string? ThumbnailUrl,
    string? ContentJson,
    int OrderIndex,
    bool IsPublished
);