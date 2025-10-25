using System.Net;

namespace DummyClient;

public class RegistryServiceTester
{
    private readonly RegistryServiceClient registryServiceClient;
    private readonly AuthServiceClient authServiceClient;
    private readonly TestResultLogger logger;

    public RegistryServiceTester(
        RegistryServiceClient registryServiceClient,
        AuthServiceClient authServiceClient,
        TestResultLogger logger)
    {
        this.registryServiceClient = registryServiceClient;
        this.authServiceClient = authServiceClient;
        this.logger = logger;
    }

    public async Task RunAllTests()
    {
        logger.PrintSectionHeader("Testing RegistryService Connection");

        await TestRegisterGameServer();
        await TestSendHeartbeat();
        await TestAllocateServerWithoutJwt();
        // await TestAllocateServerWithJwt();
        // await TestDisconnectFromServer();
    }

    private async Task TestRegisterGameServer()
    {
        try
        {
            // Arrange
            var testName = "Test 3: Register game server";
            var serverId = "test-server-1";
            var host = "game";
            var port = 7777;
            var maxPlayers = 10;

            logger.PrintTestHeader(
                testName,
                registryServiceClient.BuildRegisterEndpointUrl(),
                $"serverId={serverId}",
                $"host={host}, port={port}, maxPlayers={maxPlayers}",
                "should succeed");

            // Act
            var response = await registryServiceClient.RegisterGameServer(serverId, host, port, maxPlayers);

            // Assert
            if (response.IsSuccessStatusCode && response.Data != null)
            {
                logger.PrintTestPassed(testName, "Game server registered successfully");
                Console.WriteLine($"Status Code: {response.StatusCode}");
                Console.WriteLine($"Server ID: {response.Data.ServerId}");
                Console.WriteLine($"Heartbeat Interval: {response.Data.HeartbeatInterval}s");
                Console.WriteLine($"Response: {response.ResponseBody}");
                Console.WriteLine("========================================");
            }
            else
            {
                logger.PrintTestFailedLoginRejected(testName, response.StatusCode, response.ResponseBody);
            }

            logger.PrintBlankLine();
        }
        catch (Exception ex)
        {
            logger.PrintTestConnectionError("Test 3", ex.Message, shouldExitApplication: false);
        }
    }

    private async Task TestSendHeartbeat()
    {
        try
        {
            // Arrange
            var testName = "Test 4: Send heartbeat";
            var serverId = "test-server-1";
            var currentPlayers = 5;

            logger.PrintTestHeader(
                testName,
                registryServiceClient.BuildHeartbeatEndpointUrl(),
                $"serverId={serverId}",
                $"currentPlayers={currentPlayers}",
                "should succeed");

            // Act
            var response = await registryServiceClient.SendHeartbeat(serverId, currentPlayers);

            // Assert
            if (response.IsSuccessStatusCode && response.Data != null)
            {
                logger.PrintTestPassed(testName, "Heartbeat received successfully");
                Console.WriteLine($"Status Code: {response.StatusCode}");
                Console.WriteLine($"Message: {response.Data.Message}");
                Console.WriteLine($"Response: {response.ResponseBody}");
                Console.WriteLine("========================================");
            }
            else
            {
                logger.PrintTestFailedLoginRejected(testName, response.StatusCode, response.ResponseBody);
            }

            logger.PrintBlankLine();
        }
        catch (Exception ex)
        {
            logger.PrintTestConnectionError("Test 4", ex.Message, shouldExitApplication: false);
        }
    }

    private async Task TestAllocateServerWithoutJwt()
    {
        try
        {
            // Arrange
            var testName = "Test 5: Allocate server WITHOUT JWT token";
            var expectedStatusCode = HttpStatusCode.Unauthorized;

            logger.PrintTestHeader(
                testName,
                registryServiceClient.BuildAllocateEndpointUrl(),
                "No Authorization header",
                "",
                "should fail with 401");

            // Act
            var response = await registryServiceClient.AllocateServer(jwtToken: null);

            // Assert
            if (response.StatusCode == expectedStatusCode)
            {
                logger.PrintTestPassed(testName, "Allocation rejected without JWT as expected");
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
            logger.PrintTestConnectionError("Test 5", ex.Message, shouldExitApplication: false);
        }
    }

    // private async Task TestAllocateServerWithJwt()
    // {
    //     try
    //     {
    //         // Arrange
    //         var testName = "Test 6: Allocate server WITH JWT token";
    //
    //         // Login first to get JWT token
    //         var loginResponse = await authServiceClient.SendLoginRequest("admin", "admin");
    //
    //         if (!loginResponse.IsSuccessStatusCode || !loginResponse.HasValidToken)
    //         {
    //             Console.WriteLine($"{testName}: FAILED - Could not obtain JWT token from AuthService");
    //             logger.PrintBlankLine();
    //             return;
    //         }
    //
    //         var jwtToken = loginResponse.AuthenticationData!.Token;
    //
    //         logger.PrintTestHeader(
    //             testName,
    //             registryServiceClient.BuildAllocateEndpointUrl(),
    //             $"Authorization: Bearer {jwtToken.Substring(0, Math.Min(20, jwtToken.Length))}...",
    //             "",
    //             "should succeed");
    //
    //         // Act
    //         var response = await registryServiceClient.AllocateServer(jwtToken);
    //
    //         // Assert
    //         if (response.IsSuccessStatusCode && response.Data != null)
    //         {
    //             logger.PrintTestPassed(testName, "Server allocated successfully");
    //             Console.WriteLine($"Status Code: {response.StatusCode}");
    //             Console.WriteLine($"Allocated Server ID: {response.Data.ServerId}");
    //             Console.WriteLine($"Host: {response.Data.Host}");
    //             Console.WriteLine($"Port: {response.Data.Port}");
    //             Console.WriteLine($"Message: {response.Data.Message}");
    //             Console.WriteLine($"Full Response: {response.ResponseBody}");
    //             Console.WriteLine("========================================");
    //         }
    //         else
    //         {
    //             logger.PrintTestFailedLoginRejected(testName, response.StatusCode, response.ResponseBody);
    //         }
    //
    //         logger.PrintBlankLine();
    //     }
    //     catch (Exception ex)
    //     {
    //         logger.PrintTestConnectionError("Test 6", ex.Message, shouldExitApplication: false);
    //     }
    // }
    //
    // private async Task TestDisconnectFromServer()
    // {
    //     try
    //     {
    //         // Arrange
    //         var testName = "Test 7: Disconnect from server";
    //         var serverId = "test-server-1";
    //
    //         // Login first to get JWT token (disconnect requires authentication)
    //         var loginResponse = await authServiceClient.SendLoginRequest("admin", "admin");
    //
    //         if (!loginResponse.IsSuccessStatusCode || !loginResponse.HasValidToken)
    //         {
    //             Console.WriteLine($"{testName}: FAILED - Could not obtain JWT token from AuthService");
    //             logger.PrintBlankLine();
    //             return;
    //         }
    //
    //         var jwtToken = loginResponse.AuthenticationData!.Token;
    //
    //         logger.PrintTestHeader(
    //             testName,
    //             registryServiceClient.BuildDisconnectEndpointUrl(),
    //             $"serverId={serverId}",
    //             $"Authorization: Bearer {jwtToken.Substring(0, Math.Min(20, jwtToken.Length))}...",
    //             "should succeed");
    //
    //         // Act
    //         var response = await registryServiceClient.DisconnectFromServer(serverId, jwtToken);
    //
    //         // Assert
    //         if (response.IsSuccessStatusCode && response.Data != null)
    //         {
    //             logger.PrintTestPassed(testName, "Disconnected successfully");
    //             Console.WriteLine($"Status Code: {response.StatusCode}");
    //             Console.WriteLine($"Message: {response.Data.Message}");
    //             Console.WriteLine($"Response: {response.ResponseBody}");
    //             Console.WriteLine("========================================");
    //         }
    //         else
    //         {
    //             logger.PrintTestFailedLoginRejected(testName, response.StatusCode, response.ResponseBody);
    //         }
    //
    //         logger.PrintBlankLine();
    //     }
    //     catch (Exception ex)
    //     {
    //         logger.PrintTestConnectionError("Test 7", ex.Message, shouldExitApplication: false);
    //     }
    // }
}
