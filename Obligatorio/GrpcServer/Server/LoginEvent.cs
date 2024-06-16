
namespace GrpcServer.Server
{
    internal class LoginEvent
    {
        public Guid UserId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}