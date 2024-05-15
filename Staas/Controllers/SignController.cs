
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Security.Claims;
using Excid.Staas.Models;
using Excid.Staas.Data;
using Excid.Sigstore.Rekor.v1.Models;
using Excid.Staas.Security;
using Excid.Sigstore.Fulcio.v2.Models;
using Staas.Security;

namespace idp.Controllers
{
	public class SignController : Controller
	{

		private readonly ILogger<SignController> _logger;
		private readonly IConfiguration _configuration;
        private readonly ISecureDbAccess _secureDbAccess;
        private readonly IRegistrySigner _registrySigner;
        private readonly string _issuer = string.Empty;

        public SignController(IConfiguration configuration, ILogger<SignController> logger, StassDbContext context, ISecureDbAccess secureDbAccess, IRegistrySigner registrySigner)
		{
			_logger = logger;
			_configuration = configuration;
            _secureDbAccess = secureDbAccess;
            _registrySigner = registrySigner;
            _issuer = _configuration.GetValue<string>("IdP:iss") ?? string.Empty;

        }

        public IActionResult Index()
        {
            ViewData["issuer"] = _issuer;
            return View();
        }

        public IActionResult List()
		{
            ViewData["issuer"] = _issuer;
            var signedItems = _secureDbAccess.ListSignedItems();
            return View(signedItems);
		}

        public IActionResult Certificate()
        {

            string fulcioCAPem = _configuration.GetValue<string>("Sigstore:FulcioCAPem") ?? string.Empty;
            if (fulcioCAPem == string.Empty)
            {
                _logger.LogError("Error in reading Fulcio certificate from configuration");
                throw new StaasException("Fulcio certificate has not been properly configured");
            }
            try
            {
                string filecontents = System.IO.File.ReadAllText(fulcioCAPem);
                return new FileContentResult(Encoding.ASCII.GetBytes(filecontents), "text/plain")
                {
                    FileDownloadName = "ca.pem"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception in FileJwtSigner:" + ex.ToString());
                return Redirect("List");
            }
           
        }

        public IActionResult Create()
        {
            return View();
        }

		[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SignRequestForm signRequestForm)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
			var signedItem = new SignedItem();
            string? signer = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
			if(signer == null)
            {
                _logger.LogError("Error in parsing id token: identity token does not inlcude an email");
                throw new StaasException("Error in parsing id token: identity token does not inlcude am email");
            }
            try
            {
                byte[] hash = Convert.FromBase64String(signRequestForm.HashBase64);
                signedItem = await _registrySigner.SignFileHash(signer, hash, signRequestForm.Comment);
                /*
                 * Update the database
                 */
                await _secureDbAccess.AddSignedItem(signedItem);
                return RedirectToAction("List");
            }catch(Exception ex)
            {
                _logger.LogError("Exception in signing file:" + ex.ToString());
                throw new StaasException("Error in signing file");
            }
        }

        public IActionResult Details(int? id)
        {

            var entry = _secureDbAccess.GetSignedItem(id);
            if (entry == null)
            {
                _logger.LogWarning("A user tried to access Details of an authorized item");
                throw new StaasException("Cannot access the requested item");
            }
            return View(entry);
        }

        public IActionResult Download(int? id)
        {

            var entry = _secureDbAccess.GetSignedItem(id);
            if (entry == null)
            {
                _logger.LogWarning("A user tried to access Details of an authorized item");
                throw new StaasException("Cannot access the requested item");
            }

            string bundle = JsonSerializer.Serialize(entry.CosignBundle);
            return new FileContentResult(Encoding.ASCII.GetBytes(bundle), "text/plain")
            {
                FileDownloadName = "signature.bundle"
            };
        }

        public IActionResult Delete(int? id)
        {
            var entry = _secureDbAccess.GetSignedItem(id);
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
            bool result = await _secureDbAccess.DeleteSignedItem(id);
            if (!result)
            {
                _logger.LogWarning("A user tried to access Details of an authorized item");
                throw new StaasException("Cannot access the requested item");
            }
            return RedirectToAction(nameof(Index));
        }
	}
}
