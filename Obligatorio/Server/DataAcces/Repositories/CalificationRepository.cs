using DataAcces;
using Server.BL;
using Server.BL.BLException;
using Server.BL.Repositories;

namespace Server.DataAcces.Repositories;

public class CalificationRepository : ICalificationRepository
{
    public void Add(Calification calification)
    {
        Context context = Context.GetAccessWriteCalification();
        context.CalificationList.Add(calification.GetGuid(), calification);
        Context.ReturnWriteAccessCalification();
    }
    private void Add(Calification calification, Context context)
    {
        context.CalificationList.Add(calification.GetGuid(), calification);
    }

    public void Delete(Calification calification)
    {
        Context context = Context.GetAccessWriteCalification();
        Guid asociated = calification.GetGuid();
        context.CalificationList.Remove(asociated);
        Context.ReturnWriteAccessTrip();
    }

    public void Delete(Guid id)
    {
        Context context = Context.GetAccessWriteCalification();
        context.CalificationList.Remove(id);
        Context.ReturnWriteAccessCalification();
    }

    public Calification Get(Guid id)
    {
        Calification? asociated = null;
        Context context = Context.GetAccessReadCalification();
        context.CalificationList.TryGetValue(id, out asociated);
        Context.ReturnReadAccessCalification();
        if (asociated != null)
        {
            return asociated;
        }
        throw new CalificationManagerException($"Error 404, no se encuentra un Calification con el Guid {id}");
    }

    public List<Calification> GetAll()
    {
        Context context = Context.GetAccessReadCalification();
        List<Calification> all = new List<Calification>();
        foreach (var calification in context.CalificationList)
        {
            all.Add(calification.Value);
        }
        Context.ReturnReadAccessCalification();

        return all;
    }

    public void Update(Calification calification)
    {
        Guid id = calification.GetGuid();
        Context context = Context.GetAccessWriteCalification();
        if (context.CalificationList.ContainsKey(id))
        {
            context.CalificationList[id] = calification;
        }
        else
        {
            Add(calification, context);
        }
        Context.ReturnWriteAccessCalification();
    }

    
}