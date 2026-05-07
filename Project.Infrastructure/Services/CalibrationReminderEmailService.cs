// THIS SHI IS FROM AN OLD PROJECT SO YEAH FIX AND MAKE ONE FOR CALIBRATION


using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Project.Application.Common;
using Project.Application.Interfaces;
using Project.Domain.Entities;

namespace Project.Infrastructure.Services;

public sealed class TaskReminderEmailService : ITaskReminderEmailService
{
    private readonly ApprovalNotificationSettings _settings;
    private readonly ILogger<TaskReminderEmailService> _logger;

    public TaskReminderEmailService(
        IOptions<ApprovalNotificationSettings> settings,
        ILogger<TaskReminderEmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<int> SendDueDateReminderAsync(
        long taskId,
        long projectId,
        string projectName,
        string taskName,
        DateOnly dueDate,
        IReadOnlyList<User> recipients)
    {
        if (!_settings.Enabled)
        {
            _logger.LogDebug("Task reminder emails are disabled. Skipping reminder for task {TaskId}.", taskId);
            return 0;
        }

        if (string.IsNullOrWhiteSpace(_settings.SmtpHost) || string.IsNullOrWhiteSpace(_settings.SenderEmail))
        {
            _logger.LogWarning("Task reminder emails are enabled but SMTP settings are incomplete. Skipping reminder for task {TaskId}.", taskId);
            return 0;
        }

        var validRecipients = recipients
            .Where(r => !string.IsNullOrWhiteSpace(r.Email))
            .GroupBy(r => r.Email!, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();

        if (validRecipients.Count == 0)
            return 0;

        var subject = $"Task due today: {taskName}";
        var projectUrl = BuildProjectUrl(projectId, taskId);
        var body = $"""
            <p>Hello,</p>
            <p>This is a reminder that a task assigned or related to you is due today.</p>
            <p><strong>Project:</strong> {WebUtility.HtmlEncode(projectName)}</p>
            <p><strong>Task:</strong> {WebUtility.HtmlEncode(taskName)}</p>
            <p><strong>Due Date:</strong> {WebUtility.HtmlEncode(dueDate.ToString("yyyy-MM-dd"))}</p>
            <p><a href="{WebUtility.HtmlEncode(projectUrl)}">Open project</a></p>
            <p>Task ID: {taskId}</p>
            """;

        var sentCount = 0;
        using var client = CreateSmtpClient();

        foreach (var recipient in validRecipients)
        {
            try
            {
                using var message = new MailMessage
                {
                    From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                message.To.Add(new MailAddress(recipient.Email!, recipient.Username));
                await client.SendMailAsync(message);
                sentCount++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send due date reminder for task {TaskId} to user {UserId}.", taskId, recipient.UserId);
            }
        }

        return sentCount;
    }

    private SmtpClient CreateSmtpClient()
    {
        var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
        {
            EnableSsl = _settings.UseSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false
        };

        if (!string.IsNullOrWhiteSpace(_settings.Username))
            client.Credentials = new NetworkCredential(_settings.Username, _settings.Password);

        return client;
    }

    private string BuildProjectUrl(long projectId, long taskId)
    {
        if (!string.IsNullOrWhiteSpace(_settings.ProjectUrlTemplate))
        {
            return _settings.ProjectUrlTemplate
                .Replace("{projectId}", projectId.ToString(), StringComparison.OrdinalIgnoreCase)
                .Replace("{taskId}", taskId.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        if (!string.IsNullOrWhiteSpace(_settings.FrontendBaseUrl))
            return $"{_settings.FrontendBaseUrl.TrimEnd('/')}/projects/{projectId}";

        return $"projects/{projectId}";
    }
}
