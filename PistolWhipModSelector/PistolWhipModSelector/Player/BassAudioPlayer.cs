using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using ManagedBass;

namespace PistolWhipModSelector.Player
{
    public class BassAudioPlayer : IDisposable
    {
        private int stream = 0;
        private bool initialized = false;
        private bool vgmstreamAvailable = false;
        private readonly Stopwatch playbackClock = new Stopwatch();
        private double playbackClockOffset = 0.0;

        // bass_vgmstream is not a BASS plugin (it does not export BASSplugin), so
        // BASS_PluginLoad cannot load it. It instead returns a regular BASS
        // HSTREAM through this dedicated API.
        [DllImport("bass_vgmstream.dll", EntryPoint = "BASS_VGMSTREAM_StreamCreate",
            CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private static extern int VgmStreamCreate(string file, BassFlags flags);

        public BassAudioPlayer()
        {
            try
            {
                // Initialize BASS (ManagedBass + native runtime must be present)
                initialized = Bass.Init(-1, 44100);
            }
            catch
            {
                initialized = false;
            }

            vgmstreamAvailable = File.Exists(Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "bass_vgmstream.dll"));
        }

        public void Play(string path)
        {
            Stop();

            if (!initialized)
                throw new InvalidOperationException("BASS not initialized. Ensure ManagedBass.Native.x64 (native) is available and Platform target is x64.");

            if (!File.Exists(path))
                throw new FileNotFoundException(path);

            // Let BASS handle its native formats first, then use vgmstream for
            // formats such as WEM. Both APIs return a BASS HSTREAM.
            BassFlags playbackFlags = BassFlags.Prescan;
            stream = Bass.CreateStream(path, 0, 0, playbackFlags);
            if (stream == 0 && vgmstreamAvailable)
            {
                try
                {
                    stream = VgmStreamCreate(path, playbackFlags);
                }
                catch (DllNotFoundException)
                {
                    vgmstreamAvailable = false;
                }
                catch (EntryPointNotFoundException)
                {
                    vgmstreamAvailable = false;
                }
            }

            if (stream == 0)
                throw new InvalidOperationException("Failed to open stream. Ensure bass_vgmstream.dll and its dependencies are present for WEM files.");

            if (!Bass.ChannelPlay(stream))
                throw new InvalidOperationException("BASS could not start playback: " + Bass.LastError);

            playbackClockOffset = 0.0;
            playbackClock.Restart();
        }

        public void Stop()
        {
            try
            {
                if (stream != 0)
                {
                    try { Bass.ChannelStop(stream); } catch { }
                    try { Bass.StreamFree(stream); } catch { }
                    stream = 0;
                }
                playbackClock.Reset();
                playbackClockOffset = 0.0;
            }
            catch { }
        }

        public double GetPosition()
        {
            if (stream == 0) return 0.0;
            try
            {
                long pos = Bass.ChannelGetPosition(stream, PositionFlags.Bytes);
                double bassPosition = pos >= 0 ? Bass.ChannelBytes2Seconds(stream, pos) : -1.0;
                double clockPosition = playbackClockOffset + playbackClock.Elapsed.TotalSeconds;

                // Some vgmstream-created BASS streams play normally but do not
                // expose a changing byte position. Keep the UI moving in that case.
                return bassPosition > 0.0 ? bassPosition : clockPosition;
            }
            catch { return 0.0; }
        }

        public double GetDuration()
        {
            if (stream == 0) return 0.0;
            try
            {
                long len = Bass.ChannelGetLength(stream, PositionFlags.Bytes);
                if (len <= 0) return 0.0;
                return Bass.ChannelBytes2Seconds(stream, len);
            }
            catch { return 0.0; }
        }

        public void Seek(double seconds)
        {
            if (stream == 0) return;
            try
            {
                long pos = Bass.ChannelSeconds2Bytes(stream, seconds);
                if (Bass.ChannelSetPosition(stream, pos, PositionFlags.Bytes))
                {
                    playbackClockOffset = seconds;
                    playbackClock.Restart();
                }
            }
            catch { }
        }

        // Decode to WAV file using a decode stream
        public void SaveAsWav(string sourcePath, string outWavPath)
        {
            if (!File.Exists(sourcePath)) throw new FileNotFoundException(sourcePath);

            int decodeStream = 0;
            try
            {
                decodeStream = Bass.CreateStream(sourcePath, 0, 0, BassFlags.Decode);
                if (decodeStream == 0 && vgmstreamAvailable)
                    decodeStream = VgmStreamCreate(sourcePath, BassFlags.Decode);
            }
            catch
            {
                decodeStream = 0;
            }

            if (decodeStream == 0) throw new InvalidOperationException("Failed to open decode stream for conversion.");

            try
            {
                // defaults
                int samplerate = 44100;
                int channels = 2;

                try
                {
                    ChannelInfo info;
                    if (Bass.ChannelGetInfo(decodeStream, out info))
                    {
                        samplerate = (int)info.Frequency;
                        channels = info.Channels;
                    }
                }
                catch { }

                int bits = 16; // we will request 16-bit PCM

                using (var fs = new FileStream(outWavPath, FileMode.Create, FileAccess.Write))
                using (var bw = new BinaryWriter(fs))
                {
                    // write WAV header placeholder
                    bw.Write(new char[4] { 'R', 'I', 'F', 'F' });
                    bw.Write((int)0); // filesize
                    bw.Write(new char[4] { 'W', 'A', 'V', 'E' });
                    bw.Write(new char[4] { 'f', 'm', 't', ' ' });
                    bw.Write((int)16);
                    bw.Write((short)1);
                    bw.Write((short)channels);
                    bw.Write((int)samplerate);
                    int byteRate = samplerate * channels * bits / 8;
                    bw.Write((int)byteRate);
                    bw.Write((short)(channels * bits / 8));
                    bw.Write((short)bits);
                    bw.Write(new char[4] { 'd', 'a', 't', 'a' });
                    bw.Write((int)0); // data size placeholder

                    byte[] buffer = new byte[65536];
                    int read;
                    int total = 0;

                    while ((read = Bass.ChannelGetData(decodeStream, buffer, buffer.Length)) > 0)
                    {
                        bw.Write(buffer, 0, read);
                        total += read;
                    }

                    // fill sizes
                    bw.Seek(4, SeekOrigin.Begin);
                    bw.Write(36 + total);
                    bw.Seek(40, SeekOrigin.Begin);
                    bw.Write(total);
                }
            }
            finally
            {
                try { Bass.StreamFree(decodeStream); } catch { }
            }
        }

        public void Dispose()
        {
            Stop();
            try { if (initialized) Bass.Free(); } catch { }
        }

        public bool IsInitialized => initialized;
        public bool IsPluginLoaded => vgmstreamAvailable;
    }
}
