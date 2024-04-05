namespace Excid.Staas.Models
{
    public class StaasException: Exception
    {
        public StaasException() : base() { }
        public StaasException(string message) : base(message) { }
        public StaasException(string message, Exception inner) : base(message, inner) { }
    }
}
