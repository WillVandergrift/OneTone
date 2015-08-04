using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;
using Windows.Networking.Proximity;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace OneTone
{
    public class NFCConnection
    {
        DataWriter _dataWriter;
        Dispatcher _d;
        ProgressBar _progress;
        TextBlock _txtStatus;
        Int64 _length;

        public NFCConnection(Dispatcher d, ProgressBar progress, TextBlock status)
        {
            //set the dispatcher and progress bar for updating the UI
            _d = d;
            _progress = progress;
            _txtStatus = status;
        }

        public async Task<bool> ReceiveSongFromSocket(StreamSocket socket)
        {
            try
            {
                SongWriter sw = new SongWriter();
                await sw.WriteSongFromSocketStream(socket, _d, _progress, _txtStatus);
                return true;
            }
            catch
            {
                return false;
                //MessageBox.Show(e.Message);
            }
        }


        //send song data over the socket
        //this uses an app specific socket protocol: send title, artist, then song data
        public async Task<bool> SendSongOverSocket(StreamSocket socket, string fileName, string songTitle, string songFileSize)
        {
            try
            {
                // Create DataWriter for writing to peer.
                _dataWriter = new DataWriter(socket.OutputStream);

                //send song title
                await SendStringOverSocket(songTitle);

                //send song file size
                await SendStringOverSocket(songFileSize);

                // read song from Isolated Storage and send it
                using (var fileStream = new IsolatedStorageFileStream(fileName, FileMode.Open, FileAccess.Read, IsolatedStorageFile.GetUserStoreForApplication()))
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead;
                    int readCount = 0;
                    _length = fileStream.Length;

                    //Initialize the User Interface elements
                    InitializeUI(fileName);

                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        readCount += bytesRead;
                        //UpdateUI
                        UpdateProgressBar(readCount);
                        //size of the packet
                        _dataWriter.WriteInt32(bytesRead);
                        //packet data sent
                        _dataWriter.WriteBytes(buffer);
                        try
                        {
                            await _dataWriter.StoreAsync();
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message);
                        }
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }

        }

        /// <summary>
        /// Setup the User Interface elements for showing progress
        /// </summary>
        private void InitializeUI(string filename)
        {
            _d.BeginInvoke(() =>
            {
                _progress.Value = 0;
            });
        }

        /// <summary>
        /// Update the progress bar
        /// </summary>
        private void UpdateProgressBar(int transferSize)
        {
            _d.BeginInvoke(() =>
            {
                _progress.Value = (transferSize / _length) * 100;
            });
            
        }

        private async Task SendStringOverSocket(string stringToSend)
        {
            UInt32 length = (UInt32)stringToSend.Length;
            //size of the packet/string
            _dataWriter.WriteUInt32(length);
            _dataWriter.WriteString(stringToSend);
            await _dataWriter.StoreAsync();
        }
    }
}
