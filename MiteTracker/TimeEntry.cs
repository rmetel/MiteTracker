using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Custom
{
    class TimeEntry: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int id; 

        public int Id
        {
            get { return id; }
            set { id = value; }
        }        
    
        private int projectId;

        public int ProjectId
        {
            get { return projectId; }
            set { projectId = value; }
        }        

        private string project;

        public string Project
        {
            get { return project; }
            set { project = value; }
        }

        private int serviceId;

        public int ServiceId
        {
            get { return serviceId; }
            set { serviceId = value; }
        }        

        private string service;

        public string Service
        {
            get { return service; }
            set { service = value; }
        }

        private string note;

        public string Note
        {
            get { return note; }
            set { note = value; }
        }

        private int minutes;

        public int Minutes
        {
            get { return minutes; }
            set { minutes = value; OnPropertyChanged(new PropertyChangedEventArgs("Minutes")); }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }    

        public TimeEntry(int id, int projectId, string project, int serviceId, string service, string note, int minutes)
        {
            this.id = id;
            this.projectId = projectId;
            this.project = project;
            this.serviceId = serviceId;
            this.service = service;
            this.note = note;
            this.minutes = minutes;
        }  
    }
}
