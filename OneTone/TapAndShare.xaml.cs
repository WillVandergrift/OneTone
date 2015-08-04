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
using OneTone.Helper;

namespace OneTone
{
    public partial class TapAndShare : PhoneApplicationPage
    {
        //Are we currently connected
        bool started = false;

        //Socket used for transferring files
        StreamSocket streamSocket;

        //NFC Communications
        NFCConnection conn;

        public TapAndShare()
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
                started = false;
            }
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
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
            if (args.State == TriggeredConnectState.PeerFound)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        txtStatus.Text = "Peer Found";
                    });
            }
            else if (args.State == TriggeredConnectState.Completed)
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    txtStatus.Text = "Transferring";
                });

                //Stop broadcasting the connection
                PeerFinder.Stop();
                started = false;

                //Set the socket
                streamSocket = args.Socket;

                //Send the selected ringtone
                conn = new NFCConnection(Deployment.Current.Dispatcher, progressSend, txtStatus);
                await conn.SendSongOverSocket(streamSocket, Session.SharedFile.Name, Session.SharedFile.Name, Session.SharedFile.Size.ToString());

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    NavigationService.GoBack();
                });
            }
        }
    }
}