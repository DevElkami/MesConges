using System;

namespace WebApplicationConges.Model
{
    public class Manager
    {
        public String Id { get; set; }
        public long ServiceId { get; set; }
        public User User { get; set; }
        public Service Service { get; set; }

        public override string ToString()
        {
            if (User != null)
                return User.Name;
            return Id;
        }
    }
}