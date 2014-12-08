using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.IO;
using System.Web;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Media;
using NAudio.Wave;

namespace MusicStream
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {                        
            ignoreSSLCertificate();
            initWorker = new BackgroundWorker();
            
            Config = new config();
            LoginData();

            initWorker.DoWork += Login;
            initWorker.ProgressChanged += ProgressChanged;
            initWorker.RunWorkerAsync();
            //player.changeState += player_changeState;

            InitializeComponent();
        }

        config Config;

        void player_changeTime()
        {
            NotifyPropertyChanged("CurrentTime");
            NotifyPropertyChanged("TotalTime");
            NotifyPropertyChanged("DurationProgress");
        }

        void player_changeState()
        {
            NotifyPropertyChanged("PlayButtonImage");
        }

        void song_change()
        {
            NotifyPropertyChanged("SongName");
            player_changeTime();
        }

        bool repeatAll = false;

        bool Repeat { get; set; }

        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Progress = (double)e.ProgressPercentage / 100.0;
        }

        private double progress;

        public double Progress
        {
            get
            {
                return progress;
            }

            set
            {
                progress = value;
                NotifyPropertyChanged("Progress");
            }
        }

        private void ignoreSSLCertificate()
        {
            ServicePointManager.ServerCertificateValidationCallback +=
                    (sender, cert, chain, error) =>
                    {
                        return true;
                    };
        }
        
        BackgroundWorker initWorker;

        #region init

        private void LoginData()
        {

            var domains = Config.GetEntrys();

            // open ConfigWindow
            ConfigWindow dialog = new ConfigWindow(Config);
            dialog.ShowDialog();
            if (dialog.DialogResult == true)
            {
                connection.domain = dialog.SelectedEntry.Domain;
                connection.user = dialog.SelectedEntry.User;
                connection.passwd = dialog.SelectedEntry.Password;
            }
            else
            {
                this.Close();
            }
        }

        void Login(object sender, DoWorkEventArgs e)
        {
            if (connection.domain != null && !string.IsNullOrWhiteSpace(connection.domain))
            {
                if (connection.user != null && !string.IsNullOrWhiteSpace(connection.user) && connection.passwd != null && !string.IsNullOrWhiteSpace(connection.passwd))
                {
                    try
                    {
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(connection.domain);
                        request.Credentials = CredentialCache.DefaultCredentials;
                        request.UserAgent = "Bernd";
                        request.Method = "POST";

                        string postDataString = "usr_field" + "=" + connection.user + "&";
                        postDataString += "pw_field" + "=" + connection.passwd;

                        request = connection.prepRequestforPost(postDataString);

                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                        Stream responseStream = response.GetResponseStream();

                        StreamReader myStreamReader = new StreamReader(responseStream, Encoding.Default);

                        string pageContent = myStreamReader.ReadToEnd();

                        myStreamReader.Close();
                        responseStream.Close();
                        response.Close();

                        if(response.Headers["Set-Cookie"] == null)
                        {
                            MessageBox.Show("Login Failed");
                        }

                        string[] cookieString = response.Headers["Set-Cookie"].Split('=');
                        if (cookieString == null || cookieString.Length != 2) return;

                        connection.loginCookie = new Cookie(cookieString[0], cookieString[1]);

                        //-------------------------------------------------

                        InitPlaylists();

                        //-------------------------------------------------

                        postDataString = "api=cache";

                        request = connection.prepRequestforGet(postDataString);

                        response = (HttpWebResponse)request.GetResponse();

                        Stream responseStream2 = response.GetResponseStream();

                        StreamReader myStreamReader2 = new StreamReader(responseStream2, Encoding.Default);

                        string pageContent2 = myStreamReader2.ReadToEnd();

                        parseList(pageContent2, (BackgroundWorker)sender);

                        myStreamReader2.Close();
                        responseStream2.Close();
                        response.Close();
                    }
                    catch(WebException ex)
                    {
                        MessageBox.Show("Error: \r\n" + ex.Message + "\r\n \r\nResponse: \r\n" + ex.Response);
                        return;
                    }

                }
            }
            return;
        }

        public ObservableCollection<artist> artists;

        public ObservableCollection<artist> Artists { get { return artists; } }

        public List<album> Albums = new List<album>();

        public Dictionary<string, song> Songs = new Dictionary<string,song>();

        void parseList(string list, BackgroundWorker worker)
        {
            worker.WorkerReportsProgress = true;
            worker.ReportProgress(20);
            if (list != null && !string.IsNullOrWhiteSpace(list))
            {
                artists = new ObservableCollection<artist>();
                string[] temp = null;
                temp = list.Split('\n');
                List<string> SongList = new List<string>(temp);
                int SongCount = SongList.Count;
                if (SongCount > 0)
                {
                    int i = 0;
                    foreach (string title in SongList)
                    {
                        temp = new string[] { "::" };
                        string[] splitLine = title.Split(temp, StringSplitOptions.None);
                        i++;
                        worker.ReportProgress((i / SongCount) * 100);
                        if (splitLine.Length != 7)
                        {
                            continue;
                        }
                        var temp1 = artists.Where(x => x.Name == splitLine[1]);
                        artist currentArtist = null;
                        // artist handling
                        if (temp1 != null && temp1.Count() > 0)
                        {
                            // artist exits
                            currentArtist = temp1.First();
                        }
                        else
                        {
                            // artirst not exits
                            currentArtist = new artist(splitLine[1]);
                            artists.Add(currentArtist);
                        }
                        // album handling
                        var temp2 = currentArtist.Albums.Where(x => x.Name == splitLine[2]);
                        album currentAlbum = null;
                        if (temp2 != null && temp2.Count() > 0)
                        {
                            // album exits
                            currentAlbum = temp2.First();
                        }
                        else
                        {
                            // album not exits
                            currentAlbum = new album(splitLine[2]);
                            Albums.Add(currentAlbum);
                            currentArtist.Albums.Add(currentAlbum);
                        }
                        // song handling
                        var temp3 = currentAlbum.Songs.Where(x => x.Name == splitLine[3]);
                        song currentSong = null;
                        if (temp3 != null && temp3.Count() > 0)
                        {
                            // song exits
                            currentSong = temp3.First();
                        }
                        else
                        {
                            // song not exits
                            int sec = -1;
                            int.TryParse(splitLine[6], out sec);
                            currentSong = new song(splitLine[4], splitLine[5], splitLine[0], sec);
                            currentAlbum.Songs.Add(currentSong);
                            Songs.Add(currentSong.UID, currentSong);
                        }
                    }
                }
                NotifyPropertyChanged("Artists");
            }
        }

        private ObservableCollection<playlist> Playlists = new ObservableCollection<playlist>();

        public ObservableCollection<playlist> PlaylistsList { get { return Playlists; } }

        private void InitPlaylists()
        {
            // load Playlists
            BackgroundWorker PlaylistLoader = new BackgroundWorker();
            PlaylistLoader.DoWork += PlaylistLoader_DoWork;
            PlaylistLoader.RunWorkerCompleted += loadSongsInPlaylists;
            PlaylistLoader.RunWorkerAsync();
        }

        private void loadSongsInPlaylists(object sender, RunWorkerCompletedEventArgs e)
        {
            // wait as long initWorker is Working
            while(initWorker.IsBusy)
            {
                System.Threading.Thread.Sleep(50);
            }
            if(Playlists.Count > 0)
            {
                foreach(playlist pl in Playlists)
                {
                    string postDataString = "api=pl&item=" + pl.Name;
                    string pageContent = makeRequest(postDataString);

                    pageContent.Replace("\r\n", "\n");
                    string[] lines = pageContent.Split('\n');
                    string[] temp = new string[] { "::" };
                    foreach (string song in lines)
                    {

                        string[] splitLine = song.Split(temp, StringSplitOptions.None);
                        if (splitLine.Count() < 6) continue;
                        string uid = splitLine[5];
                        if(Songs.ContainsKey(uid))
                        {
                            pl.Songs.Add(Songs[uid]);
                        }
                    }
                    
                }
            }
            NotifyPropertyChanged("PlaylistList");
        }

        private string makeRequest(string quary, bool post = false, string PostContent = null)
        {
            try
            {
                if (quary == null) return null;
                string postDataString = quary;
                WebRequest request;
                if (post)
                    if(PostContent != null)
                        request = connection.prepRequestforPost(postDataString, PostContent);
                    else
                    request = connection.prepRequestforPost(postDataString);
                else
                    request = connection.prepRequestforGet(postDataString);

                WebResponse response = (HttpWebResponse)request.GetResponse();

                Stream responseStream = response.GetResponseStream();

                StreamReader myStreamReader = new StreamReader(responseStream, Encoding.Default);

                string pageContent = myStreamReader.ReadToEnd();

                myStreamReader.Close();
                responseStream.Close();
                response.Close();
                return pageContent;
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
                return "";
            }
        }

        private void PlaylistLoader_DoWork(object sender, DoWorkEventArgs e)
        {
            if (connection.loginCookie != null)
            {
                string postDataString = "api=pl";

                string pageContent = makeRequest(postDataString);

                pageContent.Replace("\r\n", "\n");
                string[] pllist = pageContent.Split('\n');
                foreach (string name in pllist)
                    if (name != null && !string.IsNullOrWhiteSpace(name))
                        this.Dispatcher.BeginInvoke(
                            new Action(() => Playlists.Add(new playlist(name))));
            }
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

        public string SongName { get { return currentSong != null ? currentSong.Name : "none"; } }


        public ObservableCollection<song> Playlist { get { return new ObservableCollection<song>(playlist); } }

        private player1 m_player;

        private player1 player
        {
            get
            {
                if (m_player == null)
                {
                    m_player = new player1();
                    NotifyPropertyChanged("Volume");
                    m_player.StateChanged += m_player_StateChanged;
                    player.changeTime += player_changeTime;
                }
                return m_player;
            }
        }

        public bool Shuffle { get; set; }

        private Random rand = new Random();

        private void PlayNext(object sender, RoutedEventArgs e)
        {
            if (currentSong != null && playlist != null && playlist.Count > 0)
            {
                currentSong = getNextSong();
                if (currentSong == null) return;
                currentSong.Selected = true;
                song_change();
                player.stop();
                System.Threading.Thread.Sleep(200);
                player.play(currentSong);
                player_changeState();
            }
        }

        private song getNextSong()
        {
            if (currentSong != null && playlist != null && playlist.Count > 0)
            {
                int index = playlist.IndexOf(currentSong);
                index++;
                if(Shuffle)
                {
                    int newindex = index - 1;
                    while (newindex == index - 1) newindex = rand.Next(0, playlist.Count - 1);
                    index = newindex;
                }
                if (index > playlist.Count - 1) index = 0;
                return playlist[index]; ;
            }
            return null;
        }

        private void m_player_StateChanged()
        {
            if(m_player.playbackState == StreamingPlaybackState.Finished)
            {
                // get next song
                if (currentSong != null && playlist != null && playlist.Count > 0)
                {
                    currentSong = getNextSong();
                    if (currentSong == null) return;
                    currentSong.Selected = true;
                    song_change();
                    player.stop();
                    System.Threading.Thread.Sleep(200);
                    player.play(currentSong);
                    player_changeState();
                }
            }
        }

        public double Volume
        {
            get
            {
                return player.Volume;
            }

            set
            {
                player.Volume = value;
            }
        }

        private List<song> playlist = new List<song>();

        private void PlayThisSong(object sender, MouseButtonEventArgs e)
        {
            song Song = (song)((ListViewItem)sender).DataContext;
            player.stop();
            currentSong = Song;
            currentSong.Selected = true;
            player.play(Song);
            player_changeState();
            song_change();
        }

        private void AddSong(object sender, MouseButtonEventArgs e)
        {
            if (sender.GetType() == typeof(TreeViewItem))
            {
                if (playlist.Contains((song)((TreeViewItem)sender).DataContext)) return;
                playlist.Add((song)((TreeViewItem)sender).DataContext);
            }
            else if (sender.GetType() == typeof(ListViewItem))
            {
                if (playlist.Contains((song)((ListViewItem)sender).DataContext)) return;
                playlist.Add((song)((ListViewItem)sender).DataContext);
            }
            NotifyPropertyChanged("Playlist");
        }

        private void AddArtist(object sender, MouseButtonEventArgs e)
        {
            player.stop();
            currentSong = null;
            player_changeState();
            song_change();
            artist Artist = ((artist)((TreeViewItem)sender).DataContext);
            playlist.Clear();
            playlist.AddRange(Artist.Songs);
            NotifyPropertyChanged("Playlist");
        }

        private void AddPlaylist(object sender, MouseButtonEventArgs e)
        {
            player.stop();
            currentSong = null;
            player_changeState();
            song_change();
            playlist Playlist = ((playlist)((TreeViewItem)sender).DataContext);
            playlist.Clear();
            playlist.AddRange(Playlist.Songs);
            NotifyPropertyChanged("Playlist");
        }

        private void AddAlbum(object sender, MouseButtonEventArgs e)
        {
            player.stop();
            currentSong = null;
            player_changeState();
            song_change();
            album Album = ((album)((TreeViewItem)sender).DataContext);
            playlist.Clear();
            playlist.AddRange(Album.Songs);
            NotifyPropertyChanged("Playlist");
        }

        private song currentSong;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            player.stop();
            player_changeState();
        }

        private void Button_Click1(object sender, RoutedEventArgs e)
        {
            if (player.playbackState != StreamingPlaybackState.Playing) player.play(currentSong); else player.pause();
            player_changeState();
        }

        public string TotalTime { get { return currentSong != null ? currentSong.TotalTime.ToString(@"mm\:ss") : "00:00"; } }
        public string CurrentTime { get { return player.CurrentTime != null ? player.CurrentTime.ToString(@"mm\:ss") : "00:00"; } }
        public double DurationProgress
        {
            get
            {
                if (currentSong == null || currentSong.TotalTime == null || player.CurrentTime == null)
                {
                    return 0;
                }
                return ((player.CurrentTime.TotalMilliseconds / currentSong.TotalTime.TotalMilliseconds) * 100);
            }
        }
        public BitmapSource PlayButtonImage
        {
            get
            {

                if (player.playbackState == StreamingPlaybackState.Stopped || player.playbackState == StreamingPlaybackState.Paused)
                {
                    var hBitmap = Properties.Resources.ios7_play.GetHbitmap();
                    return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap( hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                }
                else
                {
                    var hBitmap = Properties.Resources.ios7_pause.GetHbitmap();
                    return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
                }
            }
        }

        private void ListView_Selected(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                song newSong = e.AddedItems[0] as song;
                currentSong = newSong;
                player.stop();
                player_changeState();
                song_change();
            }
        }

        #region Playlist



        private void AddPlaylist(object sender, RoutedEventArgs e)
        {
            // get list of currentPlaylists
            string playlists = makeRequest("api=pl");
            if (playlists != null && !string.IsNullOrWhiteSpace(playlists))
            {
                string name;
                PlaylistName dl = new PlaylistName();
                dl.ShowDialog();
                if (dl.DialogResult != true) return;
                name = dl.Name;
                if (name != null && !string.IsNullOrWhiteSpace(name))
                {
                    char[] sep = { '\n' };
                    if (!playlists.Split(sep).Contains(name))
                    {
                        string quaryString = "nores=create&item=" + name;
                        makeRequest(quaryString);
                        PlaylistsList.Add(new playlist(name));
                        NotifyPropertyChanged("PlaylistsList");
                    }
                    else
                    {
                        MessageBox.Show("There is already a playlist with this name!");
                    }
                }
            }
            
        }

        private void DelPlaylist(object sender, RoutedEventArgs e)
        {
            if(PlaylistComboBox.SelectedValue != null)
            {
                playlist pl = PlaylistComboBox.SelectedValue as playlist;
                makeRequest("api=pl&delete", true, "pl=" + pl.Name);

                PlaylistsList.Remove(pl);
                NotifyPropertyChanged("PlaylistsList");

                PlaylistComboBox.SelectedValue = null;
                m_selectetPlaylist = null;
                NotifyPropertyChanged("SelectedPlaylist");
                NotifyPropertyChanged("PlaylistSelected");
            }
        }

        private void AddSongToPlaylist(object sender, RoutedEventArgs e)
        {
            if (PlaylistComboBox.SelectedValue != null)
            {
                playlist pl = PlaylistComboBox.SelectedValue as playlist;
                SongPicker sp = new SongPicker(new List<artist>(artists.AsEnumerable()));
                sp.ShowDialog();
                if(sp.DialogResult == true)
                {
                    var songs = sp.Songs;
                    if(songs != null && songs.Count > 0)
                    {
                        string PostContent = "";
                        foreach(song s in songs)
                        {
                            if (!pl.Songs.Contains(s))
                            {
                                PostContent += "uid=" + s.UID + "&";
                                pl.Songs.Add(s);
                            }                        
                        }
                        PostContent = PostContent.Substring(0, PostContent.Length - 1);
                        makeRequest("nores=add&post=" + pl.Name, true, PostContent);
                    }
                }
            }
        }

        private void DelSongFromPlaylist(object sender, RoutedEventArgs e)
        {

            if (PlaylistComboBox.SelectedValue != null && PlaylistSongs.SelectedItems != null && PlaylistSongs.SelectedItems.Count > 0)
            {
                playlist pl = PlaylistComboBox.SelectedValue as playlist;
                List<song> songs = new List<song>(PlaylistSongs.SelectedItems.Cast<song>());
                // TODO:
                string PostContent = "pl="+pl.Name+"&";
                foreach(song Song in songs)
                {
                    PostContent += "uid=" + Song.UID + "&";
                    pl.Songs.Remove(Song);
                }
                PostContent = PostContent.Substring(0, PostContent.Length - 1);
                makeRequest("api=pl&delete=track", true, PostContent);
                NotifyPropertyChanged("SelectedPlaylist");
            }
        }

        private playlist m_selectetPlaylist;

        public playlist SelectedPlaylist { get { return m_selectetPlaylist; } }

        private void ChangePlaylist(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems.Count > 0)
            {
                m_selectetPlaylist = e.AddedItems[0] as playlist;
            }
            else
            {
                m_selectetPlaylist = null;
            }
            NotifyPropertyChanged("SelectedPlaylist");
            NotifyPropertyChanged("PlaylistSelected");
        }

        public bool PlaylistSelected { get { return SelectedPlaylist != null; } }

        #endregion

        private void InsertPlaylist(object sender, RoutedEventArgs e)
        {
            if (PlaylistComboBox.SelectedValue != null)
            {
                player.stop();
                currentSong = null;
                player_changeState();
                song_change();
                playlist Playlist = PlaylistComboBox.SelectedValue as playlist;
                playlist.Clear();
                playlist.AddRange(Playlist.Songs);
                NotifyPropertyChanged("Playlist");
            }
        }


    }
}
