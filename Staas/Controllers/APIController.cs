using Excid.Staas.Data;
using Excid.Staas.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Staas.Security;
using System.Text;
using System.Text.Json;

namespace Staas.Controllers
{
    [AllowAnonymous]
    public class APIController : ControllerBase
    {
        private readonly ILogger<APIController> _logger;
        private readonly ISecureDbAccess _secureDbAccess;
        private readonly IRegistrySigner _registrySigner;

        public APIController( ILogger<APIController> logger, ISecureDbAccess secureDbAccess, IRegistrySigner registrySigner)
        {
            _logger = logger;
            _secureDbAccess = secureDbAccess;
            _registrySigner = registrySigner;

        }


        [HttpPost]
        public async Task<IActionResult> Sign([FromBody] SignRequestForm signRequestForm)
        {

            string? signer = _secureDbAccess.GetSignerFromBearer();
            if (signer == null)
            {
                return Unauthorized();
            }
            try
            {
                byte[] hash = Convert.FromBase64String(signRequestForm.HashBase64);
                var signedItem = new SignedItem();
                signedItem = await _registrySigner.SignFileHash(signer, hash, signRequestForm.Comment);
                /*
                 * Update the database
                 */
                await _secureDbAccess.AddSignedItem(signedItem);
                return CreatedAtAction(nameof(Download), new { id = signedItem.Id }, signedItem);
            }catch (Exception ex)
            {
                _logger.LogError("Exception in API/Sign:" + ex.ToString());
                return StatusCode(500);
            }


        }

        [HttpGet]
        public IActionResult Download(int? id)
        {

            var entry = _secureDbAccess.GetSignedItem(id,UserAuthenticationMethod.BEARER);
            if (entry == null)
            {
                _logger.LogWarning("A user tried to access Details of an authorized item");
                return NotFound();
            }

            string bundle = JsonSerializer.Serialize(entry.CosignBundle);
            return new FileContentResult(Encoding.ASCII.GetBytes(bundle), "text/plain")
            {
                FileDownloadName = "signature.bundle"
            };
        }
    }
}
