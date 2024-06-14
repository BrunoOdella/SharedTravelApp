using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcServer.Server;
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

        public override async Task<Empty> CreateTrip(CreateTripRequest request, ServerCallContext context)
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

            await _tripRepository.AddAsync(trip);
            return new Empty();
        }

        public override async Task<TripElem> GetNextTrips(NextTripsRequest request,
            IServerStreamWriter<TripElem> responseStream, ServerCallContext context)
        {
            LaunchServer.ReadyToReceiveTrips();

            var notifier = Notifier.CreateInsance();
            var cant = request.Quantity;

            for (int i = 0; i < cant; i++)
            {
                var mensaje = await notifier.ConsumeAsync();
                
                var trip = new TripElem
                {
                    Index = i + 1,
                    Origin = mensaje.Origin,
                    Destination = mensaje.Destination,
                    Departure = mensaje.Departure.ToString(),
                    PricePerPassenger = mensaje.PricePerPassenger
                };

                await responseStream.WriteAsync(trip);
            }

            LaunchServer.StopReceivingTrips();

            notifier.Reset();

            return new TripElem();
        }
    }
}
