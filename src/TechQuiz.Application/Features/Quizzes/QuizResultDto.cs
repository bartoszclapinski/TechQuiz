using TechQuiz.Application.Common.Dtos;
using TechQuiz.Domain;

namespace TechQuiz.Application.Features.Quizzes;

public sealed record QuizResultDto(
    Guid AttemptId,
    DateTimeOffset StartedAt,
    DateTimeOffset CompletedAt,
    int CorrectCount,
    int TotalCount,
    double Percentage,
    IReadOnlyDictionary<Difficulty, DifficultyBreakdownDto> ByDifficulty,
    IReadOnlyList<QuestionResultDto> Questions);
