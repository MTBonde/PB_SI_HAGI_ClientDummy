using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DummyClient;

public class RegistryServiceClient
{
    private readonly HttpClient httpClient;
    private readonly string baseUrl;
    private readonly string apiVersion;

    public RegistryServiceClient(HttpClient httpClient, string baseUrl, string apiVersion)
    {
        this.httpClient = httpClient;
        this.baseUrl = baseUrl;
        this.apiVersion = apiVersion;
    }

    public async Task<RegistryServiceResponse<RegisterServerResponse>> RegisterGameServer(
        string serverId,
        string host,
        int port,
        int maxPlayers)
    {
        var payload = new
        {
            serverId,
            host,
            port,
            maxPlayers
        };

        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        var endpointUrl = BuildRegisterEndpointUrl();
        var response = await httpClient.PostAsync(endpointUrl, content);
        var responseBody = await response.Content.ReadAsStringAsync();

        return new RegistryServiceResponse<RegisterServerResponse>
        {
            StatusCode = response.StatusCode,
            ResponseBody = responseBody,
            Data = ParseResponse<RegisterServerResponse>(responseBody)
        };
    }

    public async Task<RegistryServiceResponse<HeartbeatResponse>> SendHeartbeat(string serverId, int currentPlayers)
    {
        var payload = new
        {
            serverId,
            currentPlayers
        };

        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        var endpointUrl = BuildHeartbeatEndpointUrl();
        var response = await httpClient.PostAsync(endpointUrl, content);
        var responseBody = await response.Content.ReadAsStringAsync();

        return new RegistryServiceResponse<HeartbeatResponse>
        {
            StatusCode = response.StatusCode,
            ResponseBody = responseBody,
            Data = ParseResponse<HeartbeatResponse>(responseBody)
        };
    }

    public async Task<RegistryServiceResponse<AllocateServerResponse>> AllocateServer(string? jwtToken = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, BuildAllocateEndpointUrl());

        if (!string.IsNullOrEmpty(jwtToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
        }

        var response = await httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        return new RegistryServiceResponse<AllocateServerResponse>
        {
            StatusCode = response.StatusCode,
            ResponseBody = responseBody,
            Data = ParseResponse<AllocateServerResponse>(responseBody)
        };
    }

    public async Task<RegistryServiceResponse<DisconnectResponse>> DisconnectFromServer(string serverId, string? jwtToken = null)
    {
        var payload = new { serverId };

        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, BuildDisconnectEndpointUrl())
        {
            Content = content
        };

        if (!string.IsNullOrEmpty(jwtToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
        }

        var response = await httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        return new RegistryServiceResponse<DisconnectResponse>
        {
            StatusCode = response.StatusCode,
            ResponseBody = responseBody,
            Data = ParseResponse<DisconnectResponse>(responseBody)
        };
    }

    public string BuildRegisterEndpointUrl() => $"{baseUrl}/api/{apiVersion}/registry/register";
    public string BuildHeartbeatEndpointUrl() => $"{baseUrl}/api/{apiVersion}/registry/heartbeat";
    public string BuildAllocateEndpointUrl() => $"{baseUrl}/api/{apiVersion}/registry/allocate";
    public string BuildDisconnectEndpointUrl() => $"{baseUrl}/api/{apiVersion}/registry/disconnect";

    private T? ParseResponse<T>(string jsonResponseBody)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(jsonResponseBody);
        }
        catch
        {
            return default;
        }
    }
}

// Response wrapper
public class RegistryServiceResponse<T>
{
    public HttpStatusCode StatusCode { get; init; }
    public string ResponseBody { get; init; } = string.Empty;
    public T? Data { get; init; }

    public bool IsSuccessStatusCode => StatusCode == HttpStatusCode.OK;
}

// Response models
public class RegisterServerResponse
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("serverId")]
    public string ServerId { get; set; } = string.Empty;

    [JsonPropertyName("heartbeatInterval")]
    public int HeartbeatInterval { get; set; }
}

public class HeartbeatResponse
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}

public class AllocateServerResponse
{
    [JsonPropertyName("serverId")]
    public string ServerId { get; set; } = string.Empty;

    [JsonPropertyName("host")]
    public string Host { get; set; } = string.Empty;

    [JsonPropertyName("port")]
    public int Port { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}

public class DisconnectResponse
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}
