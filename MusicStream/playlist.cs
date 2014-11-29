using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicStream
{
    public class playlist
    {
        private List<song> m_songs;
        public playlist(string Name, List<song> Songs = null)
        {
            m_songs = new List<song>();
            this.Name = Name;
            if(Songs != null)
            {
                m_songs.AddRange(Songs);
            }
        }

        public List<song> Songs
        {
            get
            {
                return m_songs;
            }
        }

        public string Name { get; set; }
    }
}
