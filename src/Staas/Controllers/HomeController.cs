using Excid.Oidc.Models;
using Excid.Staas.Models;
using Excid.Staas.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text.Json;

namespace idp.Controllers
{
	[AllowAnonymous]
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly IConfiguration _configuration;
        private readonly IJwtSigner _jwtSigner;

        public HomeController(IConfiguration configuration, ILogger<HomeController> logger, IJwtSigner jwtSigner)
		{
			_logger = logger;
			_configuration = configuration;
			_jwtSigner = jwtSigner;
        }

		public IActionResult Index()
		{
			return View();
		}


		public IActionResult Jwks()
		{
			return Content(JsonSerializer.Serialize(_jwtSigner.GetJwkSet()), "application/json");
		}

        public IActionResult Verify()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			string exceptionMessage = string.Empty;
			var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
			if (exceptionHandlerPathFeature?.Error is StaasException)
			{
				exceptionMessage = exceptionHandlerPathFeature.Error.Message;
			}
			ViewData["exceptionMessage"] = exceptionMessage;
			return View();
		}

		[Authorize]
		public async Task Logout()
		{
			await HttpContext.SignOutAsync("cookies");
			await HttpContext.SignOutAsync("oidc");

		}
	}
}