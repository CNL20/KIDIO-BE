using KIDIO.Business.DTOs.Vocabulary;

namespace KIDIO.Business.Interfaces;

public interface IVocabularyService
{
    Task<PagedVocabularyResponse> GetPagedAsync(int page, int pageSize, Guid? lessonId = null, CancellationToken ct = default);
    Task<List<VocabularyResponse>> GetAllAsync(CancellationToken ct = default);
    Task<List<VocabularyResponse>> SearchAsync(string keyword, Guid? lessonId = null, CancellationToken ct = default);
    Task<List<VocabularyResponse>> GetByLessonAsync(Guid lessonId, CancellationToken ct = default);
    Task<VocabularyResponse> GetByIdAsync(Guid vocabId, CancellationToken ct = default);
    Task<VocabularyResponse> CreateAsync(CreateVocabularyRequest request, CancellationToken ct = default);
    Task<VocabularyResponse> UpdateAsync(Guid vocabId, UpdateVocabularyRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid vocabId, CancellationToken ct = default);
    Task RestoreAsync(Guid vocabId, CancellationToken ct = default);
    Task HardDeleteAsync(Guid vocabId, CancellationToken ct = default);
    Task<bool> ExistsByWordAsync(string word, Guid lessonId, CancellationToken ct = default);
}