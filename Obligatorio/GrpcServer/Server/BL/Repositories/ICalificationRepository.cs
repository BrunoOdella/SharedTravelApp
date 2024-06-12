namespace GrpcServer.Server.BL.Repositories
{
    public interface ICalificationRepository
    {
        void Add(Calification calification);
        void Update(Calification calification);
        void Delete(Calification calification);
        void Delete(Guid id);
        Calification Get(Guid id);
        List<Calification> GetAll();

        List<Calification> GetCalificationsByTripId(Guid tripId);
    }
}
