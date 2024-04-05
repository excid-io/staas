using Excid.Staas.Models;

namespace Excid.Staas.Data
{
    public interface ISecureDbAccess
    {
        public Task<bool> AddSignedItem(SignedItem entry);
        public SignedItem? GetSignedItem(int? id, AccessLevel accessLevel = AccessLevel.WRITE);
        public Task<bool> DeleteSignedItem(int? id);
        public List<SignedItem>? ListSignedItems(AccessLevel accessLevel = AccessLevel.WRITE);
    }

    public enum AccessLevel
    {
        READ = 0,
        WRITE = 1
    }
}
