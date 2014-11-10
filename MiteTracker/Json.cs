using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Custom
{
    static class Json
    {
        /// Parst Json-Array zu einer bestimmten Ressource
        /// </summary>
        /// <param name="json">Json</param>
        /// <param name="user">User</param>
        /// <param name="password">Passwort</param>
        /// <returns></returns>
        public static Dictionary<int, string> Parse(string json, Metadata res)
        {
            JArray jArray;
            JObject jObj;            
            Dictionary<int, string> items = new Dictionary<int, string>();            
            int key = 0;
            string value = "";

            try
            {
                jArray = JArray.Parse(json);
                for (int i = 0; i < jArray.Count(); i++)
                {
                    jObj = (JObject)jArray[i];
                    switch (res)
                    {
                        case Metadata.Projects:
                            key = (int)jObj.SelectToken("project.id");
                            value = String.Format("{0} ({1})", jObj.SelectToken("project.name").ToString(), jObj.SelectToken("project.customer_name").ToString());                                                        
                            break;
                        case Metadata.Services:
                            key = (int)jObj.SelectToken("service.id");
                            value = jObj.SelectToken("service.name").ToString();                            
                            break;                        
                    }
                    
                    items.Add(key, value);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return items;
        }        

        /// Wandelt JSON-Array in eine Liste mit Tickets um
        /// </summary>
        /// <param name="json">Json</param>
        /// <param name="user">User</param>
        /// <param name="password">Passwort</param>
        /// <returns></returns>
        public static List<TimeEntry> ParseTimeEntries(string json)
        {
            JArray jArray;
            JObject jObj;            
            List<TimeEntry> timeEntries = new List<TimeEntry>();
            TimeEntry timeEntry;
            int id = 0;
            int projectId = 0;
            int serviceId = 0;
            string project = "";
            string service = "";
            string note = "";
            int minutes = 0;
            
            try
            {
                jArray = JArray.Parse(json);
                for (int i = 0; i < jArray.Count(); i++)
                {
                    jObj = (JObject)jArray[i].SelectToken("time_entry");

                    if (PropertyExists(jObj, "id")) id = (int)jObj.SelectToken("id");
                    if (PropertyExists(jObj, "project_id")) projectId = (int)jObj.SelectToken("project_id");
                    if (PropertyExists(jObj, "project_name")) project = jObj.SelectToken("project_name").ToString();
                    if (PropertyExists(jObj, "service_id")) serviceId = (int)jObj.SelectToken("service_id");
                    if (PropertyExists(jObj, "service_name")) service = jObj.SelectToken("service_name").ToString();
                    if (PropertyExists(jObj, "note")) note = jObj.SelectToken("note").ToString();
                    if (PropertyExists(jObj, "minutes")) minutes = (int)jObj.SelectToken("minutes");

                    timeEntry = new TimeEntry(id, projectId, project, serviceId, service, note, minutes);
                    timeEntries.Add(timeEntry);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return timeEntries;
        }

        public static bool ParseTicketStatus(string json)
        {            
            JObject jObj;
            bool isLocked = false;

            jObj = JObject.Parse(json);            
            jObj = (JObject)jObj.SelectToken("time_entry");

            isLocked = PropertyExists(jObj, "tracking");

            return isLocked;
        }

        private static bool PropertyExists(JObject jObj, string key)
        {
            JProperty property = jObj.Property(key);

            return property != null;
        }
    }
}
