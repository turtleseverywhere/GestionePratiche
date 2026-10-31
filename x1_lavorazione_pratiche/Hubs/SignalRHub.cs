using x1_lavorazione_pratiche.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace x1_lavorazione_pratiche.Hubs
{
    [Authorize(Policy = ConstantValues.Auth.Claims.Types.CanRegisterToSignalR)]
    public class SignalRHub : Hub
    {
        public class ConnectedClient
        {
            public Guid ClientId { get; set; }
            /*questo UserID viene preso dal context tramite il token autenticativo,
             ovviamente non inviato esplicitamente dal client :)*/
            public string? UserID { get; set; }
            public string CurrentConnectionId { get; set; } = string.Empty;
            public string OldConnectionId { get; set; } = string.Empty;
        }
        public static List<ConnectedClient> ConnectedClients = new List<ConnectedClient>();

        public async Task RegisterClientConnection(string oldConnectionId)
        {
            ConnectedClient? lastConnection = ConnectedClients
               .Where(cc => cc.CurrentConnectionId == oldConnectionId).FirstOrDefault();
            if (lastConnection != null)
            {
                lastConnection.UserID = Context.UserIdentifier;
                lastConnection.OldConnectionId = lastConnection.CurrentConnectionId;
                lastConnection.CurrentConnectionId = Context.ConnectionId;
            }
            else
            {
                lastConnection = new ConnectedClient()
                {
                    UserID = Context.UserIdentifier,
                    ClientId = Guid.NewGuid(),
                    OldConnectionId = Context.ConnectionId,
                    CurrentConnectionId = Context.ConnectionId
                };
                SignalRHub.ConnectedClients.Add(lastConnection);
            }
            System.Diagnostics.Debug.WriteLine(lastConnection?.ClientId);
            System.Diagnostics.Debug.WriteLine(lastConnection?.UserID);
            System.Diagnostics.Debug.WriteLine(lastConnection?.OldConnectionId);
            System.Diagnostics.Debug.WriteLine(lastConnection?.CurrentConnectionId);
        }
        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            return base.OnDisconnectedAsync(exception);
        }
    }
}
