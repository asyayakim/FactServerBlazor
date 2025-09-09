using FactServerBlazor.Components.Config;
using Microsoft.Extensions.Options;
namespace FactServerBlazor.Components.Services;

public class FactServer
{
    private readonly HttpClient _httpClient;
    private readonly CloudflareAi _cloudflareConfig;

    public FactServer(HttpClient httpClient,  IOptions<CloudflareAi> cloudflareConfig)
    {
        _httpClient = httpClient;
        _cloudflareConfig = cloudflareConfig.Value;
    }

    public async Task<string> GetRandomFact()
    {
        var urlFact = "https://uselessfacts.jsph.pl/random.json";
        var factResponse = await _httpClient.GetFromJsonAsync<UselessFact>(urlFact);
        if (factResponse != null)
        {
            return factResponse.Text;
        }

        return string.Empty;
    }

    public async Task<string> GetImageServer(string textFact)
    {
        var requestBody = new { prompt = textFact };
        var accountId = _cloudflareConfig.AccountId;
        var apiToken = _cloudflareConfig.ApiToken;
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"https://api.cloudflare.com/client/v4/accounts/{accountId}/ai/run/@cf/lykon/dreamshaper-8-lcm")
        {
            Content = JsonContent.Create(requestBody)
        };
        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiToken);

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var bytes = await response.Content.ReadAsByteArrayAsync();
        return Convert.ToBase64String(bytes);
    }

    private class UselessFact
    {
        public string Id { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
    }
}