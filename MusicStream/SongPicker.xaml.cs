using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MusicStream
{

    public enum SearchFilter
    {
        Song = 0,
        Album = 1,
        Artist = 2
    }

    /// <summary>
    /// Interaktionslogik für SongPicker.xaml
    /// </summary>
    public partial class SongPicker : Window , INotifyPropertyChanged
    {
        public List<string> FilterList
        {
            get
            {
                string[] op = {"Song", "Album", "Artist"};
                return new List<string>(op);
            }
        }

        public SongPicker(List<artist> Artists, string searchString = "", SearchFilter Filter = SearchFilter.Song )
        {
            SearchString = searchString;
            m_artists = new ObservableCollection<artist>(Artists);
            SearchingVisibility = Visibility.Collapsed;

            InitializeComponent();

            FilterCombo.SelectedIndex = (int)Filter;
        }

        private ObservableCollection<artist> m_artists;
        public ObservableCollection<artist> Artists { get { return m_artists; } }

        private void Apply(object sender, RoutedEventArgs e)
        {
            foreach (object o in m_SelectedList)
            {
                if (o.GetType() == typeof(song))
                {
                    ((song)o).MultiSelected = false;
                }
                else if (o.GetType() == typeof(album))
                {
                    ((album)o).MultiSelected = false;
                }
                else if (o.GetType() == typeof(artist))
                {
                    ((artist)o).MultiSelected = false;
                }
            }
            DialogResult = true;
            this.Close();
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            foreach (object o in m_SelectedList)
            {
                if (o.GetType() == typeof(song))
                {
                    ((song)o).MultiSelected = false;
                }
                else if (o.GetType() == typeof(album))
                {
                    ((album)o).MultiSelected = false;
                }
                else if (o.GetType() == typeof(artist))
                {
                    ((artist)o).MultiSelected = false;
                }
            }
            DialogResult = false;
            this.Close();
        }

        #region MultiSelect
        private List<object> m_SelectedList;

        private object SelectedItem;

        private void SongTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            object selected = e.NewValue;
            object deselected = e.OldValue;
            // Control selecting
            if(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (m_SelectedList == null) m_SelectedList = new List<object>();
                setMutliSelected(selected, true);
                setMutliSelected(deselected, true);
                m_SelectedList.Add(selected);
                return;
            }
            // Shift selecting
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                if (m_SelectedList == null) m_SelectedList = new List<object>();
                return;
            }
            // single selecting
            if(selected != null)
            {
                if (m_SelectedList == null) m_SelectedList = new List<object>();
                clearSelectedList();
                setMutliSelected(deselected, false);
                setMutliSelected(selected, true);
                m_SelectedList.Add(selected);
            }
        }

        private void setMutliSelected(object o, bool value)
        {
            if(o != null)
            {
                if (o.GetType() == typeof(song))
                {
                    ((song)o).MultiSelected = value;
                }
                else if (o.GetType() == typeof(album))
                {
                    ((album)o).MultiSelected = value;
                }
                else if (o.GetType() == typeof(artist))
                {
                    ((artist)o).MultiSelected = value;
                }
            }
        }

        private void clearSelectedList()
        {
            if (m_SelectedList != null && m_SelectedList.Count > 0)
            {
                foreach (object o in m_SelectedList)
                {
                    if (o.GetType() == typeof(song))
                    {
                        ((song)o).MultiSelected = false;
                    }
                    else if (o.GetType() == typeof(album))
                    {
                        ((album)o).MultiSelected = false;
                    }
                    else if (o.GetType() == typeof(artist))
                    {
                        ((artist)o).MultiSelected = false;
                    }
                }
                m_SelectedList.Clear();
            }
        }
        #endregion
        public List<song> Songs
        {
            get
            {
                var list = new List<song>();
                if (m_SelectedList != null)
                {
                    foreach (object o in m_SelectedList)
                    {
                        if (o.GetType() == typeof(song))
                        {
                            ((song)o).MultiSelected = false;
                            list.Add((song)o);
                        }
                        else if (o.GetType() == typeof(album))
                        {
                            ((album)o).MultiSelected = false;
                            list.AddRange(((album)o).Songs);
                        }
                        else if (o.GetType() == typeof(artist))
                        {
                            ((artist)o).MultiSelected = false;
                            list.AddRange(((artist)o).Songs);
                        }
                    }
                }
                return list;
            }
        }

        #region Search

        public string SearchString { get; set; }

        public Visibility SearchingVisibility { get; set; }

        public ObservableCollection<INamed> SearchResult { get; set; }

        private void Search(object sender, TextChangedEventArgs e)
        {
            string searchString = ((TextBox)sender).Text;
            SearchingVisibility = Visibility.Visible;
            NotifyPropertyChanged("SearchingVisibility");
            Searching(searchString);
        }

        private void Searching(string searchString)
        {
            if (searchString == null)
            {
                searchString = "";
            }
            // Searching for artist
            if(FilterCombo.SelectedIndex == 2)
            {
                List<INamed> result = new List<INamed>(Artists.Where(x => x.Name.Contains(searchString)));
                SearchResult = new ObservableCollection<INamed>(result);
                NotifyPropertyChanged("SearchResult");
            }
            // Searching for album
            else if(FilterCombo.SelectedIndex == 1)
            {
                List<album> albums = new List<album>();
                foreach(artist a in Artists)
                {
                    albums.AddRange(a.Albums);
                }
                List<INamed> result = new List<INamed>(albums.Where(x => x.Name.Contains(searchString)));
                SearchResult = new ObservableCollection<INamed>(result);
                NotifyPropertyChanged("SearchResult");
            }
            // Searching for song
            else if (FilterCombo.SelectedIndex == 0)
            {
                List<song> songs = new List<song>();
                foreach (artist a in Artists)
                {
                    songs.AddRange(a.Songs);
                }
                List<INamed> result = new List<INamed>(songs.Where(x => x.Name.Contains(searchString)));
                SearchResult = new ObservableCollection<INamed>(result);
                NotifyPropertyChanged("SearchResult");
            }
        }

        private void CloseSearching(object sender, RoutedEventArgs e)
        {
            SearchingVisibility = Visibility.Collapsed;
            NotifyPropertyChanged("SearchingVisibility");
        }

        private void ChangeFilter(object sender, SelectionChangedEventArgs e)
        {
            Searching(SearchBox.Text);
        }
        #endregion

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

        private void ChangeSearchSelection(object sender, SelectionChangedEventArgs e)
        {
            List<object> list = new List<object>();
            clearSelectedList();
            foreach (object o in e.AddedItems)
            {
                list.Add(o);
                setMutliSelected(o, true);
            }
            m_SelectedList = list;
        }


    }
}
