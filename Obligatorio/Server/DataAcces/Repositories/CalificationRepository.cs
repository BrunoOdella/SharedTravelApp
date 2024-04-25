using DataAcces;
using Server.BL;
using Server.BL.BLException;
using Server.BL.Repositories;

namespace Server.DataAcces.Repositories;

public class CalificationRepository : ICalificationRepository
{
    public void Add(Calification calification)
    {
        CalificationContext context = CalificationContext.GetAccessWriteCalification();
        context.CalificationList.Add(calification.GetGuid(), calification);
        CalificationContext.ReturnWriteAccessCalification();
    }
    private void Add(Calification calification, CalificationContext context)
    {
        context.CalificationList.Add(calification.GetGuid(), calification);
    }

    public void Delete(Calification calification)
    {
        CalificationContext context = CalificationContext.GetAccessWriteCalification();
        Guid asociated = calification.GetGuid();
        context.CalificationList.Remove(asociated);
        CalificationContext.ReturnWriteAccessCalification();
    }

    public void Delete(Guid id)
    {
        CalificationContext context = CalificationContext.GetAccessWriteCalification();
        context.CalificationList.Remove(id);
        CalificationContext.ReturnWriteAccessCalification();
    }

    public Calification Get(Guid id)
    {
        Calification? asociated = null;
        CalificationContext context = CalificationContext.GetAccessReadCalification();
        context.CalificationList.TryGetValue(id, out asociated);
        CalificationContext.ReturnReadAccessCalification();
        if (asociated != null)
        {
            return asociated;
        }
        throw new CalificationManagerException($"Error 404, no se encuentra un Calification con el Guid {id}");
    }

    public List<Calification> GetAll()
    {
        CalificationContext context = CalificationContext.GetAccessReadCalification();
        List<Calification> all = new List<Calification>();
        foreach (var calification in context.CalificationList)
        {
            all.Add(calification.Value);
        }
        CalificationContext.ReturnReadAccessCalification();

        return all;
    }

    public void Update(Calification calification)
    {
        Guid id = calification.GetGuid();
        CalificationContext context = CalificationContext.GetAccessWriteCalification();
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

    public List<Calification> GetCalificationsByTripId(Guid tripId)
    {
        CalificationContext context = CalificationContext.GetAccessReadCalification();
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