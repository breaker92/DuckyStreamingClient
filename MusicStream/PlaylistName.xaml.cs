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
using System.Windows.Shapes;

namespace MusicStream
{
    /// <summary>
    /// Interaktionslogik für PlaylistName.xaml
    /// </summary>
    public partial class PlaylistName : Window
    {
        public PlaylistName()
        {
            InitializeComponent();
        }

        private string m_Name;

        public string Name { get { return m_Name; } }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string name = NameBox.Text;
            if (name != null && !string.IsNullOrWhiteSpace(name))
            {
                m_Name = name;
                DialogResult = true;
            }
            else
                DialogResult = false;
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            m_Name = null;
            DialogResult = false;
            this.Close();
        }
    }
}
