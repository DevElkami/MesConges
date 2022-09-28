using System;

namespace WebApplicationConges.Model
{
    public class Service
    {
        public long Id { get; set; }
        public String Name { get; set; }
        public String Description { get; set; }
        public Manager Manager { get; set; }

        public override String ToString()
        {
            return Name;
        }
    }
}