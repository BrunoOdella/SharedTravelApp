using Grpc.Core;
using GrpcServer.Server.BL.Repositories;
using GrpcServer.Server.DataAcces.Repositories;
using GrpcServer.Server.BL;

namespace GrpcServer.Services
{
    public class AdminGrpcService : AdminGrpc.AdminGrpcBase
    {
        private readonly ITripRepository _tripRepository;
        public AdminGrpcService()
        {
            _tripRepository = new TripRepository(); // Asegúrate de que esto esté usando el contexto compartido.
        }

        public override Task<Empty> CreateTrip(CreateTripRequest request, ServerCallContext context)
        {
            Trip trip = new Trip();
            trip._owner = Guid.Parse(request.OwnerId);
            trip.Origin = request.Origin;
            trip.Destination = request.Destination;
            trip.Departure = DateTime.Parse(request.Departure);
            trip.AvailableSeats = request.AvailableSeats;
            trip.TotalSeats = request.TotalSeats;
            trip.PricePerPassanger = request.PricePerPassenger;
            trip.Pet = request.PetsAllowed;

            _tripRepository.Add(trip);
            return Task.FromResult(new Empty());
        }

    }
}
