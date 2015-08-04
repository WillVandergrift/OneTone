using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Media.PhoneExtensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace OneTone
{
    public class SongWriter
    {
        public string _songTitle;
        public string _songFileSize;
        public string _songFileName;
        
        public SongWriter()
        {

        }

        public async Task WriteSongFromSocketStream(StreamSocket socket, Dispatcher _d, ProgressBar _progress, TextBlock _status)
        {
            UInt32 totalBytesRead = 0;
            UInt32 initialLength = 4; //for the int @ the front of every packet that denotes size of the packet coming
            var dr = new DataReader(socket.InputStream);
            UInt32 packetLength = 0;

            //get song title _songTitle
            if ((packetLength = await GetNextPacketSize(initialLength, dr)) > 0)
            {
                if ((await dr.LoadAsync((uint)packetLength)) != packetLength)
                {
                    //the only way this would happen is if the sender sent a song title incorrectly
                    MessageBox.Show("Couldn't obtain song title.");
                    return;
                }
                else
                {
                    _songFileName = dr.ReadString(packetLength);
                    _songTitle = _songFileName.Substring(0, _songFileName.LastIndexOf("."));
                    //Update the song title
                    Deployment.Current.Dispatcher.BeginInvoke(() =>
                    {
                        _status.Text = _songTitle;
                    });
                }
            }
            else
            {
                MessageBox.Show("Couldn't obtain song title.");
                return;
            }

            //get file size
            if ((packetLength = await GetNextPacketSize(initialLength, dr)) > 0)
            {
                if ((await dr.LoadAsync((uint)packetLength)) != packetLength)
                {
                    MessageBox.Show("Couldn't obtain song file size.");
                    return;
                }
                else
                {
                    _songFileSize = dr.ReadString(packetLength);
                }
            }
            else
            {
                MessageBox.Show("Couldn't obtain song file size.");
                return;
            }

            //Delete the song if it alread exists in local storage
            IfFileExistsInIsoStoreDeleteIt(_songFileName);

            var fileStream = new IsolatedStorageFileStream(_songFileName, FileMode.CreateNew, IsolatedStorageFile.GetUserStoreForApplication());
            while ((packetLength = await GetNextPacketSize(initialLength, dr)) > 0)
            {
                //read buffer of that size
                if (packetLength > 0)
                {
                    byte[] buffer = new byte[packetLength];
                    await dr.LoadAsync(packetLength);
                    dr.ReadBytes(buffer);

                    //write to iso store song file
                    fileStream.Write(buffer, 0, (int)packetLength);
                }

                totalBytesRead += packetLength;
                
                //Update the progress
                Int64 size;
                if (Int64.TryParse(_songFileSize, out size))
                {
                    _d.BeginInvoke(() =>
                    {
                        _progress.Value = (totalBytesRead / size) * 100;
                    });
                }
            }

            fileStream.Close();
            SaveRingtone(_songTitle, _songFileName);
        }

        private static async Task<UInt32> GetNextPacketSize(uint initialLength, DataReader dr)
        {
            //get packet length
            await dr.LoadAsync(initialLength);
            UInt32 packetLength = dr.ReadUInt32();

            if (packetLength > 1024)
            {
                return 0;
            }
            else
            {
                return packetLength;
            }            
        }

        private static void IfFileExistsInIsoStoreDeleteIt(string songFileName)
        {
            // If file already exists delete it
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (store.FileExists(songFileName))
                    store.DeleteFile(songFileName);
            }
        }

        /// <summary>
        /// Save the ringtone
        /// </summary>
        /// <param name="songTitle"></param>
        /// <param name="fileName"></param>
        public void SaveRingtone(string songTitle, string fileName)
        {
            //The Save ringtone chooser
            SaveRingtoneTask saveRingtoneChooser = new SaveRingtoneTask();

            saveRingtoneChooser.Source = new Uri(string.Concat("isostore:/", fileName));
            saveRingtoneChooser.DisplayName = songTitle;
            saveRingtoneChooser.Show();
        }
    }
}
