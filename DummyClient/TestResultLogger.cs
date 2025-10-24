using System.Net;

namespace DummyClient;

public class TestResultLogger
{
    public void PrintTestHeader(string testName, string endpointUrl, string username, string password, string expectedBehavior)
    {
        Console.WriteLine(testName);
        Console.WriteLine("========================================");
        Console.WriteLine($"POST {endpointUrl}");
        Console.WriteLine($"Credentials: {username}/{password} ({expectedBehavior})");
        Console.WriteLine();
    }

    public void PrintTestPassed(string testName, string reason)
    {
        Console.WriteLine($"{testName}: PASSED - {reason}");
        Console.WriteLine("========================================");
    }

    public void PrintTestDetails(HttpStatusCode statusCode, string responseBody)
    {
        Console.WriteLine($"Status Code: {statusCode}");
        Console.WriteLine($"Response: {responseBody}");
        Console.WriteLine("========================================");
    }

    public void PrintTestUnexpectedResult(string testName, HttpStatusCode expectedStatusCode, HttpStatusCode actualStatusCode, string responseBody)
    {
        Console.WriteLine($"{testName}: UNEXPECTED RESULT");
        Console.WriteLine("========================================");
        Console.WriteLine($"Expected: {expectedStatusCode}");
        Console.WriteLine($"Actual: {actualStatusCode}");
        Console.WriteLine($"Response: {responseBody}");
        Console.WriteLine("========================================");
    }

    public void PrintTestPassedWithToken(string testName, HttpStatusCode statusCode, string token)
    {
        Console.WriteLine($"{testName}: PASSED - LOGIN SUCCESS");
        Console.WriteLine("========================================");
        Console.WriteLine($"Status Code: {statusCode}");
        Console.WriteLine($"Token Length: {token.Length} characters");
        Console.WriteLine($"Token Preview: {token.Substring(0, Math.Min(20, token.Length))}...");
        Console.WriteLine("========================================");
    }

    public void PrintTestFailedNoToken(string testName, HttpStatusCode statusCode, string responseBody)
    {
        Console.WriteLine($"{testName}: FAILED - No token in response");
        Console.WriteLine("========================================");
        Console.WriteLine($"Status Code: {statusCode}");
        Console.WriteLine($"Response Body: {responseBody}");
        Console.WriteLine("========================================");
    }

    public void PrintTestFailedLoginRejected(string testName, HttpStatusCode statusCode, string responseBody)
    {
        Console.WriteLine($"{testName}: FAILED - Login rejected");
        Console.WriteLine("========================================");
        Console.WriteLine($"Status Code: {statusCode}");
        Console.WriteLine($"Response Body: {responseBody}");
        Console.WriteLine("========================================");
    }

    public void PrintTestConnectionError(string testName, string errorMessage, bool shouldExitApplication)
    {
        Console.WriteLine();
        Console.WriteLine($"{testName}: FAILED - Connection Error");
        Console.WriteLine("========================================");
        Console.WriteLine($"Error: {errorMessage}");
        Console.WriteLine("Did u forget to start docker?");
        Console.WriteLine("========================================");

        if (shouldExitApplication)
        {
            Environment.Exit(1);
        }
    }

    public void PrintSectionHeader(string sectionTitle)
    {
        Console.WriteLine(sectionTitle);
        Console.WriteLine("========================================");
        Console.WriteLine();
    }

    public void PrintBlankLine()
    {
        Console.WriteLine();
    }
}
