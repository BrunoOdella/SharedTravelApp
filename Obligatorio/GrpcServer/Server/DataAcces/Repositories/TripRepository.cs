﻿using GrpcServer.Server.BL.BLException;
using GrpcServer.Server.DataAcces.Contexts;
using GrpcServer.Server.BL.Repositories;
using GrpcServer.Server.BL;

namespace GrpcServer.Server.DataAcces.Repositories
{
    public class TripRepository : ITripRepository
    {
        public void Add(Trip trip)
        {
            Task<TripContext> contextTask = TripContext.GetAccessWriteTrip();
            TripContext context = contextTask.Result;
            context.TripList.Add(trip.GetGuid(), trip);
            TripContext.ReturnWriteAccessTrip();
        }

        private void Add(Trip trip, TripContext context)
        {
            context.TripList.Add(trip.GetGuid(), trip);
        }

        public void Remove(Trip trip)
        {
            Task<TripContext> contextTask = TripContext.GetAccessWriteTrip();
            TripContext context = contextTask.Result;
            Guid asociated = trip.GetGuid();
            context.TripList.Remove(asociated);
            TripContext.ReturnWriteAccessTrip();
        }

        public void Remove(Guid id)
        {
            Task<TripContext> contextTask = TripContext.GetAccessWriteTrip();
            TripContext context = contextTask.Result;
            context.TripList.Remove(id);
            TripContext.ReturnWriteAccessTrip();
        }

        public Trip Get(Guid id)
        {
            Trip? asociated = null;
            Task<TripContext> contextTask = TripContext.GetAccessReadTrip();
            TripContext context = contextTask.Result;
            context.TripList.TryGetValue(id, out asociated);
            TripContext.ReturnReadAccessTrip();
            if (asociated != null)
            {
                return asociated;
            }

            throw new TripManagerException($"Error 404, no se encuentra un Trip con el Guid {id}");
        }

        public void Update(Trip trip)
        {
            Guid id = trip.GetGuid();
            Task<TripContext> contextTask = TripContext.GetAccessWriteTrip();
            TripContext context = contextTask.Result;
            if (context.TripList.ContainsKey(id))
            {
                context.TripList[id] = trip;
            }
            else
            {
                Add(trip, context);
            }

            TripContext.ReturnWriteAccessTrip();
        }

        /*
        private Guid GetGuid(Trip trip)
        {
            Guid asociated = Guid.Empty;
            TripContext context = TripContext.GetInstance();
            foreach (var elementTrip in context.TripList)
            {
                if (elementTrip.Equals(trip))
                {
                    asociated = elementTrip.Key;
                    break;
                }
            }
            TripContext.GetSemaphore().Release();
            return asociated;
        }
        */

        public List<Trip> GetAll()
        {
            Task<TripContext> contextTask = TripContext.GetAccessReadTrip();
            TripContext context = contextTask.Result;
            List<Trip> all = new List<Trip>();
            foreach (var trip in context.TripList)
            {
                all.Add(trip.Value);
            }

            TripContext.ReturnReadAccessTrip();

            return all;
        }

        public List<Trip> GetAll(Guid userGuid)
        {
            List<Trip> all = GetAll();
            List<Trip> response = new List<Trip>();
            foreach (var trip in all)
            {
                if (trip._passengers.Contains(userGuid))
                    response.Add(trip);
            }

            return response;
        }

        public List<Trip> GetAllTripsToOriginAndDestinationAvailableToJoin(string origin, string destination)
        {
            Task<TripContext> contextTask = TripContext.GetAccessReadTrip();
            TripContext context = contextTask.Result;
            List<Trip> tripsByOriginDestination = new List<Trip>();

            foreach (var trip in context.TripList)
            {
                if (trip.Value.Destination.Equals(destination, StringComparison.OrdinalIgnoreCase)
                    && trip.Value.Origin.Equals(origin, StringComparison.OrdinalIgnoreCase)
                    && trip.Value.AvailableSeats > 0
                    && trip.Value.Departure > DateTime.Now)
                {
                    tripsByOriginDestination.Add(trip.Value);
                }
            }

            if (tripsByOriginDestination.Count == 0)
            {
                throw new Exception(
                    "No hay viajes disponibles para el origen y destino especificado con asientos disponibles");
            }

            TripContext.ReturnReadAccessTrip();
            return tripsByOriginDestination;
        }

        public List<Trip> GetAllTripsToOriginAndDestination(string origin, string destination)
        {
            Task<TripContext> contextTask = TripContext.GetAccessReadTrip();
            TripContext context = contextTask.Result;
            List<Trip> tripsByOriginDestination = new List<Trip>();

            foreach (var trip in context.TripList)
            {
                if (trip.Value.Destination.Equals(destination, StringComparison.OrdinalIgnoreCase)
                    && trip.Value.Origin.Equals(origin, StringComparison.OrdinalIgnoreCase))
                {
                    tripsByOriginDestination.Add(trip.Value);
                }
            }

            if (tripsByOriginDestination.Count == 0)
            {
                throw new Exception("No hay viajes disponibles para el origen y destino especificado.");
            }

            TripContext.ReturnReadAccessTrip();
            return tripsByOriginDestination;
        }

        public List<Trip> GetTripsFilteredByPetFriendly(bool petFriendly)
        {
            Task<TripContext> contextTask = TripContext.GetAccessReadTrip();
            TripContext context = contextTask.Result;
            List<Trip> trips = new List<Trip>();

            foreach (var trip in context.TripList)
            {
                if (trip.Value.Pet.Equals(petFriendly))
                {
                    trips.Add(trip.Value);
                }
            }

            if (trips.Count == 0)
            {
                throw new Exception("No hay viajes disponibles para el filtro indicado");
            }

            TripContext.ReturnReadAccessTrip();
            return trips;
        }



        public bool isJoined(Guid tripId, Guid userId)
        {
            var trip = Get(tripId);
            if (trip == null)
            {
                throw new Exception("Trip not found.");
            }

            return trip._passengers.Contains(userId);
        }

        public bool isOwner(Guid tripId, Guid userId)
        {
            var trip = Get(tripId);
            if (trip == null)
            {
                throw new Exception("Trip not found.");
            }

            return trip.GetOwner() == userId;
        }



        public List<Trip> GetTripsByOwner(Guid ownerId)
        {
            Task<TripContext> contextTask = TripContext.GetAccessReadTrip();
            TripContext context = contextTask.Result;
            List<Trip> ownedTrips = new List<Trip>();
            foreach (var tripEntry in context.TripList)
            {
                if (tripEntry.Value.GetOwner() == ownerId)
                {
                    ownedTrips.Add(tripEntry.Value);
                }
            }

            TripContext.ReturnReadAccessTrip();
            return ownedTrips;
        }

        public List<Trip> FilterByDeparture(List<Trip> trips)
        {
            Task<TripContext> contextTask = TripContext.GetAccessReadTrip();
            TripContext context = contextTask.Result;
            List<Trip> newTripList = new List<Trip>();
            foreach (var trip in trips)
            {
                if (trip.Departure > DateTime.Now)
                {
                    newTripList.Add(trip);
                }
            }

            TripContext.ReturnReadAccessTrip();
            return newTripList;
        }

        public async Task AddAsync(Trip trip)
        {
            TripContext context = await TripContext.GetAccessWriteTrip();
            context.TripList.Add(trip.GetGuid(), trip);
            TripContext.ReturnWriteAccessTrip();
        }

        public async Task RemoveAsync(Trip trip)
        {
            TripContext context = await TripContext.GetAccessWriteTrip();
            Guid asociated = trip.GetGuid();
            context.TripList.Remove(asociated);
            TripContext.ReturnWriteAccessTrip();
        }

        public async Task RemoveAsync(Guid id)
        {
            TripContext context = await TripContext.GetAccessWriteTrip();
            context.TripList.Remove(id);
            TripContext.ReturnWriteAccessTrip();
        }

        public async Task<Trip> GetAsync(Guid id)
        {
            Trip? asociated = null;
            TripContext context = await TripContext.GetAccessReadTrip();
            context.TripList.TryGetValue(id, out asociated);
            await TripContext.ReturnReadAccessTrip();
            if (asociated != null)
            {
                return asociated;
            }

            throw new TripManagerException($"Error 404, no se encuentra un Trip con el Guid {id}");
        }

        public async Task UpdateAsync(Trip trip)
        {
            Guid id = trip.GetGuid();
            TripContext context = await TripContext.GetAccessWriteTrip();
            if (context.TripList.ContainsKey(id))
            {
                context.TripList[id] = trip;
            }
            else
            {
                Add(trip, context);
            }

            TripContext.ReturnWriteAccessTrip();
        }

        public async Task<List<Trip>> GetAllAsync()
        {
            TripContext context = await TripContext.GetAccessReadTrip();
            List<Trip> all = new List<Trip>();
            foreach (var trip in context.TripList)
            {
                all.Add(trip.Value);
            }

            await TripContext.ReturnReadAccessTrip();

            return all;
        }

        public async Task<List<Trip>> GetAllAsync(Guid userGuid)
        {
            List<Trip> all = await GetAllAsync();
            List<Trip> response = new List<Trip>();
            foreach (var trip in all)
            {
                if (trip._passengers.Contains(userGuid))
                    response.Add(trip);
            }

            return response;
        }

        public async Task<List<Trip>> GetAllTripsToOriginAndDestinationAvailableToJoinAsync(string origin,
            string destination)
        {
            TripContext context = await TripContext.GetAccessReadTrip();
            List<Trip> tripsByOriginDestination = new List<Trip>();

            foreach (var trip in context.TripList)
            {
                if (trip.Value.Destination.Equals(destination, StringComparison.OrdinalIgnoreCase)
                    && trip.Value.Origin.Equals(origin, StringComparison.OrdinalIgnoreCase)
                    && trip.Value.AvailableSeats > 0
                    && trip.Value.Departure > DateTime.Now)
                {
                    tripsByOriginDestination.Add(trip.Value);
                }
            }

            if (tripsByOriginDestination.Count == 0)
            {
                throw new Exception(
                    "No hay viajes disponibles para el origen y destino especificado con asientos disponibles");
            }

            await TripContext.ReturnReadAccessTrip();
            return tripsByOriginDestination;
        }

        public async Task<List<Trip>> GetAllTripsToOriginAndDestinationAsync(string origin, string destination)
        {
            TripContext context = await TripContext.GetAccessReadTrip();
            List<Trip> tripsByOriginDestination = new List<Trip>();

            foreach (var trip in context.TripList)
            {
                if (trip.Value.Destination.Equals(destination, StringComparison.OrdinalIgnoreCase)
                    && trip.Value.Origin.Equals(origin, StringComparison.OrdinalIgnoreCase))
                {
                    tripsByOriginDestination.Add(trip.Value);
                }
            }

            if (tripsByOriginDestination.Count == 0)
            {
                throw new Exception("No hay viajes disponibles para el origen y destino especificado.");
            }

            await TripContext.ReturnReadAccessTrip();
            return tripsByOriginDestination;
        }

        public async Task<List<Trip>> GetTripsFilteredByPetFriendlyAsync(bool petFriendly)
        {
            TripContext context = await TripContext.GetAccessReadTrip();
            List<Trip> trips = new List<Trip>();

            foreach (var trip in context.TripList)
            {
                if (trip.Value.Pet.Equals(petFriendly))
                {
                    trips.Add(trip.Value);
                }
            }

            if (trips.Count == 0)
            {
                throw new Exception("No hay viajes disponibles para el filtro indicado");
            }

            await TripContext.ReturnReadAccessTrip();
            return trips;
        }

        public async Task<bool> IsJoinedAsync(Guid tripId, Guid userId)
        {
            var trip = await GetAsync(tripId);
            if (trip == null)
            {
                throw new Exception("Trip not found.");
            }

            return trip._passengers.Contains(userId);
        }

        public async Task<bool> IsOwnerAsync(Guid tripId, Guid userId)
        {
            var trip = await GetAsync(tripId);
            if (trip == null)
            {
                throw new Exception("Trip not found.");
            }

            return trip.GetOwner() == userId;
        }

        public async Task<List<Trip>> GetTripsByOwnerAsync(Guid ownerId)
        {
            TripContext context = await TripContext.GetAccessReadTrip();
            List<Trip> ownedTrips = new List<Trip>();
            foreach (var tripEntry in context.TripList)
            {
                if (tripEntry.Value.GetOwner() == ownerId)
                {
                    ownedTrips.Add(tripEntry.Value);
                }
            }

            await TripContext.ReturnReadAccessTrip();
            return ownedTrips;
        }

        public async Task<List<Trip>> FilterByDepartureAsync(List<Trip> trips)
        {
            TripContext context = await TripContext.GetAccessReadTrip();
            List<Trip> newTripList = new List<Trip>();
            foreach (var trip in trips)
            {
                if (trip.Departure > DateTime.Now)
                {
                    newTripList.Add(trip);
                }
            }

            await TripContext.ReturnReadAccessTrip();
            return newTripList;
        }
    }
}
