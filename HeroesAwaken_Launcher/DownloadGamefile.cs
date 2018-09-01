using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Threading;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace OSH_Launcher
{
    class DownloadGamefile
    {
        public volatile bool _completed;
        bool ReadyForNextFile;
        private string GameLocation;
        private string[] FileName;
        private string name;
        private int missingfiles;
        private int nowmissingfiles;
        public DateTime lastUpdate;
        public long lastBytes;
        private string SystemAssemblyPath;
        private MainWindow mw; // statt Vererbung einfach MainWindow Objekt übergeben
        private Dictionary<string, string> LocalFileMD = new Dictionary<string, string>();
        private Dictionary<string, string> ServerFileMD = new Dictionary<string, string>();
        private string[] ExceptionFiles = { "mods/bfheroes/mugshot.png", "__init__","debug","nextWave.exe" };
        private string DownloadHost = "http://nextwave3.ddns.net/download/";
        private ArrayList DownloadQueue;

        public DownloadGamefile(MainWindow mw, string GameLocation)
        {
            this.mw = mw; // Übergeben
            
            this.GameLocation = GameLocation;
            SystemAssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            CreateHierarchies();
            CompareMD5();
            
        }

        public void isCompleted()
        {
            Thread.Sleep(2000);
            bool breakFlag = false;
            while (!breakFlag)
            {
                Thread.Sleep(2000);
                if (nowmissingfiles == 0)
                {
                    Application.Current.Dispatcher.Invoke(new Action(() =>
                    {
                        mw.label_progresbar.Content = "Finished!";
                        mw.progressbar.Value = 100;
                        mw.button_play.IsEnabled = true;
                        if (_completed)
                        {
                            breakFlag = !breakFlag;
                        }
                    }));
                }
            }
        }

        public void CreateHierarchies() //Create folders so files can be saved 
        {
            DownloadSingleFile(DownloadHost + "directorz.txt", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/directorz.txt"); //DL list of needed directories
            string[] Paths = File.ReadAllLines("directorz.txt");
            try
            {
                for (int i = 0; i < Paths.Length; i++)
                {
                    if (!Directory.Exists(SystemAssemblyPath + Paths[i]))
                    {
                        Directory.CreateDirectory(SystemAssemblyPath + "/" + Paths[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(new Action(() => //Dispatcher to interact with WPF GUI
                {
                    MessageBox.Show("Error during Hierarchie Creation: " + ex);
                }));
            }
            
        }

        public void DownloadSingleFile(string address, string location) 
        {
            WebClient client = new WebClient();
            Uri Uri = new Uri(address);
            client.DownloadFile(Uri, location);
        }

        public void DownloadFile(string address, string location)
        {
            WebClient client = new WebClient();
            Uri Uri = new Uri(address);
            _completed = false;
            mw.downloading = true;
            if (address.Contains("\\"))
            {
                FileName = address.Split('\\');
            }
            else
            {
                FileName = address.Split('/');
            }

            name = FileName.Last();

            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(Client_DownloadProgressChanged);
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
            try
            {
                client.DownloadFileAsync(Uri, location);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error calling DownloadFileAsync: " + ex);
            }

        }

        public bool DownloadCompleted { get { return _completed; } }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            _completed = false;

            Application.Current.Dispatcher.Invoke(new Action(() => //Dispatcher to interact with WPF GUI
            {
                double bytesIn = double.Parse(e.BytesReceived.ToString());
                double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
                double percentage = bytesIn / totalBytes * 100;
                double newBytesIn = Math.Round(bytesIn / 1000000, 2);
                double newtotalBytes = Math.Round(totalBytes / 1000000, 2);
                mw.label_progresbar.Content = "Download " + name + " | " + newBytesIn + "MB of " + newtotalBytes + "MB  | " + e.ProgressPercentage + "%";
                mw.progressbar.Value = int.Parse(Math.Truncate(percentage).ToString());
            }));
        }

        private void Completed(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                Console.WriteLine("Download has been canceled.");
            }
            else
            {
                Console.WriteLine("Download completed!");
            }
            ReadyForNextFile = true;
            _completed = true;
            nowmissingfiles--;
        }

        public void GetLocalMD5()
        {
            try
            {
                string text = SystemAssemblyPath + "/game/";
                string[] localfilelist = Directory.GetFiles(text, "*", SearchOption.AllDirectories); //Reads all files' paths from the path specified in the constructor
                foreach (var file in localfilelist)
                {
                    string tmp = file.Replace(SystemAssemblyPath + @"/game/", "");
                    LocalFileMD.Add(tmp, CalculateMD5(file)); //Add the Files path (with name) and its MD5 to Dictionary
                }
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(new Action(() => //Dispatcher to interact with WPF GUI
                {
                    MessageBox.Show("Error getting local MD5s: " + ex);
                }));
            }
            
        }

        public void GetServerMD5() //Needs to be adjusted to whatever method is chosen to get the MD5s from server files.
        {
            DownloadSingleFile(DownloadHost+"manifest.txt", System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)+"/manifest.txt"); //Download manifest from webhost
            
            Thread.Sleep(3000);
            string[] localfilelist = File.ReadAllLines("manifest.txt"); //Change to downloaded file
            foreach (var file in localfilelist)
            {
                string[] Data = file.Split('='); //Assuming the format is path=filename=hash the indexes are 0=1=2
                ServerFileMD.Add(Data[0], Data[2]); //Add Path and hash as key and value like in the localmd5
            }
        }

        public bool IsExceptionFile(string FileName)
        {
            for (int i = 0; i < ExceptionFiles.Length; i++)
            {
                Console.WriteLine("FileName: "+FileName+" | "+SystemAssemblyPath+FileName);
                if (FileName.Contains(ExceptionFiles[i]) && !File.Exists(SystemAssemblyPath + "/"+FileName))
                {
                    return true;
                }
            }
            return false;
        }

        public void CompareMD5()
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                mw.button_play.IsEnabled = false; //Make sure the user cant start the game before files are done being verified
            }));
            GetLocalMD5();
            GetServerMD5();
            ArrayList MissingFiles = new ArrayList();

            foreach (KeyValuePair<string, string> file in ServerFileMD) //Key = Path; Value = MD5
            {
                if (!LocalFileMD.TryGetValue(file.Key, out string currentFile)) Console.WriteLine("NotFoundFile: "+file.Key); //If hash for the path in the manifest.txt isnt found skip the item
                
                if (currentFile != file.Value && !IsExceptionFile(file.Key)) //Check if the LocalMD5s Dictioniary has the same hash as the one in ServerFileMD && Check if the file thats missing/faulty is a file that is suppose to change
                {
                        MissingFiles.Add("game/"+file.Key); //If they are different add to Arraylist
                }
                else
                {
                        Console.WriteLine("VALID FILE FOUND! "+file.Key+" | "+file.Value);
                }
            }

            if (MissingFiles.Count == 0)
            {
                Application.Current.Dispatcher.Invoke(new Action(() => //Dispatcher to interact with WPF GUI
                {
                    mw.label_progresbar.Content = "Ready!";
                    mw.progressbar.Value = 100;
                    nowmissingfiles = 0;
                    mw.button_play.IsEnabled = true;
                }));
            }
            else
            {
                Console.WriteLine(MissingFiles.Count + " file(s) invalid or missing.");
                nowmissingfiles = MissingFiles.Count;
                DownloadMissingFiles(MissingFiles);
            }
        }

        public void DownloadMissingFiles(ArrayList FilePaths)
        {
            Thread t = new Thread(() => isCompleted()); //Thread to check if all downloads are completed
            t.Start();

            

            foreach (var MissingFile in FilePaths) //Start Asynchronous downloads. Can be changed to DL file when the one before is done, this would fix the Progressbar issue
            {
                try
                {
                    ReadyForNextFile = false;
                    Console.WriteLine("Missing File: "+MissingFile);
                    DownloadFile(DownloadHost+MissingFile, MissingFile.ToString());
                    while (!ReadyForNextFile)
                    {
                        Thread.Sleep(100);
                    }
                }
                catch (Exception ex)
                {
                        MessageBox.Show("Error downloading " + MissingFile + " Error Message:\n" + ex);
                }
            }
        }

        public string CalculateMD5(string filename) //calculate MD5s the same way HashMyFiles does it
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}
