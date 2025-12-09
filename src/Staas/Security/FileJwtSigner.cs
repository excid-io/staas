using idp.Controllers;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace Excid.Staas.Security
{
    /*
     * Signs an Id token using a key stored in a file
     */ 
    public class FileJwtSigner:IJwtSigner
    {
        private readonly IConfiguration _configuration;
        private readonly ECDsa _signingKey;
        private readonly ILogger<FileJwtSigner> _logger;

        public FileJwtSigner(ILogger<FileJwtSigner> logger, IConfiguration configuration)
        {
            _configuration = configuration;
            _logger = logger;
            _signingKey = ECDsa.Create();
            string privateKeyPem = _configuration.GetValue<string>("IdP:PrivateKeyPem") ?? "";
            string privateKeyPemPassord = _configuration.GetValue<string>("IdP:PrivateKeyPemPassord") ?? "";
            try
            {
                string pemKey = File.ReadAllText(privateKeyPem);
                _signingKey.ImportFromEncryptedPem(new ReadOnlySpan<char>(pemKey.ToCharArray()), privateKeyPemPassord);
            }catch (Exception ex)
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
    }
}
