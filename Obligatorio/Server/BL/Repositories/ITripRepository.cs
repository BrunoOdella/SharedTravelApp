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


    List<Trip> GetAllTripsToOriginAndDestination(string origin, string destination);
}