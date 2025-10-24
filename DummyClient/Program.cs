using DummyClient;

// Setup
using var httpClient = new HttpClient();
var baseAuthUrl = Environment.GetEnvironmentVariable("AUTH_URL") ?? "http://localhost:5000";
var baseRegistryUrl = Environment.GetEnvironmentVariable("REGISTRY_URL") ?? "http://localhost:5001";
var authApiVersion = "v0.2.1";
var registryApiVersion = "v0.2.2";

// Startup message
Console.WriteLine($"DummyClient v{DummyClient.ServiceVersion.Current} starting...");
Console.WriteLine("========================================");
Console.WriteLine();

// Wait for services to start
Console.WriteLine("Waiting for services to start...");
await Task.Delay(5000);

// Initialize clients and logger
var authServiceClient = new AuthServiceClient(httpClient, baseAuthUrl, authApiVersion);
var registryServiceClient = new RegistryServiceClient(httpClient, baseRegistryUrl, registryApiVersion);
var logger = new TestResultLogger();

// Run auth service tests
var authServiceTester = new AuthServiceTester(authServiceClient, logger);
await authServiceTester.RunAllTests();

// Run registry service tests
var registryServiceTester = new RegistryServiceTester(registryServiceClient, authServiceClient, logger);
await registryServiceTester.RunAllTests();
