using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PistolWhipModSelector.Settings;
using PistolWhipModSelector.SaveOriginalFiles;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace PistolWhipModSelector
{
    public partial class MainForm : Form
    {
        private void Log(string message)
        {
            try { Trace.WriteLine($"[MainForm] {message}"); } catch { }
        }

        private List<AudioLineProperties> audioLines = null;
        private ModsFolder modsFolder = null;
        private Player.BassAudioPlayer audioPlayer = null;
        private Button playButton = null;
        private Button stopButton = null;
        private System.Windows.Forms.Timer playbackTimer = null;
        private bool isUserSeeking = false;
        private double observedMaxPosition = 0.0;
        private bool skipMouseUpSeek = false; // no-op to ensure patch tool state

        // Win32 helpers for native loader diagnostics
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern int FormatMessage(uint dwFlags, IntPtr lpSource, uint dwMessageId, uint dwLanguageId, System.Text.StringBuilder lpBuffer, uint nSize, IntPtr Arguments);

        public MainForm(ModsFolder modsFolder)
        {
            this.modsFolder = modsFolder;
            InitializeComponent();
            this.Text = "Pistol Whip Custom Songs v" + Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            Rectangle workingArea = Screen.FromControl(this).WorkingArea;
            int preferredWidth = Math.Min(1200, workingArea.Width - 80);
            int preferredHeight = Math.Min(750, workingArea.Height - 80);
            if (preferredWidth > this.ClientSize.Width && preferredHeight > this.ClientSize.Height)
            {
                this.ClientSize = new Size(preferredWidth, preferredHeight);
                this.CenterToScreen();
            }

            ReadAllAudioSongs readAllAudioSongs = new ReadAllAudioSongs();
            this.audioLines = readAllAudioSongs.AudioLines;

            OriginalFilesSaver originalFilesSaver = new OriginalFilesSaver(this.audioLines);
            modsFolder.AvaiableCustomSongsFolder(audioLines);

            GlobalVariables.GetCustomSongFolderPath(audioLines[3].ID);

            this.FillOriginalSongNamesList();
            this.ChangeState("Ready", Color.Green);

            // initialize audio player (BASS)
            this.audioPlayer = new Player.BassAudioPlayer();
            // check native dependencies and plugin; show diagnostics if something is wrong
            if (!this.audioPlayer.IsInitialized || !this.audioPlayer.IsPluginLoaded)
            {
                string diag = GetBassDiagnostics();
                var result = MessageBox.Show("BASS or plugin problem detected:\n\n" + diag + "\n\nOpen application folder?", "Audio diagnostics", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                    Process.Start("explorer.exe", AppDomain.CurrentDomain.BaseDirectory);
            }
            // timer to update progress
            this.playbackTimer = new System.Windows.Forms.Timer();
            this.playbackTimer.Interval = 500;
            this.playbackTimer.Tick += PlaybackTimer_Tick;
            Log("Playback timer created, interval=500ms");

            // Ensure duration label is visible and placed near the playback control
            try
            {
                this.lblDuration.AutoSize = true;
                this.lblDuration.ForeColor = Color.Black;
                this.lblDuration.Text = "00:00 / 00:00";
                // place below the trackbar so it's always visible
                this.lblDuration.Location = new Point(this.playbackTrackBar.Left, this.playbackTrackBar.Bottom + 2);
                this.lblDuration.BringToFront();
                this.lblDuration.Visible = true;
            }
            catch { }
        }
        private void ReloadAllButton_Click(object sender, EventArgs e)
        {
            this.ReloadSongs();
        }

        private void ReloadSongs()
        {
            songsTreeView.SelectedNode = songsTreeView.SelectedNode;
        }

        private void OriginalSongCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            FillOriginalSongNamesList();
        }

        private void FillOriginalSongNamesList()
        {
            songsTreeView.Nodes.Clear();
            foreach (AudioLineProperties properties in audioLines)
            {
                string newItem = "";

                if (OriginalSongShowIDCheckBox.Checked && OriginalSongShowNameCheckBox.Checked)
                {
                    newItem = properties.ID + " - " + properties.AudioName;
                }
                else if (OriginalSongShowIDCheckBox.Checked)
                {
                    newItem = properties.ID;
                }
                else if (OriginalSongShowNameCheckBox.Checked)
                {
                    newItem = properties.AudioName;
                }
                else
                {
                    break;
                }

                bool found = false;
                TreeNode newNode = new TreeNode();
                newNode.Text = newItem;
                newNode.Tag = properties.ID;
                foreach (TreeNode node in songsTreeView.Nodes)
                {
                    if (node.Text.StartsWith(properties.AudioSectionName))
                    {
                        node.Nodes.Add(newNode);
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    TreeNode newSection = new TreeNode(properties.AudioSectionName + " (" + audioLines.Where(x => x.AudioSectionName == properties.AudioSectionName).Count() + ")");

                    newSection.Nodes.Add(newNode);
                    songsTreeView.Nodes.Add(newSection);
                }
            }

            songsTreeView.ExpandAll();
        }

        private void songsTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if ((string)songsTreeView.SelectedNode.Tag == null)
            {
                return;
            }

            CustomSongsDataGridView.Rows.Clear();
            AudioLineProperties selectedAudio = audioLines.Find(x => x.ID == (string)songsTreeView.SelectedNode.Tag);
            string customSongsFolderPath = GlobalVariables.GetCustomSongFolderPath(selectedAudio.ID);

            GlobalVariables.CurrentID = selectedAudio.ID;

            string[] allFiles = Directory.GetFiles(customSongsFolderPath);
            var files = allFiles.Where(f => f.EndsWith(".wem", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase)).ToArray();

            if (files.Count() > 0)
            {
                CustomSongsDataGridView.Rows.Add(files.Count());
                int counter = 0;
                foreach (string file in files)
                {
                    var row = CustomSongsDataGridView.Rows[counter];
                    row.Cells["songPath"].Value = file;

                    string fileName = Path.GetFileNameWithoutExtension(file);

                    string[] sort = fileName.Split('-');

                    Array.Resize(ref sort, 4);

                    string id = sort[0];
                    string title = sort[1];
                    string author = sort[2];

                    row.Cells["songTitle"].Value = title;
                    row.Cells["songAuthor"].Value = author;

                    counter++;
                }
            }

            CustomSongsDataGridView.AllowDrop = true;
            CustomSongsDeleteButton.Enabled = true;
            CustomSongsEditButton.Enabled = true;
            CustomSongsReplaceButton.Enabled = true;
            CustomSongsResetButton.Enabled = true;
            ReloadAllButton.Enabled = true;

            this.ChangeState($"\"{selectedAudio.AudioNameID}\" selected!", Color.Green);

            string originalPath = Path.Combine(GlobalVariables.OriginalSongsFolderPath, selectedAudio.ID + ".wem");
            _ = PlayAudioFileAsync(originalPath);
        }

        private void CustomSongsDataGridView_DragDrop(object sender, DragEventArgs e)
        {
            string[] fileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            AddNewSongs.AddNewSongs addNewSongs = new AddNewSongs.AddNewSongs(fileList);

            this.ReloadSongs();
        }

        private void CustomSongsDataGridView_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private string GetBassDiagnostics()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string bassPath = Path.Combine(baseDir, "bass.dll");
                string pluginPath = Path.Combine(baseDir, "bass_vgmstream.dll");
                bool proc64 = Environment.Is64BitProcess;

                string bassExists = File.Exists(bassPath) ? "present" : "missing";
                string pluginExists = File.Exists(pluginPath) ? "present" : "missing";

                string bassBitness = "unknown";
                if (File.Exists(bassPath)) bassBitness = GetPeBitness(bassPath);
                string pluginBitness = "unknown";
                if (File.Exists(pluginPath)) pluginBitness = GetPeBitness(pluginPath);

                string initOk = this.audioPlayer.IsInitialized ? "OK" : "Failed";
                string pluginOk = this.audioPlayer.IsPluginLoaded ? "OK" : "Not loaded";

                string lastErr = "";
                try { lastErr = "BASS.LastError=" + ManagedBass.Bass.LastError; } catch { }

                var summary = $"Process is64: {proc64}\n" +
                       $"bass.dll: {bassExists} ({bassBitness})\n" +
                       $"bass_vgmstream.dll: {pluginExists} ({pluginBitness})\n" +
                       $"BASS Init: {initOk}\n" +
                       $"Plugin: {pluginOk}\n" +
                       lastErr;

                if (!this.audioPlayer.IsPluginLoaded)
                {
                    // Append detailed diagnostics when plugin not loaded
                    try
                    {
                        summary += "\n--- Detailed plugin diagnostics ---\n" + GetBassDiagnosticsDetailed();
                    }
                    catch (Exception) { }
                }

                return summary;
            }
            catch (Exception ex)
            {
                return "Diagnostics error: " + ex.Message;
            }
        }

        private string GetBassDiagnosticsDetailed()
        {
            try
            {
                // Constants for LoadLibraryEx / FormatMessage
                const uint LOAD_WITH_ALTERED_SEARCH_PATH = 0x00000008;
                const uint FORMAT_MESSAGE_FROM_SYSTEM = 0x00001000;
                const uint FORMAT_MESSAGE_IGNORE_INSERTS = 0x00000200;

                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string pluginPath = Path.Combine(baseDir, "bass_vgmstream.dll");
                var sb = new System.Text.StringBuilder();

                sb.AppendLine("Base directory: " + baseDir);
                sb.AppendLine("Files in folder:");
                foreach (var f in Directory.GetFiles(baseDir, "*.dll"))
                {
                    try
                    {
                        var fi = new FileInfo(f);
                        var ver = FileVersionInfo.GetVersionInfo(f).FileVersion ?? "n/a";
                        var bit = GetPeBitness(f);
                        var blocked = "no";
                        try { var zone = File.ReadAllText(f + ":Zone.Identifier"); if (!string.IsNullOrEmpty(zone)) blocked = "yes"; } catch { }
                        sb.AppendLine($"  {Path.GetFileName(f)} size={fi.Length} ver={ver} bit={bit} blocked={blocked}");
                    }
                    catch (Exception ex)
                    {
                        sb.AppendLine($"  {Path.GetFileName(f)} error: {ex.Message}");
                    }
                }

                sb.AppendLine();
                sb.AppendLine("Attempting explicit PluginLoad of bass_vgmstream.dll (for diagnostics)...");
                if (!File.Exists(pluginPath))
                {
                    sb.AppendLine("Plugin file not found: " + pluginPath);
                    return sb.ToString();
                }

                try
                {
                    int pluginHandle = ManagedBass.Bass.PluginLoad(pluginPath);
                    sb.AppendLine("PluginLoad returned handle: " + pluginHandle);
                    try
                    {
                        var last = ManagedBass.Bass.LastError;
                        sb.AppendLine("BASS.LastError (enum): " + last.ToString());
                        sb.AppendLine("BASS.LastError (int): " + ((int)last).ToString());
                        // If FileFormat (41) then list DLL exports to see what symbols are present
                        try
                        {
                            int lastInt = (int)last;
                            if (lastInt == 41)
                            {
                                sb.AppendLine();
                                sb.AppendLine("Exported symbols from plugin (for FileFormat debugging):");
                                sb.AppendLine(ListDllExports(pluginPath));
                            }
                        }
                        catch { }
                    }
                    catch { }
                    if (pluginHandle != 0)
                    {
                        try { ManagedBass.Bass.PluginFree(pluginHandle); sb.AppendLine("Plugin unloaded after test."); } catch { sb.AppendLine("Failed to free plugin handle."); }
                    }
                }
                catch (Exception ex)
                {
                    sb.AppendLine("Exception during PluginLoad: " + ex.Message);
                }

                // Also attempt a raw LoadLibraryEx to get Win32 loader error (missing dependency diagnostics)
                try
                {
                    sb.AppendLine();
                    // diagnostic LoadLibraryEx test
                    sb.AppendLine("Attempting LoadLibraryEx for native loader errors...");
                    IntPtr h = LoadLibraryEx(pluginPath, IntPtr.Zero, LOAD_WITH_ALTERED_SEARCH_PATH);
                    if (h == IntPtr.Zero)
                    {
                        int err = System.Runtime.InteropServices.Marshal.GetLastWin32Error();
                        sb.AppendLine("LoadLibraryEx failed, GetLastWin32Error=" + err);
                        var buf = new System.Text.StringBuilder(512);
                        int fm = FormatMessage(FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_IGNORE_INSERTS, IntPtr.Zero, (uint)err, 0, buf, (uint)buf.Capacity, IntPtr.Zero);
                        if (fm > 0) sb.AppendLine("Win32 message: " + buf.ToString().Trim());
                    }
                    else
                    {
                        sb.AppendLine("LoadLibraryEx succeeded (module loaded). Unloading...");
                        FreeLibrary(h);
                    }
                }
                catch (Exception ex)
                {
                    sb.AppendLine("Exception during LoadLibraryEx test: " + ex.Message);
                }

                var output = sb.ToString();
                try { System.Diagnostics.Debug.WriteLine(output); } catch { }
                try { Console.WriteLine(output); } catch { }
                try { File.WriteAllText(Path.Combine(baseDir, "bass_diagnostics.log"), output); } catch { }
                return output;
            }
            catch (Exception ex)
            {
                return "Detailed diagnostics error: " + ex.Message;
            }
        }

        private string GetPeBitness(string file)
        {
            try
            {
                using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                using (var br = new BinaryReader(fs))
                {
                    // DOS header
                    fs.Seek(0x3C, SeekOrigin.Begin);
                    int peOffset = br.ReadInt32();
                    fs.Seek(peOffset, SeekOrigin.Begin);
                    uint pe = br.ReadUInt32(); // 'PE\0\0'
                    if (pe != 0x00004550) return "unknown";
                    ushort machine = br.ReadUInt16();
                    if (machine == 0x8664) return "x64";
                    if (machine == 0x014c) return "x86";
                    return "other";
                }
            }
            catch { return "error"; }
        }

        private string ListDllExports(string file)
        {
            try
            {
                using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                using (var br = new BinaryReader(fs, Encoding.UTF8))
                {
                    fs.Seek(0x3C, SeekOrigin.Begin);
                    int peOffset = br.ReadInt32();
                    fs.Seek(peOffset, SeekOrigin.Begin);
                    uint peSig = br.ReadUInt32();
                    if (peSig != 0x00004550) return "Not a PE file";

                    // IMAGE_FILE_HEADER
                    ushort machine = br.ReadUInt16();
                    ushort numberOfSections = br.ReadUInt16();
                    br.ReadUInt32(); // TimeDateStamp
                    br.ReadUInt32(); // PointerToSymbolTable
                    br.ReadUInt32(); // NumberOfSymbols
                    ushort sizeOfOptionalHeader = br.ReadUInt16();
                    br.ReadUInt16(); // Characteristics

                    long optionalHeaderStart = fs.Position;
                    ushort magic = br.ReadUInt16();
                    bool is64 = (magic == 0x20b);

                    // DataDirectory offset from optional header start
                    long dataDirOffset = optionalHeaderStart + (is64 ? 112 : 96);
                    fs.Seek(dataDirOffset, SeekOrigin.Begin);
                    uint exportRva = br.ReadUInt32();
                    uint exportSize = br.ReadUInt32();
                    if (exportRva == 0) return "No export table";

                    // Read section headers
                    long sectionHeadersStart = optionalHeaderStart + sizeOfOptionalHeader;
                    fs.Seek(sectionHeadersStart, SeekOrigin.Begin);
                    var sections = new List<(uint VirtualAddress, uint VirtualSize, uint PointerToRawData, uint SizeOfRawData)>();
                    for (int i = 0; i < numberOfSections; i++)
                    {
                        byte[] name = br.ReadBytes(8);
                        uint virtualSize = br.ReadUInt32();
                        uint virtualAddress = br.ReadUInt32();
                        uint sizeOfRawData = br.ReadUInt32();
                        uint pointerToRawData = br.ReadUInt32();
                        br.ReadBytes(16); // skip the rest
                        sections.Add((virtualAddress, virtualSize, pointerToRawData, sizeOfRawData));
                    }

                    uint RvaToOffset(uint rva)
                    {
                        foreach (var s in sections)
                        {
                            if (rva >= s.VirtualAddress && rva < s.VirtualAddress + Math.Max(1, s.VirtualSize))
                            {
                                return s.PointerToRawData + (rva - s.VirtualAddress);
                            }
                        }
                        return 0;
                    }

                    uint exportOffset = RvaToOffset(exportRva);
                    if (exportOffset == 0) return "Cannot map export RVA to file offset";

                    fs.Seek(exportOffset, SeekOrigin.Begin);
                    uint characteristics = br.ReadUInt32();
                    uint timeStamp = br.ReadUInt32();
                    ushort majorVer = br.ReadUInt16();
                    ushort minorVer = br.ReadUInt16();
                    uint nameRva = br.ReadUInt32();
                    uint baseVal = br.ReadUInt32();
                    uint numberOfFunctions = br.ReadUInt32();
                    uint numberOfNames = br.ReadUInt32();
                    uint addressOfFunctions = br.ReadUInt32();
                    uint addressOfNames = br.ReadUInt32();
                    uint addressOfNameOrdinals = br.ReadUInt32();

                    var names = new List<string>();
                    for (uint i = 0; i < numberOfNames; i++)
                    {
                        uint nameRvaEntryOffset = RvaToOffset(addressOfNames + i * 4);
                        if (nameRvaEntryOffset == 0) continue;
                        fs.Seek(nameRvaEntryOffset, SeekOrigin.Begin);
                        uint nameRvaValue = br.ReadUInt32();
                        uint nameOffset = RvaToOffset(nameRvaValue);
                        if (nameOffset == 0) continue;
                        fs.Seek(nameOffset, SeekOrigin.Begin);
                        var sb = new StringBuilder();
                        int b;
                        while ((b = fs.ReadByte()) > 0)
                        {
                            sb.Append((char)b);
                        }
                        names.Add(sb.ToString());
                    }

                    if (names.Count == 0) return "No exported names found";
                    return string.Join("\n", names);
                }
            }
            catch (Exception ex)
            {
                return "Error reading exports: " + ex.Message;
            }
        }

        private void CustomSongsResetButton_Click(object sender, EventArgs e)
        {
            string destinationPath = GlobalVariables.ModsFolderPath + "\\Original\\" + GlobalVariables.CurrentID + ".wem";

            this.ReplaceSong(destinationPath);
        }

        private void CustomSongsReplaceButton_Click(object sender, EventArgs e)
        {
            string destinationPath = this.GetDestinationPath();

            if (!String.IsNullOrWhiteSpace(destinationPath))
            {
                this.ReplaceSong(destinationPath);
            }
        }

        private void CustomSongsDeleteButton_Click(object sender, EventArgs e)
        {
            string path = this.GetDestinationPath();

            if (!String.IsNullOrWhiteSpace(path))
            {
                File.Delete(path);
            }
            this.ReloadSongs();
        }

        private void CustomSongsEditButton_Click(object sender, EventArgs e)
        {
            string[] path = { this.GetDestinationPath() };

            AddNewSongs.AddNewSongs add = new AddNewSongs.AddNewSongs(path);

            this.ReloadSongs();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/PaRcS-Dev/PistolWhipModSelector");
        }

        private void CustomSongsDataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (CustomSongsDataGridView.SelectedCells.Count > 0)
            {
                int selectedrowindex = CustomSongsDataGridView.SelectedCells[0].RowIndex;
                DataGridViewRow selectedRow = CustomSongsDataGridView.Rows[selectedrowindex];
                string destinationPath = Convert.ToString(selectedRow.Cells["songPath"].Value);

                CustomSongFullPathLabel.Text = destinationPath;
            }
            else
            {
                CustomSongFullPathLabel.Text = "";
            }
        }

        private void ReplaceSong(string destinationPath)
        {
            string targetPath = GlobalVariables.SongsFolderPath + "\\" + GlobalVariables.CurrentID + ".wem";

            File.Copy(destinationPath, targetPath, true);

            this.ReloadSongs();
        }

        private string GetDestinationPath()
        {
            if (CustomSongsDataGridView.SelectedCells.Count > 0)
            {
                int selectedrowindex = CustomSongsDataGridView.SelectedCells[0].RowIndex;
                DataGridViewRow selectedRow = CustomSongsDataGridView.Rows[selectedrowindex];
                string destinationPath = Convert.ToString(selectedRow.Cells["songPath"].Value);

                return destinationPath;
            }

            return null;
        }

        private void ChangeState(string text, Color color)
        {
            CopyStateLabel.Text = text;
            CopyStateLabel.ForeColor = color;
        }

        private string GetCachedConvertedPath(string sourcePath)
        {
            try
            {
                string cacheDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PistolWhipModSelector", "converted_cache");
                Directory.CreateDirectory(cacheDir);
                long ticks = File.GetLastWriteTimeUtc(sourcePath).Ticks;
                string key = sourcePath + "|" + ticks.ToString();
                using (var sha = SHA1.Create())
                {
                    byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(key));
                    string hex = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                    return Path.Combine(cacheDir, hex + ".wav");
                }
            }
            catch { return Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".wav"); }
        }

        private async void PlayButton_Click(object sender, EventArgs e)
        {
            await PlayAudioFileAsync(this.GetDestinationPath()).ConfigureAwait(false);
        }

        private async Task PlayAudioFileAsync(string path)
        {
            if (String.IsNullOrWhiteSpace(path) || !File.Exists(path) || this.audioPlayer == null)
                return;

            try
            {
                string playPath = path;

                // convert WEM files to WAV first because some WEM streams don't expose duration
                if (path.EndsWith(".wem", StringComparison.OrdinalIgnoreCase))
                {
                    string cachePath = GetCachedConvertedPath(path);
                    if (File.Exists(cachePath))
                    {
                        playPath = cachePath;
                    }
                    else
                    {
                        // show marquee progress
                        try { this.conversionProgressBar.Visible = true; } catch { }
                        try
                        {
                            await Task.Run(() => this.audioPlayer.SaveAsWav(path, cachePath)).ConfigureAwait(false);
                            playPath = cachePath;
                            Log($"PlayButton: conversion finished, playing cached file {cachePath}");
                        }
                        catch (Exception ex)
                        {
                            Log("PlayButton: conversion failed: " + ex.Message);
                            playPath = path; // fallback
                        }
                        finally
                        {
                            try { this.Invoke(new Action(() => this.conversionProgressBar.Visible = false)); } catch { }
                        }
                    }
                }

                // play
                try { this.Invoke(new Action(() => this.audioPlayer.Play(playPath))); } catch { this.audioPlayer.Play(playPath); }
                Log($"PlayButton: started playing path={playPath}");
                try { this.playbackTrackBar.Value = 0; } catch { }
                observedMaxPosition = 0.0;
                try { this.Invoke(new Action(() => this.playbackTimer.Start())); } catch { this.playbackTimer.Start(); }
                Log("PlayButton: playbackTimer started");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error playing file: " + ex.Message);
            }
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            try
            {
                this.audioPlayer.Stop();
                Log("StopButton: audioPlayer stopped");
                this.playbackTimer.Stop();
                Log("StopButton: playbackTimer stopped");
                try { this.playbackTrackBar.Value = 0; } catch { }
                observedMaxPosition = 0.0;
                // no temporary deletion needed; cached converted files persist
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error stopping playback: " + ex.Message);
            }
        }

        private void PlaybackTimer_Tick(object sender, EventArgs e)
        {
            if (this.isUserSeeking) return;
            try
            {
                double duration = this.audioPlayer.GetDuration();
                double position = this.audioPlayer.GetPosition();
                const int sliderMaximum = 1000;
                if (this.playbackTrackBar.Maximum != sliderMaximum)
                    this.playbackTrackBar.Maximum = sliderMaximum;

                try
                {
                    int val = 0;
                    if (duration > 0 && !Double.IsNaN(duration) && !Double.IsInfinity(duration))
                    {
                        val = (int)Math.Round(position / duration * sliderMaximum);
                    }
                    else
                    {
                        // duration unknown: use adaptive observed max position so slider still moves
                        if (position > observedMaxPosition) observedMaxPosition = position;
                        double denom = observedMaxPosition > 0.0 ? observedMaxPosition : 1.0;
                        val = (int)Math.Round(position / denom * sliderMaximum);
                    }

                    if (val < 0) val = 0;
                    if (val > sliderMaximum) val = sliderMaximum;

                    this.playbackTrackBar.Value = val;

                    // update duration label
                    try
                    {
                        TimeSpan posTs = TimeSpan.FromSeconds(position);
                        double durSec = duration > 0 ? duration : (observedMaxPosition > 0 ? observedMaxPosition : 0.0);
                        TimeSpan durTs = TimeSpan.FromSeconds(durSec);
                        string text = posTs.ToString(@"mm\:ss") + " / " + durTs.ToString(@"mm\:ss");
                        try { this.lblDuration.Text = text; } catch { }
                    }
                    catch { }
                }
                catch { }
            }
            catch (Exception ex) { Log("PlaybackTimer_Tick: exception: " + ex.Message); }
        }

        private void playbackTrackBar_MouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.Button == MouseButtons.Left)
                {
                    const int sliderMaximum = 1000;
                    if (this.playbackTrackBar.Maximum != sliderMaximum)
                        this.playbackTrackBar.Maximum = sliderMaximum;

                    double ratio = e.X / (double)Math.Max(1, this.playbackTrackBar.Width);
                    int val = (int)Math.Round(ratio * this.playbackTrackBar.Maximum);
                    if (val < 0) val = 0;
                    if (val > this.playbackTrackBar.Maximum) val = this.playbackTrackBar.Maximum;

                    try { this.playbackTrackBar.Value = val; } catch { }
                    // update visual position; actual seek will occur on MouseUp to support dragging
                }
            }
            catch { }
            this.isUserSeeking = true;
        }

        private void playbackTrackBar_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                double duration = this.audioPlayer.GetDuration();
                if (duration > 0)
                {
                    double seconds = duration * this.playbackTrackBar.Value / this.playbackTrackBar.Maximum;
                    this.audioPlayer.Seek(seconds);
                }
                else
                {
                    // duration unknown: use observed max fallback
                    double seconds = (observedMaxPosition > 0.0 ? observedMaxPosition * this.playbackTrackBar.Value / this.playbackTrackBar.Maximum : 0.0);
                    this.audioPlayer.Seek(seconds);
                }
            }
            catch { }
            finally { this.isUserSeeking = false; }
        }

        private void playbackTrackBar_Scroll(object sender, EventArgs e)
        {
            // optional: show preview while dragging. Actual seek occurs on MouseUp.
        }
    }


}
