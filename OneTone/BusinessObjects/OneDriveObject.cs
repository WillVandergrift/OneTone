using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using OneTone.Helper;
using System.ComponentModel;
using System.Windows.Media;
using Microsoft.Live;
using System.Windows;
using System.Collections.ObjectModel;

namespace OneTone.BusinessObjects
{
    public class OneDriveObject : INotifyPropertyChanged
    {
        //Private Properties
        private string name;
        private string description;
        private string id;
        private string parentID;
        private string objectType;
        private int size;
        private DateTime createdTime;
        private DateTime updatedTime;
        private double downloadProgress;
        private SolidColorBrush textColor;


        /// <summary>
        /// The name of the object
        /// </summary>
        public string Name 
        {
            get { return name; }
            set 
            { 
                name = value;
                NotifyPropertyChanged("Name");
            }
        }

        /// <summary>
        /// The description for the object
        /// </summary>
        public string Description
        {
            get { return description; }
            set
            {
                description = value;
                NotifyPropertyChanged("Description");
            }
        }

        /// <summary>
        /// The object's ID
        /// </summary>
        public string ID
        {
            get { return id; }
            set
            {
                id = value;
                NotifyPropertyChanged("ID");
            }
        }

        /// <summary>
        /// The object's parent ID
        /// </summary>
        public string ParentID
        {
            get { return parentID; }
            set
            {
                parentID = value;
                NotifyPropertyChanged("ParentID");
            }
        }

        /// <summary>
        /// The object's type
        /// </summary>
        public string ObjectType
        {
            get { return objectType; }
            set
            {
                objectType = value;
                NotifyPropertyChanged("ObjectType");
            }
        }

        /// <summary>
        /// The object's file size
        /// </summary>
        public int Size
        {
            get { return size; }
            set
            {
                size = value;
                NotifyPropertyChanged("Size");
            }
        }

        /// <summary>
        /// The object's creation time stamp
        /// </summary>
        public DateTime CreatedTime
        {
            get { return createdTime; }
            set
            {
                createdTime = value;
                NotifyPropertyChanged("CreatedTime");
            }
        }

        /// <summary>
        /// The object's last update time stamp
        /// </summary>
        public DateTime UpdatedTime
        {
            get { return updatedTime; }
            set
            {
                updatedTime = value;
                NotifyPropertyChanged("UpdatedTime");
            }
        }


        /// <summary>
        /// The object's download progress
        /// </summary>
        public double DownloadProgress
        {
            get { return downloadProgress; }
            set
            {
                downloadProgress = value;
                NotifyPropertyChanged("DownloadProgress");
            }
        }

        /// <summary>
        /// The object's text color
        /// </summary>
        public SolidColorBrush TextColor
        {
            get { return textColor; }
            set
            {
                textColor = value;
                NotifyPropertyChanged("TextColor");
            }
        }


        /// <summary>
        /// The display icon
        /// </summary>
        public BitmapImage Icon
        {
            get
            {
                switch (ObjectType)
                {
                    case "audio":
                        return new BitmapImage(new Uri("Assets/Images/Music-Note-icon.png", UriKind.Relative));
                    case "file":
                        return new BitmapImage(new Uri("Assets/Images/File-icon.png", UriKind.Relative));
                    case "folder":
                        return new BitmapImage(new Uri("Assets/Images/Folders-icon.png", UriKind.Relative));
                    default:
                        return new BitmapImage(new Uri("Assets/Images/Folders-icon.png", UriKind.Relative));
                }
            }
        }

        /// <summary>
        /// The display description
        /// </summary>
        public string DisplayDescription
        {
            get
            {
                return string.Concat("Created: ", CreatedTime.ToShortDateString(), " Size: ", FileSizeDisplay.DisplayFileSize(Size));
            }
        }


        /// <summary>
        /// Default constructor
        /// </summary>
        public OneDriveObject()
        {

        }

        /// <summary>
        /// Constructor used when creating an object from operation results
        /// </summary>
        /// <param name="name">The object's name</param>
        /// <param name="description">The object's description</param>
        /// <param name="id">The object's ID</param>
        /// <param name="parentID">The object's parent's ID</param>
        /// <param name="type">The object's type</param>
        /// <param name="size">The object's file size</param>
        /// <param name="createdTime">The object's creation time</param>
        /// <param name="updatedTime">The object's last update time</param>
        public OneDriveObject(string name, string description, string id,
                                string parentID, string objectType, int size,
                                string createdTime, string updatedTime)
        {
            Name = name;
            Description = description;
            ID = id;
            ParentID = parentID;
            ObjectType = objectType;
            Size = size;
            CreatedTime = Convert.ToDateTime(createdTime);
            UpdatedTime = Convert.ToDateTime(updatedTime);
            TextColor = new SolidColorBrush(Colors.White);
        }

        //INotifzPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Gets an object from the user's OneDrive directory
        /// </summary>
        /// <param name="path">The path to the object</param>
        /// <returns></returns>
        public static async Task<OneDriveObject> GetObject(string path)
        {
            try
            {
                //Create a new One Drive Object
                OneDriveObject curObject = new OneDriveObject();

                LiveConnectClient connect = new LiveConnectClient(Session.CurSession);
                LiveOperationResult operationResult = await connect.GetAsync(path);
                dynamic result = operationResult.Result;
                if (result != null)
                {
                    //Create a new One Drive object from the results
                    curObject = new OneDriveObject(result.name, result.description, result.id, result.parent_id,
                                                    result.type, result.size, result.created_time, result.updated_time);
                }

                //Return our object
                return curObject;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Fetch a list of objects in the selected one drive directory
        /// </summary>
        /// <param name="path">The One Drive path to get a list of objects for</param>
        /// <returns></returns>
        public static async Task<ObservableCollection<OneDriveObject>> GetDirectory(string path)
        {
            try
            {
                ObservableCollection<OneDriveObject> results = new ObservableCollection<OneDriveObject>();

                LiveConnectClient connect = new LiveConnectClient(Session.CurSession);
                LiveOperationResult operationResult = await connect.GetAsync(path);
                dynamic result = operationResult.Result;
                if (result != null)
                {
                    foreach (dynamic item in result.data)
                    {
                        //Create a new One Drive object from the results
                        results.Add(new OneDriveObject(item.name, item.description, item.id, item.parent_id,
                                                                    item.type, item.size, item.created_time, item.updated_time));
                    }
                }

                return results;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
        }
    }
}
