namespace Project.Domain.Entities;

public sealed class NotificationPreference
{
    public int UserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public DateTime UpdatedAt { get; set; }
}
