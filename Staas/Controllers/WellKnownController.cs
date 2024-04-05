using Microsoft.AspNetCore.Mvc;
using Excid.Oidc.Models;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace idp.Controllers
{
    [AllowAnonymous]
    public class WellKnownController : Controller
	{
		private readonly ILogger<WellKnownController> _logger;
		private readonly IConfiguration _configuration;
		private readonly string _iss = String.Empty;

		public WellKnownController(IConfiguration configuration, ILogger<WellKnownController> logger)
		{
			_logger = logger;
			_configuration = configuration;
			_iss = _configuration.GetValue<string>("IdP:iss") ?? "";
        }


		public IActionResult Index()
		{
			return View();
		}

		[ActionName("openid-configuration")]
		public IActionResult OpenIDConfiguration()
		{
			_logger.LogInformation(".well-known: openid-configuration requested");
			
			var openIDConfiguration = new OpenIDConfiguration();
			openIDConfiguration.Issuer = _iss;
			openIDConfiguration.TokenEndpoint = _iss + "/Token";
			openIDConfiguration.AuthorizationEndpoint = _iss + "/Authorize";
			openIDConfiguration.JwksUri = _iss + "/Home/Jwks";
			return Content(JsonSerializer.Serialize(openIDConfiguration), "application/json");
		}
	}
}
