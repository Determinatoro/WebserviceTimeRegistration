using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebserviceTimeRegistration.Database_objects
{
    public class TimeRegistration
    {
        private int timeRegId;
        private string startTime;
        private string endTime;
        private TimeSpan total;
        private int orderId;
        private int userId;
        private string description;
        private string note;

        public TimeRegistration()
        {
            
        }

        public TimeRegistration(int timeRegId, string startTime, string endTime, TimeSpan total, int orderId, int userId, string description, string note)
        {
            this.timeRegId = timeRegId;
            this.startTime = startTime;
            this.endTime = endTime;
            this.total = total;
            this.orderId = orderId;
            this.userId = userId;
            this.description = description;
            this.note = note;
        }

        public int TimeRegId
        {
            get
            {
                return timeRegId;
            }

            set
            {
                timeRegId = value;
            }
        }

        public string StartTime
        {
            get
            {
                return startTime;
            }

            set
            {
                startTime = value;
            }
        }

        public string EndTime
        {
            get
            {
                return endTime;
            }

            set
            {
                endTime = value;

                if (EndTime != "")
                {
                    TimeSpan timeSpan = new TimeSpan();

                    timeSpan = (DateTime.Parse(endTime) - DateTime.Parse(startTime));

                    total = timeSpan;
                }
            }
        }

        public int OrderId
        {
            get
            {
                return orderId;
            }

            set
            {
                orderId = value;
            }
        }

        public int UserId
        {
            get
            {
                return userId;
            }

            set
            {
                userId = value;
            }
        }

        public string Description
        {
            get
            {
                return description;
            }

            set
            {
                description = value;
            }
        }

        public string Note
        {
            get
            {
                return note;
            }

            set
            {
                note = value;
            }
        }

        public string Total
        {
            get
            {                
                return total.ToString("d'd 'h'h 'm'm'"); ;
            }
        }
    }
}