﻿namespace Server.BL.Repositories;

public interface ITripRepository
{
    void Add(Trip trip);
    void Remove(Trip trip);
    void Remove(Guid id);
    Trip Get(Guid id);
    void Update(Trip trip);
    List<Trip> GetAll();
    List<Trip> GetAll(Guid userGuid);

    List<Trip> GetAllTripsToOriginAndDestinationAvailableToJoin(string origin, string destination);
    List<Trip> GetAllTripsToOriginAndDestination(string origin, string destination);
    List<Trip> GetTripsFilteredByPetFriendly(bool petFriendly);
    List<Trip> GetTripsByOwner(Guid ownerId);
    bool isJoined(Guid tripId, Guid userId);
    bool isOwner(Guid tripId, Guid userId);
    List<Trip> FilterByDeparture(List<Trip> trips);
}