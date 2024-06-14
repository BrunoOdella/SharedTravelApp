using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcServer.Server;
using GrpcServer.Server.BL.Repositories;
using GrpcServer.Server.DataAcces.Repositories;
using GrpcServer.Server.BL;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;

namespace GrpcServer.Services
{
    public class AdminGrpcService : AdminGrpc.AdminGrpcBase
    {
        private readonly ITripRepository _tripRepository;
        private readonly IUserRepository _userRepository;

        public AdminGrpcService()
        {
            _tripRepository = new TripRepository();
            _userRepository = new UserRepository();
        }

        public override async Task<Empty> CreateTrip(CreateTripRequest request, ServerCallContext context)
        {
            Trip trip = new Trip();
            User user = await _userRepository.GetUserByUsernameAsync(request.OwnerId);

            if (user == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "User not found"));
            }

            trip._owner = user.GetGuid();
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

        public override async Task<GetTripsResponse> GetAllTrips(Empty request, ServerCallContext context)
        {
            GetTripsResponse response = new GetTripsResponse();
            List<Trip> trips = await _tripRepository.GetAllAsync();
            int count= 0;
            foreach (Trip trip in trips)
            {
                response.Trips.Add(new TripElem
                {
                    Index = count++,
                    Origin = trip.Origin,
                    Destination = trip.Destination,
                    Departure = trip.Departure.ToString(),
                    PricePerPassenger = trip.PricePerPassanger,
                    PetsAllowed = trip.Pet
                });
            }
            return response;
        }

        public override async Task<Empty> UpdateTrip(UpdateTripRequest request, ServerCallContext context)
        {
            List<Trip> trips = await _tripRepository.GetAllAsync();
            trips[request.Index].Origin = request.Origin;
            trips[request.Index].Destination = request.Destination;
            trips[request.Index].Departure = DateTime.Parse(request.Departure);
            trips[request.Index].PricePerPassanger = request.PricePerPassenger;
            trips[request.Index].Pet = request.PetsAllowed;
            _tripRepository.UpdateAsync(trips[request.Index]);
            return new Empty();
        }

        public override async Task<Empty> DeleteTrip(TripIndex request, ServerCallContext context)
        {
            List<Trip> trips = await _tripRepository.GetAllAsync();
            await _tripRepository.RemoveAsync(trips[request.Index]);
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
