using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Media;
using MusicStream;

namespace MusicStreamOld
{
    public static class player
    {
        private static List<song> playlist = new List<song>();

        public static void addSong(song Song)
        {
            if (playlist.Contains(Song)) return;
            Song.startDownloading();
            playlist.Add(Song);
        }

        private static IWavePlayer waveOut;

        public static void clearPlaylist()
        {
            foreach(song Song in playlist)
            {
                Song.delData();
            }
        }

        private static Thread playerThread;

        private static song m_currentSong;

        public static song currentSong
        {
            get
            {
                return m_currentSong;
            }
        }

        private static void play(song Song)
        {
            if (waveOut != null)
            {
                waveOut.Stop();
                waveOut.Dispose();
                waveOut = null;
            }
            waveOut = new WaveOut();
            MemoryStream ms = Song.getData();
            ms.Position = 0;
            
            //Song.isInUse = true;
            var mp3fr = new Mp3FileReader(ms);
            WaveStream blockAlignedStream = new BlockAlignReductionStream( WaveFormatConversionStream.CreatePcmStream(mp3fr));
            m_totaltime = blockAlignedStream.TotalTime;

            var waveEvent = new ManualResetEvent(false);
            waveOut.PlaybackStopped += (sender, e) => waveEvent.Set();
            waveEvent.Reset();

            waveOut.Init(blockAlignedStream);
            waveOut.Play();
            if (waveOut.PlaybackState != PlaybackState.Stopped)
            {
                waveEvent.WaitOne(); /* block thread for a while because I don't want async play back                                
                                                        (to be analogical as usage of SoundPlayer Play method) */
            }
            m_currentSong = Song;
            changeState();
            while (waveOut.PlaybackState != PlaybackState.Stopped)
            {
                m_currenttime = blockAlignedStream.CurrentTime;
                changeTime();
                System.Threading.Thread.Sleep(100);
            }
            m_currenttime = blockAlignedStream.CurrentTime;
            changeTime();
            changeState();
            waveOut.Stop();
            waveOut.Dispose();
            waveOut = null;


            ms.Seek(0, SeekOrigin.Begin);
            ms.Position = 0;
            //Song.isInUse = false;
        }

        public static PlaybackState State { get { return waveOut != null ? waveOut.PlaybackState : PlaybackState.Stopped; } }

        private static TimeSpan m_totaltime;
        public static TimeSpan Totaltime { get { return m_totaltime; } }

        private static TimeSpan m_currenttime;
        public static TimeSpan Currenttime { get { return m_currenttime; } }

        //public static event changeState changeState;

        public static void playSong(int position)
        {
            if (playerThread != null)
            {
                playerThread.Abort();
                playerThread = null;
            }
            if(position >= 0 && playlist != null && playlist.Count > position)
            {
                song Song = playlist[position];
                playerThread = new Thread(() => play(Song));
                playerThread.IsBackground = true;
                playerThread.Start();
            }
        }

        public static void playSong(song Song)
        {
            int index = playlist.IndexOf(Song);
            if(playlist.IndexOf(Song) >= 0)
            {
                playSong(index);
            }
        }

        /// <summary>
        /// hold on the player
        /// </summary>
        public static void Pause()
        {
            if(waveOut != null)waveOut.Pause();
            changeState();
        }
        /// <summary>
        /// play the current song or play the first song in the playlist or do nothing
        /// </summary>
        public static void Play()
        {
            if (playlist.Count == 0) return;
            if (m_currentSong == null)
            {
                m_currentSong = playlist[0];
                playSong(0);
            }
            else
            {
                if (waveOut != null) waveOut.Play();
                else playSong(m_currentSong);
            } 
            changeState();
        }

        public static PlaybackState PlaybackState { get { return waveOut.PlaybackState; } }
        public static void Stop()
        {
            if (waveOut != null) waveOut.Stop();
            m_currentSong = null;
            m_currenttime = TimeSpan.Zero;
            m_totaltime = TimeSpan.Zero;
            changeState();
            changeTime();
        }

        public static void Backward()
        {
            if (waveOut != null);
            m_currentSong = null;
            changeState();
        }

        public static event changeState changeState;
        public static event changeTime changeTime;

        public static List<song> Playlist { get { return playlist; } }

        

    }

    public delegate void changeState();
    public delegate void changeTime();
}
