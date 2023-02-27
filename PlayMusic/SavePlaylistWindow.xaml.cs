using System;
using System.Collections.Generic;
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

namespace PlayMusic
{
    /// <summary>
    /// Interaction logic for SavePlaylistWindow.xaml
    /// </summary>
    public partial class SavePlaylistWindow : Window
    {
        public SavePlaylistWindow()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SaveNameTextbox.Text == "")
            {
                ErrorMsg.Text = "Please enter Playlist's name!";
            }
            else
            {
                DialogResult = true;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult= false;
        }
    }
}
