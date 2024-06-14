using System.Threading.Channels;

namespace GrpcServer.Server;

public class Notifier
{
    private Channel<Mensaje> _channel = Channel.CreateUnbounded<Mensaje>();
    private static Notifier _instance = null;

    public static Notifier CreateInsance()
    {
        if(_instance == null)
        {
            _instance = new Notifier();
        }
        return _instance;
    }

    public async Task<Mensaje> ConsumeAsync()
    {
        return await _channel.Reader.ReadAsync();
    }

    public async Task ProduceAsync(Mensaje message)
    {
        await _channel.Writer.WriteAsync(message);
    }

    public void Reset()
    {
        _channel.Writer.Complete();

        _channel = Channel.CreateUnbounded<Mensaje>();
    }
}

public class Mensaje
{
    public string Origin { get; set; }
    public string Destination { get; set; }
    public DateTime Departure { get; set; }
    public float PricePerPassenger { get; set; }
}