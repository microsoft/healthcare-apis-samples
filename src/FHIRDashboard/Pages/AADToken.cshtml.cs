using FHIRDashboard.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


namespace FHIRDashboard.Pages
{
    public class AADTokenModel : PageModel
    {
        private ILogger<AADTokenModel> _logger;
        private DashboardAppSettings _config { get; set; }
        private string _accesstoken;

        public AADTokenModel(IOptions<DashboardAppSettings> settings, ILogger<AADTokenModel> logger)
        {
            _logger = logger;
            _config = settings.Value;
        }

        //public void  OnGet()
        public async System.Threading.Tasks.Task OnGet()
        {
            ViewData["Title"] = "AADToken";

            ViewData["AADToken"] = await AADTokenProvision.CheckAADToken(_config);

        }

    }

    
}

