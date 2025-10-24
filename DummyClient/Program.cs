using DummyClient;

// Setup
using var httpClient = new HttpClient();
var baseAuthUrl = Environment.GetEnvironmentVariable("AUTH_URL") ?? "http://localhost:5000";
var authApiVersion = "v0.2.1";

// Startup message
Console.WriteLine($"DummyClient v{DummyClient.ServiceVersion.Current} starting...");
Console.WriteLine("========================================");
Console.WriteLine();

// Wait for services to start
Console.WriteLine("Waiting for services to start...");
await Task.Delay(5000);

// Run auth service tests
var authServiceClient = new AuthServiceClient(httpClient, baseAuthUrl, authApiVersion);
var logger = new TestResultLogger();
var authServiceTester = new AuthServiceTester(authServiceClient, logger);

await authServiceTester.RunAllTests();
