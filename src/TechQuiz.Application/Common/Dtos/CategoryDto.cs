namespace TechQuiz.Application.Common.Dtos;

public sealed record CategoryDto(
    Guid Id,
    string Name,
    string Description,
    string IconCode,
    int Position,
    int QuestionCount,
    double UserBestScore);
