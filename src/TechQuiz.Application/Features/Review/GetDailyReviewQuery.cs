using MediatR;
using TechQuiz.Application.Common.Dtos;

namespace TechQuiz.Application.Features.Review;

public sealed record GetDailyReviewQuery(int Count = 10)
    : IRequest<IReadOnlyList<ReviewQuestionDto>>;
