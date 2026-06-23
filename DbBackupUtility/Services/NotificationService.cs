using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DbBackupUtility.Services
{
    public static class NotificationService
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public static async Task SendSlackNotificationAsync(string? webhookUrl, string message)
        {
            if (string.IsNullOrWhiteSpace(webhookUrl)) return;

            var payload = new { text = message };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync(webhookUrl, content);
                if (!response.IsSuccessStatusCode)
                {
                    LoggingService.LogWarning($"Slack notification failed with status code {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                LoggingService.LogWarning($"Failed to send Slack notification: {ex.Message}");
            }
        }
    }
}
