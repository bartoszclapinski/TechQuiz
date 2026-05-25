using MediatR;
using TechQuiz.Application.Common.Dtos;

namespace TechQuiz.Application.Features.Auth;

public sealed record RegisterCommand(string Email, string Password) : IRequest<AuthTokensDto>;
