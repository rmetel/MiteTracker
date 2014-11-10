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
using System.Threading;
using System.Windows.Threading;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.IO;
using System.Windows.Media.Animation;

namespace Custom
{    
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Gibt an, ob Zeiterfassung angehalten wurde
        /// </summary>
        public bool IsPaused = false;
        // Loginfenster
        private LoginWindow loginWindow;
        // Infobox
        Infobox infobox;
        // User
        private static string user;
        // Passwort
        private static string password;
        // Mite URL
        private const string MITE_URL = "https://hrc4.mite.yo.lk/";
        // Tickets
        List<TimeEntry> timeEntries;
        // Aktives ListBoxItem
        ListBoxItem activeRow;
        // Aktives Ticket
        TimeEntry activeTicket;
        // Gibt zulässige Dauer in Sekunden an, die der User inaktiv sein darf (= 300sek | 5min)
        private const int ALLOWED_IDLE_DURATION = 300;
        // Gibt in Sekunden an, wie lange eine Meldung angezeigt wird
        private const int MESSAGE_SHOW_DURATION = 3;
        // Gibt in Sekunden an, wie lange die Melung bereits zu sehen ist
        private int messageShown = 0;
        // Gibt an, ob Anwendung im Leerlauf ist
        private bool isIdle;
        // Gibt Dauer in Sekunden an, die der User bereits inaktiv ist
        private int idleDuration;
        // Grau
        private SolidColorBrush MiteGrey = new SolidColorBrush(Color.FromRgb(220, 220, 220));
        // Orange
        private SolidColorBrush MiteOrange = new SolidColorBrush(Color.FromRgb(255, 130, 21));
        // Timer aktiv
        private bool isRunning = false;
        // Nebenprozess
        private Thread thread;
        // Timer
        private DispatcherTimer timer;
        // Startzeitpunkt
        private DateTime start;
        // Aufwand
        TimeSpan timeEffort;
        // Gibt die Bildschirmbreite an
        private double screenWidth;
        // Gibt die Bildschirmhöhe an
        private double screenHeight;
        // ActivityEventHandler
        public delegate void ActivityEventHandler(object sender, ActivityEventArgs e);
        /// <summary>
        /// Wird ausgelöst, wenn User aktiv oder inaktiv wird
        /// </summary>
        public event ActivityEventHandler ActivityChanged;
        // TrackingEventHandler
        public delegate void TrackingEventHandler(object sender, TrackingEventArgs e);
        /// <summary>
        /// Wird ausgelöst, wenn eine weitere Minute bei der Zeiterfassung vergeht
        /// </summary>
        public event TrackingEventHandler MinuteElapsed;
        // Liste mit Projekten
        private Dictionary<int, string> projects = new Dictionary<int, string>();
        // Liste mit Leistungen
        private Dictionary<int, string> services = new Dictionary<int, string>();
        // Gibt an, ob ein Ticket bearbeitet wird
        private bool updateTicket = false;
        // ID des zu aktualisierenden Tickets
        private int updateTicketId = 0;

        // Taskleistensymbol
        public System.Windows.Forms.NotifyIcon notifyIcon;
        // Kontextmenü
        private System.Windows.Forms.ContextMenu contextMenu;
        // Kontextmenü-Unterpunkte
        private System.Windows.Forms.MenuItem menuItemMainWindow;
        private System.Windows.Forms.MenuItem menuItemInfoBox;
        private System.Windows.Forms.MenuItem menuItemExit;

        // Importiere Funktionen (Idle State)
        //------------------------------------------------
        [DllImport("user32.dll")]
        static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

        [StructLayout(LayoutKind.Sequential)]
        struct LASTINPUTINFO
        {
            public static readonly int SizeOf = Marshal.SizeOf(typeof(LASTINPUTINFO));

            [MarshalAs(UnmanagedType.U4)]
            public UInt32 cbSize;
            [MarshalAs(UnmanagedType.U4)]
            public UInt32 dwTime;
        }

        static int GetLastInputTime()
        {
            uint idleTime = 0;
            LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
            lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);
            lastInputInfo.dwTime = 0;

            uint envTicks = (uint)Environment.TickCount;

            if (GetLastInputInfo(ref lastInputInfo))
            {
                uint lastInputTick = lastInputInfo.dwTime;

                idleTime = envTicks - lastInputTick;
            }

            return (int)((idleTime > 0) ? (idleTime / 1000) : 0);
        }
        //------------------------------------------------        

        public MainWindow()
        {
            InitializeComponent();
            
            // Login
            loginWindow = new LoginWindow();
            loginWindow.Logged += new LoginWindow.LoginEventHandler(loginWindow_Logged);
            loginWindow.ShowDialog();

            // Timer starten
            // Timer starten
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += new EventHandler(TimeElapsed);
            timer.Start();

            // Bildschirmparameter auslesen - werden benötigt, um die Infobox richtig zu positionieren
            screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;

            // Symbol in die Taskleiste einfügen
            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Text = "mite tracker";
            notifyIcon.Icon = Custom.Properties.Resources.play;
            notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(notifyIcon_MouseDoubleClick);

            // Kontextmenü erstellen
            menuItemMainWindow = new System.Windows.Forms.MenuItem("Mainwindow");
            menuItemMainWindow.Index = 0;
            menuItemMainWindow.Name = "Mainwindow";
            menuItemMainWindow.Enabled = false;
            menuItemMainWindow.Click += new EventHandler(menuItemMainWindow_Click);

            menuItemInfoBox = new System.Windows.Forms.MenuItem("Infobox");
            menuItemInfoBox.Index = 1;
            menuItemInfoBox.Name = "Infobox";
            menuItemInfoBox.Enabled = false;
            menuItemInfoBox.Click += new EventHandler(menuItemInfoBox_Click);

            menuItemExit = new System.Windows.Forms.MenuItem("Beenden");
            menuItemExit.Index = 2;
            menuItemExit.Name = "Beenden";
            menuItemExit.Enabled = true;
            menuItemExit.Click += new EventHandler(menuItemExit_Click);

            contextMenu = new System.Windows.Forms.ContextMenu();
            contextMenu.MenuItems.Add(menuItemMainWindow);
            contextMenu.MenuItems.Add(menuItemInfoBox);
            contextMenu.MenuItems.Add(menuItemExit);
            notifyIcon.ContextMenu = contextMenu;
        }

        private void TimeElapsed(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            this.Dispatcher.Invoke(new Action(delegate()
            {                
                lblClock.Content = DateTime.Now.ToLongTimeString();
            }));

            // Anzahl der vergangenen Sekunden seit der letzten Aktivität ermitteln
            idleDuration = MainWindow.GetLastInputTime();

            // Zeiterfassung unterbrechen, wenn User für längere Zeit inaktiv ist oder, wenn Pause gedrückt wurde.
            if ((idleDuration > ALLOWED_IDLE_DURATION /*&& chkIsAutoStandby.IsChecked == true*/) || IsPaused)
            {                
                // Ereignis nur auslösen, wenn Anwendung vorher NICHT im Leerlauf war
                if (!isIdle)
                {
                    isIdle = true;
                    OnActivityChanged();
                }
            }
            // Tracker aktualisieren
            else
            {
                // Ereignis nur auslösen, wenn Anwendung vorher im Leerlauf war
                if (isIdle)
                {
                    isIdle = false;
                    OnActivityChanged();
                }

                // Wenn genau eine Minute nach dem Start verstrichen ist
                if (now.Second == start.Second && isRunning)                
                {                    
                    // Es wird kontrolliert, dass nicht bereits zum Zeitpunkt des Starts eine Minute dazugezählt wird
                    // Es muss mindestens eine Minute seit dem Start verlaufen sein
                    if (!(now.Hour == start.Hour && now.Minute == start.Minute))
                    {
                        // Zeitaufwand um eine 1 Minute erhöhen
                        timeEffort = timeEffort.Add(new TimeSpan(0, 1, 0));
                        this.Dispatcher.Invoke(new Action(delegate()
                        {
                            activeTicket.Minutes += 1;
                            // Ticket in Mite aktualisieren
                            ThreadPool.QueueUserWorkItem(new WaitCallback(delegate(object obj){
                                try
                                {
                                    MiteAPI.Update(MITE_URL + "time_entries/" + activeTicket.Id + ".xml", user, password, activeTicket);
                                }
                                catch (Exception ex)
                                {
                                    ShowMessage(ex.Message, false, true);
                                }
                            }));                            
                        }));

                        // Ereignis auslösen
                        OnMinuteElapsed();
                    }
                }
            }

            // Statusanzeige nach vorgegebener Zeit wieder ausblenden
            if (lblStatus.Content != null)
            {
                if (messageShown <= MESSAGE_SHOW_DURATION)
                {
                    messageShown++;
                }
                else if (messageShown > MESSAGE_SHOW_DURATION)
                {
                    if (lblStatus.Content != null)
                        lblStatus.Content = null;
                    messageShown = 0;
                }
            }
        }

        /// <summary>
        /// Übergabe von Benutzerdaten
        /// </summary>
        /// <param name="sender">Loginfenster</param>
        /// <param name="e">Benutzerdaten</param>
        void loginWindow_Logged(object sender, LoginEventArgs e)
        {
            user = e.User;
            password = e.Password;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(LoadMetadata), Metadata.Projects);
            ThreadPool.QueueUserWorkItem(new WaitCallback(LoadMetadata), Metadata.Services);
            ThreadPool.QueueUserWorkItem(new WaitCallback(LoadTickets));

            // NotifyIcon in der Taskleiste anzeigen
            notifyIcon.Visible = true;
        }

        private void LoadMetadata(object meta)
        {
            string result = null;            
            //Dictionary<int, string> items = new Dictionary<int, string>();

            try
            {
                switch ((Metadata)meta)
                {
                    case Metadata.Projects:
                        result = MiteAPI.Read(MITE_URL + "projects.json", user, password);
                        projects = Json.Parse(result, Metadata.Projects);
                        this.Dispatcher.Invoke(new Action(delegate()
                        {
                            cboProjects.ItemsSource = projects;
                            cboProjects.SelectedValuePath = "Key";
                            cboProjects.DisplayMemberPath = "Value";
                            cboProjects.SelectedIndex = 0;                                                   
                        }));
                        break;
                    case Metadata.Services:
                        result = MiteAPI.Read(MITE_URL + "services.json", user, password);
                        services = Json.Parse(result, Metadata.Services);
                        this.Dispatcher.Invoke(new Action(delegate()
                        {
                            cboServices.ItemsSource = services;
                            cboServices.SelectedValuePath = "Key";
                            cboServices.DisplayMemberPath = "Value";
                            cboServices.SelectedIndex = 0;                            
                        }));
                        break;                    
                }
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, false, true);
            }            
        }        

        private void LoadTickets(object obj)
        {            
            string result = null;
            timeEntries = new List<TimeEntry>();

            try
            {
                result = MiteAPI.Read(MITE_URL + "daily.json", user, password);
                timeEntries = Json.ParseTimeEntries(result);
                this.Dispatcher.Invoke(new Action(delegate()
                {
                    this.ListBoxTimeEntries.ItemsSource = timeEntries;
                }));
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, false, true);
            }            
        }

        /// <summary>
        /// Prüft, ob das Ticket im Mite gesperrt ist, da aktiv.
        /// </summary>
        /// <param name="id"></param>
        private bool IsLocked(int id)
        {
            string result;
            bool isLocked = false;

            try
            {
                result = MiteAPI.Read(MITE_URL + "time_entries/" + id + ".json", user, password);
                isLocked = Json.ParseTicketStatus(result);               
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, false, false);
            }

            return isLocked;
        }

        /// <summary>
        /// Bewegt das Fenster
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveScreen(object sender, RoutedEventArgs e)
        {
            this.DragMove();
        }

        // Anwendung minimieren
        private void btnMinimize_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
            menuItemMainWindow.Enabled = true;
        }

        private void btnClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (isRunning)
            {
                ShowMessage("Timer ist noch aktiv!", false, false);
            }
            else
            {
                notifyIcon.Dispose();
                Environment.Exit(0);
            }
        }

        private void btnCreate_MouseEnter(object sender, MouseEventArgs e)
        {
            ((Label)sender).Background = new SolidColorBrush(Color.FromRgb(247, 171, 64));
        }

        private void btnCreate_MouseLeave(object sender, MouseEventArgs e)
        {
            ((Label)sender).Background = MiteOrange;
        }

        private void btnCreate_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenForm();
        }

        private void btnCreate_MouseUp(object sender, MouseButtonEventArgs e)
        {           
            ((Label)sender).Background = MiteOrange;
        }

        private void TrackTime(object sender, EventArgs e)
        {
            ListBoxItem row = null;
            TimeEntry timeEntry;

            // Gehe rekursiv eine Ebene höher, bis das ausgewählte ListBoxItem erreicht ist
            for (var vis = sender as Visual; vis != null; vis = VisualTreeHelper.GetParent(vis) as Visual)
            {
                if (vis is ListBoxItem)
                {
                    row = (ListBoxItem)vis;                  

                    try
                    {                       
                        // Tag standardmäßig auf false setzen
                        if(row.Tag == null) 
                            row.Tag = false;

                        // Timer starten
                        // Sicherstellen, dass der Timer noch nicht aktiv war
                        if (!(bool)row.Tag && !isRunning) 
                        {
                            timeEntry = (TimeEntry)(sender as Button).DataContext;
                            // Prüfen, ob das Ticket im Mite gestartet wurde
                            if(!IsLocked(timeEntry.Id)){
                                row.Tag = true;
                                ((Label)GetTemplateElement(row, "Clock", typeof(Label))).Background = MiteOrange;
                                // Der Anwendung den Status mitteilen
                                App.IsTracking = true;
                                activeRow = row;
                                isRunning = true;                            
                                start = DateTime.Now;
                                activeTicket = timeEntry;
                                timeEffort = new TimeSpan(0, activeTicket.Minutes, 0);

                                // Bearbeiten deaktivieren
                                ((Button)GetTemplateElement(activeRow, "btnEdit", typeof(Button))).Visibility = System.Windows.Visibility.Hidden;
                                // Löschen deaktivieren
                                ((Button)GetTemplateElement(activeRow, "btnDelete", typeof(Button))).Visibility = System.Windows.Visibility.Hidden;

                                // Infobox starten      
                                infobox = new Infobox(activeTicket.Project, activeTicket.Note.Length < 35 ? activeTicket.Note : activeTicket.Note.Substring(0, 32) + "...", GetElapsedTime(timeEffort), this);
                                infobox.Left = screenWidth - infobox.Width - 5;
                                infobox.Top = screenHeight - infobox.Height - 25;
                                infobox.Show();

                                // Button 'Neuer Eintrag' deaktivieren
                                btnCreate.Background = MiteGrey;
                                btnCreate.IsEnabled = false;
                            }
                            else
                            {
                                ShowMessage("Dieses Ticket wird gerade in Mite bearbeitet!", false, false);
                            }                                                        
                        }
                        // Nur das Ticket, welches den Timer gestartet hat, darf ihn auch anhalten                        
                        else if((bool)row.Tag)
                        {                                                        
                            StopTracking();
                        }
                        // Fehlermeldung
                        else
                        {
                            ShowMessage("Halten Sie erst den anderen Timer an!", false, false);
                        }                            
                    }
                    catch (Exception ex)
                    {
                        ShowMessage(ex.Message, false, false);
                    }                                  
                }
            }
        }

        public void StopTracking()
        {            
            ThreadPool.QueueUserWorkItem(new WaitCallback(delegate(object obj)
            {
                try
                {
                    if (MiteAPI.Update(MITE_URL + "time_entries/" + activeTicket.Id + ".xml", user, password, activeTicket))
                    {
                        Release();
                        ShowMessage("Gespeichert!", true, true);
                        this.Dispatcher.Invoke(new Action(delegate()
                        {
                            if (this.WindowState == System.Windows.WindowState.Minimized) this.WindowState = System.Windows.WindowState.Normal;
                        }));
                    }
                }
                catch (Exception ex)
                {
                    this.Dispatcher.Invoke(new Action(delegate()
                    {
                        Custom.Message.Show(ex.Message, "Fehler", NotificationImage.Critical);
                        Release();
                    }));
                }
            }));
        }

        /// <summary>
        /// Ermittelt ein Template-Element innerhalb eines ListBoxItem's
        /// </summary>
        /// <param name="row">aktive Zeile (ListBoxItem)</param>
        /// <param name="elementName">Name des gesuchten Elements</param>
        /// <param name="objClass">Klasse des gesuchten Elements</param>
        /// <returns></returns>
        private object GetTemplateElement(ListBoxItem row, string elementName, Type objClass)
        {
            ContentPresenter contentPresenter;
            DataTemplate dataTemplate;
            Object obj;

            // Getting the ContentPresenter of myListBoxItem
            contentPresenter = FindVisualChild<ContentPresenter>(row);

            // Finding textBlock from the DataTemplate that is set on that ContentPresenter
            dataTemplate = contentPresenter.ContentTemplate;
            obj = dataTemplate.FindName(elementName, contentPresenter);

            return obj;
        }
        
        private void Release()
        {            
            this.Dispatcher.Invoke(new Action(delegate()
            {
                // Merken, dass Zeiterfassung angehalten wurde
                App.IsTracking = false;
                isRunning = false;
                activeRow.Tag = false;
                // Pause zurücksetzen
                IsPaused = false;
                // Zeitaufwand zurücksetzen
                timeEffort = new TimeSpan(0, 0, 0);
                // Uhr auf Grau setzen
                ((Label)GetTemplateElement(activeRow, "Clock", typeof(Label))).Background = MiteGrey;
                // Bearbeiten aktivieren
                ((Button)GetTemplateElement(activeRow, "btnEdit", typeof(Button))).Visibility = System.Windows.Visibility.Visible;
                // Löschen aktivieren
                ((Button)GetTemplateElement(activeRow, "btnDelete", typeof(Button))).Visibility = System.Windows.Visibility.Visible;
                // Infobox schliessen
                infobox.Close();
                // Button 'Neuer Eintrag' aktivieren
                btnCreate.Background = MiteOrange;
                btnCreate.IsEnabled = true;
            }));
        }

        /// <summary>
        /// Wird ausgeführt, wenn Anwendung in den Leerlaufmodus verfällt oder wieder aktiviert wird
        /// </summary>
        private void OnActivityChanged()
        {
            if (ActivityChanged != null) ActivityChanged(this, new ActivityEventArgs(this.isIdle));
        }

        /// <summary>
        /// Wird ausgeführt, wenn eine weitere Minute zum Zeitaufwand dazugezählt wird
        /// </summary>
        private void OnMinuteElapsed()
        {
            // Ereignis auslösen (aktualisiert Infobox)
            if (MinuteElapsed != null) MinuteElapsed(this, new TrackingEventArgs(GetElapsedTime(timeEffort)));
        }

        private childItem FindVisualChild<childItem>(DependencyObject obj)
        where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                    return (childItem)child;
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        private void btnCancel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CloseForm();            
        }

        private void OpenForm()
        {
            if (NewEntry.Height == 0)
            {
                DoubleAnimation anim = new DoubleAnimation();
                anim.From = 0;
                anim.To = 250;
                anim.Duration = new Duration(TimeSpan.FromMilliseconds(500));
                NewEntry.BeginAnimation(Grid.HeightProperty, anim, HandoffBehavior.SnapshotAndReplace);
            }            
        }

        private void CloseForm()
        {            
            updateTicket = false;            
            updateTicketId = 0;
            Note.Text = "";
            Duration.Text = "";

            DoubleAnimation anim = new DoubleAnimation();
            anim.From = 250;
            anim.To = 0;
            anim.Duration = new Duration(TimeSpan.FromMilliseconds(500));
            NewEntry.BeginAnimation(Grid.HeightProperty, anim, HandoffBehavior.SnapshotAndReplace);
        } 

        private void btnSave_MouseDown(object sender, MouseButtonEventArgs e)
        {
            int projectId = 0;
            int serviceId = 0;
            int minutes = 0;
            string project;
            string service;
            string note;
            
            try
            {
                projectId = (int)cboProjects.SelectedValue;
                project = ((KeyValuePair<int, string>)cboProjects.SelectedItem).Value;
                serviceId = (int)cboServices.SelectedValue;
                service = ((KeyValuePair<int, string>)cboServices.SelectedItem).Value;
                note = Note.Text;
                if (Duration.Text != "")
                    minutes = ConvertTimeToMinutes(Duration.Text);

                // Neues Ticket
                if (!updateTicket)
                {
                    TimeEntry timeEntry = new TimeEntry(0, projectId, project, serviceId, service, note, minutes);
                    thread = new Thread(new ParameterizedThreadStart(CreateTicket));
                    thread.Start(timeEntry);
                }
                // Ticket aktualisieren
                else
                {
                    TimeEntry timeEntry = new TimeEntry(updateTicketId, projectId, project, serviceId, service, note, minutes);
                    thread = new Thread(new ParameterizedThreadStart(UpdateTicket));
                    thread.Start(timeEntry);
                }

                Duration.Text = "";
                Note.Text = "";
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, false, false);
            }            
        }

        private int ConvertTimeToMinutes(string time)
        {
            Regex regex;
            int minutes = 0;
            string[] arr;

            regex = new Regex(@"^[\d]{1,2}:[\d]{2}$");

            if (regex.IsMatch(time))
            {
                arr = time.Split(':');
                minutes = (Convert.ToInt32(arr[0]) * 60) + Convert.ToInt32(arr[1]);
            }
            else
            {
                throw new Exception("Falsches Zeitrformat!");
            }            

            return minutes;
        }

        private void UpdateTicket(object ticket)
        {

            try
            {
                if (MiteAPI.Update(MITE_URL + "time_entries/" + ((TimeEntry)ticket).Id + ".xml", user, password, (TimeEntry)ticket))
                {
                    this.Dispatcher.Invoke(new Action(delegate()
                    {
                        updateTicket = false;
                        updateTicketId = 0;
                        CloseForm();
                    }));
                    // Aktualisiere die Liste
                    ThreadPool.QueueUserWorkItem(new WaitCallback(LoadTickets));
                }
            }
            catch (Exception ex)
            {
                this.Dispatcher.Invoke(new Action(delegate()
                {
                    Custom.Message.Show(ex.Message, "Fehler", NotificationImage.Critical);                    
                }));
            }
        }

        private void CreateTicket(object ticket)
        {
            if(MiteAPI.Create(MITE_URL + "time_entries.xml", user, password, (TimeEntry)ticket)){
                this.Dispatcher.Invoke(new Action(delegate()
                {
                    CloseForm();                    
                }));
                // Aktualisiere die Liste
                ThreadPool.QueueUserWorkItem(new WaitCallback(LoadTickets));
            }            
        }

        /// <summary>
        /// Rechnet Zeitspanne in Stunden und Minuten um
        /// </summary>
        /// <param name="timeSpan">Zeitspanne</param>
        /// <returns></returns>
        private string GetElapsedTime(TimeSpan timeSpan)
        {
            string result = "";

            // Stunden
            if (timeSpan.Hours == 0)
                result = "00:";
            else if (timeSpan.Hours > 0 && timeSpan.Hours < 10)
                result = "0" + timeSpan.Hours + ":";
            else if (timeSpan.Hours >= 10) result = timeSpan.Hours + ":";

            // Minuten
            if (timeSpan.Minutes == 0)
                result += "00";
            else if (timeSpan.Minutes > 0 && timeSpan.Minutes < 10)
                result += "0" + timeSpan.Minutes;
            else if (timeSpan.Minutes >= 10)
                result += timeSpan.Minutes;

            return result;
        }

        void notifyIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (isRunning)
            {
                if (infobox.Visibility == System.Windows.Visibility.Hidden)
                    infobox.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                // Linksklick
                if (e.Button.ToString() == "Left")
                {
                    if (this.Visibility == System.Windows.Visibility.Hidden)
                    {
                        this.Visibility = System.Windows.Visibility.Visible;
                        menuItemMainWindow.Enabled = false;
                    }
                    else
                    {
                        if (this.WindowState == System.Windows.WindowState.Minimized)
                        {
                            this.WindowState = System.Windows.WindowState.Normal;
                            this.Activate();
                        }
                    }
                }
            }
        }

        void menuItemMainWindow_Click(object sender, EventArgs e)
        {
            if (this.Visibility == System.Windows.Visibility.Hidden || this.WindowState == System.Windows.WindowState.Minimized)
            {
                this.Visibility = System.Windows.Visibility.Visible;
                this.WindowState = System.Windows.WindowState.Normal;
            }
            else
            {
                if (!this.IsActive) this.Activate();
            }
        }

        void menuItemInfoBox_Click(object sender, EventArgs e)
        {
            infobox.Visibility = System.Windows.Visibility.Visible;
            menuItemInfoBox.Enabled = false;
        }

        void menuItemExit_Click(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            Environment.Exit(0);
        }

        /// <summary>
        /// Zeigt Meldungen an
        /// </summary>
        /// <param name="text">Meldung</param>
        /// <param name="success">Bestimmt die Art der Meldung</param>
        /// <param name="async">Legt fest, ob die Meldung aus einem Thread hearus angezeigt wird</param>
        private void ShowMessage(string text, bool success, bool async)
        {
            if (async)
            {
                lblStatus.Dispatcher.Invoke(new Action(() => lblStatus.Content = text));
               
                if (success)
                    lblStatus.Dispatcher.Invoke(new Action(() => lblStatus.Foreground = new SolidColorBrush(Colors.White)));
                else
                    lblStatus.Dispatcher.Invoke(new Action(() => lblStatus.Foreground = new SolidColorBrush(Colors.White)));
                    //lblStatus.Dispatcher.Invoke(new Action(() => lblStatus.Foreground = new SolidColorBrush(Color.FromRgb(242, 83, 83))));

                this.Dispatcher.Invoke(new Action(() => messageShown = 0));
            }
            else
            {
                lblStatus.Content = text;                
                if (success)  
                    lblStatus.Foreground = new SolidColorBrush(Colors.White);              
                else
                    lblStatus.Dispatcher.Invoke(new Action(() => lblStatus.Foreground = new SolidColorBrush(Colors.White)));
                    //lblStatus.Foreground = new SolidColorBrush(Color.FromRgb(242, 83, 83));              

                messageShown = 0;
            }
        }

        private void DeleteTicket(object sender, RoutedEventArgs e)
        {
            TimeEntry timeEntry = (TimeEntry)((Button)sender).DataContext;

            // Prüfen, ob das Ticket im Mite gestartet wurde
            if (!IsLocked(timeEntry.Id))
            {
                if (Custom.Message.Show("Möchten Sie das Ticket unwiderruflich löschen?", "Löschen", NotificationImage.Question) == NotificationResult.Yes)
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(delegate(object obj)
                    {
                        if (MiteAPI.DeleteRequest(String.Format("{0}time_entries/{1}.xml", MITE_URL, timeEntry.Id), user, password))
                        {
                            ShowMessage("Ticket wurde gelöscht!", true, true);
                            // Aktualisiere die Liste
                            ThreadPool.QueueUserWorkItem(new WaitCallback(LoadTickets));
                        }
                    }));
                }
            }
            else
            {
                ShowMessage("Dieses Ticket wird gerade in Mite bearbeitet!", false, false);
            } 
        }

        private void EditTicket(object sender, RoutedEventArgs e)
        {
            TimeEntry timeEntry = (TimeEntry)((Button)sender).DataContext;
            Custom.Converters.MinutesToStringConverter converter = new Converters.MinutesToStringConverter();

            // Prüfen, ob das Ticket im Mite gestartet wurde
            if (!IsLocked(timeEntry.Id))
            {
                updateTicket = true;
                updateTicketId = timeEntry.Id;

                OpenForm();
                Duration.Text = (string)converter.Convert(timeEntry.Minutes, null, null, null);
                cboProjects.IsSynchronizedWithCurrentItem = true;
                cboProjects.SelectedItem = new KeyValuePair<int, string>(timeEntry.ProjectId, projects[timeEntry.ProjectId]);
                cboServices.SelectedItem = new KeyValuePair<int, string>(timeEntry.ServiceId, services[timeEntry.ServiceId]);
                Note.Text = timeEntry.Note;
            }
            else
            {
                ShowMessage("Dieses Ticket wird gerade in Mite bearbeitet!", false, false);
            } 
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                if (!isRunning)
                    ThreadPool.QueueUserWorkItem(new WaitCallback(LoadTickets));
                else
                    ShowMessage("Aktualisieren geht während der Zeiterfassung nicht!", false, false);
            }
        }               
    }

    public enum Metadata
    {
        Projects,
        Services        
    }

    public class ActivityEventArgs : EventArgs
    {
        private bool isIdle;

        public bool IsIdle
        {
            get { return isIdle; }
            set { isIdle = value; }
        }

        public ActivityEventArgs(bool isIdle)
        {
            this.isIdle = isIdle;
        }
    }

    public class TrackingEventArgs : EventArgs
    {
        private string timeElapsed;

        public string TimeElapsed
        {
            get { return timeElapsed; }
            set { timeElapsed = value; }
        }

        public TrackingEventArgs(string timeElapsed)
        {
            this.timeElapsed = timeElapsed;
        }
    }
}
