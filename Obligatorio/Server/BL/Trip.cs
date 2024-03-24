namespace Server.BL;

public class Trip
{
    private int _id;
    private int _owner;
    private List<int> _passengers;

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
        this._id = 0; //generar un numero acorde a algo
        this._owner = 0;//generar un numero acorde a algo
        this._passengers = new List<int>();
    }


}