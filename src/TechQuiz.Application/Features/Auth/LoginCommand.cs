using MediatR;
using TechQuiz.Application.Common.Dtos;

namespace TechQuiz.Application.Features.Auth;

public sealed record LoginCommand(string Email, string Password) : IRequest<AuthTokensDto>;
