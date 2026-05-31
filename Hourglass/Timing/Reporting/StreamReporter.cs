using System.Collections.Generic;
using Fleck;

namespace Hourglass.Timing.Reporting;

public static class StreamReporter
{
    private const string TargetWebSocket = "127.0.0.1:8181";

    private static WebSocketServer? _server;

    private static readonly List<IWebSocketConnection> Sockets = [];

    public static void Initialize()
    {
        if (_server is not null)
        {
            return;
        }

        _server = new WebSocketServer($"ws://{TargetWebSocket}");
        _server.Start(connection =>
        {
            connection.OnOpen = () => OnOpen(connection);
            connection.OnClose = () => OnClose(connection);
        });
    }

    private static void OnClose(IWebSocketConnection connection) => Sockets.Remove(connection);

    private static void OnOpen(IWebSocketConnection connection) => Sockets.Add(connection);

    public static void BroadcastTimerState(string? title, string? timeLeft, double? timeLeftAsPercent, string state)
    {
        if (Sockets.Count == 0)
        {
            return;
        }

        string percentString = timeLeftAsPercent?.ToString("0.00") ?? "0";

        // Handle potential nulls in timeLeft
        string safeTimeLeft = timeLeft ?? "00:00";

        string jsonPayLoad = $"{{\"title\": \"{title}\", \"timeLeft\": \"{safeTimeLeft}\", \"timeLeftAsPercent\": \"{percentString}\", \"state\": \"{state}\"}}";
        foreach (var socket in Sockets)
        {
            // Fleck handles async sending under the hood
            socket.Send(jsonPayLoad);
        }
    }
}