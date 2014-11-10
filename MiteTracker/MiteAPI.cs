using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Web;

namespace Custom
{
    class MiteAPI
    {
        /// <summary>
        /// Liest Daten (HTTP GET)
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="user">Benutzer</param>
        /// <param name="password">Passwort</param>
        /// <returns></returns>
        public static string Read(string url, string user, string password)
        {
            string result = null;
            StreamReader reader = null;
            HttpWebResponse response = null;
            HttpWebRequest request = null;

            try
            {
                // Neuer GET Request
                request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.Credentials = new NetworkCredential(user, password);

                using (response = (HttpWebResponse)request.GetResponse())
                {
                    reader = new StreamReader(response.GetResponseStream());
                    result = reader.ReadToEnd();
                }
            }
            catch (WebException we)
            {
                throw new WebException(GetErrorMessage(we));
            }
            finally
            {
                if (reader != null) reader.Close();
                if (response != null) response.Close();
            }

            return result;
        }

        /// <summary>
        /// Schreibt Daten (HTTP POST)
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="user">Benutzer</param>
        /// <param name="password">Passwort</param>
        /// <param name="timeEntry">Zeitaufwand</param>
        /// <returns></returns>
        public static bool Create(string url, string user, string password, TimeEntry timeEntry)
        {
            string data = "";
            Stream dataStream = null;
            HttpWebResponse response = null;
            HttpWebRequest request = null;

            try
            {
                // Neuer POST Request
                request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/xml";
                request.Credentials = new NetworkCredential(user, password);

                // Daten in XML umwandeln
                data = ConvertToXml(timeEntry);

                // Vorbereitung der Daten
                byte[] byteArray = Encoding.UTF8.GetBytes(data);

                // Länge des Inhalts festlegen
                request.ContentLength = byteArray.Length;

                // Stream
                dataStream = request.GetRequestStream();

                // Schreibe Daten in den Stream
                dataStream.Write(byteArray, 0, byteArray.Length);

                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException we)
            {
                throw new WebException(GetErrorMessage(we));
            }
            finally
            {
                if (dataStream != null) dataStream.Close();
                if (response != null) response.Close();
            }

            return true;
        }

        /// <summary>
        /// Aktualisiert Daten (HTTP PUT)
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="user">Benutzer</param>
        /// <param name="password">Passwort</param>
        /// <param name="timeEntry">Ticket</param>
        /// <returns></returns>
        public static bool Update(string url, string user, string password, TimeEntry timeEntry)
        {
            string data = "";
            Stream dataStream = null;
            HttpWebResponse response = null;
            HttpWebRequest request = null;

            try
            {
                // Neuer PUT Request
                request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "PUT";
                request.ContentType = "application/xml";
                request.Credentials = new NetworkCredential(user, password);

                // Daten in XML umwandeln
                data = ConvertToXml(timeEntry);

                // Vorbereitung der Daten
                byte[] byteArray = Encoding.UTF8.GetBytes(data);

                // Länge des Inhalts festlegen
                request.ContentLength = byteArray.Length;

                // Stream
                dataStream = request.GetRequestStream();

                // Schreibe Daten
                dataStream.Write(byteArray, 0, byteArray.Length);

                // Response
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException we)
            {
                throw new WebException(GetErrorMessage(we));
            }
            finally
            {
                if (dataStream != null) dataStream.Close();
                if (response != null) response.Close();
            }

            return true;
        }

        /// <summary>
        /// Löscht Daten (HTTP DELETE)
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="user">Benutzer</param>
        /// <param name="password">Passwort</param>
        /// <param name="timeEntry">Ticket</param>
        /// <returns></returns>
        public static bool DeleteRequest(string url, string user, string password)
        {            
            Stream dataStream = null;
            HttpWebResponse response = null;
            HttpWebRequest request = null;
            HttpStatusCode statusCode;

            try
            {
                // Neuer PUT Request
                request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "DELETE";                
                request.Credentials = new NetworkCredential(user, password);                

                // Response
                response = (HttpWebResponse)request.GetResponse();
                // Status
                statusCode = response.StatusCode;
                
            }
            catch (WebException we)
            {
                throw new WebException(GetErrorMessage(we));
            }
            finally
            {
                if (dataStream != null) dataStream.Close();
                if (response != null) response.Close();
            }

            return statusCode == HttpStatusCode.OK ? true : false;
        }

        /// <summary>
        /// Konvertiert ein Ticket ins XML
        /// </summary>
        /// <param name="timeEntry">Ticket</param>
        /// <returns></returns>
        private static string ConvertToXml(TimeEntry timeEntry)
        {
            string xml;
            DateTime date = DateTime.Now;            

            xml = "<?xml version=\"1.0\"?>";
            xml += "<time-entry>";
            xml += "<date-at>" + date.Year + "-" + date.Month + "-" + date.Day + "</date-at>";
            xml += "<minutes>" + timeEntry.Minutes + "</minutes>";
            xml += "<note>" + timeEntry.Note + "</note>";
            xml += "<service-id>" + timeEntry.ServiceId + "</service-id>";
            xml += "<project-id>" + timeEntry.ProjectId + "</project-id>";
            xml += "</time-entry>";

            return xml;
        }        

        /// <summary>
        /// Gibt eine Fehlermeldung anhand vom Statuscode aus
        /// </summary>
        /// <param name="statusCode">Statuscode</param>
        /// <returns></returns>
        private static string GetErrorMessage(WebException we)
        {
            // Erstmal die Originalnachricht speichern
            string errorMessage = we.Message;
            HttpStatusCode statusCode;

            if (we.Status == WebExceptionStatus.ConnectFailure)
            {
                errorMessage = "Es besteht keine Internetverbindung!";
            }
            else if (we.Status == WebExceptionStatus.Timeout)
            {
                errorMessage = "Anfrage wurde wegen Zeitüberschreitung abgebrochen!";
            }
            else if (we.Status == WebExceptionStatus.UnknownError)
            {
                errorMessage = "Unbekannter Fehler in Mite aufgetreten!";
            }
            else if (we.Status == WebExceptionStatus.RequestCanceled)
            {
                errorMessage = "Anfrage wurde abgebrochen!";
            }
            else if (we.Status == WebExceptionStatus.ProtocolError)
            {
                statusCode = ((HttpWebResponse)we.Response).StatusCode;

                switch (statusCode)
                {
                    case HttpStatusCode.Forbidden:
                        errorMessage = "Die Anfrage wurde vom Server verweigert!";
                        break;
                    case HttpStatusCode.Gone:
                        errorMessage = "Daten sind nicht mehr verfügbar!";
                        break;
                    case HttpStatusCode.InternalServerError:
                        errorMessage = "Interner Fehler in Mite aufgetreten!";
                        break;
                    case HttpStatusCode.MethodNotAllowed:
                        errorMessage = "Aktion nicht erlaubt!";
                        break;
                    case HttpStatusCode.NotFound:
                        errorMessage = "Ticket existiert nicht mehr!";
                        break;
                    case HttpStatusCode.RequestEntityTooLarge:
                        errorMessage = "Datenmenge ist zu groß!";
                        break;
                    case HttpStatusCode.RequestTimeout:
                        errorMessage = "Zeitüberschreitung der Anforderung!";
                        break;
                    case HttpStatusCode.ServiceUnavailable:
                        errorMessage = "Mite ist momentan wegen Überlastung oder Wartung nicht erreichbar!";
                        break;
                    case HttpStatusCode.Unauthorized:
                        errorMessage = "Benutzername oder Passwort falsch!";
                        break;
                    default:
                        errorMessage = "Unbekannter Fehler in Mite aufgetreten!";
                        break;
                }
            }

            return errorMessage;
        }
    }   
}
