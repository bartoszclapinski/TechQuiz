namespace TechQuiz.Domain;

public sealed class Score
{
    public int CorrectCount { get; }
    public int TotalCount { get; }
    public IReadOnlyDictionary<Difficulty, (int Correct, int Total)> ByDifficulty { get; }

    public double Percentage => TotalCount == 0
        ? 0d
        : (double)CorrectCount / TotalCount * 100d;

    private Score(
        int correctCount,
        int totalCount,
        IReadOnlyDictionary<Difficulty, (int Correct, int Total)> byDifficulty)
    {
        CorrectCount = correctCount;
        TotalCount = totalCount;
        ByDifficulty = byDifficulty;
    }

    public static Score Calculate(IEnumerable<Question> questions, IEnumerable<Answer> answers)
    {
        ArgumentNullException.ThrowIfNull(questions);
        ArgumentNullException.ThrowIfNull(answers);

        var answerByQuestion = answers.ToDictionary(a => a.QuestionId);
        var byDiff = new Dictionary<Difficulty, (int Correct, int Total)>();
        var correct = 0;
        var total = 0;

        foreach (var q in questions)
        {
            total++;
            var isCorrect =
                answerByQuestion.TryGetValue(q.Id, out var answer)
                && answer.SelectedOptionId.HasValue
                && q.Options.Any(o => o.Id == answer.SelectedOptionId.Value && o.IsCorrect);

            if (isCorrect)
            {
                correct++;
            }

            var bucket = byDiff.TryGetValue(q.Difficulty, out var existing) ? existing : (Correct: 0, Total: 0);
            byDiff[q.Difficulty] = (bucket.Correct + (isCorrect ? 1 : 0), bucket.Total + 1);
        }

        return new Score(correct, total, byDiff);
    }
}
