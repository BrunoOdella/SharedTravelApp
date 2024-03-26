namespace Server.BL.Repositories;

public interface ITripRepository
{
    void AddTrip(Trip trip);
    void RemoveTrip(Trip trip);
    void RemoveTrip(int id);
    Trip GetTrip(int id);
    void UpdateTrip(Trip trip);
}