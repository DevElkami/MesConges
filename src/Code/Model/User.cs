using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebApplicationConges.Model
{
    public class User
    {
        [Required]
        [EmailAddress]
        [MaxLength(50)]
        [RegularExpression("[^<>:='\"]*")]
        public String Email { get; set; }

        [MaxLength(64)]
        public String HashPwd { get; set; }

        public int ServiceId { get; set; }

        [RegularExpression("[^<>:='\"]*")]
        [MaxLength(50)]
        public String Name { get; set; }
        public DateTime DateCreate { get; set; } = DateTime.Now;

        [RegularExpression("[^<>:='\"]*")]
        [MaxLength(50)]
        public String Surname { get; set; }

        [RegularExpression("[^<>:='\"]*")]
        [MaxLength(50)]
        public String FamilyName { get; set; }

        [Phone]
        [MaxLength(20)]
        [RegularExpression("[^<>:='\"]*")]
        public String PhoneNumber { get; set; }

        [RegularExpression("[^<>:='\"]*")]
        [MaxLength(200)]
        public String Description { get; set; }

        [RegularExpression("[^<>:='\"]*")]
        [MaxLength(30)]
        public String Matricule { get; set; }
        public DateTime LastConnection { get; set; } = DateTime.Now;

        public List<Conge> CongesValidated { get; set; } = new List<Conge>();
        public List<Conge> CongesInProgress { get; set; } = new List<Conge>();
        public List<Conge> CongesRefused { get; set; } = new List<Conge>();
        public Manager Manager { get; set; }

        [Required]
        [RegularExpression("[^<>:='\"]*")]
        [MaxLength(50)]
        public String Login { get; set; }
        public bool IsManager { get; set; }
        public bool IsDrh { get; set; }
        public bool IsAdmin { get; set; }
        public Service Service { get; set; }
        public bool Imported { get; set; } = false;

        public override string ToString()
        {
            return Name;
        }
    }
}