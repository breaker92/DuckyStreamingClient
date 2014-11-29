using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MusicStream
{
    public class album
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
    }
}
