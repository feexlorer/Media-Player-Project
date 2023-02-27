using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using static System.Net.WebRequestMethods;
using Newtonsoft.Json;
using File = System.IO.File;
using System.Data;
using System.Runtime.InteropServices;
using WindowsInput.Native;
using WindowsInput;
using NHotkey.Wpf;
using NHotkey;
using Eagle._Components.Public;

namespace PlayMusic
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public class PlayList
    {
        public List<Media> playlist { get; set; }
    }
    public class Media
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
    }

    class MusicID3Tag

    {

        public byte[] TAGID = new byte[3];      //  3
        public byte[] Title = new byte[30];     //  30
        public byte[] Artist = new byte[30];    //  30 
        public byte[] Album = new byte[30];     //  30 
        public byte[] Year = new byte[4];       //  4 
        public byte[] Comment = new byte[30];   //  30 
        public byte[] Genre = new byte[1];      //  1

    }

    public partial class MainWindow : Window
    {


 
        private string _currentPlaying = string.Empty;
        private bool _playing = false;
        private double volume = 0.2;
        private bool _randomPlay = false;
        private bool _checkStart = false;
        private bool _checkRepeat = false;
        private MediaElement _tempPlayer;
        public MainWindow()
        {
            InitializeComponent();
            VolumeSlide.Maximum = 1;
            VolumeSlide.Value = volume;
            Player.MediaEnded += handleMediaEnd;
            //HotkeyManager.Current.Initialize(this);
            // Register the hotkeys
            HotkeyManager.Current.AddOrReplace("Play", Key.F1, ModifierKeys.Control, OnPlay);
            HotkeyManager.Current.AddOrReplace("Pause", Key.F2, ModifierKeys.Control, OnPause);
            HotkeyManager.Current.AddOrReplace("Skip", Key.F3, ModifierKeys.Control, OnSkip);

        }
        // Handle the play hotkey
        private void OnPlay(object sender, HotkeyEventArgs e)
        {
            this._playing = true;
            _timer.Start();
            btnPlayPause.Kind = MaterialDesignThemes.Wpf.PackIconKind.PauseCircleOutline;
            Player.Play();
            ListMusic.Focus();
        }

        // Handle the pause hotkey
        private void OnPause(object sender, HotkeyEventArgs e)
        {
            this._playing = false;
            _timer.Stop();
            Player.Pause();
            btnPlayPause.Kind = MaterialDesignThemes.Wpf.PackIconKind.PlayCircleOutline;
        }

        // Handle the skip hotkey
        private void OnSkip(object sender, HotkeyEventArgs e)
        {
            var currentIndex = ListMusic.SelectedIndex;
            var n = ListMusic.Items.Count - 1;
            if (currentIndex < n)
            {
                ListMusic.SelectedIndex = currentIndex + 1;
                ListBox_MouseDoubleClick(sender, e);
            }
            ListMusic.Focus();
        }
        DispatcherTimer _timer;
        // Danh sách phát hiện tại
        public PlayList _currentPlaylist = new PlayList()
        {
            playlist = new List<Media>()
        };
        // Danh sách đã xem gần đây ( tối đa 10 )
        public PlayList _recentlyPlayed = new PlayList()
        {
            playlist = new List<Media>()
        };
        private double _tempValue;
        
        private void ProcessSlide_MouseMove(object sender, MouseEventArgs e)
        {
         
            Point position = e.GetPosition(ProcessSlide);
            _tempValue = ProcessSlide.Minimum + (ProcessSlide.Maximum - ProcessSlide.Minimum) * (position.X / ProcessSlide.ActualWidth);
            TimeSpan newPosition = TimeSpan.FromSeconds(_tempValue);
            PreviewPlayer.Source = Player.Source;
            PreviewPlayer.Position = newPosition;
            PreviewPlayer.Visibility = Visibility.Visible;
            
        }

        static string[] _mediaType = 
        {
            ".WAV", ".MID", ".MIDI", ".WMA", ".MP3", ".OGG", ".RMA",
            ".AVI", ".MP4", ".DIVX", ".WMV",
        };
        static bool IsMediaFile(string path)
        {
            return -1 != Array.IndexOf(_mediaType, System.IO.Path.GetExtension(path).ToUpperInvariant());
        }
        private void Browse_button_Click(object sender, RoutedEventArgs e)
        {
            var screen = new OpenFileDialog();
            screen.Multiselect = true;

            if (screen.ShowDialog() == true)
            {
                for (int i = 0; i < screen.FileNames.Length; i++)
                {
                        if (IsMediaFile(screen.SafeFileNames[i]))
                        {
                            _currentPlaylist.playlist.Add(new Media()
                            {
                                FilePath = screen.FileNames[i],
                                FileName = screen.SafeFileNames[i]
                            });
                            this.ListMusic.Items.Add(screen.SafeFileNames[i]);
                            break;
                        }
                }
                ListMusic.SelectedIndex = 0;
                ListMusic.Focus();
            }
        }

        private void _timer_Tick(object? sender, EventArgs e)
        {
            int hours = Player.Position.Hours;
            int minutes = Player.Position.Minutes;
            int seconds = Player.Position.Seconds;
            if (hours == 0)
            {
                if (minutes >= 0 && minutes < 10)
                {
                    if (seconds >= 0 && seconds < 10)
                    {
                        progressSlider.Text = $"0{minutes}:0{seconds}";
                    }
                    else
                    {
                        progressSlider.Text = $"0{minutes}:{seconds}";
                    }
                }
                else
                {
                    if (seconds >= 0 && seconds < 10)
                    {
                        progressSlider.Text = $"{minutes}:0{seconds}";
                    }
                    else
                    {
                        progressSlider.Text = $"{minutes}:{seconds}";
                    }
                }
            }
            else
            {
                if (minutes >= 0 && minutes < 10)
                {
                    if (seconds >= 0 && seconds < 10)
                    {
                        progressSlider.Text = $"{hours}:0{minutes}:0{seconds}";
                    }
                    else
                    {
                        progressSlider.Text = $"{hours}:0{minutes}:{seconds}";
                    }
                }
                else
                {
                    if (seconds >= 0 && seconds < 10)
                    {
                        progressSlider.Text = $"{hours}:{minutes}:0{seconds}";
                    }
                    else
                    {
                        progressSlider.Text = $"{hours}:{minutes}:{seconds}";
                    }
                }
            }
            //Title = $"{hours}:{minutes}:{seconds}";
            ProcessSlide.Value = Player.Position.TotalSeconds;
            //ProcessSlide.Value = Player.Position.TotalSeconds / ProcessSlide.Maximum;

        }

        private void player_MediaOpened(object sender, RoutedEventArgs e)
        {
            int hours = Player.NaturalDuration.TimeSpan.Hours;
            int minutes = Player.NaturalDuration.TimeSpan.Minutes;
            int seconds = Player.NaturalDuration.TimeSpan.Seconds;
            if (hours == 0)
            {
                if (minutes >= 0 && minutes < 10)
                {
                    if (seconds >= 0 && seconds < 10)
                    {
                        TotalSlider.Text = $"0{minutes}:0{seconds}";
                    }
                    else
                    {
                        TotalSlider.Text = $"0{minutes}:{seconds}";
                    }
                }
                else
                {
                    if (seconds >= 0 && seconds < 10)
                    {
                        TotalSlider.Text = $"{minutes}:0{seconds}";
                    }
                    else
                    {
                        TotalSlider.Text = $"{minutes}:{seconds}";
                    }
                }
            }
            else
            {
                if (minutes >= 0 && minutes < 10)
                {
                    if (seconds >= 0 && seconds < 10)
                    {
                        TotalSlider.Text = $"{hours}:0{minutes}:0{seconds}";
                    }
                    else
                    {
                        TotalSlider.Text = $"{hours}:0{minutes}:{seconds}";
                    }
                }
                else
                {
                    if (seconds >= 0 && seconds < 10)
                    {
                        TotalSlider.Text = $"{hours}:{minutes}:0{seconds}";
                    }
                    else
                    {
                        TotalSlider.Text = $"{hours}:{minutes}:{seconds}";
                    }
                }
            }


            // cập nhật max value của slider
            ProcessSlide.Maximum = Player.NaturalDuration.TimeSpan.TotalSeconds;

        }

        private void Play_Pause_Click(object sender, RoutedEventArgs e)
        {
            if (ListMusic.Items.Count < 1)
            {
                return;
            }

            if (_checkStart == false)
            {
                if (ListMusic.SelectedIndex != -1)
                {

                }
                else if (_randomPlay == true)
                {
                    int n = ListMusic.Items.Count;
                    Random r = new Random();
                    int rInt = 0;
                    if (n > 1)
                    {
                        do
                        {
                            rInt = r.Next(0, n - 1);
                        } while (rInt == ListMusic.SelectedIndex);
                    }
                    ListMusic.SelectedIndex = rInt;
                    ListMusic.Focus();

                }
                else
                {
                    ListMusic.SelectedIndex = 0;
                }
                ListBox_MouseDoubleClick(sender, e);
                _checkStart = true;
                return;
            }

            if (this._playing == false)
            {
                this._playing = true;
                _timer.Start();
                btnPlayPause.Kind = MaterialDesignThemes.Wpf.PackIconKind.PauseCircleOutline;
                Player.Play();
                ListMusic.Focus();
            }
            else
            {
                this._playing = false;
                _timer.Stop();
                Player.Pause();
                btnPlayPause.Kind = MaterialDesignThemes.Wpf.PackIconKind.PlayCircleOutline;
                ListMusic.Focus();
                //play_pause_icon.Source = bitmap;
            }
        }

        private void Slide_valueChange(object sender, EventArgs e)
        {
            double value = ProcessSlide.Value;
            TimeSpan newPosition = TimeSpan.FromSeconds(value);
            Player.Position = newPosition;
        }

        private void ListBox_MouseDoubleClick(object sender, EventArgs e)
        {
            if (File.Exists(_currentPlaylist.playlist[ListMusic.SelectedIndex].FilePath))
            {
                _checkStart = true;

                if (ListMusic.SelectedIndex != -1)
                {
                    int chose = ListMusic.SelectedIndex;
                    //Load file nhac
                    //var n = filePath;
                    _currentPlaying = _currentPlaylist.playlist[chose].FilePath;
                    SaveRecentlyPlayedFiles(_currentPlaylist.playlist[chose]);
                    string json = JsonConvert.SerializeObject(_recentlyPlayed);
                    Directory.CreateDirectory(".\\Data\\RecentlyPlayed");

                    File.WriteAllText(".\\Data\\RecentlyPlayed\\RecentlyPlayed.json", json);
                    Player.Play();
                    Player.Stop();
                    Player.Source = new Uri(_currentPlaying, UriKind.Absolute);
                    _tempPlayer = new MediaElement();
                    _tempPlayer.Source = new Uri(_currentPlaying, UriKind.Absolute);
                    Name_playmusic.Text = ListMusic.SelectedItem.ToString();
                    //Tinh time
                    _timer = new DispatcherTimer();
                    _timer.Interval = new TimeSpan(0, 0, 0, 1, 0); ;
                    _timer.Tick += _timer_Tick;

                    //Tao lai icon khi chuyen nhac

                    _timer.Start();
                    Player.Play();
                    btnPlayPause.Kind = MaterialDesignThemes.Wpf.PackIconKind.PauseCircleOutline;
                    //play_pause_icon.Source = bitmap;
                    _playing = true;

                    Player.Volume = VolumeSlide.Value;
                }
            }
            else
            {
                MessageBox.Show("Media file not found!");
            }
        }

        //xử lý volume
        private void VolumeSlide_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Player.Volume = VolumeSlide.Value;
        }

        private void btn_muted_Click(object sender, RoutedEventArgs e)
        {
            Player.Volume = 0;
            VolumeSlide.Value = 0;
            ListMusic.Focus();
        }

        private void btn_maxVolume_Click(object sender, RoutedEventArgs e)
        {
            Player.Volume = 1;
            VolumeSlide.Value = 1;
            ListMusic.Focus();
        }

        // xử lý khi media chạy xong 1 file
        private void handleMediaEnd(object sender, RoutedEventArgs e)
        {
            if (_checkRepeat)
            {
                ListMusic.Focus();
                ListBox_MouseDoubleClick(sender, e);
                return;
            }

            if (_randomPlay == true)
            {
                int n = ListMusic.Items.Count;
                Random r = new Random();
                int rInt = 0;
                if (n > 1)
                {
                    do
                    {
                        rInt = r.Next(0, n - 1);
                    } while (rInt == ListMusic.SelectedIndex);
                }

                ListMusic.SelectedIndex = rInt;
                ListMusic.Focus();
                ListBox_MouseDoubleClick(sender, e);
            }
            else if (_randomPlay == false)
            {
                var currentIndex = ListMusic.SelectedIndex;
                var n = ListMusic.Items.Count - 1;
                if (currentIndex < n)
                {
                    ListMusic.SelectedIndex = currentIndex + 1;
                    ListBox_MouseDoubleClick(sender, e);
                }
                ListMusic.Focus();
            }
        }

        // chế độ chơi ngẫu nhiên
        private void Random_button_Click(object sender, RoutedEventArgs e)
        {
            if (_randomPlay == true)
            {
                iconShuffle.Foreground = Brushes.Black;
                _randomPlay = false;
            }
            else
            {
                _randomPlay = true;
                iconShuffle.Foreground = Brushes.DarkOrchid;
            }
            ListMusic.Focus();
        }

        //chơi file trước
        private void Prev_button_Click(object sender, RoutedEventArgs e)
        {
            var currentIndex = ListMusic.SelectedIndex;
            if (currentIndex > 0)
            {
                ListMusic.SelectedIndex = currentIndex - 1;
                ListBox_MouseDoubleClick(sender, e);
            }
            ListMusic.Focus();
        }

        //chơi file sau
        private void Next_button_Click(object sender, RoutedEventArgs e)
        {
            var currentIndex = ListMusic.SelectedIndex;
            var n = ListMusic.Items.Count - 1;
            if (currentIndex < n)
            {
                ListMusic.SelectedIndex = currentIndex + 1;
                ListBox_MouseDoubleClick(sender, e);
            }
            ListMusic.Focus();
        }

        // Lưu Playlist
        private void SavePlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPlaylist.playlist.Count == 0)
            {
                MessageBox.Show("Playlist is empty!");
            }
            else
            {
                string json = JsonConvert.SerializeObject(_currentPlaylist);
                Directory.CreateDirectory(".\\Data\\SavedPlaylist");
                var screen = new SavePlaylistWindow();
                string saveName = "";
                if (screen.ShowDialog() == true)
                {
                    saveName = $"{screen.SaveNameTextbox.Text}.json";
                }
                else
                {
                    //Do nothing
                }
                File.WriteAllText($".\\Data\\SavedPlaylist\\{saveName}", json);
                iconheart.Foreground = Brushes.HotPink;
            }
        }
        // Chọn Playlist đã lưu
        private void LoadPlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            var screen = new LoadPlaylistWindow();
            if (screen.ShowDialog() == true)
            {
                string playlistName = screen.ChosenPlaylist;
                if (playlistName != "")
                {
                    var json = File.ReadAllText($".\\Data\\SavedPlaylist\\{playlistName}.json");
                    _currentPlaylist = JsonConvert.DeserializeObject<PlayList>(json);
                    for (int i = 0; i < _currentPlaylist.playlist.Count; i++)
                    {
                        ListMusic.Items.Add(_currentPlaylist.playlist[i].FileName);
                    }
                }
                iconheart.Foreground = Brushes.HotPink;
            }
            else
            {
                //Do nothing
            }

        }
        // Chọn file trong mục Recently played (Double click file sẽ được thêm vào danh sách phát hiện tại)
        private void RecentlyPlayed_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int index = RecentlyPlayed.SelectedIndex;
            _currentPlaylist.playlist.Add(new Media()
            {
                FileName = _recentlyPlayed.playlist[index].FileName,
                FilePath = _recentlyPlayed.playlist[index].FilePath,
            });
            ListMusic.Items.Add(_recentlyPlayed.playlist[index].FileName);
        }
        // Hàm lưu và load những file được mở gần nhất
        public void SaveRecentlyPlayedFiles(Media playing)
        {
            if (_recentlyPlayed.playlist.Count == 10)
            {
                _recentlyPlayed.playlist.RemoveAt(0);
            }
            _recentlyPlayed.playlist.Add(new Media()
            {
                FileName = playing.FileName,
                FilePath = playing.FilePath,
            });
            _recentlyPlayed.playlist = _recentlyPlayed.playlist.Distinct().ToList();
            string json = JsonConvert.SerializeObject(_recentlyPlayed);
            Directory.CreateDirectory(".\\Data\\RecentlyPlayed");

            File.WriteAllText(".\\Data\\RecentlyPlayed\\RecentlyPlayed.json", json);
            LoadRecentlyPlayedFiles();
        }
        public void LoadRecentlyPlayedFiles()
        {
            _recentlyPlayed.playlist.Clear();
            RecentlyPlayed.Items.Clear();
            Directory.CreateDirectory(".\\Data\\RecentlyPlayed");
            File.AppendAllText(".\\Data\\RecentlyPlayed\\RecentlyPlayed.json", "");
            var json = File.ReadAllText(".\\Data\\RecentlyPlayed\\RecentlyPlayed.json");
            if (json != "" && _recentlyPlayed != null)
            {
                _recentlyPlayed = JsonConvert.DeserializeObject<PlayList>(json);
                for (int i = 0; i < _recentlyPlayed.playlist.Count; i++)
                {
                    RecentlyPlayed.Items.Add(_recentlyPlayed.playlist[i].FileName);
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadRecentlyPlayedFiles();
        }

        private void Loop_button_Click(object sender, RoutedEventArgs e)
        {
            var selected = ListMusic.SelectedIndex;
            if (selected == -1)
            {
                return;
            }

            if (_checkRepeat == true)
            {
                iconRepeat.Kind = MaterialDesignThemes.Wpf.PackIconKind.RepeatOff;
                _checkRepeat = false;
            }
            else
            {
                iconRepeat.Kind = MaterialDesignThemes.Wpf.PackIconKind.Repeat;
                _checkRepeat = true;
            }
            ListMusic.Focus();
        }

        private void speedRatioSlider_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Player.SpeedRatio = Convert.ToDouble(speedRatioSlider.SelectedValue);
            //ComboBoxItem value = (ComboBoxItem)speedRatioSlider.SelectedItem;
            //Player.SpeedRatio = value.GetValue;

        }

        [DllImport("user32.dll")]
        static extern uint GetDoubleClickTime();
        System.Timers.Timer timeClick = new System.Timers.Timer((int)GetDoubleClickTime())
        {
            AutoReset = false
        };

        bool fullScreen = false;
        public void GoFullScreen()
        {
            if (!timeClick.Enabled)
            {
                timeClick.Enabled = true;
                return;
            }
            if (timeClick.Enabled)
            {
                if (!fullScreen)
                {
                    LayoutRoot.Children.Remove(Player);
                    this.Background = new SolidColorBrush(Colors.Black);
                    Player.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
                    Player.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
                    this.Content = Player;
                    this.WindowStyle = WindowStyle.None;
                    this.WindowState = WindowState.Maximized;
                }
                else
                {
                    this.Content = LayoutRoot;
                    Player.Width = 720;
                    Player.Height = 400;
                    LayoutRoot.Children.Add(Player);
                    this.Background = new SolidColorBrush(Colors.White);

                    this.WindowStyle = WindowStyle.SingleBorderWindow;
                    this.WindowState = WindowState.Normal;
                }
                fullScreen = !fullScreen;
            }
        }

        private void Player_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            GoFullScreen();
        }

        private void FullScreenButton_Click(object sender, RoutedEventArgs e)
        {
            if (!fullScreen)
            {
                LayoutRoot.Children.Remove(Player);
                this.Background = new SolidColorBrush(Colors.Black);
                Player.Width = System.Windows.SystemParameters.PrimaryScreenWidth;
                Player.Height = System.Windows.SystemParameters.PrimaryScreenHeight;
                this.Content = Player;
                this.WindowStyle = WindowStyle.None;
                this.WindowState = WindowState.Maximized;
            }
            else
            {
                this.Content = LayoutRoot;
                Player.Width = 720;
                Player.Height = 400;
                LayoutRoot.Children.Add(Player);
                this.Background = new SolidColorBrush(Colors.White);

                this.WindowStyle = WindowStyle.SingleBorderWindow;
                this.WindowState = WindowState.Normal;
            }
            fullScreen = !fullScreen;
        }

        private void DeletePlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            if (ListMusic.SelectedIndex != -1)
            {
                int chose = ListMusic.SelectedIndex;
                ListMusic.Items.RemoveAt(chose);
                _currentPlaylist.playlist.RemoveAt(chose);
                icondustbin.Foreground = Brushes.White;

            }
        }
        private void MouseDown(object sender, EventArgs e)
        {
            icondustbin.Foreground = Brushes.Blue;
        }

    } 
}
