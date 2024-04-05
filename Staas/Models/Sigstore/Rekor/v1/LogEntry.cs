namespace Excid.Sigstore.Rekor.v1.Models
{

	public class LogEntry
	{
		public string LogID { get; set; } = string.Empty;
		public int LogIndex { get; set; }
		public string Body { get; set; } = string.Empty;
		public int IntegratedTime { get; set; }
		public Attestation? Attestation { get; set; } = null;
		public Verification? Verification { get; set; } = null;	
	}

	public class Attestation
	{
		public string Data { get; set; } = string.Empty;
	}

	public class Verification
	{
		public InclusionProof InclusionProof { get; set; }	= new InclusionProof();
		public string SignedEntryTimestamp { get; set; } = string.Empty;

	}

	public class InclusionProof
	{
		public int LogIndex { get; set; }
		public string RootHash { get; set; } = string.Empty;
		public int TreeSize { get; set; }
		public List<string> Hashes { get; set; } = new List<string>();
		public string CheckPoint { get; set; } = string.Empty;
	}
}
