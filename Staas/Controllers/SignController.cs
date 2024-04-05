
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

namespace idp.Controllers
{
	public class SignController : Controller
	{

		private readonly ILogger<SignController> _logger;
		private readonly IConfiguration _configuration;
        private readonly ISecureDbAccess _secureDbAccess;
        private readonly IJwtSigner _jwtSigner;
        private readonly string _fulcioURL = string.Empty;
        private readonly string _issuer = string.Empty;

        public SignController(IConfiguration configuration, ILogger<SignController> logger, StassDbContext context, ISecureDbAccess secureDbAccess, IJwtSigner jwtSigner)
		{
			_logger = logger;
			_configuration = configuration;
            _secureDbAccess = secureDbAccess;
            _jwtSigner = jwtSigner;
            _fulcioURL = _configuration.GetValue<string>("Sigstore:FulcioURL") ?? string.Empty;
            _issuer = _configuration.GetValue<string>("IdP:iss") ?? string.Empty;
            if (_fulcioURL == string.Empty)
            {
                _logger.LogError("Error in initalizing SignController: Fulcio URL was not provided in the configuration");
                throw new StaasException("Error in initializing, Fulcio URL was not provided in the configuration");
            }
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
            
            signedItem.SignedAt = DateTime.UtcNow;
			signedItem.Signer = signer;
			signedItem.Comment = signRequestForm.Comment ?? string.Empty;
            var ecdsa = ECDsa.Create(); // generate asymmetric key pair
            var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(2);
            /*
			 * Prepare OIDC token
			 */
            var iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var exp = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds();
            var iss = _configuration.GetValue<string>("IdP:iss");
            var jwtpayload = new JwtPayload()
                {
                    { "iat", iat },
                    { "exp", exp },
                    { "iss", iss ?? ""},
                    {"aud", "sigstore" },
                    {"email_verified", true },
                    {"email", signer }

            };

            var token = _jwtSigner.GetSignedJWT(jwtpayload);
            /*
			 * Request certificate
			 */
            string certificate;
            try
			{
                var req = new CertificateRequest("cn="+ signer, ecdsa, HashAlgorithmName.SHA256);
                var csr = req.CreateSigningRequest();
                string pem = new string(PemEncoding.Write("CERTIFICATE REQUEST", csr));
                var signingCertificateRequest = new SigningCertificateRequest()
                {
                    Credentials = new Credentials()
                    {
                        OidcIdentityToken = token
                    },
                    CertificateSigningRequest = Convert.ToBase64String(Encoding.ASCII.GetBytes(pem))
                };
                /*
                 * Invoke Fulcio
                 */
                var postRequest = await httpClient.PostAsJsonAsync(_fulcioURL + "/api/v2/signingCert", signingCertificateRequest);
                var signingCertificateResponse = await postRequest.Content.ReadFromJsonAsync<SigningCertificateResponse>();
                certificate = signingCertificateResponse!.SignedCertificateDetachedSct.Chain.Certificates[0];
				var caKey = signingCertificateResponse!.SignedCertificateDetachedSct.Chain.Certificates[1];
                signedItem.Certificate = certificate;
				signedItem.CAKey = caKey;
            }
			catch (Exception ex)
			{
				_logger.LogError("Exception in requesting certificate:" + ex.ToString());
				throw new StaasException("Error in requesting certificate");
			}
			/*
             * Sign file
             */
			byte[]? data;
            byte[]? signature;
            try
			{
                var memoryStream = new MemoryStream();
                await signRequestForm.Data.CopyToAsync(memoryStream);
                data = memoryStream.ToArray();
                signature = ecdsa.SignData(data, HashAlgorithmName.SHA256,
                DSASignatureFormat.Rfc3279DerSequence);
				signedItem.Signature = Convert.ToBase64String(signature);
            }
            catch (Exception ex)
			{
                _logger.LogError("Exception in generating signature:" + ex.ToString());
                throw new StaasException("Error in generating singature");
            }
            /*
            * Store in Rekor
            */
            try
            {
                SHA256 sha256 = SHA256.Create();
                byte[] digest = sha256.ComputeHash(data);
                var hashedRekord = new HashedRekord()
                {
                    Spec = new Spec()
                    {
                        Signature = new Signature()
                        {
                            Content = Convert.ToBase64String(signature),
                            PublicKey = new Excid.Sigstore.Rekor.v1.Models.PublicKey()
                            {
                                Content = Convert.ToBase64String((Encoding.ASCII.GetBytes(certificate)))
                            }
                        },
                        Data = new Data()
                        {
                            Hash = new Hash()
                            {
                                Algorithm = "sha256",
                                Value = Convert.ToHexString(digest).ToLower()
                            }
                        }
                    }
                };
                var postRequest = await httpClient.PostAsJsonAsync("https://rekor.sigstore.dev" + "/api/v1/log/entries", hashedRekord);
                var entries = await postRequest.Content.ReadFromJsonAsync<Dictionary<string, LogEntry>>();
				//assume a single entry
				if (entries != null && entries.Count > 0)
				{
					var entry = entries.First();
					signedItem.RekorLogEntryUUID = entry.Key;
					signedItem.RekorLogEntry = JsonSerializer.Serialize(entry.Value);
				}
            }
            catch (Exception ex)
            {
                _logger.LogError("Exception in storing to the Transparency Registry:" + ex.ToString());
                throw new StaasException("Error in storing to the Transparency Registry");
            }
            /*
			 * Update the database
			 */
            await _secureDbAccess.AddSignedItem(signedItem);
            return RedirectToAction("List");
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
