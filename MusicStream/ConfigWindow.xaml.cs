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
    /// <summary>
    /// Interaktionslogik für ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow : Window, INotifyPropertyChanged
    {
        public ConfigWindow(config Config)
        {
            this.Config = Config;
            m_entrys = new ObservableCollection<ConfigEntry>(Config.GetEntrys());
            if(m_entrys.Count > 0)
                SelectedEntry = m_entrys[0];
            NotifyPropertyChanged("SelectedEntry");
            InitializeComponent();
        }

        public ObservableCollection<ConfigEntry> Entrys { get { return m_entrys; } }

        private config Config;

        private ObservableCollection<ConfigEntry> m_entrys;

        public ConfigEntry SelectedEntry
        {
            get;
            set;
        }

        private void Aplly(object sender, RoutedEventArgs e)
        {
            Config.Save(new List<ConfigEntry>(m_entrys.AsEnumerable()));
            this.DialogResult = true;
            this.Close();
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Add(object sender, RoutedEventArgs e)
        {
            ConfigEntry newEntry = new ConfigEntry("domain", "user", "password");
            m_entrys.Add(newEntry);
            SelectedEntry = newEntry;
            NotifyPropertyChanged("Entrys");
            NotifyPropertyChanged("SelectedEntry");
        }

        private void Del(object sender, RoutedEventArgs e)
        {
            if(SelectedEntry != null)
            {
                Entrys.Remove(SelectedEntry);
                SelectedEntry = null;
                NotifyPropertyChanged("Entrys");
                NotifyPropertyChanged("SelectedEntry");
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

        private void EntryChange(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems.Count > 0)
            {
                var sItem = e.AddedItems[0] as ConfigEntry;
                SelectedEntry = sItem;
            }
        }
    }
}
