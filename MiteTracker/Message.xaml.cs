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
    /// Interaktionslogik für Message.xaml
    /// </summary>
    public partial class Message : Window
    {        
        private static Message message;
        private static NotificationResult result = NotificationResult.Ok;

        public Message()
        {
            InitializeComponent();
        }

        private Brush SetColor(string hexColorCode)
        {
            var brushConverter = new BrushConverter();
            return (Brush)brushConverter.ConvertFrom(hexColorCode);
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (Ok.Content.ToString() == "OK") result = NotificationResult.Ok;
            else if (Ok.Content.ToString() == "Ja") result = NotificationResult.Yes;
            this.Close();
        }

        private void DragMe(object sender, MouseButtonEventArgs e)
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

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (Cancel.Content.ToString() == "Cancel") result = NotificationResult.Cancel;
            else if (Cancel.Content.ToString() == "Nein") result = NotificationResult.No;
            this.Close();
        }

        public static NotificationResult Show(string text, string title, NotificationImage image)
        {
            message = new Message();
            message.Text.Text = text;
            message.Title.Content = title;

            switch (image)
            {
                case NotificationImage.Confirm:
                    {
                        //cmb.Icon.Fill = new ImageBrush(new BitmapImage(new Uri(@"..\..\pics\notification_confirm.png", UriKind.Relative)));
                        message.SetImage("imgConfirm");
                        break;
                    }
                case NotificationImage.Critical:
                    {
                        //cmb.Icon.Fill = new ImageBrush(new BitmapImage(new Uri(@"..\..\pics\notification_critical.png", UriKind.Relative)));
                        message.SetImage("imgCritical");
                        break;
                    }
                case NotificationImage.Information:
                    {
                        //cmb.Icon.Fill = new ImageBrush(new BitmapImage(new Uri(@"..\..\pics\notification_critical.png", UriKind.Relative)));
                        message.SetImage("imgInformation");
                        break;
                    }
                case NotificationImage.Question:
                    {
                        message.Cancel.Visibility = Visibility.Visible;
                        message.Ok.Content = "Ja";
                        message.Cancel.Content = "Nein";
                        //cmb.Icon.Fill = new ImageBrush(new BitmapImage(new Uri(@"..\..\pics\notification_question.png", UriKind.Relative)));
                        message.SetImage("imgQuestion");
                        break;
                    }
                case NotificationImage.Warning:
                    {
                        //cmb.Icon.Fill = new ImageBrush(new BitmapImage(new Uri(@"..\..\pics\notification_warning.png", UriKind.Relative)));
                        message.SetImage("imgWarning");
                        break;
                    }
            }
            //cmb.ShowInTaskbar = false;
            message.ShowDialog();

            return result;
        }

        /// <summary>
        /// Setz ein ausgewähltes Icon
        /// </summary>
        /// <param name="img"></param>
        private void SetImage(string img)
        {
            ImageSource imageSource;
            ImageBrush imageBrush;

            imageSource = (ImageSource)this.FindResource(img);
            imageBrush = new ImageBrush(imageSource);
            this.Icon.Fill = imageBrush;
        }

        private void Meldung_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && Ok.Content.ToString() == "OK")
            {
                result = NotificationResult.Ok;
                this.Close();
            }
        }
    }
}
