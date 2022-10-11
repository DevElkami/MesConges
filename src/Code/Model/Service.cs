using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplicationConges.Model
{
    public class Service
    {
        public long Id { get; set; }

        [RegularExpression(@"[A-Za-z0-9 _.-]*")]
        [MaxLength(70)]
        public String Name { get; set; }

        [RegularExpression(@"[A-Za-z0-9 _.-]*")]
        [MaxLength(200)]
        public String Description { get; set; }
        public Manager Manager { get; set; }

        public override String ToString()
        {
            return Name;
        }
    }
}