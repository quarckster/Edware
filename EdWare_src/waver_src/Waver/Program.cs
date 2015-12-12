using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.IO;
using System.Threading;

namespace Waver
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("{0} v.{1}", System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);

            if (args == null || args.Length == 0)
            {
                Console.WriteLine("Filename not specified. Use shell command: waver.exe <filename> [>filepath.log]");
                return -1;
            }
            string filename = args[0];

            if (!File.Exists(filename))
            {
                Console.WriteLine("File not found or bad file name/path \"{0}\"", filename);
                return -2;
            }

            try
            {
                using (var output = new OutputDevice())
                {
                    Console.WriteLine("Output device \"{0}\" created", output.Device.FriendlyName);
                    
                    using (var wasapi = new WasapiOut(output.Device, AudioClientShareMode.Exclusive, false, 25))
                    {
                        Console.WriteLine("Wasapi interface created with exclusive mode, not event driven, latency - 25");

                        using (var reader = new AudioFileReader(filename))
                        {
                            Console.WriteLine(
@"AudioFileReader created:
    filename - {0},
    length - {1}({2:hh:mm:ss.fff}),
    encoding - {3},
    sample rate - {4},
    bits per sample - {5},
    channels - {6},
    average bytes per second - {7},
    block align - {8},
    extra size - {9}",
                                filename,
                                reader.Length,
                                reader.TotalTime,
                                reader.WaveFormat.Encoding,
                                reader.WaveFormat.SampleRate,
                                reader.WaveFormat.BitsPerSample,
                                reader.WaveFormat.Channels,
                                reader.WaveFormat.AverageBytesPerSecond,
                                reader.WaveFormat.BlockAlign,
                                reader.WaveFormat.ExtraSize);
                            
                            wasapi.Init(reader);
                            Console.WriteLine(
@"Wasapi interface initiated, output format:
    encoding - {0},
    sample rate - {1},
    bits per sample - {2},
    channels - {3},
    average bytes per second - {4},
    block align - {5},
    extra size - {6}", 
                                wasapi.OutputWaveFormat.Encoding,
                                wasapi.OutputWaveFormat.SampleRate,
                                wasapi.OutputWaveFormat.BitsPerSample,
                                wasapi.OutputWaveFormat.Channels,
                                wasapi.OutputWaveFormat.AverageBytesPerSecond,
                                wasapi.OutputWaveFormat.BlockAlign,
                                wasapi.OutputWaveFormat.ExtraSize);

                            Console.WriteLine("Start play...");
                            wasapi.Play();

                            do
                            {
                                Thread.Sleep(200);
                            }
                            while (wasapi.PlaybackState == PlaybackState.Playing);

                            Console.WriteLine("Stop play");
                        }
                        Console.WriteLine("AudioFileReader disposed");
                    }
                    Console.WriteLine("Wasapi interface disposed");
                }
                Console.WriteLine("Output device disposed");
            }
            catch (Exception e)
            {
                Console.WriteLine("EXECUTION ERROR:");
                Console.WriteLine(e.ToString());
                return -3;
            }

            Console.WriteLine("Complete with no errors");
            return 0;
        }

        private class OutputDevice : IDisposable
        {
            public MMDeviceEnumerator enumerator;
            public MMDevice Device { get; private set; }

            public OutputDevice()
            {
                enumerator = new MMDeviceEnumerator();
                Device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
            }

            public void Dispose()
            {
                this.Device = null;
                this.enumerator = null;
            }
        }
    }
}
