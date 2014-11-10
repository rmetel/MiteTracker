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
using System.Windows.Shapes;

namespace Custom
{
    /// <summary>
    /// Interaktionslogik für Infobox.xaml
    /// </summary>
    public partial class Infobox : Window
    {        
        private string subject;
        private string task;
        private string elapsed;
        private MainWindow mainWindow;
        private ImageSource imageSource;
        private ImageBrush imageBrush;

        public string Subject
        {
            get { return subject; }
            set { subject = value; }
        }

        public string Task
        {
            get { return task; }
            set { task = value; }
        }

        public string Elapsed
        {
            get { return elapsed; }
            set { elapsed = value; }
        }


        public Infobox(string subject, string task, string elapsed, MainWindow mainWindow)
        {
            this.Subject = subject;
            this.Task = task;
            this.Elapsed = elapsed;
            this.mainWindow = mainWindow;
            this.mainWindow.ActivityChanged += new MainWindow.ActivityEventHandler(mainWindow_ActivityChanged);
            this.mainWindow.MinuteElapsed += new MainWindow.TrackingEventHandler(mainWindow_MinuteElapsed);
            InitializeComponent();
        }

        /// <summary>
        /// Wird ausgeführt, wenn das Hauptfenster weitere Minute erfasst hat
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void mainWindow_MinuteElapsed(object sender, TrackingEventArgs e)
        {            
            infoElapsed.Content = e.TimeElapsed;            
        }

        /// <summary>
        /// Wird ausgeführt wenn das Hauptfenster sich als aktiv bzw. inaktiv meldet
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        void mainWindow_ActivityChanged(object source, ActivityEventArgs e)
        {
            try
            {
                // Leerlauf
                if (e.IsIdle) Pause();
                // Aktivieren
                else Play();
            }
            catch (Exception ex)
            {
                //CustomMessageBox.Show(ex.Message, "Warnung", NotificationImage.Warning);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            infoSubject.Content = Subject;
            infoTask.Content = Task;
            infoElapsed.Content = Elapsed;
            this.ShowInTaskbar = false;
        }

        // Tracker anhalten
        private void stop_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mainWindow.StopTracking();
        }

        // Fenster verschieben
        private void infoSubject_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        // Play - Pause
        private void play_MouseDown(object sender, MouseButtonEventArgs e)
        {
            mainWindow.IsPaused = !mainWindow.IsPaused;
            if (mainWindow.IsPaused) Pause();
            else Play();
        }

        // Mit Doppelklick auf den Header wird die Infobox ausgeblendet
        private void infoSubject_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            mainWindow.notifyIcon.ContextMenu.MenuItems["Infobox"].Enabled = true;
            this.Hide();
        }

        /// <summary>
        /// Versetzt das Fenster visuell in Ruhezustand
        /// </summary>
        private void Pause()
        {
            // Dunkelgrauer Hintergrund
            GUI.Fill = new SolidColorBrush(Color.FromRgb(3, 3, 3));
            // Transparenz senken
            GUI.Opacity = 0.3;
            // Dunkler Text
            infoTask.Foreground = new SolidColorBrush(Color.FromRgb(3, 3, 3));
            //play.Fill = new ImageBrush(new BitmapImage(new Uri("playback_play.png", UriKind.Relative)));
            imageSource = (ImageSource)this.FindResource("imgPlay");
            imageBrush = new ImageBrush(imageSource);
            play.Fill = imageBrush;
        }

        /// <summary>
        /// Versetzt das Fenster visuell in aktiven Zustand
        /// </summary>
        private void Play()
        {
            // Weisser Hintergrund
            GUI.Fill = new SolidColorBrush(Color.FromRgb(246, 246, 246));
            // Transparenz erhöhen
            GUI.Opacity = 1;
            // Orangener Text
            infoTask.Foreground = new SolidColorBrush(Color.FromRgb(255, 130, 21));
            //play.Fill = new ImageBrush(new BitmapImage(new Uri("playback_pause.png", UriKind.RelativeOrAbsolute)));
            imageSource = (ImageSource)this.FindResource("imgPause");
            imageBrush = new ImageBrush(imageSource);
            play.Fill = imageBrush;
        }
    }
}
