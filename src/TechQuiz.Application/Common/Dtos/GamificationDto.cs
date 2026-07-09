namespace TechQuiz.Application.Common.Dtos;

/// <summary>
/// The gamification block of the dashboard (ADR-025) — XP, level progress, and Skill IQ, all derived
/// on read from the user's completed attempts (no persisted counters). All-time state, like the
/// streak: the dashboard range filter never scopes it. Empty when the user has no completed attempts.
/// </summary>
public sealed record GamificationDto(
    int TotalXp,
    int Level,
    int XpIntoLevel,
    int XpForNextLevel,
    int SkillIq,
    int SkillIqWeeklyDelta,
    string Tier);
