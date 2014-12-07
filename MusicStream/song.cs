using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Media;
using System.Windows.Media;
namespace MusicStream
{
    public class song : IDisposable, INotifyPropertyChanged, INamed
    {
        public song(string name, string uid, string path, int sec)
        {
            this.name = name;
            this.uid = uid;
            this.path = path;
            length = sec > -1 ? new TimeSpan(0, 0, sec) : TimeSpan.Zero;
        }

        private TimeSpan length;

        public TimeSpan TotalTime { get { return length; } }

        private string path;
        public string FileName 
        { 
            get
            {
                return Path.GetFileName(path);
            }
        }

        private string name;
        public string Name { get { return name; } }

        private string uid;
        public string UID { get { return uid; } }

        private BackgroundWorker downloader;
        private MemoryStream songStream;
        private string uriKind;

        public MemoryStream getData()
        {
            // Wait
            while (songStream == null) System.Threading.Thread.Sleep(100);
            return songStream;
        }


        public Uri Uri { get { return new Uri(uriKind); } }

        public void startDownloading()
        {
            string sendData = "api=provide&item=" + this.UID;
            var request = connection.prepRequestforGet(sendData);
            var response = request.GetResponse();
            Stream responseStream = response.GetResponseStream();

            responseStream.Close();
            response.Close();

            string songPath = connection.domain + "sessions/public/" + this.UID;
            uriKind = songPath;
            return;
        }

        public bool isInUse { get; set; }

        void downloader_DoWork(object sender, DoWorkEventArgs e)
        {
            Stream stream = e.Argument as Stream;
            while(true)
            {
                if (!isInUse)
                {
                    songStream = new MemoryStream();
                    byte[] buffer = new byte[32768];
                    int read;
                    while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        songStream.Write(buffer, 0, read);
                    }
                    break;
                }
                Thread.Sleep(100);
            }
        }


        public void delData()
        {
            if(songStream != null)
            {
                songStream.Close();
                songStream = null;
            }
        }

        public void Dispose()
        {
            delData();
        }


        private bool m_selected;

        public bool Selected
        {
            get
            {
                return m_selected;
            }

            set
            {
                m_selected = value;
                NotifyPropertyChanged("Selected");
            }
        }

        #region Changing

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion

        private bool m_MultiSelected;

        public bool MultiSelected
        {
            get
            {
                return m_MultiSelected;
            }

            set
            {
                m_MultiSelected = value;
                NotifyPropertyChanged("MultiSelected");
            }
        }

    }
}
