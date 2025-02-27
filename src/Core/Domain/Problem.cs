namespace Core.Domain;

public class Problem
{
    public Problem(string title, DateTime now)
    {
        Title = title;
        CreatedAt = now;
    }

    public Guid Id { get; } = Guid.NewGuid();
    public string Title { get; private set; }
    public DateTime CreatedAt { get; private set; }
}
