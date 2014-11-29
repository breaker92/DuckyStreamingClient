using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicStream
{
    public class artist
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
    }
}
