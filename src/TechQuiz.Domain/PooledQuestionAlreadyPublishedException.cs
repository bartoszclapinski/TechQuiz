namespace TechQuiz.Domain;

public sealed class PooledQuestionAlreadyPublishedException : DomainException
{
    public PooledQuestionAlreadyPublishedException()
        : base("PooledQuestion is already published.") { }
}
