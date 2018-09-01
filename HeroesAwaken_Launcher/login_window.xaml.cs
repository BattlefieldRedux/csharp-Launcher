using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;
using LitJson;
using System.Threading;
using System.IO;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MessageBox = System.Windows.MessageBox;

namespace OSH_Launcher
{
    public partial class login_window : Window
    {
        private double version = 1;
        private string[] RegionCode = { "EU", "SA"};
        //where to request the laravel token
        private string[] RegionAPI = { "http://nextwave.ddns.net/api/", "http://nextwave.ddns.net/api/" };
        public static string[] ApplicationNames = {"BFHeroes1.exe", "BFHeroes1.exe"};
        private string optimalRegion;
        public static int RegionIdentifier;

        public login_window()
        {
            InitializeComponent();
            Thread t = new Thread(() => CreateFile());
            textBox_username.Focus();
            t.Start();
            Thread compare = new Thread(()=>CompareVersion());
            compare.Start();
        }


        private void CompareVersion()
        {
            WebClient client = new WebClient();
            Uri Uri = new Uri("http://nextwave.ddns.net/download/version.txt");
            string versionstring = client.DownloadString(Uri);
            Console.WriteLine("Version: "+versionstring);
            double newestversion = double.Parse(versionstring);
            if (newestversion != version)
            {
                dynamic MsgResult = MessageBox.Show("A new update has been detected, would you like to Download it?\nYour Version: " + version + "\nNewest Version: " + newestversion, "Update Detected!", MessageBoxButton.YesNo);

                if (MsgResult == MessageBoxResult.Yes)
                {
                    if (!File.Exists("LauncherUpdater.exe"))
                    {
                        client.DownloadFile("http://nextwave.ddns.net/download/LauncherUpdater.exe", "LauncherUpdater.exe");
                    }
                    ProcessStartInfo processInfo = new ProcessStartInfo();
                    processInfo.FileName = "LauncherUpdater.exe";;
                    Process.Start(processInfo);
                    Environment.Exit(1);
                }
                else
                {
                    MessageBox.Show("You have chosen to ignore updating, please note we do not offer support for outdated versions of our product.\nEnjoy playing.");
                }
            }
        }


        private void CreateFile()
        {
            if (!File.Exists("LitJson.dll"))
            {
                WebClient client = new WebClient();
                Uri Uri = new Uri("http://nextwave.ddns.net/download/LitJson.dll");
                client.DownloadFile(Uri, "LitJson.dll");
            }
        }

        private void button_login_click(object sender, RoutedEventArgs e)
        {
            if(textBox_username.Text != "" && passwordBox.Password.Length > 0)
            {
                string username = textBox_username.Text;
                string password = Convert.ToString(passwordBox.Password);

                try
                {
                    //MessageBox.Show(getToken(username, password), "Token", MessageBoxButton.OK);

                    MainWindow mw = new MainWindow(checkToken(username, password)); //Problem -> Fenster öffnet sich nicht :( -> gelöst :)
                    mw.Show();
                    username = null; //             Token und Namen übergeben
                    password = null; //--> sicherheitshalber
                    login_win.Close(); // Window erst schließen nachdem das andere aufgemacht wurde
                    Close();
                    textBox_username.Text = "";
                    passwordBox.Password = "";
                    textBox_username.Focus();
                }
                catch (Exception ex)
                {
                    textBox_username.Text = "";
                    passwordBox.Password = "";
                    textBox_username.Focus();
                    if (ex.ToString().Contains("500"))
                    {
                        MessageBox.Show("Could not reach Server.");
                    }
                    MessageBox.Show("Error Occured: " + ex);
                }
            }
            else
            {
                MessageBox.Show("Empty textbox", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public User checkToken(string user, string password) //Token überprüfen, dadurch wird für Login keine Datenbank benötigt.
        {
            try
            {
                string url = String.Format(optimalRegion + "token?username={0}&password={1}", user,
                    WebUtility.HtmlEncode(password));

                Console.WriteLine(url);

                using (var client = new WebClient())
                {
                    string result = client.DownloadString(url);
                    Console.WriteLine(result);
                    Token myToken = JsonMapper.ToObject<Token>(result);
                    Console.WriteLine(myToken.error);
                    if (myToken.token == "")
                    {
                        throw new InvalidOperationException("Could not log in" + myToken.error);
                    }

                    url = String.Format(optimalRegion + "user?token={0}", myToken.token);
                    result = client.DownloadString(url);
                    User myUser = JsonMapper.ToObject<User>(result);

                    return myUser;
                }
            }
            catch (WebException ex)
            {
                MessageBox.Show("Error while attempting to get info from Database. ERROR: " + ex);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Logging in! \nERROR: " + ex);
            }
            return null;
        }


        public string createToken(string user, string password) //Zum Token erstellen
        {
            string token = "testtoken";

            return token;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e) //xaml needed to be edited
        {
            if (e.Key == Key.Enter)
            {
                button_login_click(sender, e);
            }
        }

        private void button_keeplogin_Click(object sender, RoutedEventArgs e)
        {
            //Für spätere Versionen

        }

        private void regionbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            optimalRegion = RegionAPI[regionbox.SelectedIndex];
            RegionIdentifier = regionbox.SelectedIndex;
        }

        private void textBox_username_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}