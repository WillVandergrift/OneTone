using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Windows.Networking.Proximity;
using Windows.Networking.Sockets;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using OneTone.Helper;

namespace OneTone
{
    public partial class ReceiveTone : PhoneApplicationPage
    {
        //Has a connection been started
        bool started = false;

        //Socket used to transfer files
        StreamSocket streamSocket;

        //NFC Communications
        NFCConnection conn;

        public ReceiveTone()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Navigated away from the page
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            //Close the proximity connection
            if (started)
            {
                // Detach the callback handler (there can only be one PeerConnectProgress handler).
                Windows.Networking.Proximity.PeerFinder.TriggeredConnectionStateChanged -= PeerFinder_TriggeredConnectionStateChanged;
                // Detach the incoming connection request event handler.
                Windows.Networking.Proximity.PeerFinder.ConnectionRequested -= PeerFinder_ConnectionRequested;
                Windows.Networking.Proximity.PeerFinder.Stop();
                //CloseSocket();
                started = false;
            }
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            //Prevent the user from going back to the intro page
            NavigationService.RemoveBackEntry();

            //Initialize proximity communication
            AdvertiseConnection();
        }

        void AdvertiseConnection()
        {
            //Connection has already been started
            if (started) return;

            PeerFinder.TriggeredConnectionStateChanged += PeerFinder_TriggeredConnectionStateChanged;
            PeerFinder.ConnectionRequested += PeerFinder_ConnectionRequested;
            PeerFinder.Start();
            started = true;
        }

        /// <summary>
        /// A proximity connection was requested
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void PeerFinder_ConnectionRequested(object sender, ConnectionRequestedEventArgs args)
        {
           
        }

        /// <summary>
        /// Connection state changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        async void PeerFinder_TriggeredConnectionStateChanged(object sender, TriggeredConnectionStateChangedEventArgs args)
        {
            try
            {
                if (args.State == TriggeredConnectState.PeerFound)
                {
                    //Display a status message
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        txtStatus.Text = "Peer Found!";
                    });
                }
                else if (args.State == TriggeredConnectState.Completed)
                {
                    //Display a status message
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        txtStatus.Text = "Receiving File...";
                    });

                    //Set the socket
                    streamSocket = args.Socket;

                    //Start receiving the song
                    conn = new NFCConnection(Deployment.Current.Dispatcher, progressReceive, txtFile);
                    await conn.ReceiveSongFromSocket(streamSocket);


                    //Navigate to the home page
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        NavigationService.Navigate(new Uri("/BrowsePage.xaml", UriKind.Relative));
                    });
                }
            }
            catch (Exception ex)
            {
                string error = ex.Message;
                MessageBox.Show(ex.Message);
            }

        }
    }
}