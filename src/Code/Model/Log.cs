using System;

namespace WebApplicationConges.Model
{
    public class Log
    {
        public long Id { get; set; }
        public String UserId { get; set; }
        public DateTime ActionDate { get; set; }
        public String Description { get; set; }
    }
}