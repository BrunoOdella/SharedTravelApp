using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcServer.Server.BL;

namespace GrpcServer.Services
{
    public class AdminGrpcService : AdminGrpc.AdminGrpcBase
    {
        private readonly ILogger<AdminGrpcService> _logger;

        public AdminGrpcService(ILogger<AdminGrpcService> logger)
        {
            _logger = logger;
        }

        public override Task GetNextTrips(NextTripsRequest request, IServerStreamWriter<TripElem> responseStream, ServerCallContext context)
        {
            throw new NotImplementedException();

            return Task.CompletedTask;
        }
    }
}
