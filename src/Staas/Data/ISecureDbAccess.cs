using Excid.Staas.Models;

namespace Excid.Staas.Data
{
    public interface ISecureDbAccess
    {
        public string? GetSignerFromBearer();
        public Task<bool> AddSignedItem(SignedItem entry);
        public SignedItem? GetSignedItem(int? id, UserAuthenticationMethod userAuthenticationMethod= UserAuthenticationMethod.COOKIE,AccessLevel accessLevel = AccessLevel.WRITE);
        public Task<bool> DeleteSignedItem(int? id);
        public List<SignedItem>? ListSignedItems(AccessLevel accessLevel = AccessLevel.WRITE);
        public Task<bool> AddAPIToken(APIToken entry);
        public APIToken? GetAPIToken(int? id, AccessLevel accessLevel = AccessLevel.WRITE);
        public APIToken? GetAPITokenbyValue(string value);
        public List<APIToken>? ListAPITokens(AccessLevel accessLevel = AccessLevel.WRITE);
        public Task<bool> DeleteAPIToken(int? id);
    }

    public enum AccessLevel
    {
        READ = 0,
        WRITE = 1
    }

    public enum UserAuthenticationMethod
    {
        COOKIE = 0,
        BEARER = 1
    }
}
