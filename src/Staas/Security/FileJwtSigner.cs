using Excid.Oidc.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace Excid.Staas.Security
{
    /*
     * Signs an Id token using a key stored in a file
     * Required parameters in the configuration:
     * "PrivateKeyPem": and path to the private key file in PEM format
     * "PrivateKeyPemPassord": Password for the private key file
     */
    public class FileJwtSigner : IJwtSigner
    {
        private readonly IConfiguration _configuration;
        private readonly ECDsa _signingKey;
        private readonly ILogger<FileJwtSigner> _logger;
        private readonly JwkSet _jwkSet;

        public FileJwtSigner(ILogger<FileJwtSigner> logger, IConfiguration configuration)
        {
            _configuration = configuration;
            _logger = logger;
            _signingKey = ECDsa.Create();
            _jwkSet = new JwkSet();
            string privateKeyPem = _configuration.GetValue<string>("IdP:PrivateKeyPem") ?? "";
            string privateKeyPemPassord = _configuration.GetValue<string>("IdP:PrivateKeyPemPassord") ?? "";
            try
            {
                string pemKey = File.ReadAllText(privateKeyPem);
                _signingKey.ImportFromEncryptedPem(new ReadOnlySpan<char>(pemKey.ToCharArray()), privateKeyPemPassord);
                var publicJWK = JsonWebKeyConverter.ConvertFromECDsaSecurityKey(new ECDsaSecurityKey(_signingKey));
                publicJWK.D = null; // Remove the private key component
                _jwkSet.Keys.Add(publicJWK);

            }
            catch (Exception ex)
            {
                _logger.LogError("Exception in FileJwtSigner:" + ex.ToString());
            }

        }

        public string GetSignedJWT(JwtPayload jwtPayload)
        {

            var jwtHeader = new JwtHeader(
                new SigningCredentials(
                    key: new ECDsaSecurityKey(_signingKey),
                    algorithm: SecurityAlgorithms.EcdsaSha256)
                );
            var jwtToken = new JwtSecurityToken(jwtHeader, jwtPayload);
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            return jwtTokenHandler.WriteToken(jwtToken);
        }

        public JwkSet GetJwkSet()
        {
            return _jwkSet;
        }
    }
}
