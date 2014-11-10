using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Custom
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        private static bool isTracking;
        public static bool IsTracking
        {
            get { return isTracking; }
            set { isTracking = value; }
        }

        /// <summary>
        /// Beendet die Anwendung
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            // Herunterfahren | Abmelden
            if (e.ReasonSessionEnding == ReasonSessionEnding.Shutdown || e.ReasonSessionEnding == ReasonSessionEnding.Logoff)
            {
                // Es wird versucht den Vorgang des Herunterfahrens abzubrechen, falls Zeiterfassung noch läuft
                // Zumindest wird der User vom Betriebssystem benachrichtigt, dass die Anwendung den Vorgang blockiert
                if (IsTracking)
                {
                    //CustomMessageBox.Show("Zeiterfassung läuft noch!", "Warnung", NotificationImage.Mode.Critical);
                    e.Cancel = true;
                }
            }
        }
    }
}
