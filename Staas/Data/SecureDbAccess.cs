using Excid.Staas.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Excid.Staas.Data
{
    public class SecureDbAccess : ISecureDbAccess
    {
        private readonly StassDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public string? Signer
        {
            get
            {
                return _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            }
        }

        public SecureDbAccess(StassDbContext context, IHttpContextAccessor httpContextAccessor) 
        { 
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        /*
         * Adds a signed item
         */
        public async Task<bool> AddSignedItem(SignedItem entry)
        {
            _context.Add(entry);
            await _context.SaveChangesAsync(); ;
            return true;
        }

        /*
         * Adds an API token
         */
        public async Task<bool> AddAPIToken(APIToken entry)
        {
            _context.Add(entry);
            await _context.SaveChangesAsync(); ;
            return true;
        }

        /*
         * Get a signed item if it belongs to the user
         */
        public SignedItem? GetSignedItem(int? id, AccessLevel accessLevel)
        {
            if (id is null) return null;
            if (Signer is null)
            {
                return null;
            }

            var entry = _context.SignedItems.Where(m => m.Id == id && m.Signer == Signer).FirstOrDefault();
            return entry;
        }

        /*
         * Get an API token if it belongs to the user
         */
        public APIToken? GetAPIToken(int? id, AccessLevel accessLevel)
        {
            if (id is null) return null;
            if (Signer is null)
            {
                return null;
            }

            var entry = _context.APITokens.Where(m => m.Id == id && m.Signer == Signer).FirstOrDefault();
            return entry;
        }

        /*
         * Get a list of signed items that belong to the user
         */
        public List<SignedItem>? ListSignedItems(AccessLevel accessLevel)
        {
            if (Signer is null)
            {
                return null;
            }

            var entry = _context.SignedItems.Where(m => m.Signer == Signer).OrderByDescending(q => q.Id).ToList();
            return entry;
        }

        /*
         * Get a list of APITokens that belong to the user
         */
        public List<APIToken>? ListAPITokens(AccessLevel accessLevel)
        {
            if (Signer is null)
            {
                return null;
            }

            var entry = _context.APITokens.Where(m => m.Signer == Signer).OrderByDescending(q => q.Id).ToList();
            return entry;
        }

        /*
         * Deletes a signed item if it belongs to the user
         */
        public async Task<bool> DeleteSignedItem(int? id)
        {
            if (id is null)
            {
                return false;
            }
            var entry = _context.SignedItems.Where(m => m.Id == id).FirstOrDefault();
            if (entry is null)
            {
                return false;
            }
            _context.SignedItems.Remove(entry);
            await _context.SaveChangesAsync();
            return true;
        }

        /*
         * Deletes an API token if it belongs to the user
         */
        public async Task<bool> DeleteAPIToken(int? id)
        {
            if (id is null)
            {
                return false;
            }
            var entry = _context.APITokens.Where(m => m.Id == id).FirstOrDefault();
            if (entry is null)
            {
                return false;
            }
            _context.APITokens.Remove(entry);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
