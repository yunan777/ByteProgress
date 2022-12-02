using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ByteProgress;

public sealed class ByteProgress : IProgress<int>, IDisposable
{
    private bool _disposed = false;
    private readonly IConnectableObservable<long> _rawTotalObservable;
    private readonly IDisposable _totalObservableConnection;
    private readonly IObservable<int> _rawReportObservable;

    private event Action<int>? ProgressReported;

    /// <summary>
    /// Get a raw observable sequence of total read bytes. This sequence is not sampled.
    /// </summary>
    public IObservable<long> RawTotalObservable => _rawTotalObservable;

    /// <summary>
    /// Get a raw observable sequence of byte reading report. This sequence is not sampled.
    /// </summary>
    public IObservable<int> RawReportObservable => _rawReportObservable;

    /// <summary>
    /// Create an IProgress object for reporting read bytes.
    /// </summary>
    public ByteProgress()
    {
        _rawReportObservable = Observable.FromEvent<int>(handler => ProgressReported += handler,
                                                         handler => ProgressReported -= handler);
        _rawTotalObservable = _rawReportObservable.Scan(0L, (total, current) => total + current)
                                                  .Publish();
        _totalObservableConnection = _rawTotalObservable.Connect();
    }

    /// <summary>
    /// Get an observable sequence of total bytes read.
    /// </summary>
    /// <param name="interval">Sequence update interval.</param>
    /// <returns>Observable object</returns>
    public IObservable<long> GetTotalObservable(TimeSpan interval)
    {
        return _rawTotalObservable.Sample(interval);
    }

    /// <summary>
    /// Get an observable sequence of byte reading speed.
    /// </summary>
    /// <param name="interval">Speed update interval.</param>
    /// <returns>Observable object</returns>
    public IObservable<long> GetSpeedObservable(TimeSpan interval)
    {
        long ms = (long)interval.TotalMilliseconds;
        long GetSpeed(IList<int> buffer) => (buffer.Sum(value => (long)value) / ms) * 1000;
        return _rawReportObservable.Buffer(interval).Select(GetSpeed);
    }

    /// <summary>
    /// Dispose the connection of the observable total bytes.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _totalObservableConnection.Dispose();
        _disposed = true;
    }

    /// <summary>
    /// Report how many bytes has been read once.
    /// </summary>
    /// <param name="value">Amount of bytes read.</param>
    public void Report(int value)
    {
        ProgressReported?.Invoke(value);
    }

}
