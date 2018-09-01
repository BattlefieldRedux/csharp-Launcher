using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Threading;
using System.Reflection;
using System.Windows.Navigation;


namespace OSH_Launcher
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        User user;
        string InstallPath;
        bool dirfound = false;
        public bool downloading = false;



        public MainWindow(User user)
        {            

            InitializeComponent();

            website_browser.Navigate("http://nextwave.ddns.net/games");
            website_browser.Navigating += Website_browser_Navigating;



            this.user = user;
            //MessageBox.Show(token);

            label_progresbar.Content = "Checking for New content...";
            label_welcome.Content = "Welcome,  Hero " + this.user.username + "!";
            Thread t = new Thread(() => CheckDownloads());
            t.IsBackground = true;
            t.Start();
        }

        public void CheckDownloads()
        {      
            DownloadGamefile dwnld = new DownloadGamefile(this, System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            downloading = true;
        }
 
        private void button_forum_click(object sender, RoutedEventArgs e)
        {
            Process.Start("http://nextwave.ddns.net/");
        }

        private void Website_browser_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            e.Cancel = true;
        }

        private void button_logout_click(object sender, RoutedEventArgs e)
        {
            login_window LW = new login_window();
            LW.Show();
            Close();
        }

        private void button_play_click(object sender, RoutedEventArgs e)
        {
                string parameters = String.Format(" +sessionId \"{0}\" +magma 0 +punkbuster 0", user.game_token);
                string path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/game/"+login_window.ApplicationNames[login_window.RegionIdentifier];
                Console.WriteLine(path + parameters);
                ProcessStartInfo processInfo = new ProcessStartInfo();
                processInfo.FileName = path;
                processInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(path);
                processInfo.Arguments = parameters;
                Process proc = Process.Start(processInfo);
 
            //DownloadGamefile dgf = new DownloadGamefile();
            //dgf.DownloadFile("http://osheroesbr.ddns.net/download", @"OSHv3.zip");
            
        }

        private void grid_logo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("http://nextwave.ddns.net/");
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void progressbar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }
    }
}
