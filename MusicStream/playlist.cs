using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace MusicStream
{
    public class playlist
    {
        private ObservableCollection<song> m_songs;
        public playlist(string Name, List<song> Songs = null)
        {
            m_songs = new ObservableCollection<song>();
            this.Name = Name;
            if(Songs != null)
            {
                foreach (song s in Songs) m_songs.Add(s);
                //m_songs.AddRange(Songs);
            }
        }

        public ObservableCollection<song> Songs
        {
            get
            {
                return m_songs;
            }
        }

        public string Name { get; set; }
    }
}
