namespace Server.BL.Repositories;

public interface ITripRepository
{
    void AddTrip(Trip trip);
    void RemoveTrip(Trip trip);
    void RemoveTrip(Guid id);
    Trip GetTrip(Guid id);
    void UpdateTrip(Trip trip, Guid? id);
}