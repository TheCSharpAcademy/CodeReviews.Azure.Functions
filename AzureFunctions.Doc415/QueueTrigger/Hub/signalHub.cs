using Microsoft.AspNetCore.SignalR;


namespace QueueTrigger.SignalHub;

internal class NotificationHub:Hub
{
    public async Task SendMessage(string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", message);
    }
}
