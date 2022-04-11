using ChatClient.Core.Events;
using ChatClient.Core.Service;
using ChatClient.ViewModel;
using System.Windows;

namespace ChatClient.Views
{
    public partial class StartView : Window
    {
        public StartView()
        {
            InitializeComponent();
            DataContext = new StartViewModel(HttpService.HttpServiceInstance);
            (DataContext as StartViewModel).OnSuccessfulConnect += ChangeWindow;
        }

        private void ChangeWindow(object sender, ConnectionEventArgs e)
        {
            MainView mainView = new MainView(e.MainViewModelContext);
            mainView.Show();
            (DataContext as StartViewModel).OnSuccessfulConnect -= ChangeWindow;
            Close();
        }
    }
}
