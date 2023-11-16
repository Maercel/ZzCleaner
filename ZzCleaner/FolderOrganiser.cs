using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Timers;
using System.Text;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Security.Principal;
using System.Security.AccessControl;

namespace ZzCleaner
{
    internal class FolderOrganiser : Form1
    {
        private string folder_name;
        static string FolderOrganiserPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\";
        static string FolderToWatch = Path.Combine(FolderOrganiserPath.TrimEnd('\\'));
        private static string folder_path, file_name;
        private static string extension = "noname";
        private static List<string> _changedFiles = new List<string>();
        private RichTextBox outPutLog;
        private FileSystemWatcher watcher = new FileSystemWatcher(FolderToWatch);
        private static Dictionary<string, bool> _changed = new Dictionary<string, bool>();
        private static string SoundsDirectoryPath = (@"\Zzcleaner" + @"\Sounds");
        private static string musicFolderDirectory = MusicFolder(); //Check if not null

        internal FolderOrganiser(string folder_name, RichTextBox outPutLog)
        {
            this.outPutLog = outPutLog;
            this.folder_name = folder_name;
            CreateFolderIfNone(this.folder_name);
            Watcher();
            MessageBox.Show(FolderOrganiserPath);
            MessageBox.Show(Path.Combine(FolderOrganiserPath + folder_name));
            MessageBox.Show(musicFolderDirectory);
        }

        internal static void CreateFolderIfNone(string folder_name)
        {
            string[] dirs = Directory.GetDirectories(FolderOrganiserPath, folder_name, SearchOption.TopDirectoryOnly);
            if (!(dirs.Length > 0))
                Directory.CreateDirectory(Path.Combine(FolderOrganiserPath + @"\"+  folder_name));
        }

        internal void ChangedItemInCleaner(string folder_name, bool value)
        {
            if(_changed.ContainsKey(folder_name))
            {
                _changed[folder_name] = value;
            }
        }

        internal void AddItemToCleaner(string folder_name, bool value)
        {
            if (!_changed.ContainsKey(folder_name))
            {
                _changed.Add(folder_name, value);
            }
        }

        internal void RemoveItemFromCleaner(string folder_name)
        {
            if (_changed.ContainsKey(folder_name))
            {
                _changed.Remove(folder_name);
            }
        }

        public void StopMonitoring()
        {
            //watcher.EnableRaisingEvents = false;
            watcher?.Dispose();
            watcher = null; 
        }

        internal void Watcher()
        {
            watcher = new FileSystemWatcher(FolderToWatch); 

            watcher.Path = FolderToWatch; 
            watcher.Filter = "*.*";
            watcher.NotifyFilter = NotifyFilters.FileName
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.LastWrite;
 
            watcher.Created += OnCreated; 
            watcher.EnableRaisingEvents = true;
            watcher.IncludeSubdirectories = false; 


            //watcher.Dispose(); 
        }

        protected void OnCreated(object sender, FileSystemEventArgs e)
        {
            lock (_changedFiles)
            {
                if (_changedFiles.Contains(e.FullPath))
                {
                    return;
                }
                _changedFiles.Add(e.FullPath);
            }

            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                extension = Path.GetExtension(e.FullPath);
                file_name = Path.GetFileNameWithoutExtension(e.FullPath);
                folder_path = ExtensionFolder(extension);
                bool folderIsGettingTracked = false;
                if (_changed.TryGetValue(folder_name, out folderIsGettingTracked))
                {
                    if (folderIsGettingTracked && folder_path != (FolderOrganiserPath + "\\" + "Uncategorized"))
                        MoveFile(e.FullPath, folder_path, file_name, extension);
                    else
                        watcher?.Dispose(); watcher = null; //throw new FileNotFoundException(); 
                }
            }

            System.Timers.Timer timer = new System.Timers.Timer(1000) { AutoReset = false };
            timer.Elapsed += (timerElapsedSender, timerElapsedArgs) =>
            {
                lock (_changedFiles)
                {
                    _changedFiles.Remove(e.FullPath);
                }
            };
            timer.Start();
        }

        protected void MoveFile(string folderToMoveFrom, string folder_path, string file_name, string extension)
        {
            DateTime now = DateTime.Now;
            string year = now.Year.ToString();
            string month = now.ToString("MMM");
            string fullFolderPath;

            if (!folder_path.Contains(year))
                folder_path = Path.Combine(folder_path + @"\" +  year);
            if (!folder_path.Contains(month))
                folder_path = Path.Combine(folder_path + @"\" +  month);
            Directory.CreateDirectory(folder_path);

            fullFolderPath = folder_path + "\\" + Path.GetFileName(folderToMoveFrom);
            
            if (File.Exists(fullFolderPath))
            {
                int counter = 1; 
                string newFileName = file_name + extension; 
                while(File.Exists(folder_path + @"\" + newFileName))
                {
                    newFileName = $"{file_name}({counter}){extension}";
                    counter++;
                }
                fullFolderPath = folder_path + @"\" + newFileName;
            }

            using (var soundPlayer = new System.Media.SoundPlayer(musicFolderDirectory + "\\whoosh.wav"))
            {
                soundPlayer.PlaySync();
            }
                
            File.Move(folderToMoveFrom, fullFolderPath);

            if (outPutLog.InvokeRequired)
            {
                outPutLog.Invoke(new Action(() =>
                {
                    using (Font font = new Font("Arial", 6.00f, FontStyle.Italic))
                    {
                        outPutLog.SelectionFont = font;
                        outPutLog.AppendText(Environment.NewLine + $"File moved to {fullFolderPath}");
                        outPutLog.SelectionAlignment = HorizontalAlignment.Left;
                    }
                }));
            }
            else
            {
                using (Font font = new Font("Arial", 6.00f, FontStyle.Italic))
                {
                    outPutLog.SelectionFont = font;
                    outPutLog.AppendText(Environment.NewLine + $"File moved to {fullFolderPath}");
                    outPutLog.SelectionAlignment = HorizontalAlignment.Left;
                }
            }
        }

        internal static string ExtensionFolder(string ext)
        {

            Dictionary<string, string> extensions = new Dictionary<string, string>
            {
                {"noname", FolderOrganiserPath + @"\Uncategorized"},

                // Pictures
                {".png", FolderOrganiserPath + "Pictures"},
                {".jpg", FolderOrganiserPath + "Pictures"},
                {".jpeg", FolderOrganiserPath + "Pictures"},
                {".gif", FolderOrganiserPath + "Pictures"},
                {".bmp", FolderOrganiserPath + "Pictures"},
                {".tiff", FolderOrganiserPath + "Pictures"},

                // Videos
                {".mp4", FolderOrganiserPath + "Videos"},
                {".avi", FolderOrganiserPath + "Videos"},
                {".mkv", FolderOrganiserPath + "Videos"},
                {".mov", FolderOrganiserPath + "Videos"},

                // Audio
                {".mp3", FolderOrganiserPath + "Music"},
                {".wav", FolderOrganiserPath + "Music"},
                {".ogg", FolderOrganiserPath + "Music"},
                {".flac", FolderOrganiserPath + "Music"},

                //Microsoft
                {".doc", FolderOrganiserPath + "Microsoft\\Word" }
            };

            string destinationFolder; 

            if(extensions.TryGetValue(ext, out destinationFolder))
                return destinationFolder;
            else
                return Path.Combine(FolderOrganiserPath, "Uncatagorized");
        }

        internal static string MusicFolder()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            int index = currentDirectory.LastIndexOf("\\", currentDirectory.LastIndexOf("\\") - 1);

            if (index != - 1)
            {
                string musicFolderDir = currentDirectory.Substring(0, index) + "\\Sounds";
                if (Directory.Exists(musicFolderDir)) return musicFolderDir;
            }
                

            throw new InvalidOperationException("Sounds directory not found.");
        }
    }

}

