using MediatR;
using TechQuiz.Application.Abstractions;
using TechQuiz.Application.Common.Dtos;

namespace TechQuiz.Application.Features.Review;

public sealed class GetReviewSessionDetailQueryHandler(
    IQuizRepository quizRepository,
    IUserContext userContext)
    : IRequestHandler<GetReviewSessionDetailQuery, ReviewSessionDetailDto>
{
    public async Task<ReviewSessionDetailDto> Handle(
        GetReviewSessionDetailQuery request,
        CancellationToken cancellationToken)
    {
        var session = await quizRepository.GetReviewSessionDetailAsync(request.SessionId, cancellationToken)
            ?? throw new KeyNotFoundException($"Review session {request.SessionId} not found.");

        if (session.UserId != userContext.UserId)
        {
            throw new ForbiddenAccessException("Review session does not belong to the current user.");
        }

        return new ReviewSessionDetailDto(session.Id, session.CompletedAt, session.Items);
    }
}
