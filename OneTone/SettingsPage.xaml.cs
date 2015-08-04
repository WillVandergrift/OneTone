using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Live;

namespace OneTone
{
    public partial class SettingsPage : PhoneApplicationPage
    {
        public SettingsPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handle user authentication
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSignin_SessionChanged(object sender, Microsoft.Live.Controls.LiveConnectSessionChangedEventArgs e)
        {
            if (e.Status != LiveConnectSessionStatus.Connected)
            {
                Session.CurSession = e.Session;
                NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                NavigationService.RemoveBackEntry();
            }
            else
            {
            }
        }
    }
}