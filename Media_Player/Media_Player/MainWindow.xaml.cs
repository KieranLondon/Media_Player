using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;

namespace Media_Player
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool mediaPlayerIsPlaying = false;
        private bool userIsDraggingSlider = false;

        public MainWindow()
        {
            InitializeComponent();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
        }

        // For Slider and timer
        private void timer_Tick(object sender, EventArgs e)
        {
            if ((mePlayer.Source != null) && (mePlayer.NaturalDuration.HasTimeSpan) && (!userIsDraggingSlider))
            {
                sliProgress.Minimum = 0;
                sliProgress.Maximum = mePlayer.NaturalDuration.TimeSpan.TotalSeconds;
                sliProgress.Value = mePlayer.Position.TotalSeconds;
            }
        }

        // -- File Menu --
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        // -- Open --
        private void Open_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                // Open file
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Media files (*.mp3;*.mpg;*.mpeg)|*.mp3;*.mpg;*.mpeg|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == true)
                {
                    mePlayer.Source = new Uri(openFileDialog.FileName);

                    // Current song info to show
                    using (TagLib.File taggedFile = TagLib.File.Create(mePlayer.Source.LocalPath))
                    {
                        Song_Title.Text = taggedFile.Tag.Title;
                        TitleEntry.Text = taggedFile.Tag.Title;
                        Song_Artist.Text = taggedFile.Tag.FirstPerformer;
                        ArtistEntry.Text = taggedFile.Tag.FirstPerformer;
                    }
                }
            }
            catch
            {
                MessageBox.Show("There was an error loading your song!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // -- Play --
        private void Play_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (mePlayer != null) && (mePlayer.Source != null);
        }

        private void Play_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mePlayer.Play();
            mediaPlayerIsPlaying = true;
        }

        // -- Pause --
        private void Pause_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mediaPlayerIsPlaying;
        }

        private void Pause_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mePlayer.Pause();
        }

        // -- Stop --
        private void Stop_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = mediaPlayerIsPlaying;
        }

        private void Stop_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            mePlayer.Stop();
            mediaPlayerIsPlaying = false;
        }

        // -- Progress Bar --
        private void sliProgress_DragStarted(object sender, DragStartedEventArgs e)
        {
            userIsDraggingSlider = true;
        }

        private void sliProgress_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            userIsDraggingSlider = false;
            mePlayer.Position = TimeSpan.FromSeconds(sliProgress.Value);
        }

        private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Time.Text = TimeSpan.FromSeconds(sliProgress.Value).ToString(@"hh\:mm\:ss");
        }

        // -- Start / Stop edits --
        private void Edit_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (mePlayer != null) && (mePlayer.Source != null);
        }

        private void Edit_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (EntryPanel.Visibility == Visibility.Hidden) 
            {
                EntryPanel.Visibility = Visibility.Visible;
            }else
            {
                EntryPanel.Visibility = Visibility.Hidden;
            }
        }

        // -- Save --
        private void Save_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (mePlayer != null) && (mePlayer.Source != null);
        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                // Check if the media is playing if so stop
                if (mediaPlayerIsPlaying)
                {
                    mePlayer.Stop();
                    mediaPlayerIsPlaying = false;
                }

                // Open tag file and remove the source of media so it can save
                using (TagLib.File taggedFile = TagLib.File.Create(mePlayer.Source.LocalPath))
                {
                    mePlayer.Source = null;
                    taggedFile.Tag.Title = TitleEntry.Text;
                    taggedFile.Tag.Performers = new string[] { ArtistEntry.Text };
                    taggedFile.Save();
                    MessageBox.Show("Song's meta data saved", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch
            {
                MessageBox.Show("There was an error saving your song's meta data!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
