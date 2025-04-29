using Excid.Sigstore.Fulcio.v2.Models;
using Excid.Sigstore.Rekor.v1.Models;
using Excid.Staas.Data;
using Excid.Staas.Models;
using Excid.Staas.Security;
using idp.Controllers;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;

namespace Staas.Security
{
    /* Signs the digest of a file and submits it to SigStore registry*/
    public class RekorRegistrySigner:IRegistrySigner
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<RekorRegistrySigner> _logger;
        private readonly IJwtSigner _jwtSigner;
        private readonly string _fulcioURL = string.Empty;


        public RekorRegistrySigner(IConfiguration configuration, ILogger<RekorRegistrySigner> logger, IJwtSigner jwtSigner)
        {
            _logger = logger;
            _configuration = configuration;
            _jwtSigner = jwtSigner;
            _fulcioURL = _configuration.GetValue<string>("Sigstore:FulcioURL") ?? string.Empty;
 
            if (_fulcioURL == string.Empty)
            {
                _logger.LogError("Error in initalizing RekorRegistrySigner: Fulcio URL was not provided in the configuration");
                throw new StaasException("Error in initializing RekorRegistrySigner, Fulcio URL was not provided in the configuration");
            }
        }

        public async Task<SignedItem> SignFileHash(string signer, byte[] hash, string comment = "")
        {
            var signedItem = new SignedItem();
            signedItem.SignedAt = DateTime.UtcNow;
            signedItem.Signer = signer;
            signedItem.Comment = comment;

            var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256); // generate asymmetric key pair
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
                var req = new CertificateRequest("cn=" + signer, ecdsa, HashAlgorithmName.SHA256);
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
            byte[]? signature;
            try
            {
                var memoryStream = new MemoryStream();
                signature = ecdsa.SignHash(hash, DSASignatureFormat.Rfc3279DerSequence);
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
                                Value = Convert.ToHexString(hash).ToLower()
                            }
                        }
                    }
                };
                var postRequest = await httpClient.PostAsJsonAsync("https://rekor.sigstore.dev" + "/api/v1/log/entries", hashedRekord);
                string responseContent = await postRequest.Content.ReadAsStringAsync();
                //_logger.LogError("Received from  Transparency Registry:" + responseContent);
                var entries = JsonSerializer.Deserialize<Dictionary<string, LogEntry>>(responseContent);
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

            return signedItem;
        }
    }
}
