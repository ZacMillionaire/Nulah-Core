﻿using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NulahCore.Models;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NulahCore.Extensions.Logging {
    public partial class ScreamingLogger : ILogger {

        private readonly IDatabase _redis;
        private readonly AppSetting _settings;
        private readonly string KEY_logKey;

        public ScreamingLogger(IDatabase Redis, AppSetting Settings) {
            _redis = Redis;
            _settings = Settings;
            KEY_logKey = _settings.Redis.BaseKey + "Logs";
        }

        public IDisposable BeginScope<TState>(TState state) {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel) {
            return logLevel >= _settings.LogLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) {
            if(eventId.Id < 1000) {
                var Event = new ScreamingEvent<string> {
                    EventId = eventId.Id,
                    Event = state.ToString()
                };
                _redis.ListLeftPush(KEY_logKey, JsonConvert.SerializeObject(Event));
            } else {
                _redis.ListLeftPush(KEY_logKey, state.ToString());
            }
        }

    }

    public static class ScreamingLoggerExtensions {
        public static void LogNavigation<T>(this ILogger Logger, T LogObject, int EventId) {

            var Event = new ScreamingEvent<T> {
                EventId = EventId,
                Event = LogObject
            };


            Logger.LogInformation(EventId, JsonConvert.SerializeObject(Event));
        }
    }

    public class ScreamingLoggerProvider : ILoggerProvider {

        private readonly IDatabase _redis;
        private readonly AppSetting _settings;
        private readonly ConcurrentDictionary<string, ScreamingLogger> _loggers = new ConcurrentDictionary<string, ScreamingLogger>();

        public ScreamingLoggerProvider(IDatabase Redis, AppSetting Settings) {
            _redis = Redis;
            _settings = Settings;
        }

        public ILogger CreateLogger(string categoryName) {
            return _loggers.GetOrAdd(categoryName, name => new ScreamingLogger(_redis, _settings));
        }

        public void Dispose() {
            _loggers.Clear();
        }
    }

    public class ScreamingEvent<T> {
        public int EventId { get; set; }
        public T Event { get; set; }
    }
}
