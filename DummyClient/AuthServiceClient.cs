using System.Net;
using System.Text;
using System.Text.Json;

namespace DummyClient;

public class AuthServiceClient
{
    private readonly HttpClient httpClient;
    private readonly string baseUrl;
    private readonly string apiVersion;

    public AuthServiceClient(HttpClient httpClient, string baseUrl, string apiVersion)
    {
        this.httpClient = httpClient;
        this.baseUrl = baseUrl;
        this.apiVersion = apiVersion;
    }

    public async Task<AuthenticationServiceResponse> SendLoginRequest(string username, string password)
    {
        var loginPayload = new { username, password };

        var loginContent = new StringContent(
            JsonSerializer.Serialize(loginPayload),
            Encoding.UTF8,
            "application/json");

        var loginEndpointUrl = BuildLoginEndpointUrl();
        var response = await httpClient.PostAsync(loginEndpointUrl, loginContent);
        var responseBody = await response.Content.ReadAsStringAsync();

        return new AuthenticationServiceResponse
        {
            StatusCode = response.StatusCode,
            ResponseBody = responseBody,
            AuthenticationData = ParseAuthenticationResponse(responseBody)
        };
    }

    public string BuildLoginEndpointUrl()
    {
        return $"{baseUrl}/api/{apiVersion}/auth/login";
    }

    private AuthenticationResponse? ParseAuthenticationResponse(string jsonResponseBody)
    {
        try
        {
            return JsonSerializer.Deserialize<AuthenticationResponse>(jsonResponseBody);
        }
        catch
        {
            return null;
        }
    }
}

public class AuthenticationServiceResponse
{
    public HttpStatusCode StatusCode { get; init; }
    public string ResponseBody { get; init; } = string.Empty;
    public AuthenticationResponse? AuthenticationData { get; init; }

    public bool IsSuccessStatusCode => StatusCode == HttpStatusCode.OK;

    public bool HasValidToken => AuthenticationData?.Token != null &&
                                  !string.IsNullOrEmpty(AuthenticationData.Token);
}
