using sistema_gestione_pratiche.Hubs;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;
using static sistema_gestione_pratiche.Hubs.SignalRHub;

namespace sistema_gestione_pratiche.Services
{
    public class SignalRService
    {
        IHubContext<SignalRHub> signalRHubContext;
        public SignalRService(IHubContext<SignalRHub> signalRHubContext)
        {
            this.signalRHubContext = signalRHubContext;
        }

        public async Task SendMessageToBroadcast(object message)
        {
            System.Diagnostics.Debug.WriteLine("SendMessage args: " + message);
            await this.signalRHubContext.Clients.All.SendAsync("ReceiveMessage", message);
        }
        public async Task SendMessageToClient(string clientId, object message)
        {
            System.Diagnostics.Debug.WriteLine("SendMessageToClient args: " + clientId + "; message: " + message);
            ConnectedClient? connectedClient = SignalRHub.ConnectedClients.Where(cc => cc.ClientId.ToString() == clientId).FirstOrDefault();
            if (connectedClient != null)
            {
                await this.signalRHubContext.Clients.Clients(connectedClient.CurrentConnectionId).SendAsync("ReceiveMessage", clientId + " " + message);
            }
        }
        public async Task SendMessageToConnection(string connectionId, object message)
        {
            System.Diagnostics.Debug.WriteLine("SendMessageToConnection args: " + connectionId + "; message: " + message);
            await this.signalRHubContext.Clients.Clients(connectionId).SendAsync("ReceiveMessage", connectionId + " " + message);
        }

        public async Task SendMessageToUser(string userId, object message)
        {
            List<ConnectedClient> connectedClients = SignalRHub.ConnectedClients.Where(cc => cc.UserID == userId).ToList();
            foreach (ConnectedClient connectedClient in connectedClients)
            {
                try
                {
                    await this.signalRHubContext.Clients.Clients(connectedClient.CurrentConnectionId)
                        .SendAsync("ReceiveMessage", message);
                }
                catch (Exception ex)
                {

                }
            }
        }
    }
}