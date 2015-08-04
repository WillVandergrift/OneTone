using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using OneTone.Resources;
using Microsoft.Live;

namespace OneTone
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        /// <summary>
        /// Handle user authentication
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSignin_SessionChanged(object sender, Microsoft.Live.Controls.LiveConnectSessionChangedEventArgs e)
        {
            if (e.Status == LiveConnectSessionStatus.Connected)
            {
                Session.CurSession = e.Session;
                NavigationService.Navigate(new Uri("/BrowsePage.xaml", UriKind.Relative));
            }
            else
            {
            }
        }

        /// <summary>
        /// Check to see if the app was started from a peer connection
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Open the receive tone page
            if (e.Uri.OriginalString.Contains("PeerFinder"))
            {
                NavigationService.Navigate(new Uri("/ReceiveTone.xaml", UriKind.Relative));
            }
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            //Session already exists, go to browser page
            if (Session.CurSession != null)
            {
                NavigationService.Navigate(new Uri("/BrowsePage.xaml", UriKind.Relative));
            }
        }
    }
}