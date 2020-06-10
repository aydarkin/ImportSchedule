using System;
using System.Collections.Generic;
using System.Text;

namespace ImportSchedule.Model
{
    public class ClassNumber
    {
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
        public int Number { get; set; }

        public ClassNumber(string start, string end, int number)
        {
            Start = TimeSpan.Parse(start);
            End = TimeSpan.Parse(end);
            Number = number;
        }
    }
}
