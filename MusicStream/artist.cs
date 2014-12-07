using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace MusicStream
{
    public class artist : INotifyPropertyChanged, INamed
    {
        public artist(string name)
        {
            this.name = name;
            albums = new List<album>();
        }

        private string name;
        public string Name { get { return name; } }

        private List<album> albums;
        public List<album> Albums { get { return albums; } }

        public List<song> Songs
        {
            get
            {
                List<song> songs = new List<song>();
                if(albums != null)
                {
                    foreach(album Album in albums)
                    {
                        songs.AddRange(Album.Songs);
                    }
                }
                return songs;
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
