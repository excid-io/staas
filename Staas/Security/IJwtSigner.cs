using System.IdentityModel.Tokens.Jwt;

namespace Excid.Staas.Security
{
    public interface IJwtSigner
    {
        string GetSignedJWT(JwtPayload jwtPayload);
    }
}
