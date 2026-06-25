namespace TechQuiz.Domain;

/// <summary>
/// An AI-generated question held in the pool (ADR-007). Created in <see cref="PooledQuestionStatus.Draft"/>
/// when its author generates it — carrying attribution (author, provider name, generation timestamp)
/// and the server-side correct option — and promoted to <see cref="PooledQuestionStatus.Published"/>
/// via <see cref="Publish"/> when the author shares it publicly (ADR-020). The provider is recorded as
/// a name string: provenance metadata, not the Application <c>AiProviderKind</c>, so Domain stays free
/// of the AI integration taxonomy (hard rule #3).
/// </summary>
public class PooledQuestion
{
    private readonly List<PooledQuestionOption> _options = [];

    public Guid Id { get; init; }
    public Guid CreatedByUserId { get; init; }
    public required string Provider { get; init; }
    public required string Topic { get; init; }
    public DateTime GeneratedAtUtc { get; init; }
    public QuestionType Type { get; init; }
    public Difficulty Difficulty { get; init; }
    public required string Text { get; init; }
    public string? Explanation { get; init; }
    public PooledQuestionStatus Status { get; private set; }
    public IReadOnlyList<PooledQuestionOption> Options => _options;

    // Parameterless constructor for EF Core materialisation.
    private PooledQuestion() { }

    public static PooledQuestion Create(
        Guid id,
        Guid createdByUserId,
        string provider,
        string topic,
        DateTime generatedAtUtc,
        QuestionType type,
        Difficulty difficulty,
        string text,
        string? explanation,
        IReadOnlyList<PooledQuestionOption> options)
    {
        if (createdByUserId == Guid.Empty)
        {
            throw new InvalidPooledQuestionException("PooledQuestion must record the creating user.");
        }

        if (string.IsNullOrWhiteSpace(provider))
        {
            throw new InvalidPooledQuestionException("PooledQuestion must record the generating provider.");
        }

        if (string.IsNullOrWhiteSpace(topic))
        {
            throw new InvalidPooledQuestionException("PooledQuestion must record the topic.");
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidPooledQuestionException("PooledQuestion text must not be empty.");
        }

        if (options is null || options.Count < 2)
        {
            throw new InvalidPooledQuestionException("PooledQuestion must have at least 2 options.");
        }

        if (type == QuestionType.MultipleChoice)
        {
            var correctCount = options.Count(o => o.IsCorrect);
            if (correctCount != 1)
            {
                throw new InvalidPooledQuestionException(
                    "MultipleChoice PooledQuestion must have exactly one correct option.");
            }
        }

        var pooled = new PooledQuestion
        {
            Id = id,
            CreatedByUserId = createdByUserId,
            Provider = provider,
            Topic = topic,
            GeneratedAtUtc = generatedAtUtc,
            Type = type,
            Difficulty = difficulty,
            Text = text,
            Explanation = explanation,
            Status = PooledQuestionStatus.Draft,
        };
        pooled._options.AddRange(options);
        return pooled;
    }

    /// <summary>Shares a draft into the public pool. Idempotency is not allowed — re-publishing throws.</summary>
    public void Publish()
    {
        if (Status == PooledQuestionStatus.Published)
        {
            throw new PooledQuestionAlreadyPublishedException();
        }

        Status = PooledQuestionStatus.Published;
    }
}
