using DataAcces;
using Server.BL;
using Server.BL.BLException;
using Server.BL.Repositories;
using System.ComponentModel.Design.Serialization;

namespace Server.DataAcces.Repositories;

public class CalificationRepository : ICalificationRepository
{
    public void Add(Calification calification)
    {
        Task<CalificationContext> contextTask = CalificationContext.GetAccessWriteCalification();
        CalificationContext context = contextTask.Result;
        context.CalificationList.Add(calification.GetGuid(), calification);
        CalificationContext.ReturnWriteAccessCalification();
    }
    private void Add(Calification calification, CalificationContext context)
    {
        context.CalificationList.Add(calification.GetGuid(), calification);
    }

    public void Delete(Calification calification)
    {
        Task<CalificationContext> contextTask = CalificationContext.GetAccessWriteCalification();
        CalificationContext context = contextTask.Result;
        Guid asociated = calification.GetGuid();
        context.CalificationList.Remove(asociated);
        CalificationContext.ReturnWriteAccessCalification();
    }

    public void Delete(Guid id)
    {
        Task<CalificationContext> contextTask = CalificationContext.GetAccessWriteCalification();
        CalificationContext context = contextTask.Result;
        context.CalificationList.Remove(id);
        CalificationContext.ReturnWriteAccessCalification();
    }

    public Calification Get(Guid id)
    {
        Calification? asociated = null;
        Task<CalificationContext> contextTask = CalificationContext.GetAccessReadCalification();
        CalificationContext context = contextTask.Result;
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
        Task<CalificationContext> contextTask = CalificationContext.GetAccessReadCalification();
        CalificationContext context = contextTask.Result;
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
        Task<CalificationContext> contextTask = CalificationContext.GetAccessWriteCalification();
        CalificationContext context = contextTask.Result;
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
        Task<CalificationContext> contextTask = CalificationContext.GetAccessReadCalification();
        CalificationContext context = contextTask.Result;
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