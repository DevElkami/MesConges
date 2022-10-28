using System;
using System.ComponentModel.DataAnnotations;

namespace WebApplicationConges.Model
{
    public class Service
    {
        public long Id { get; set; }

        [RegularExpression("[^<>:='\"]*")]
        [MaxLength(70)]
        public String Name { get; set; }

        [RegularExpression("[^<>:='\"]*")]
        [MaxLength(200)]
        public String Description { get; set; }
        public Manager Manager { get; set; }

        public override String ToString()
        {
            return Name;
        }
    }
}