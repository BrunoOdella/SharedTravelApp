using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reactive.Subjects;

namespace GrpcServer.Server;

public class Notifier
{
    private ISubject<Mensaje> _subject = new ReplaySubject<Mensaje>(int.MaxValue);
    private static Notifier _instance = null;

    public static Notifier CreateInsance()
    {
        if (_instance == null)
        {
            _instance = new Notifier();
        }
        return _instance;
    }

    public IDisposable Subscribe(IObserver<Mensaje> observer)
    {
        return _subject.Subscribe(observer);
    }

    public void Publish(Mensaje message)
    {
        _subject.OnNext(message);
    }

    public void Reset()
    {
        _subject.OnCompleted();
        _subject = new ReplaySubject<Mensaje>(int.MaxValue);
    }
}

public class Mensaje
{
    public string Origin { get; set; }
    public string Destination { get; set; }
    public DateTime Departure { get; set; }
    public float PricePerPassenger { get; set; }
}
