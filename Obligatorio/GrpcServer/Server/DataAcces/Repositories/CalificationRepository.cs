using GrpcServer.Server.BL.BLException;
using GrpcServer.Server.DataAcces.Contexts;
using GrpcServer.Server.BL.Repositories;
using GrpcServer.Server.BL;

namespace GrpcServer.Server.DataAcces.Repositories
{
    public class CalificationRepository : ICalificationRepository
    {
        public async Task AddAsync(Calification calification)
        {
            CalificationContext context = await CalificationContext.GetAccessWriteCalification();
            context.CalificationList.Add(calification.GetGuid(), calification);
            CalificationContext.ReturnWriteAccessCalification();
        }
        private void Add(Calification calification, CalificationContext context)
        {
            context.CalificationList.Add(calification.GetGuid(), calification);
        }

        public async Task DeleteAsync(Calification calification)
        {
            CalificationContext context = await CalificationContext.GetAccessWriteCalification();
            Guid asociated = calification.GetGuid();
            context.CalificationList.Remove(asociated);
            CalificationContext.ReturnWriteAccessCalification();
        }

        public async Task DeleteAsync(Guid id)
        {
            CalificationContext context = await CalificationContext.GetAccessWriteCalification();
            context.CalificationList.Remove(id);
            CalificationContext.ReturnWriteAccessCalification();
        }

        public async Task<Calification> GetAsync(Guid id)
        {
            Calification? asociated = null;
            CalificationContext context = await CalificationContext.GetAccessReadCalification();
            context.CalificationList.TryGetValue(id, out asociated);
            CalificationContext.ReturnReadAccessCalification();
            if (asociated != null)
            {
                return asociated;
            }
            throw new CalificationManagerException($"Error 404, no se encuentra un Calification con el Guid {id}");
        }

        public async Task<List<Calification>> GetAllAsync()
        {
            CalificationContext context = await CalificationContext.GetAccessReadCalification();
            List<Calification> all = new List<Calification>();
            foreach (var calification in context.CalificationList)
            {
                all.Add(calification.Value);
            }
            CalificationContext.ReturnReadAccessCalification();

            return all;
        }

        public async Task UpdateAsync(Calification calification)
        {
            Guid id = calification.GetGuid();
            CalificationContext context = await CalificationContext.GetAccessWriteCalification();
            if (context.CalificationList.ContainsKey(id))
            {
                context.CalificationList[id] = calification;
            }
            else
            {
                Add(calification, context);
            }
            CalificationContext.ReturnWriteAccessCalification();
        }

        public async Task<List<Calification>> GetCalificationsByTripIdAsync(Guid tripId)
        {
            CalificationContext context = await CalificationContext.GetAccessReadCalification();
            List<Calification> califications = new List<Calification>();
            foreach (var calificationEntry in context.CalificationList)
            {
                if (calificationEntry.Value.GetTrip() == tripId)
                {
                    califications.Add(calificationEntry.Value);
                }
            }
            CalificationContext.ReturnReadAccessCalification();
            return califications;
        }

    }
}
