using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Live;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;
using OneTone.BusinessObjects;

namespace OneTone
{
    public static class Session
    {
        /// <summary>
        /// The current Live session
        /// </summary>
        public static LiveConnectSession CurSession { get; set; }

        /// <summary>
        /// The system wide accent color
        /// </summary>
        public static Color AccentColor = (Color)Application.Current.Resources["PhoneAccentColor"];

        public static OneDriveObject SharedFile { get; set; }
    }
}
