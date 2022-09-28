using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Reflection;

namespace WebApplicationConges.Pages
{
    public class AboutModel : PageModel
    {
        public String Message { get; set; }
        public String Date { get { return DateTime.Now.Year.ToString(); } }
        public String Version { get; set; }

        public void OnGet()
        {
            Message = "Contactez-nous afin d'améliorer le programme.";

            Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}
