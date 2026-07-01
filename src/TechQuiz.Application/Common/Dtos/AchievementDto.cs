namespace TechQuiz.Application.Common.Dtos;

/// <summary>
/// One achievement badge as seen by the client: its stable <paramref name="Key"/>, display text, a
/// <paramref name="Group"/> the UI clusters/icons by (<c>quiz</c> / <c>review</c> / <c>streak</c>), and
/// the earned state. <paramref name="Progress"/> is clamped to <paramref name="Target"/>;
/// <paramref name="Unlocked"/> is <c>true</c> once the underlying count/streak reaches the target. All
/// derived on read — nothing about a badge is persisted.
/// </summary>
public sealed record AchievementDto(
    string Key,
    string Title,
    string Description,
    string Group,
    int Target,
    int Progress,
    bool Unlocked);

/// <summary>
/// The full badge catalogue for the current user plus a roll-up of how many are unlocked. Feeds the
/// Dashboard achievements section.
/// </summary>
public sealed record AchievementsDto(
    int UnlockedCount,
    int TotalCount,
    IReadOnlyList<AchievementDto> Items);
