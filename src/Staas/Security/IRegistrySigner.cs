using Excid.Staas.Models;

namespace Staas.Security
{
    public interface IRegistrySigner
    {
        Task<SignedItem> SignFileHash(string signer, byte[] hash, string comment);
    }
}
