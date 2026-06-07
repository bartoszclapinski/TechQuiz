using TechQuiz.Application.Common.Dtos;
using TechQuiz.Domain;

namespace TechQuiz.Application.Features.Quizzes;

public sealed record QuizResultDto(
    Guid AttemptId,
    Guid CategoryId,
    string CategoryName,
    DateTimeOffset StartedAt,
    DateTimeOffset CompletedAt,
    int CorrectCount,
    int TotalCount,
    double Percentage,
    double BestPercentage,
    double? PreviousPercentage,
    IReadOnlyDictionary<Difficulty, DifficultyBreakdownDto> ByDifficulty,
    IReadOnlyList<QuestionResultDto> Questions);
