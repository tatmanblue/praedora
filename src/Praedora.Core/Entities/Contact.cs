namespace Praedora.Core.Entities;

public class Contact
{
    public Guid Id { get; private set; }
    public Guid ApplicationId { get; private set; }
    public string Name { get; set; } = "";
    public string? Email { get; set; }
    public string? Role { get; set; }

    private Contact()
    {
    }

    public static Contact Create(Guid applicationId, string name, string? email = null, string? role = null)
    {
        return new Contact
        {
            Id = Guid.NewGuid(),
            ApplicationId = applicationId,
            Name = name,
            Email = email,
            Role = role
        };
    }
}
