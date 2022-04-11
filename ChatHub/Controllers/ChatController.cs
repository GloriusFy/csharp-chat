using DataContract.Model;
using ChatService.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ChatService.Controllers
{
    [Route("api/chat")]
    [ApiController]
    public class ChatController
    {
        private ChatHub _chathub;
        private IHubContext<ChatHub> _hubContext;
        public ChatController(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
            _chathub = new ChatHub();
        }

        [HttpPost]
        [Route("PostUser")]
        public void PostUser(UserModel user)
        {
            _chathub.AddUserData(user);
        }

        [HttpPost]
        [Route("PostMessage")]
        public void AddMessage(MessageModel message)
        {
            _chathub.AddMessageData(message, _hubContext);
        }

        [HttpGet]
        [Route("GetHeartBeat")]
        public string GetHeartBeat()
        {
            return "Alive";
        }
    }
}
