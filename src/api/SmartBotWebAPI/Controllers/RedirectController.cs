using Microsoft.AspNetCore.Mvc;


namespace SmartBotWebAPI.Controllers
{
    [Route("/")]
    [ApiController]
    public class RedirectController : ControllerBase
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Redirect("https://smartbotblazorapp.azurewebsites.net");
        }

    }
}
