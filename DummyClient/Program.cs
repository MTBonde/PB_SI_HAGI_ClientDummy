using System.Text;
using System.Text.Json;

using var httpClient = new HttpClient();
var baseAuthUrl = "http://localhost:5000";

// nice intro
Console.WriteLine($"DummyClient v{DummyClient.ServiceVersion.Current} starting...");
Console.WriteLine("========================================");
Console.WriteLine();

// Login to AuthService
try
{
    // login intro
    Console.WriteLine("Login to AuthService");
    Console.WriteLine($"POST {baseAuthUrl}/api/v0.1.0/auth/login");

    // sent test login
    var loginPayload = new
    {
        username = "test", 
        password = "test"
    };
    
    // serialize
    var loginContent = new StringContent(
        JsonSerializer.Serialize(loginPayload),
        Encoding.UTF8,
        "application/json");

    // store response
    var loginResponse = await httpClient.PostAsync($"{baseAuthUrl}/api/v0.1.0/auth/login", loginContent);
    var loginResult = await loginResponse.Content.ReadAsStringAsync();

    // show result
    Console.WriteLine($"Status: {loginResponse.StatusCode}");
    Console.WriteLine($"Response: {loginResult}");
    Console.WriteLine();
}
// Remind docker up
catch (Exception ex)
{
    Console.WriteLine();
    Console.WriteLine("========================================");
    Console.WriteLine("Did u forget to start docker?");
    Environment.Exit(1);
}
