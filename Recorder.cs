using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Channels;
using System.Drawing.Imaging;
using System.Drawing;
using Microsoft.Win32;
using System.Windows;
using NUnit.Framework;

namespace SpeechRecognition
{
    public class Recorder
    {
        private WaveFileWriter writer;
        private string outputFilePath;
        private bool closing;
        private WaveInEvent waveIn;

        public Recorder()
        { 

        waveIn = new WaveInEvent();

        writer = null;
        closing = false;

            waveIn.DataAvailable += (s, a) =>
            {
                writer.Write(a.Buffer, 0, a.BytesRecorded);
                if (writer.Position > waveIn.WaveFormat.AverageBytesPerSecond * 30)
                {
                    waveIn.StopRecording();
                }
            };

            waveIn.RecordingStopped += (s, a) =>
            {
                writer?.Dispose();
                writer = null;
                if (closing)
                {
                    waveIn.Dispose();
                }
            };

        }

        public bool Init()
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Wav file|*.wav";
            saveFileDialog1.Title = "Save an wav File";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
            {
                outputFilePath = saveFileDialog1.FileName;
                return true;
            }
            else
            {                
                return false;
            }

        }

        public void Start()
        {
            
            writer = new WaveFileWriter(outputFilePath, waveIn.WaveFormat);
            waveIn.StartRecording();
        }

        public void Stop()
        {
            waveIn.StopRecording();
        }

        public void Close()
        {
            closing = true;
            waveIn.StopRecording();
        }

        public Bitmap GetImage()
        {
            OpenFileDialog fileDlg = new OpenFileDialog();
            if (fileDlg.ShowDialog() == true)
            {
                using (WaveFileReader reader = new WaveFileReader(fileDlg.FileName))
                {
                    Assert.AreEqual(16, reader.WaveFormat.BitsPerSample, "Only works with 16 bit audio");
                    byte[] buffer = new byte[reader.Length];
                    int read = reader.Read(buffer, 0, buffer.Length);
                    Int16[] sampleBuffer = new Int16[read /2];
                    
                    Buffer.BlockCopy(buffer, 0, sampleBuffer, 0, read);

                    double[] audio = new double[read /2];

                    int x = 0;
                    foreach(int i in sampleBuffer)
                    {
                        audio[x++] = (double)i;
                    }

                    var sampleRate = waveIn.WaveFormat.SampleRate;

                    // Window your signal
                    double[] window = FftSharp.Window.Hanning(audio.Length);
                    FftSharp.Window.ApplyInPlace(window, audio);

                    // create an array of audio sample times to aid plotting
                    double[] times = ScottPlot.DataGen.Consecutive(audio.Length, 1000d / sampleRate);

                    // plot the sample audio
                    var plt1 = new ScottPlot.Plot(800, 300);
                    plt1.PlotScatter(times, audio, markerSize: 1);
                    plt1.Title("Audio Signal");
                    plt1.YLabel("Amplitude");
                    plt1.XLabel("Time (ms)");
                    plt1.AxisAuto(0);
                    

                    var bitmap = plt1.GetBitmap();
                    return bitmap;

                }
            }
            return null;
        }

        public Bitmap FFT()
        {
            OpenFileDialog fileDlg = new OpenFileDialog();
            if (fileDlg.ShowDialog() == true)
            {
                using (WaveFileReader reader = new WaveFileReader(fileDlg.FileName))
                {
                    Assert.AreEqual(16, reader.WaveFormat.BitsPerSample, "Only works with 16 bit audio");
                    byte[] buffer = new byte[reader.Length];
                    int read = reader.Read(buffer, 0, buffer.Length);
                    Int16[] sampleBuffer = new Int16[read / 2];
                    
                    Buffer.BlockCopy(buffer, 0, sampleBuffer, 0, read);

                    double[] audio;

                    var sampleRate = waveIn.WaveFormat.SampleRate;

                    if (!FftSharp.Transform.IsPowerOfTwo(sampleBuffer.Length))
                    {
                        int targetLength = 1;
                        while (targetLength < sampleBuffer.Length)
                            targetLength *= 2;

                        audio = new double[targetLength];

                        int x = 0;
                        foreach (int i in sampleBuffer)
                        {
                            audio[x++] = (double)i;
                        }

                        for(;x<targetLength;x++)
                        {
                            audio[x] = 0;
                        }
                    }
                    else
                    {
                        audio = new double[read / 2];

                        int x = 0;
                        foreach (int i in sampleBuffer)
                        {
                            audio[x++] = (double)i;
                        }
                    }

                    // Window your signal
                    double[] window = FftSharp.Window.Hanning(audio.Length);
                    FftSharp.Window.ApplyInPlace(window, audio);

                    // For audio we typically want the FFT amplitude (in dB)
                    double[] fftPower = FftSharp.Transform.FFTpower(audio);

                    // Create an array of frequencies for each point of the FFT
                    double[] freqs = FftSharp.Transform.FFTfreq(sampleRate, fftPower.Length);
                  
                    // plot the FFT amplitude
                    
                    var plt2 = new ScottPlot.Plot(800, 300);
                    plt2.PlotScatter(freqs, fftPower, markerSize: 1);
                    plt2.Title("Fast Fourier Transformation (FFT)");
                    plt2.YLabel("Power (dB)");
                    plt2.XLabel("Frequency (Hz)");
                    plt2.AxisAuto(0);

                    var bitmap = plt2.GetBitmap();
                    return bitmap;
                }
            }
            return null;
        }


    }
}
