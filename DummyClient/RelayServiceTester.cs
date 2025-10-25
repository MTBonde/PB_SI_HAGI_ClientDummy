using System.Net.WebSockets;

namespace DummyClient;

public class RelayServiceTester
{
    private readonly RelayServiceClient relayServiceClient;
    private readonly AuthServiceClient authServiceClient;
    private readonly TestResultLogger logger;

    public RelayServiceTester(
        RelayServiceClient relayServiceClient,
        AuthServiceClient authServiceClient,
        TestResultLogger logger)
    {
        this.relayServiceClient = relayServiceClient;
        this.authServiceClient = authServiceClient;
        this.logger = logger;
    }

    public async Task RunAllTests()
    {
        logger.PrintSectionHeader("Testing RelayService WebSocket Connection");

        await TestWebSocketConnectionWithoutJwt();
        await TestWebSocketConnectionWithJwt();
    }

    private async Task TestWebSocketConnectionWithoutJwt()
    {
        try
        {
            // Arrange
            var testName = "Test 8: WebSocket connection WITHOUT JWT token";
            var emptyToken = "";

            logger.PrintTestHeader(
                testName,
                relayServiceClient.BuildWebSocketEndpointUrl(emptyToken),
                "No token",
                "",
                "should fail with connection error");

            // Act
            var connectionResult = await relayServiceClient.ConnectToRelayServiceWebSocket(emptyToken);

            // Assert
            if (!connectionResult.IsConnected)
            {
                logger.PrintTestPassed(testName, "WebSocket connection rejected without JWT as expected");
                Console.WriteLine($"WebSocket State: {connectionResult.State}");
                Console.WriteLine($"Message: {connectionResult.Message}");

                if (!string.IsNullOrEmpty(connectionResult.ErrorMessage))
                {
                    Console.WriteLine($"Error: {connectionResult.ErrorMessage}");
                }

                Console.WriteLine("========================================");
            }
            else
            {
                Console.WriteLine($"{testName}: UNEXPECTED RESULT - Connection should have been rejected");
                Console.WriteLine($"WebSocket State: {connectionResult.State}");
                Console.WriteLine($"Message: {connectionResult.Message}");
                Console.WriteLine("========================================");

                await relayServiceClient.DisconnectFromWebSocket();
            }

            logger.PrintBlankLine();
        }
        catch (Exception ex)
        {
            logger.PrintTestConnectionError("Test 8", ex.Message, shouldExitApplication: false);
        }
    }

    private async Task TestWebSocketConnectionWithJwt()
    {
        try
        {
            // Arrange
            var testName = "Test 9: WebSocket connection WITH JWT token and receive welcome message";

            // Login first to get JWT token
            var loginResponse = await authServiceClient.SendLoginRequest("admin", "admin");

            if (!loginResponse.IsSuccessStatusCode || !loginResponse.HasValidToken)
            {
                Console.WriteLine($"{testName}: FAILED - Could not obtain JWT token from AuthService");
                logger.PrintBlankLine();
                return;
            }

            var jwtToken = loginResponse.AuthenticationData!.Token;

            logger.PrintTestHeader(
                testName,
                relayServiceClient.BuildWebSocketEndpointUrl("***TOKEN***"),
                $"Authorization via query param",
                $"Token length: {jwtToken.Length}",
                "should connect and receive 'Welcome to WEBSOCKET!' message");

            // Act - Connect
            var connectionResult = await relayServiceClient.ConnectToRelayServiceWebSocket(jwtToken);

            // Assert - Connection
            if (!connectionResult.IsConnected)
            {
                logger.PrintTestFailedLoginRejected(testName, System.Net.HttpStatusCode.Unauthorized,
                    connectionResult.ErrorMessage ?? "Connection failed");
                logger.PrintBlankLine();
                return;
            }

            Console.WriteLine($"{testName}: WebSocket connection established");
            Console.WriteLine($"WebSocket State: {connectionResult.State}");
            Console.WriteLine();

            // Act - Receive welcome message
            Console.WriteLine("Waiting for welcome message from server...");
            var messageResult = await relayServiceClient.ReceiveMessageFromWebSocket(timeoutMilliseconds: 10000);

            // Assert - Welcome message
            if (messageResult.Success)
            {
                var expectedWelcomeMessage = "Welcome to WEBSOCKET!";

                if (messageResult.Message == expectedWelcomeMessage)
                {
                    logger.PrintTestPassed(testName, "WebSocket connected and welcome message received");
                    Console.WriteLine($"Message Type: {messageResult.MessageType}");
                    Console.WriteLine($"Received Message: '{messageResult.Message}'");
                    Console.WriteLine($"Expected Message: '{expectedWelcomeMessage}'");
                    Console.WriteLine($"Match: YES");
                }
                else
                {
                    Console.WriteLine($"{testName}: PARTIAL SUCCESS - Connected but unexpected message");
                    Console.WriteLine($"Expected: '{expectedWelcomeMessage}'");
                    Console.WriteLine($"Received: '{messageResult.Message}'");
                }

                Console.WriteLine("========================================");
            }
            else
            {
                Console.WriteLine($"{testName}: PARTIAL SUCCESS - Connected but no message received");
                Console.WriteLine($"Error: {messageResult.ErrorMessage}");
                Console.WriteLine("========================================");
            }

            // Cleanup - Disconnect
            await relayServiceClient.DisconnectFromWebSocket();
            Console.WriteLine("WebSocket connection closed gracefully");
            Console.WriteLine("========================================");

            logger.PrintBlankLine();
        }
        catch (Exception ex)
        {
            logger.PrintTestConnectionError("Test 9", ex.Message, shouldExitApplication: false);

            try
            {
                await relayServiceClient.DisconnectFromWebSocket();
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
