namespace Project.Application.Common;

public sealed class EmailNotificationSettings
{
    public const string SectionName = "EmailNotifications";

    public bool Enabled { get; init; }
    public string SmtpHost { get; init; } = string.Empty;
    public int SmtpPort { get; init; } = 587;
    public bool UseSsl { get; init; } = true;
    public string SenderEmail { get; init; } = string.Empty;
    public string SenderName { get; init; } = "Calibration Management System";
    public string? Username { get; init; }
    public string? Password { get; init; }
    public string? FrontendBaseUrl { get; init; }
    public string? CalibrationUrlTemplate { get; init; }
}
