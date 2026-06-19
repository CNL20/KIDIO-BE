using KIDIO.Business.DTOs.Vocabulary;
using KIDIO.Business.Extensions;
using KIDIO.Business.Interfaces;
using KIDIO.Common;
using KIDIO.Data.Entities;
using KIDIO.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace KIDIO.Business.Services;

public class VocabularyService : IVocabularyService
{
    private readonly IUnitOfWork _uow;

    public VocabularyService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<PagedResponse<VocabularyResponse>> GetPagedAsync(
        int page, int pageSize, Guid? lessonId = null, CancellationToken ct = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 10 : pageSize;

        IQueryable<Vocabulary> query = _uow.Vocabularies.Query()
            .Include(v => v.Lesson);

        if (lessonId.HasValue)
            query = query.Where(v => v.LessonId == lessonId.Value);

        return await query
            .OrderBy(v => v.OrderIndex)
            .ThenBy(v => v.CreatedAt)
            .Select(v => new VocabularyResponse(
                v.Id,
                v.Word,
                v.Meaning,
                v.PhoneticText,
                v.AudioUrl,
                v.ImageUrl,
                v.ExampleSentence,
                v.OrderIndex,
                v.LessonId,
                v.Lesson.Title,
                v.CreatedAt
            ))
            .ToPagedResponseAsync(page, pageSize, ct);
    }

    public async Task<PagedResponse<VocabularyResponse>> GetAllPagedAsync(
        int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        return await _uow.Vocabularies.Query()
            .Include(v => v.Lesson)
            .OrderBy(v => v.OrderIndex)
            .ThenBy(v => v.CreatedAt)
            .Select(v => new VocabularyResponse(
                v.Id,
                v.Word,
                v.Meaning,
                v.PhoneticText,
                v.AudioUrl,
                v.ImageUrl,
                v.ExampleSentence,
                v.OrderIndex,
                v.LessonId,
                v.Lesson.Title,
                v.CreatedAt
            ))
            .ToPagedResponseAsync(pageNumber, pageSize, ct);
    }

    public async Task<PagedResponse<VocabularyResponse>> SearchPagedAsync(
        string keyword, Guid? lessonId = null, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        var normalized = keyword.Trim().ToLower();

        IQueryable<Vocabulary> query = _uow.Vocabularies.Query()
            .Include(v => v.Lesson)
            .Where(v =>
                v.Word.ToLower().Contains(normalized) ||
                v.Meaning.ToLower().Contains(normalized) ||
                (v.PhoneticText ?? string.Empty).ToLower().Contains(normalized) ||
                (v.ExampleSentence ?? string.Empty).ToLower().Contains(normalized) ||
                v.Lesson.Title.ToLower().Contains(normalized));

        if (lessonId.HasValue)
            query = query.Where(v => v.LessonId == lessonId.Value);

        return await query
            .OrderBy(v => v.OrderIndex)
            .ThenBy(v => v.CreatedAt)
            .Select(v => new VocabularyResponse(
                v.Id,
                v.Word,
                v.Meaning,
                v.PhoneticText,
                v.AudioUrl,
                v.ImageUrl,
                v.ExampleSentence,
                v.OrderIndex,
                v.LessonId,
                v.Lesson.Title,
                v.CreatedAt
            ))
            .ToPagedResponseAsync(pageNumber, pageSize, ct);
    }

    public async Task<PagedResponse<VocabularyResponse>> GetByLessonPagedAsync(
        Guid lessonId, int pageNumber = 1, int pageSize = 10, CancellationToken ct = default)
    {
        _ = await _uow.Lessons.GetByIdAsync(lessonId, ct)
            ?? throw new NotFoundException("Lesson");

        return await _uow.Vocabularies.Query()
            .Include(v => v.Lesson)
            .Where(v => v.LessonId == lessonId)
            .OrderBy(v => v.OrderIndex)
            .ThenBy(v => v.CreatedAt)
            .Select(v => new VocabularyResponse(
                v.Id,
                v.Word,
                v.Meaning,
                v.PhoneticText,
                v.AudioUrl,
                v.ImageUrl,
                v.ExampleSentence,
                v.OrderIndex,
                v.LessonId,
                v.Lesson.Title,
                v.CreatedAt
            ))
            .ToPagedResponseAsync(pageNumber, pageSize, ct);
    }

    public async Task<List<VocabularyResponse>> GetAllAsync(CancellationToken ct = default)
    {
        return await _uow.Vocabularies.Query()
            .Include(v => v.Lesson)
            .OrderBy(v => v.OrderIndex)
            .ThenBy(v => v.CreatedAt)
            .Select(v => new VocabularyResponse(
                v.Id,
                v.Word,
                v.Meaning,
                v.PhoneticText,
                v.AudioUrl,
                v.ImageUrl,
                v.ExampleSentence,
                v.OrderIndex,
                v.LessonId,
                v.Lesson.Title,
                v.CreatedAt
            ))
            .ToListAsync(ct);
    }

    public async Task<List<VocabularyResponse>> SearchAsync(
        string keyword, Guid? lessonId = null, CancellationToken ct = default)
    {
        var normalized = keyword.Trim().ToLower();

        IQueryable<Vocabulary> query = _uow.Vocabularies.Query()
            .Include(v => v.Lesson)
            .Where(v =>
                v.Word.ToLower().Contains(normalized) ||
                v.Meaning.ToLower().Contains(normalized) ||
                (v.PhoneticText ?? string.Empty).ToLower().Contains(normalized) ||
                (v.ExampleSentence ?? string.Empty).ToLower().Contains(normalized) ||
                v.Lesson.Title.ToLower().Contains(normalized));

        if (lessonId.HasValue)
            query = query.Where(v => v.LessonId == lessonId.Value);

        return await query
            .OrderBy(v => v.OrderIndex)
            .ThenBy(v => v.CreatedAt)
            .Select(v => new VocabularyResponse(
                v.Id,
                v.Word,
                v.Meaning,
                v.PhoneticText,
                v.AudioUrl,
                v.ImageUrl,
                v.ExampleSentence,
                v.OrderIndex,
                v.LessonId,
                v.Lesson.Title,
                v.CreatedAt
            ))
            .ToListAsync(ct);
    }

    public async Task<List<VocabularyResponse>> GetByLessonAsync(
        Guid lessonId, CancellationToken ct = default)
    {
        _ = await _uow.Lessons.GetByIdAsync(lessonId, ct)
            ?? throw new NotFoundException("Lesson");

        return await _uow.Vocabularies.Query()
            .Include(v => v.Lesson)
            .Where(v => v.LessonId == lessonId)
            .OrderBy(v => v.OrderIndex)
            .ThenBy(v => v.CreatedAt)
            .Select(v => new VocabularyResponse(
                v.Id,
                v.Word,
                v.Meaning,
                v.PhoneticText,
                v.AudioUrl,
                v.ImageUrl,
                v.ExampleSentence,
                v.OrderIndex,
                v.LessonId,
                v.Lesson.Title,
                v.CreatedAt
            ))
            .ToListAsync(ct);
    }

    public async Task<VocabularyResponse> GetByIdAsync(
        Guid vocabId, CancellationToken ct = default)
    {
        var vocab = await _uow.Vocabularies.Query()
            .Include(v => v.Lesson)
            .FirstOrDefaultAsync(v => v.Id == vocabId, ct)
            ?? throw new NotFoundException("Vocabulary");

        return MapToResponse(vocab);
    }

    public async Task<VocabularyResponse> CreateAsync(
        CreateVocabularyRequest request, CancellationToken ct = default)
    {
        _ = await _uow.Lessons.GetByIdAsync(request.LessonId, ct)
            ?? throw new NotFoundException("Lesson");

        if (await ExistsByWordAsync(request.Word, request.LessonId, ct))
            throw new AppException(
                $"Word '{request.Word}' already exists in this lesson.");

        if (await _uow.Vocabularies.Query().AnyAsync(v => v.LessonId == request.LessonId && v.OrderIndex == request.OrderIndex, ct))
            throw new AppException($"OrderIndex '{request.OrderIndex}' already exists in this lesson.");

        var vocab = new Vocabulary
        {
            Word = request.Word.Trim(),
            Meaning = request.Meaning.Trim(),
            OrderIndex = request.OrderIndex,
            PhoneticText = request.PhoneticText,
            AudioUrl = request.AudioUrl,
            ImageUrl = request.ImageUrl,
            ExampleSentence = request.ExampleSentence,
            LessonId = request.LessonId
        };

        await _uow.Vocabularies.AddAsync(vocab, ct);
        await _uow.SaveChangesAsync(ct);

        return await GetByIdAsync(vocab.Id, ct);
    }

    public async Task<VocabularyResponse> UpdateAsync(
        Guid vocabId, UpdateVocabularyRequest request, CancellationToken ct = default)
    {
        var vocab = await _uow.Vocabularies.GetByIdAsync(vocabId, ct)
            ?? throw new NotFoundException("Vocabulary");

        if (await _uow.Vocabularies.Query()
            .AnyAsync(v => v.LessonId == vocab.LessonId &&
                           v.Word.ToLower() == request.Word.ToLower() &&
                           v.Id != vocabId, ct))
            throw new AppException(
                $"Word '{request.Word}' already exists in this lesson.");

        if (await _uow.Vocabularies.Query()
            .AnyAsync(v => v.LessonId == vocab.LessonId &&
                           v.OrderIndex == request.OrderIndex &&
                           v.Id != vocabId, ct))
            throw new AppException($"OrderIndex '{request.OrderIndex}' already exists.");

        vocab.Word = request.Word.Trim();
        vocab.Meaning = request.Meaning.Trim();
        vocab.OrderIndex = request.OrderIndex;
        vocab.PhoneticText = request.PhoneticText;
        vocab.AudioUrl = request.AudioUrl;
        vocab.ImageUrl = request.ImageUrl;
        vocab.ExampleSentence = request.ExampleSentence;

        _uow.Vocabularies.Update(vocab);
        await _uow.SaveChangesAsync(ct);

        return await GetByIdAsync(vocab.Id, ct);
    }

    public async Task DeleteAsync(Guid vocabId, CancellationToken ct = default)
    {
        var vocab = await _uow.Vocabularies.GetByIdAsync(vocabId, ct)
            ?? throw new NotFoundException("Vocabulary");

        vocab.IsDeleted = true;
        _uow.Vocabularies.Update(vocab);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task RestoreAsync(Guid vocabId, CancellationToken ct = default)
    {
        var vocab = await _uow.Vocabularies.Query()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(v => v.Id == vocabId, ct)
            ?? throw new NotFoundException("Vocabulary");

        if (!vocab.IsDeleted)
            throw new AppException("Vocabulary is not deleted.");

        vocab.IsDeleted = false;
        _uow.Vocabularies.Update(vocab);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task HardDeleteAsync(Guid vocabId, CancellationToken ct = default)
    {
        var vocab = await _uow.Vocabularies.Query()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(v => v.Id == vocabId, ct)
            ?? throw new NotFoundException("Vocabulary");

        _uow.Vocabularies.Remove(vocab);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task<bool> ExistsByWordAsync(string word, Guid lessonId, CancellationToken ct = default)
    {
        var normalized = word.Trim().ToLower();

        return await _uow.Vocabularies.Query()
            .AnyAsync(v => v.LessonId == lessonId && v.Word.ToLower() == normalized, ct);
    }

    private static VocabularyResponse MapToResponse(Vocabulary v) => new(
        Id: v.Id,
        Word: v.Word,
        Meaning: v.Meaning,
        PhoneticText: v.PhoneticText,
        AudioUrl: v.AudioUrl,
        ImageUrl: v.ImageUrl,
        ExampleSentence: v.ExampleSentence,
        OrderIndex: v.OrderIndex,
        LessonId: v.LessonId,
        LessonTitle: v.Lesson?.Title ?? string.Empty,
        CreatedAt: v.CreatedAt
    );
}