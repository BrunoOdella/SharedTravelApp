using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcServer.Server;
using GrpcServer.Server.BL.Repositories;
using GrpcServer.Server.DataAcces.Repositories;
using GrpcServer.Server.BL;
using System.Collections.Generic;
using System.Reactive;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Channels;

namespace GrpcServer.Services
{
    public class AdminGrpcService : AdminGrpc.AdminGrpcBase
    {
        private readonly ITripRepository _tripRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICalificationRepository _calificationRepository;

        public AdminGrpcService()
        {
            _tripRepository = new TripRepository();
            _userRepository = new UserRepository();
            _calificationRepository = new CalificationRepository();
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
            int count = 0;
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

            var channel = Channel.CreateUnbounded<Mensaje>();

            var subscription = notifier.Subscribe(Observer.Create<Mensaje>(
                onNext: async message => {
                    await channel.Writer.WriteAsync(message);
                },
                onError: error => channel.Writer.Complete(error),
                onCompleted: () => channel.Writer.Complete()
            ));

            int index = 1;

            while (await channel.Reader.WaitToReadAsync(context.CancellationToken) && cant > 0)
            {
                while (channel.Reader.TryRead(out var mensaje) && cant > 0)
                {
                    var trip = new TripElem
                    {
                        Index = index,
                        Origin = mensaje.Origin,
                        Destination = mensaje.Destination,
                        Departure = mensaje.Departure.ToString(),
                        PricePerPassenger = mensaje.PricePerPassenger
                    };

                    await responseStream.WriteAsync(trip);

                    Console.WriteLine("enviado");

                    cant--;
                    index++;
                }
            }

            LaunchServer.StopReceivingTrips();

            notifier.Reset();

            return new TripElem();
        }

        public override async Task<CalificationResponse> GetTripCalifications(TripIndex request, ServerCallContext context)
        {
            List<Trip> trips = await _tripRepository.GetAllAsync();
            List<Calification> califications = await _calificationRepository.GetCalificationsByTripIdAsync(trips[request.Index]._id);
            CalificationResponse response = new CalificationResponse();
            foreach (Calification calification in califications)
            {
                User user = await _userRepository.GetAsync(calification.GetUserGuid());
                response.Ratings.Add(new CalificationElem
                {
                    Username = user.Name,
                    Score = calification.Score,
                    Comment = calification.Comment
                });
            }
            return response;
        }

        public override async Task<GetUsersResponse> GetAllUsers(Empty request, ServerCallContext context)
        {
            GetUsersResponse response = new GetUsersResponse();
            List<User> users = await _userRepository.GetAllAsync();
            foreach (User user in users)
            {
                response.Users.Add(new UserElem
                {
                    Username = user.Name
                });
            }
            return response;
        }
    }
}
