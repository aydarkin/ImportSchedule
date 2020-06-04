using System;
using System.Collections.Generic;
using System.Text;

namespace ImportSchedule
{
    public class Group
    {
        public Group(int id, string title)
        {
            Id = id;
            Title = title;
        }

        public int Id { get; set; }
        public string Title { get; set; }

    }
}
