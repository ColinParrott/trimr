using HandyControl.Data;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace trimr
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private DispatcherTimer playbackTimer;
        private bool isDragging;
        private bool videoLoaded;
        private Uri inputVideoPath;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnLoadFromFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Filter = "Video files|*.mp4;*.wmv;*.webm|" + "MP4 video (*.mp4)|*.mp4|WMV video (*.wmv)|(*.wmv)|WebM video (*.webm)|(*.webm)|AVI video (*.avi)|(*.avi)"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                inputVideoPath = new Uri(openFileDialog.FileName);
                videoElement.Source = inputVideoPath;
            }
        }

        private async void BtnGenVideo_Click(object sender, RoutedEventArgs e)
        {
            string ext = Path.GetExtension(videoElement.Source.LocalPath);
            SaveFileDialog saveFileDialog = new()
            {
                Filter = string.Format(new CultureInfo("en-GB"), "Video file (*{0})|*{0}", ext),
                InitialDirectory = inputVideoPath.LocalPath
            };

            ShowGeneratingUI();
            if (saveFileDialog.ShowDialog() == true)
            {
                TimeSpan clipDuration = TimeSpan.FromMilliseconds(videoSlider.ValueEnd - videoSlider.ValueStart);
                Uri fileUri = new(saveFileDialog.FileName);
                Task<bool> genTask = VideoGenerator.GenerateVideo(videoElement.Source, fileUri, TimeSpan.FromMilliseconds(videoSlider.ValueStart), clipDuration, ProgressMonitor);
                if (await genTask)
                {
                    _ = HandyControl.Controls.MessageBox.Success("Video generated successfully");
                    progressBar.Value = 0;
                }
                else
                {
                   _ = HandyControl.Controls.MessageBox.Error("Failed to generate video");
                }
            }
            ShowClippingUI();
        }
        private void ProgressMonitor(double d)
        {
            Debug.WriteLine("Progress: " + d);
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                progressBar.Value = Math.Min(100, d);
            }));
        }

        private void ShowGeneratingUI()
        {
            btnLoadFromFile.IsEnabled = false;
            btnGenVideo.IsEnabled = false;
            btnGenVideo.IsChecked = true;
            videoSlider.Visibility = Visibility.Collapsed;
            progressBar.Visibility = Visibility.Visible;
        }

        private void ShowClippingUI()
        {
            btnLoadFromFile.IsEnabled = true;
            btnGenVideo.IsEnabled = true;
            btnGenVideo.IsChecked = false;
            progressBar.Visibility = Visibility.Collapsed;
            videoSlider.Visibility = Visibility.Visible;
        }

        private void VideoElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            videoElement.Position = TimeSpan.FromMilliseconds(videoSlider.ValueStart);
            videoElement.Play();
        }

        private void VideoElement_MediaOpened(object sender, RoutedEventArgs e)
        {
            videoElement.Play();
            videoSlider.Visibility = Visibility.Visible;
            videoSlider.Maximum = videoElement.NaturalDuration.TimeSpan.TotalMilliseconds;
            videoSlider.ValueStart = videoSlider.Minimum;
            videoSlider.ValueEnd = videoSlider.Maximum;
            videoSlider.TickFrequency = videoSlider.Maximum / 30;
            timePanel.Visibility = Visibility.Visible;

            playbackTimer = new();
            playbackTimer.Tick += PlaybackTimer_Tick;
            playbackTimer.Interval = TimeSpan.FromMilliseconds(100);
            playbackTimer.Start();

            Uri uri = videoElement.Source;
            if (uri.IsFile)
            {
                videoTitle.Text = Path.GetFileName(uri.LocalPath);
                videoCurrentTime.Text = Utils.MsToTimeString(0);
            }
            btnGenVideo.IsEnabled = true;
            videoElement.LoadedBehavior = System.Windows.Controls.MediaState.Manual;
            videoLoaded = true;
        }

        private void PlaybackTimer_Tick(object sender, EventArgs e)
        {
            if (videoElement.NaturalDuration.HasTimeSpan)
            {
                double ts = videoElement.Position.TotalMilliseconds;
                videoCurrentTime.Text = Utils.MsToTimeString(ts);
                if (videoElement.Position.TotalMilliseconds >= videoSlider.ValueEnd && !isDragging)
                {
                    videoElement.Position = TimeSpan.FromMilliseconds(videoSlider.ValueStart);
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            videoTitle.Text = "";
            videoCurrentTime.Text = "";
        }

        private void VideoSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<DoubleRange> e)
        {
            if (videoLoaded)
            {
                if(e.NewValue.End != e.OldValue.End)
                {
                    videoElement.Position = TimeSpan.FromMilliseconds(e.NewValue.End);
                }
                else
                {
                    videoElement.Position = TimeSpan.FromMilliseconds(e.NewValue.Start);
                }
            }
        }

        private void VideoSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            isDragging = true;
            videoElement.Pause();
        }

        private void VideoSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            if (videoLoaded)
            {
                videoElement.Position = TimeSpan.FromMilliseconds(videoSlider.ValueStart);
                videoElement.Play();
                isDragging = false;
            }
        }
    }
}
