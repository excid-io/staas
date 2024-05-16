using Excid.Staas.Models;
using idp.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Staas.Security;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

namespace Excid.Staas.Data
{
    public class SecureDbAccess : ISecureDbAccess
    {
        private readonly StassDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<SecureDbAccess> _logger;

        public string? Signer
        {
            get
            {
                return _httpContextAccessor.HttpContext?.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            }
        }

        public string? SignerFromBearer
        {
            get
            {
                if(_httpContextAccessor.HttpContext == null)
                {
                    _logger.LogInformation("HttpContext is null");
                    return null;
                }
                if (!_httpContextAccessor.HttpContext.Request.Headers.ContainsKey("Authorization"))
                {
                    _logger.LogInformation("Headers do not contain Authorization");
                    return null;
                }
                try
                {
                    StringValues authorizationHeader;
                    _httpContextAccessor.HttpContext.Request.Headers.TryGetValue("Authorization", out authorizationHeader);
                    var header = AuthenticationHeaderValue.Parse(authorizationHeader.ToString());
                    var token = header.Parameter;
                    if (token == null)
                    {
                        return null;
                    }
                    var APIToken = _context.APITokens.Where(m => m.Token == token).FirstOrDefault();
                    if (APIToken == null)
                    {
                        return null;
                    }
                    return APIToken.Signer;

                }
                catch (Exception ex)
                {
                    _logger.LogError("Exception in SignerFromBearer " + ex.ToString());
                    return null;
                }
            }
        }

        public string? GetSignerFromBearer()
        {
            return SignerFromBearer;
        }

        public SecureDbAccess(ILogger<SecureDbAccess> logger, StassDbContext context, IHttpContextAccessor httpContextAccessor) 
        { 
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
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
        public SignedItem? GetSignedItem(int? id, UserAuthenticationMethod userAuthenticationMethod, AccessLevel accessLevel)
        {
            if (id is null) return null;
            string? signer = null;
            if (userAuthenticationMethod == UserAuthenticationMethod.COOKIE)
            {
                signer = Signer;
            }else if(userAuthenticationMethod == UserAuthenticationMethod.BEARER)
            {
                signer = SignerFromBearer;
            }
            if (signer is null)
            {
                return null;
            }

            var entry = _context.SignedItems.Where(m => m.Id == id && m.Signer == signer).FirstOrDefault();
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
         * Get an API token without any access control
         */
        public APIToken? GetAPITokenbyValue(string value)
        {
            var entry = _context.APITokens.Where(m => m.Token == value).FirstOrDefault();
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
