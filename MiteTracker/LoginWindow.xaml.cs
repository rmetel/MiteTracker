using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Threading;
using System.IO;
using System.Security.Cryptography;

namespace Custom
{
    /// <summary>
    /// Interaktionslogik für LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private const string FILE = ".userdata.txt";
        private const string SECRET = "azBFG89456&_112!{//xbca5";        
        private Login login;
        private Thread thread;
        public delegate void LoginEventHandler(object sender, LoginEventArgs e);
        /// <summary>
        /// Tritt ein, wenn der User sich erfolgreich eingeloggt hat
        /// </summary>
        public event LoginEventHandler Logged;

        private void OnLogged()
        {
            if (Logged != null) Logged(this, new LoginEventArgs(login.User, login.Password));
        }

        public LoginWindow()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            CheckInput();
        }

        private void lblHeader_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                this.DragMove();
            }
            catch (Exception)
            {
                // nichts tun, wenn rechte Maustaste gedrückt wird                
            }
        }

        // Loginfenster minimieren
        private void lblMinimize_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        // Anwendung schliessen
        private void lblClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Environment.Exit(0);
        }

        /// <summary>
        /// Versucht den User in Redmine anzumelden 
        /// </summary>
        /// <param name="login"></param>
        private void Login(object login)
        {
            // Authentifizierung - Accountdaten werden versuchsweise ausgelesen
            string url = "https://hrc4.mite.yo.lk/myself.xml";
            string result = "";           

            try
            {
                result = MiteAPI.Read(url, ((Login)login).User, ((Login)login).Password);
                this.Dispatcher.Invoke(new Action(() =>
                {
                    // Ereignis wird ausgelöst, wenn der User sich erfolgreich eingeloggt hat
                    OnLogged();

                    // Userdaten speichern
                    if ((bool)chkSaveUser.IsChecked)
                        SaveUserData(((Login)login).User, ((Login)login).Password);

                    // Schliesse Loginfenster
                    this.Close();
                }));
            }
            catch (Exception ex)
            {
                this.Dispatcher.Invoke(new Action(() => lblError.Content = ex.Message));
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return) CheckInput();
        }

        /// <summary>
        /// Prüft, ob User und Passwort ausgefüllt sind und führt ggf. Login aus.
        /// </summary>
        private void CheckInput()
        {
            if (txtUser.Text == "" && txtPassword.Password == "") 
                lblError.Content = "Benutzername und Passwort erforderlich!";
            else if (txtUser.Text == "" && txtPassword.Password != "") 
                lblError.Content = "Benutzername erforderlich!";
            else if (txtUser.Text != "" && txtPassword.Password == "") 
                lblError.Content = "Passwort erforderlich!";
            else
            {
                // Bereinige Ausgabe
                if (lblError.Content != null) lblError.Content = null;

                // Logindaten speichern
                login = new Login(txtUser.Text.Trim(), txtPassword.Password.Trim());

                // Loginversuch
                thread = new Thread(new ParameterizedThreadStart(Login));
                thread.Start(login);
            }                       
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (AutoLogin())            
                txtPassword.Focus();            
            else            
                txtUser.Focus();            
        }

        private void lblHeader_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Normal;
        }

        private bool AutoLogin()
        {            
            string user = "";
            string password = "";
            int row = 0;           

            try
            {
                if (File.Exists(FILE))
                {
                    // Open the stream and read it back.
                    using (StreamReader sr = File.OpenText(FILE))
                    {
                        string s = "";
                        while ((s = sr.ReadLine()) != null)
                        {
                            if (row == 0)
                                user = Crypto.DecryptStringAES(s, SECRET);
                            else
                                password = Crypto.DecryptStringAES(s, SECRET);

                            row++;
                        }
                    }

                    txtUser.Text = user;
                    txtPassword.Password = password;
                    chkSaveUser.IsChecked = true;

                    return true;
                }                
            }
            catch (Exception ex)
            {
                lblError.Content = ex.Message;
            }

            return false;
        }

        /// <summary>
        /// Speichert Userdaten
        /// </summary>
        /// <returns></returns>
        private bool SaveUserData(string user, string password)
        {                         
            Byte[] text;
            byte[] newLine = Encoding.ASCII.GetBytes(Environment.NewLine);            

            if (!File.Exists(FILE))
            {
                // Schreibe Datei
                using (FileStream fs = File.Create(FILE))
                {
                    // User
                    text = new UTF8Encoding(true).GetBytes(Crypto.EncryptStringAES(user, SECRET));                                      
                    fs.Write(text, 0, text.Length);
                    // Neue Zeile                    
                    fs.Write(newLine, 0, newLine.Length);
                    // Passwort
                    text = new UTF8Encoding(true).GetBytes(Crypto.EncryptStringAES(password, SECRET));
                    fs.Write(text, 0, text.Length);
                }
            }             

            return true;
        }

        /// <summary>
        /// Holt einen Json String aus einer Textdatei. Anmerkung: Nur für Testzwecke gedacht!
        /// </summary>
        /// <returns></returns>
        private string DeleteAccount(Login login)
        {
            StreamReader reader = null;
            string json = "{";

            try
            {
                reader = new StreamReader("issues.txt");

                while (reader.Read() > 0)
                {
                    json += reader.ReadLine();
                }
            }
            catch (Exception ex)
            {
                lblError.Content = ex.Message;
            }
            finally
            {
                if (reader != null) reader.Close();
            }
            return json;
        }

        private void txtUser_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (txtPassword.Password != "" && txtUser.Text != "")
                chkSaveUser.IsEnabled = true;
            else
                chkSaveUser.IsEnabled = false;
        }

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (txtPassword.Password != "" && txtUser.Text != "") 
                chkSaveUser.IsEnabled = true;
            else
                chkSaveUser.IsEnabled = false;
        }

        private void chkSaveUser_Unchecked(object sender, RoutedEventArgs e)
        {
            if(File.Exists(FILE))
                File.Delete(FILE);
        }
    }

    class Login
    {
        private string user;

        public string User
        {
            get { return user; }
            set { user = value; }
        }

        private string password;

        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        public Login(string user, string password)
        {
            User = user;
            Password = password;
        }
    }

    public class LoginEventArgs : EventArgs
    {
        private string user;

        public string User
        {
            get { return user; }
            set { user = value; }
        }

        private string password;

        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        public LoginEventArgs(string user, string password)
        {
            this.user = user;
            this.password = password;
        }
    }
}
