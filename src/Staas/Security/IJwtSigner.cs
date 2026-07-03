using Excid.Oidc.Models;
using System.IdentityModel.Tokens.Jwt;

namespace Excid.Staas.Security
{
    public interface IJwtSigner
    {
        string GetSignedJWT(JwtPayload jwtPayload);
        JwkSet GetJwkSet();
        void TokenUsed(string jti) { }// Optional method to mark a token as used, can be implemented if needed
    }

}
