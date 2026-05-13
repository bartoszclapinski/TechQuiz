namespace TechQuiz.Domain;

public class Category
{
    public Guid Id { get; }
    public string Name { get; }
    public string Description { get; }
    public string IconCode { get; }

    public Category(Guid id, string name, string description, string iconCode)
    {
        Id = id;
        Name = name;
        Description = description;
        IconCode = iconCode;
    }
}
