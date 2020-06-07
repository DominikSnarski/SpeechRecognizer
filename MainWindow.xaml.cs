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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SpeechRecognition
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Recognizer recognizer;
        private Recorder recorder;
        public MainWindow()
        {
            InitializeComponent();
            recognizer = new Recognizer();
            recorder = new Recorder();
        }

        private async void StartSpeechRecognition(object sender, RoutedEventArgs e)
        {
            SpeechRecognitionButton.IsEnabled = false;
            await recognizer.RecognizeSpeechAsync();
            SpeechContent.Content = recognizer.text;
            SpeechLanguage.Content = recognizer.language;
            SpeechRecognitionButton.IsEnabled = true;
        }

        private void StartRecording(object sender, RoutedEventArgs e)
        {
            var result = recorder.Init();
            if(!result)
            {
                MessageBox.Show("Error! Utworzenie pliku nie powiodło się.");
                return;
            }
            recorder.Start();
            RecordingStatus.Content = "Nagrywanie rozpoczęte";
            RStart.IsEnabled = false;
            RStop.IsEnabled = true;
        }

        private void StopRecording(object sender, RoutedEventArgs e)
        {
            recorder.Stop();
            RecordingStatus.Content = "Nagrywanie zakończone";
            RStart.IsEnabled = true;
            RStop.IsEnabled = false;
        }

        private void RecordingImage(object sender, RoutedEventArgs e)
        {
            var image = recorder.GetImage();
            if (image != null)
            {
                var window = new ImageWindow(image);
                window.Show();
            }
            else
            {
                MessageBox.Show("ERROR! Nie udało się otworzyć wybranego pliku .wav.");
            }

        }

        private void FFT(object sender, RoutedEventArgs e)
        {
            var image = recorder.FFT();
            if (image != null)
            {
                var window = new ImageWindow(image);
                window.Show();
            }
            else
            {
                MessageBox.Show("ERROR! Nie udało się otworzyć wybranego pliku .wav.");
            }

        }

        private void Exit(object sender, RoutedEventArgs e)
        {
            recorder.Close();
            this.Close();

        }

    }
}
