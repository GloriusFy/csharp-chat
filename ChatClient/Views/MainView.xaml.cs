using ChatClient.ViewModel;
using System;
using System.Windows;

namespace ChatClient.Views
{
    public partial class MainView : Window
    {
        public MainView(MainViewModel context)
        {
            InitializeComponent();
            DataContext = context;
            (DataContext as MainViewModel).OnDisconnect += ChangeToHomeWindow;
        }

        private void ChangeToHomeWindow(object sender, EventArgs e)
        {
            StartView startView = new StartView();
            startView.Show();
            (DataContext as MainViewModel).OnDisconnect -= ChangeToHomeWindow;
            Close();
        }
    }
}
