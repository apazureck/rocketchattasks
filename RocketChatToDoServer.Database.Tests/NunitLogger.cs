using System;
using Microsoft.Extensions.Logging;

namespace NUnit.Framework
{
    public static class ILoggerExtensions {
        public static ILoggerFactory AddNunit(this ILoggerFactory factory)
        {
            factory.AddProvider(new TestLoggerProvider());
            return factory;
        }
    }
    public class TestLoggerProvider : ILoggerProvider
    {

        public static ILogger Create<T>(string categoryName = "")
        {
            var logger = new NUnitLogger<T>(categoryName);
            return logger;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return Create<object>(categoryName);
        }

        public void Dispose()
        {
            
        }

        public class NUnitLogger<T> : ILogger<T>, IDisposable
        {
            public NUnitLogger(string categoryName = "")
            {
                this.categoryName = categoryName;
            }

            private readonly Action<string> output = Console.WriteLine;
            private string categoryName;

            public void Dispose()
            {
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
                Func<TState, Exception, string> formatter) => output($"{logLevel}: {categoryName}{(eventId != null ? "[" + eventId.Id + "]" : "")}: {formatter(state, exception)}");

            public bool IsEnabled(LogLevel logLevel) => true;

            public IDisposable BeginScope<TState>(TState state) => this;
        }
    }
}
