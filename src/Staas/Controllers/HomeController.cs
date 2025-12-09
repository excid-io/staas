using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using System.Security.Cryptography;
using Excid.Staas.Models;
using Excid.Oidc.Models;

namespace idp.Controllers
{
	[AllowAnonymous]
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly IConfiguration _configuration;
		private readonly JsonWebKey _publicJWK;


        public HomeController(IConfiguration configuration, ILogger<HomeController> logger)
		{
			_logger = logger;
			_configuration = configuration;
			_publicJWK = new JsonWebKey();
            string privateKeyPem = _configuration.GetValue<string>("IdP:PrivateKeyPem") ?? "";
            string privateKeyPemPassord = _configuration.GetValue<string>("IdP:PrivateKeyPemPassord") ?? "";
            try
            {
                string pemKey = System.IO.File.ReadAllText(privateKeyPem);
                var signingecdsa = ECDsa.Create();
                signingecdsa.ImportFromEncryptedPem(new System.ReadOnlySpan<char>(pemKey.ToCharArray()), privateKeyPemPassord);
                _publicJWK = JsonWebKeyConverter.ConvertFromECDsaSecurityKey(new ECDsaSecurityKey(signingecdsa));
                _publicJWK.D = null;
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception in Homecontroller:" + ex.ToString());
            }
        }

		public IActionResult Index()
		{
			return View();
		}


		public IActionResult Jwks()
		{
            var JwkSet = new JwkSet();
			JwkSet.Keys.Add(_publicJWK);
			return Content(JsonSerializer.Serialize(JwkSet), "application/json");
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