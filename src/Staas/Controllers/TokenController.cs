using Excid.Staas.Data;
using Excid.Staas.Models;
using idp.Controllers;
using Microsoft.AspNetCore.Mvc;
using Staas.Security;
using System.Security.Claims;

namespace Staas.Controllers
{
    public class TokenController : Controller
    {
        private readonly ILogger<TokenController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ISecureDbAccess _secureDbAccess;

        public TokenController(IConfiguration configuration, ILogger<TokenController> logger, ISecureDbAccess secureDbAccess)
        {
            _logger = logger;
            _configuration = configuration;
            _secureDbAccess = secureDbAccess;

        }

        public IActionResult Index()
        {
            var apiTokens = _secureDbAccess.ListAPITokens();
            return View(apiTokens);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(APIToken apiToken)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            string? signer = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (signer == null)
            {
                _logger.LogError("Error in parsing id token: identity token does not inlcude an email");
                throw new StaasException("Error in parsing id token: identity token does not inlcude am email");
            }
            var newToken = new APIToken();
            newToken.Description = apiToken.Description;
            newToken.Signer = signer;
            newToken.Token = Guid.NewGuid().ToString();
            await _secureDbAccess.AddAPIToken(newToken);
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int? id)
        {
            var entry = _secureDbAccess.GetAPIToken(id);
            if (entry == null)
            {
                _logger.LogWarning("A user tried to access Details of an authorized item");
                throw new StaasException("Cannot access the requested item");
            }

            return View();
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            bool result = await _secureDbAccess.DeleteAPIToken(id);
            if (!result)
            {
                _logger.LogWarning("A user tried to access Details of an authorized item");
                throw new StaasException("Cannot access the requested item");
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
