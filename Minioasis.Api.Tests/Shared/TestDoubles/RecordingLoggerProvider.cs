using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Minioasis.Api.Tests.Shared.TestDoubles;

internal sealed class RecordingLoggerProvider : ILoggerProvider
{
    public ConcurrentQueue<(string Message, Exception? Exception)> Entries { get; } = new();

    public ILogger CreateLogger(string categoryName) => new RecordingLogger(this);

    public void Dispose()
    {
    }

    private sealed class RecordingLogger(RecordingLoggerProvider owner) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter) => owner.Entries.Enqueue((formatter(state, exception), exception));
    }
}
