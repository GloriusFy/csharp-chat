using DataContract.Model;
using Microsoft.AspNetCore.SignalR;

namespace ChatService.Hubs
{
    public class ChatHub : Hub
    {
        private static DataModel _usersAndMessages = new DataModel();
        private static List<string> _connections = new List<string>();
        private void SendMessages(IHubContext<ChatHub> hub)
        {
            hub.Clients.All.SendAsync("ReceiveData", _usersAndMessages);
        }
        public void AddUserData(UserModel data)
        {
            _usersAndMessages.Users.Add(data);
        }
        public void AddMessageData(MessageModel data, IHubContext<ChatHub> hub)
        {
            _usersAndMessages.Messages.Add(data);
            SendMessages(hub);
            
        }
        public override Task OnConnectedAsync()
        {
            Clients.Caller.SendAsync("Connected", _usersAndMessages);
            Clients.Others.SendAsync("ReceiveData", _usersAndMessages);
            _connections.Add(Context.ConnectionId);
            return base.OnConnectedAsync();
        }
        public override Task OnDisconnectedAsync(Exception exception)
        {
            var disconnectedConnection = Context.ConnectionId;
            var index = _connections.IndexOf(disconnectedConnection);
            _connections.RemoveAt(index);
            _usersAndMessages.Users.RemoveAt(index);
            Clients.All.SendAsync("ReceiveData", _usersAndMessages);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
