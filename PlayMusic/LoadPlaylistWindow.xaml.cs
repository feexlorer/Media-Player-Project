using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace PlayMusic
{
    /// <summary>
    /// Interaction logic for LoadPlaylistWindow.xaml
    /// </summary>
    public partial class LoadPlaylistWindow : Window
    {
        public string ChosenPlaylist { get; set; } ="";
        public LoadPlaylistWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var currentDirName = Directory.GetCurrentDirectory();
            currentDirName += "\\Data\\SavedPlaylist";
            Directory.CreateDirectory(currentDirName);
            DirectoryInfo dir = new DirectoryInfo(currentDirName);
            FileInfo[] jsonFiles = dir.GetFiles("*.json");
            foreach (FileInfo jsonFile in jsonFiles)
            {
                LoadPlaylist.Items.Add(Path.GetFileNameWithoutExtension(jsonFile.Name));
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (LoadPlaylist.SelectedIndex!=-1)
            {
                int index = LoadPlaylist.SelectedIndex;
                ChosenPlaylist = LoadPlaylist.Items[index].ToString();
                DialogResult = true;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
