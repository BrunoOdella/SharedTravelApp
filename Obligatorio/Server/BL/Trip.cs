namespace Server.BL;

public class Trip
{
    public Guid _id;
    public Guid _owner;
    public List<Guid> _passengers;

    public string Origin { get; set; }
    public string Destination { get; set; }
    public DateTime Departure { get; set; }
    public int AvailableSeats { get; set; } = 0;
    public int TotalSeats { get; set; } = 0;
    public float PricePerPassanger { get; set; } = 0f;
    public bool Pet { get; set; }
    public string Photo { get; set; }

    public Trip()
    {
        this._id = Guid.NewGuid(); //generar un numero acorde a algo
        this._owner = Guid.NewGuid();//generar un numero acorde a algo
        this._passengers = new List<Guid>();
    }

    public override bool Equals(object? obj)
    {
        if (obj is Trip)
        {
            var trip = (Trip)obj;

            return trip._id == _id;
        }
        return base.Equals(obj);
    }

    //Es una buena práctica también sobrescribir GetHashCode cuando se sobrescribe Equals
    public override int GetHashCode()
    {
        return _id.GetHashCode();
    }

    public Guid GetGuid()
    {
        return _id;
    }

    public void SetGuid(Guid id)
    {
        _id = id;
    }

    public void SetOwner(Guid owner)
    {
        _owner = owner;
    }

    public void SetPassangers(List<Guid> passangers)
    {
        _passengers = passangers;
    }
}