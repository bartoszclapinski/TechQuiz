using MediatR;
using TechQuiz.Application.Common.Dtos;

namespace TechQuiz.Application.Features.Achievements;

/// <summary>
/// Returns the current user's achievement badge catalogue (derived on read) plus an unlocked/total
/// roll-up for the Dashboard achievements section.
/// </summary>
public sealed record GetAchievementsQuery : IRequest<AchievementsDto>;
