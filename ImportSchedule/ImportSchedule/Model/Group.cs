using System;
using System.Collections.Generic;
using System.Text;

namespace ImportSchedule
{
    public class Group
    {
        public Group(string name, string faculty = "", int year = 0)
        {
            Name = name;
            Faculty = faculty;
            Year = year;
        }

        public int Year { get; set; }
        public string Faculty { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
