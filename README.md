# Class: ByteProgress

## Definition
```c#
public sealed class ByteProgress : IProgress<int>, IDisposable
```
Namespace: ByteProgress
Implements: IProgress<int>, IDisposable

## Examples
```c#
using ByteProgress
using System.Buffers;

namespace ByteProgressExample;

internal class Example
{
    public static async Task DoSomeCopyWork(Stream source, Stream destination)
    {
        using ByteProgress progress = new();
        using IDisposable totalSubscribe = progress.GetTotalObservable(TimeSpan.FromSeconds(1)).Subscribe(total => Console.WriteLine($"Copied: {total}Bytes"));
        using IDisposable speedSubscribe = progress.GetSpeedObservable(TimeSpan.FromSeconds(1)).Subscribe(speed => Console.WriteLine($"Current speed: {speed}Bytes/second"));
        await CopyDataAsync(source, destination, progress).ConfigureAwait(false);
    }

    public static async Task CopyDataAsync(Stream source, Stream destination, IProgress<int> progress)
    {
        using IMemoryOwner<byte> memoryOwner = MemoryPool<byte>.Shared.Rent(1024);
        Memory<byte> buffer = memoryOwner.Memory;
        while (true)
        {
            int bytesRead = await source.ReadAsync(buffer).ConfigureAwait(false);
            progress.Report(bytesRead);
            if (bytesRead == 0) break;
            await destination.WriteAsync(buffer).ConfigureAwait(false);
        }
    }
}

```

## Properties
```c#
IObservable<long> RawTotalObservable; // Get a raw observable sequence of total read bytes. This sequence is not sampled.

IObservable<int> RawReportObservable; // Get a raw observable sequence of byte reading report. This sequence is not sampled.

```

## Methods
```c#
IObservable<long> GetTotalObservable(TimeSpan interval);

IObservable<long> GetSpeedObservable(TimeSpan interval);

void Dispose();

void Report(int value);

```
