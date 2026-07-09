using TechQuiz.Application.Common.Dtos;
using TechQuiz.Domain;

namespace TechQuiz.Application.Features.Quizzes;

/// <summary>
/// Projects a completed <see cref="QuizAttempt"/> plus its <see cref="Quiz"/> and computed
/// <see cref="Score"/> into a <see cref="QuizResultDto"/>. Shared by the complete command
/// (which produces the result on close) and the result query (which re-reads it), so the
/// two never drift in how correctness, per-question detail, or difficulty breakdown is shaped.
/// </summary>
internal static class QuizResultProjection
{
    public static QuizResultDto Build(
        QuizAttempt attempt,
        Quiz quiz,
        Score score,
        string categoryName,
        double bestPercentage,
        double? previousPercentage)
    {
        var answersByQuestion = attempt.Answers.ToDictionary(a => a.QuestionId);

        var questionResults = quiz.Questions
            .Select(q =>
            {
                var userAnswer = answersByQuestion.GetValueOrDefault(q.Id);
                var isCorrect = userAnswer is not null
                    && userAnswer.SelectedOptionId.HasValue
                    && q.Options.Any(o => o.Id == userAnswer.SelectedOptionId.Value && o.IsCorrect);

                var options = q.Options
                    .Select(o => new OptionResultDto(o.Id, o.Text, o.OrderIndex, o.IsCorrect))
                    .ToList();

                return new QuestionResultDto(
                    q.Id,
                    q.Text,
                    q.Difficulty,
                    q.Explanation,
                    options,
                    userAnswer?.SelectedOptionId,
                    isCorrect);
            })
            .ToList();

        var byDifficulty = score.ByDifficulty.ToDictionary(
            kvp => kvp.Key,
            kvp => new DifficultyBreakdownDto(kvp.Value.Correct, kvp.Value.Total));

        return new QuizResultDto(
            attempt.Id,
            quiz.CategoryId,
            categoryName,
            attempt.StartedAt,
            attempt.CompletedAt!.Value,
            score.CorrectCount,
            score.TotalCount,
            score.Percentage,
            bestPercentage,
            previousPercentage,
            Gamification.XpForAttempt(score.CorrectCount),
            byDifficulty,
            questionResults);
    }
}
