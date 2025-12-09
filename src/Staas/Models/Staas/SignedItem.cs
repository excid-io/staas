using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Text.Json;
using Excid.Sigstore.Cosign.v1.Models;
using Excid.Sigstore.Rekor.v1.Models;

namespace Excid.Staas.Models
{
    public class SignedItem
    {
        [Key]
        public int Id { get; set; }
        public string Comment { get; set; } = string.Empty;
        public string CAKey { get; set; } = string.Empty;
        public string Certificate { get; set; } = string.Empty;
        public string Signature { get; set; } = string.Empty;
        public string Signer { get; set; } = string.Empty;
        public DateTime SignedAt { get; set; } = DateTime.UtcNow;
        public string RekorLogEntryUUID { get; set; } = string.Empty;
        public string RekorLogEntry { get; set; } = string.Empty;

        [NotMapped]
        public Bundle? CosignBundle { get
            {
                var rekorLogEntry = JsonSerializer.Deserialize<LogEntry>(RekorLogEntry);
                if (rekorLogEntry is null)
                {
                    return null;
                }
                if (rekorLogEntry.Verification is null)
                {
                    return null;
                }
                var cosignBundle = new Bundle()
                {
                    Base64Signature = Signature,
                    Cert = Convert.ToBase64String(Encoding.ASCII.GetBytes(Certificate)),
                    RekorBundle = new RekorBundle()
                    {
                        SignedEntryTimestamp = rekorLogEntry.Verification.SignedEntryTimestamp,
                        Payload = new Payload()
                        {
                            Body = rekorLogEntry.Body,
                            IntegratedTime = rekorLogEntry.IntegratedTime,
                            LogId = rekorLogEntry.LogID,
                            LogIndex = rekorLogEntry.LogIndex,
                        }

                    }
                };
                return cosignBundle;
            } 
        }
        
    }
}
