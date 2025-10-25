using System.Net.WebSockets;
using System.Text;

namespace DummyClient;

public class RelayServiceClient : IDisposable
{
    private readonly string baseUrl;
    private ClientWebSocket? webSocketClient;
    private CancellationTokenSource? cancellationTokenSource;

    public RelayServiceClient(string baseUrl)
    {
        this.baseUrl = baseUrl;
    }

    public async Task<WebSocketConnectionResult> ConnectToRelayServiceWebSocket(string jwtToken)
    {
        try
        {
            webSocketClient = new ClientWebSocket();
            cancellationTokenSource = new CancellationTokenSource();

            var webSocketUrl = BuildWebSocketEndpointUrl(jwtToken);

            await webSocketClient.ConnectAsync(new Uri(webSocketUrl), cancellationTokenSource.Token);

            if (webSocketClient.State == WebSocketState.Open)
            {
                return new WebSocketConnectionResult
                {
                    IsConnected = true,
                    State = webSocketClient.State,
                    Message = "WebSocket connected successfully"
                };
            }

            return new WebSocketConnectionResult
            {
                IsConnected = false,
                State = webSocketClient.State,
                Message = $"WebSocket connection failed. State: {webSocketClient.State}"
            };
        }
        catch (Exception ex)
        {
            return new WebSocketConnectionResult
            {
                IsConnected = false,
                State = webSocketClient?.State ?? WebSocketState.None,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<WebSocketMessageResult> ReceiveMessageFromWebSocket(int timeoutMilliseconds = 5000)
    {
        if (webSocketClient == null || webSocketClient.State != WebSocketState.Open)
        {
            return new WebSocketMessageResult
            {
                Success = false,
                ErrorMessage = "WebSocket is not connected"
            };
        }

        try
        {
            var buffer = new byte[4096];
            var cancellationToken = new CancellationTokenSource(timeoutMilliseconds).Token;

            var result = await webSocketClient.ReceiveAsync(
                new ArraySegment<byte>(buffer),
                cancellationToken);

            if (result.MessageType == WebSocketMessageType.Text)
            {
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                return new WebSocketMessageResult
                {
                    Success = true,
                    Message = message,
                    MessageType = result.MessageType
                };
            }

            if (result.MessageType == WebSocketMessageType.Close)
            {
                return new WebSocketMessageResult
                {
                    Success = false,
                    ErrorMessage = "WebSocket connection closed by server",
                    MessageType = result.MessageType
                };
            }

            return new WebSocketMessageResult
            {
                Success = false,
                ErrorMessage = $"Unexpected message type: {result.MessageType}",
                MessageType = result.MessageType
            };
        }
        catch (OperationCanceledException)
        {
            return new WebSocketMessageResult
            {
                Success = false,
                ErrorMessage = $"Timeout: No message received within {timeoutMilliseconds}ms"
            };
        }
        catch (Exception ex)
        {
            return new WebSocketMessageResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task SendMessageToWebSocket(string message)
    {
        if (webSocketClient == null || webSocketClient.State != WebSocketState.Open)
        {
            throw new InvalidOperationException("WebSocket is not connected");
        }

        var messageBytes = Encoding.UTF8.GetBytes(message);
        await webSocketClient.SendAsync(
            new ArraySegment<byte>(messageBytes),
            WebSocketMessageType.Text,
            true,
            cancellationTokenSource?.Token ?? CancellationToken.None);
    }

    public async Task DisconnectFromWebSocket()
    {
        if (webSocketClient != null && webSocketClient.State == WebSocketState.Open)
        {
            await webSocketClient.CloseAsync(
                WebSocketCloseStatus.NormalClosure,
                "Client closing connection",
                CancellationToken.None);
        }
    }

    public string BuildWebSocketEndpointUrl(string jwtToken)
    {
        var wsBaseUrl = baseUrl.Replace("http://", "ws://").Replace("https://", "wss://");
        return $"{wsBaseUrl}/ws?token={jwtToken}";
    }

    public void Dispose()
    {
        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
        webSocketClient?.Dispose();
    }
}

public class WebSocketConnectionResult
{
    public bool IsConnected { get; init; }
    public WebSocketState State { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? ErrorMessage { get; init; }
}

public class WebSocketMessageResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? ErrorMessage { get; init; }
    public WebSocketMessageType? MessageType { get; init; }
}
