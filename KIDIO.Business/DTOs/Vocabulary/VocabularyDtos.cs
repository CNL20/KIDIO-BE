namespace KIDIO.Business.DTOs.Vocabulary;

public record PagedVocabularyResponse(
    List<VocabularyResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages
);

public record CreateVocabularyRequest(
    string Word,
    string Meaning,
    string? PhoneticText,
    string? AudioUrl,
    string? ImageUrl,
    string? ExampleSentence,
    int OrderIndex,
    Guid LessonId
);

public record UpdateVocabularyRequest(
    string Word,
    string Meaning,
    string? PhoneticText,
    string? AudioUrl,
    string? ImageUrl,
    string? ExampleSentence
    ,int OrderIndex
);

public record VocabularyResponse(
    Guid Id,
    string Word,
    string Meaning,
    string? PhoneticText,
    string? AudioUrl,
    string? ImageUrl,
    string? ExampleSentence,
    int OrderIndex,
    Guid LessonId,
    string LessonTitle,
    DateTime CreatedAt
);