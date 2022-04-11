using ChatClient.MVVM.Core;
using System;
using System.Threading;
using System.Windows.Threading;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using ChatClient.Core.Service;
using ChatClient.Core.Events;
using DataContract.Model;

namespace ChatClient.ViewModel
{
    internal class StartViewModel : BindableBase
    {
        #region Fields
        private bool _isHosting;
        private bool _isConnecting = false;
        private string _name;
        private string _status;
        private string _ipAddress;
        private ConnectionType _selectedconnectionType/* = ConnectionType.External*/; /// 
        private HubConnection connection;
        private UserModel _currentUser;
        private IHttpService _httpService;
        public EventHandler<ConnectionEventArgs> OnSuccessfulConnect;
        #endregion

        public StartViewModel(IHttpService httpSservice)
        {
            _httpService = httpSservice;
        }

        #region Properties
        public string Name
        {
            get => _name;
            set => _name = value;
        }
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public bool IsConnecting
        {
            get => _isConnecting;
            set => SetProperty(ref _isConnecting, value);
        }

        public ConnectionType CurrentConnectionType
        {
            get => _selectedconnectionType;
            set => _selectedconnectionType = value;
        }
        public string CurrentIPAddress
        {
            get => _ipAddress;
            set => SetProperty(ref _ipAddress, value);
        }

        public Array ConnectionTypes { get; } = Enum.GetValues(typeof(ConnectionType));
        #endregion

        #region Commands
        public DelegateCommand Host => new DelegateCommand(HostServer, CanHostServer);
        public DelegateCommand Connect => new DelegateCommand(ConnectToServer, CanConnectToServer);
        #endregion

        #region Methods
        private bool CanHostServer()
        {
            return string.IsNullOrEmpty(_name) || _isHosting || _isConnecting ? false : true;
        }
        private bool CanConnectToServer()
        {
            if (CurrentConnectionType == ConnectionType.External && string.IsNullOrEmpty(CurrentIPAddress))
                return false;

            return string.IsNullOrEmpty(_name) || _isConnecting || _isHosting ? false : true;
        }
        private async Task ConnectToServer()
        {
            try
            {
                await Task.Run(async () =>
                {
                    _ = LogStatus("Connecting...");
                    await SendUser(_currentUser = new UserModel
                    {
                        Name = this.Name,
                        EndPoint = CurrentIPAddress
                    }, CurrentConnectionType);

                    if (CurrentConnectionType == ConnectionType.External)
                    {
                        connection = new HubConnectionBuilder()
                        .WithUrl($"http://{CurrentIPAddress}:5001/chathub")
                        .Build();
                    }
                    else
                    {
                        connection = new HubConnectionBuilder()
                        .WithUrl("http://localhost:5001/chathub")
                        .Build();
                    }

                    CreateHandlers();
                    await connection.StartAsync();
                });
            }
            catch (HttpRequestException)
            {
                _ = LogStatus($"Could not connect to the server.");
                IsConnecting = false;
            }
            catch (TaskCanceledException)
            {
                _ = LogStatus($"Could not connect to the server.");
                IsConnecting = false;
            }
            catch (FormatException y)
            {
                _ = LogStatus(y.Message);
                IsConnecting = false;
            }
        }

        private async Task HostServer()
        {
            try
            {
                await Task.Run(() =>
                {
                    if (IsServerProcessRunning())
                    {
                        _ = LogStatus("A server process is already running!");
                        return;
                    }
                    var server = GetServerProcess();
                    if (server == null)
                    {
                        _ = LogStatus("Could not find the server process!");
                        return;
                    }
                    server.Start();
                    _isHosting = true;
                    ConnectToServer();
                });
            }
            catch (IOException x)
            {
                _ = LogStatus(x.Message);
            }
            catch (UnauthorizedAccessException y)
            {
                _ = LogStatus(y.Message);
            }
            catch (InvalidOperationException z)
            {
                _ = LogStatus(z.Message);
            }
        }

        private Process GetServerProcess()
        {
            Process process = new Process();
            ProcessStartInfo processInfo = new ProcessStartInfo();
            process.StartInfo = processInfo;
            string[] files = Directory.GetFiles(Directory.GetCurrentDirectory(),
            "ChatService.exe", SearchOption.AllDirectories);
            if (files.Count() == 0)
            {
                return null;
            }
            processInfo.FileName = files[0];
            process.StartInfo = processInfo;
            return process;
        }

        private bool IsServerProcessRunning()
        {
            var serverProcess = Process.GetProcessesByName("ChatService");
            if (serverProcess.Count() > 0)
            {
                return true;
            }
            return false;
        }
        private void CreateHandlers()
        {
            connection.On<DataModel>("Connected", (data) =>
            {
               Application.Current.Dispatcher.Invoke(() =>
               {
                   OnSuccessfulConnect?.Invoke(this, new ConnectionEventArgs
                   {
                       MainViewModelContext = new MainViewModel(data, _currentUser, connection, _httpService)
                   });
               });
                connection.Remove("Connected");
            });
        }
        private async Task SendUser(UserModel user, ConnectionType connectionType)
        {
            IsConnecting = true;
            if (connectionType == ConnectionType.External)
            {
                var isSuccessful = IPAddress.TryParse(CurrentIPAddress, out _);
                if (!isSuccessful)
                {
                    throw new FormatException("IP Address formatting Incorrect!");
                }
                _httpService.SetEndPoint(new Uri($"http://{CurrentIPAddress}:5001"));
            }
            else
            {
                _httpService.SetEndPoint(new Uri("http://localhost:5001"));
            }
            var jsonData = JsonConvert.SerializeObject(user);
            await _httpService.PostData("/api/chat/PostUser", jsonData);
        }

        private async Task LogStatus(string message)
        {
            Status = message;
            await Task.Delay(2000);
            Status = default;
        }
#endregion

        public enum ConnectionType
        {
            Internal,
            External,
        }
    }
}
