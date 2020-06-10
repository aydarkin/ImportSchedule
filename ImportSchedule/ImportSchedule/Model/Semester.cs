using System;
using System.Collections.Generic;
using System.Text;

namespace ImportSchedule.Model
{
    public class Semester
    {
        public string Type { get; set; }
        public string AcademicYear { get; set; }
        public int WeekOffset { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public Semester(string type, string year, string start, string end, int weekOffset)
        {
            Type = type;
            AcademicYear = year;
            WeekOffset = weekOffset;

            StartDate = DateTime.Parse(start);
            EndDate = DateTime.Parse(end);
        }


        public string Title { get { return Type + " " + AcademicYear; } }
        
        public override string ToString()
        {
            return Title;
        }
    }

}
