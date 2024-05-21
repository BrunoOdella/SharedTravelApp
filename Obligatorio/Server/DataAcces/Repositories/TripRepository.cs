using DataAcces;
using Server.BL;
using Server.BL.Repositories;
using Server.BL.BLException;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;

namespace Server.DataAcces.Repositories
{
    public class TripRepository : ITripRepository
    {
        public void Add(Trip trip)
        {
            Task<TripContext> contextTask = TripContext.GetAccessWriteTrip();
            TripContext context = contextTask.Result;
            context.TripList.Add(trip.GetGuid(),trip);
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
            foreach(var trip in context.TripList)
            {
                all.Add(trip.Value);
            }
            TripContext.ReturnReadAccessTrip();

            return all;
        }

        public List<Trip> GetAll(Guid userGuid)
        {
            List<Trip> all = GetAll();
            List < Trip > response = new List<Trip>();
            foreach (var trip in all)
            {
                if(trip._passengers.Contains(userGuid))
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
                    && trip.Value.AvailableSeats >0 
                    && trip.Value.Departure > DateTime.Now)
                {
                    tripsByOriginDestination.Add(trip.Value);
                }
            }

            if (tripsByOriginDestination.Count == 0)
            {
                throw new Exception("No hay viajes disponibles para el origen y destino especificado con asientos disponibles");
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
            List <Trip> newTripList = new List<Trip>();
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

    }
}
