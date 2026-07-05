namespace TechQuiz.Domain;

/// <summary>
/// A top-level grouping of related quiz <see cref="Category"/> items (e.g. ".NET", "Databases").
/// A track owns categories; a category owns a quiz. Position drives display order across tracks.
/// </summary>
public class Track
{
    public Guid Id { get; }
    public string Name { get; }
    public string Description { get; }
    public string IconCode { get; }
    public int Position { get; }

    public Track(Guid id, string name, string description, string iconCode, int position)
    {
        Id = id;
        Name = name;
        Description = description;
        IconCode = iconCode;
        Position = position;
    }
}
