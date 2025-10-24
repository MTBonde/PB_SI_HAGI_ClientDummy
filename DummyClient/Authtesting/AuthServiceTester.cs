using System.Net;

namespace DummyClient;

public class AuthServiceTester
{
    private readonly AuthServiceClient authServiceClient;
    private readonly TestResultLogger logger;

    public AuthServiceTester(AuthServiceClient authServiceClient, TestResultLogger logger)
    {
        this.authServiceClient = authServiceClient;
        this.logger = logger;
    }

    public async Task RunAllTests()
    {
        logger.PrintSectionHeader("Testing AuthService Connection");

        await TestInvalidCredentialsAreRejected();
        await TestValidCredentialsReturnToken();
    }

    private async Task TestInvalidCredentialsAreRejected()
    {
        try
        {
            // Arrange
            var testName = "Test 1: Testing with invalid credentials";
            var username = "test";
            var password = "test";
            var expectedStatusCode = HttpStatusCode.Unauthorized;

            logger.PrintTestHeader(
                testName,
                authServiceClient.BuildLoginEndpointUrl(),
                username,
                password,
                "should fail");

            // Act
            var response = await authServiceClient.SendLoginRequest(username, password);

            // Assert
            if (response.StatusCode == expectedStatusCode)
            {
                logger.PrintTestPassed(testName, "Invalid credentials rejected as expected");
                logger.PrintTestDetails(response.StatusCode, response.ResponseBody);
            }
            else
            {
                logger.PrintTestUnexpectedResult(testName, expectedStatusCode, response.StatusCode, response.ResponseBody);
            }

            logger.PrintBlankLine();
        }
        catch (Exception ex)
        {
            logger.PrintTestConnectionError("Test 1", ex.Message, shouldExitApplication: true);
        }
    }

    private async Task TestValidCredentialsReturnToken()
    {
        try
        {
            // Arrange
            var testName = "Test 2: Testing with valid credentials";
            var username = "admin";
            var password = "admin";

            logger.PrintTestHeader(
                testName,
                authServiceClient.BuildLoginEndpointUrl(),
                username,
                password,
                "should succeed");

            // Act
            var response = await authServiceClient.SendLoginRequest(username, password);

            // Assert
            if (response.IsSuccessStatusCode)
            {
                if (response.HasValidToken)
                {
                    logger.PrintTestPassedWithToken(testName, response.StatusCode, response.AuthenticationData!.Token);
                }
                else
                {
                    logger.PrintTestFailedNoToken(testName, response.StatusCode, response.ResponseBody);
                }
            }
            else
            {
                logger.PrintTestFailedLoginRejected(testName, response.StatusCode, response.ResponseBody);
            }

            logger.PrintBlankLine();
        }
        catch (Exception ex)
        {
            logger.PrintTestConnectionError("Test 2", ex.Message, shouldExitApplication: true);
        }
    }
}
