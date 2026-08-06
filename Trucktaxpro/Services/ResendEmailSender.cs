using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Trucktaxpro.Services;

public class ResendEmailSender : IAppEmailSender
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public ResendEmailSender(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var apiKey = _configuration["Resend:ApiKey"];

        var payload = new
        {
            from = "TruckTaxPro <support@nexplanit.com>", // TODO: switch back to support@trucktaxpro.com once Resend verification completes,
            to = new[] { toEmail },
            subject,
            html = htmlBody
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.resend.com/emails")
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
}