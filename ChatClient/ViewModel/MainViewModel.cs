using ChatClient.Core.Service;
using ChatClient.MVVM.Core;
using DataContract.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http;
using Newtonsoft.Json;
using System.Windows;
using System.Collections.Specialized;
using System.Threading;

namespace ChatClient.ViewModel
{
    public class MainViewModel : BindableBase
    {
        #region Fields
        private UserModel _currentUser;
        private ObservableCollection<UserModel> _users;
        private ObservableCollection<MessageModel> _messages;
        private CancellationTokenSource _heartbeatToken;
        private IHttpService _httpService;
        private bool _isDisconnecting;
        private string _currentMessage;
        private HubConnection _connection;
        public event EventHandler OnDisconnect;
        #endregion

        public MainViewModel(DataModel data, UserModel currentuser, HubConnection connection, IHttpService httpService)
        {
            _currentUser = currentuser;
            _users = data.Users;
            _messages = data.Messages;
            _connection = connection;
            _httpService = httpService;
            _heartbeatToken = new CancellationTokenSource();
            CreateHandlers();
            SendHeartBeat(_heartbeatToken.Token);
        }

        #region Commands
        public DelegateCommand Send => new DelegateCommand(SendMessage, CanSendMessage);
        public DelegateCommand Disconnect => new DelegateCommand(DisconnectFromServer);
        #endregion

        #region Properties
        public ObservableCollection<UserModel> Users
        {
            get => _users;
            set => SetProperty(ref _users, value);
        }
        public ObservableCollection<MessageModel> Messages
        {
            get => _messages;
            set => SetProperty(ref _messages, value);
        }

        public string CurrentMessage
        {
            get => _currentMessage;
            set => SetProperty(ref _currentMessage, value);
        }
        public bool IsDisconnecting
        {
            get => _isDisconnecting;
            set => SetProperty(ref _isDisconnecting, value);
        }
        #endregion

        #region Methods
        private bool CanSendMessage()
        {
            return string.IsNullOrEmpty(CurrentMessage) ? false : true;
        }
        private async Task SendMessage()
        {
            var messagetoSend = new MessageModel { Message = CurrentMessage, User = _currentUser };
            var jsonData = JsonConvert.SerializeObject(messagetoSend);
            try
            {
                await _httpService.PostData("/api/chat/PostMessage", jsonData);
                CurrentMessage = default;
            }
            catch (HttpRequestException)
            {
                Messages.Add(new MessageModel
                {
                    Message = "Could not send message.",
                    User = new UserModel
                    {
                        Name = "System"
                    }
                });
            }
        }

        private async Task SendHeartBeat(CancellationToken token)
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                    break;

                await Task.Delay(2000);
                try
                {
                    var response = await _httpService.GetData("api/chat/GetHeartBeat");
                }
                catch (HttpRequestException)
                {
                    await DisconnectFromServer();
                }
            }
        }

        private void CreateHandlers()
        {
            _connection.On<DataModel>("ReceiveData", ReceiveData);
        }

        private void ReceiveData(DataModel data)
        {
            if (data.Users.Count != _users.Count)
            {
                Users = data.Users;
            }
            else if (data.Messages.Count != _messages.Count)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Messages.Add(data.Messages.LastOrDefault());
                });
            }
        }
        private async Task DisconnectFromServer()
        {
            IsDisconnecting = true;
            _connection.Remove("ReceiveData");
            _heartbeatToken.Cancel();
            await _connection.DisposeAsync();
            OnDisconnect?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
}
