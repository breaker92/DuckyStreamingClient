using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace MusicStream
{
    public class album : INotifyPropertyChanged, INamed
    {
        public album(string name)
        {
            this.name = name;
            songs = new List<song>();
        }

        private string name;
        public string Name { get { return name; } }

        private List<song> songs;
        public List<song> Songs { get { return songs; } }

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
