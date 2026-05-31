using System.Net.Http.Json;
using System.Text.Json;

namespace Planara.Accounts.Tests;

public static class ApiTestClient
{
    public static async Task<JsonDocument> PostAsync(
        this HttpClient client,
        string query,
        object? variables = null,
        CancellationToken ct = default)
    {
        var payload = new
        {
            query,
            variables
        };

        var resp = await client.PostAsJsonAsync("/graphql", payload, ct);
        var content = await resp.Content.ReadAsStringAsync(ct);

        if (!resp.IsSuccessStatusCode)
        {
            throw new HttpRequestException(
                $"GraphQL request failed with status {(int)resp.StatusCode} {resp.StatusCode}. Body: {content}"
            );
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("Empty GraphQL response");
        }

        return JsonDocument.Parse(content);
    }

    public static JsonElement? GetErrors(this JsonDocument doc)
        => doc.RootElement.TryGetProperty("errors", out var e) ? e : null;

    public static JsonElement GetData(this JsonDocument doc)
        => doc.RootElement.GetProperty("data");

    public static void AsUser(this HttpClient client, Guid userId)
    {
        client.DefaultRequestHeaders.Remove("X-Test-UserId");
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId.ToString());
    }
}