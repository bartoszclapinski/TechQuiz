using MediatR;
using TechQuiz.Application.Common.Dtos;

namespace TechQuiz.Application.Features.Auth;

public sealed record RefreshCommand(string RefreshToken) : IRequest<AuthTokensDto>;
