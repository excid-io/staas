using Excid.Oidc.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace Excid.Staas.Security
{
    /*
     * Signs an Id token using an one time key
     */
    public class OneTimeKeyJwtSigner : IJwtSigner
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<OneTimeKeyJwtSigner> _logger;
        private readonly JwkSet _jwkSet;

        public OneTimeKeyJwtSigner(ILogger<OneTimeKeyJwtSigner> logger, IConfiguration configuration)
        {
            _configuration = configuration;
            _logger = logger;
            _jwkSet = new JwkSet();

        }

        public string GetSignedJWT(JwtPayload jwtPayload)
        {
            ECDsa _signingKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            var publicJWK = JsonWebKeyConverter.ConvertFromECDsaSecurityKey(new ECDsaSecurityKey(_signingKey));
            publicJWK.D = null; // Remove the private key component
            publicJWK.Kid= jwtPayload.Jti; 
            _jwkSet.Keys.Add(publicJWK);
            var jwtHeader = new JwtHeader(
                new SigningCredentials(
                    key: new ECDsaSecurityKey(_signingKey) { KeyId = jwtPayload.Jti },
                    algorithm: SecurityAlgorithms.EcdsaSha256)
                );
            var jwtToken = new JwtSecurityToken(jwtHeader, jwtPayload);
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            return jwtTokenHandler.WriteToken(jwtToken);
        }

        public void TokenUsed(string jti)
        {
            // Remove the key with the given jti from the JWK set
            _jwkSet.Keys.RemoveAll(k => k.Kid == jti);
        }

        public JwkSet GetJwkSet()
        {
            return _jwkSet;
        }
    }
}
