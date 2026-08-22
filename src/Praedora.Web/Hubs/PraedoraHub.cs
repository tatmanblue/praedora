using Microsoft.AspNetCore.SignalR;

namespace Praedora.Web.Hubs;

// Pushes board + review queue updates to connected clients (design doc §10). Not yet mapped in
// Program.cs — SignalR push arrives in build sequence step 5, driven by SignalRNotifier.
public class PraedoraHub : Hub
{
}
