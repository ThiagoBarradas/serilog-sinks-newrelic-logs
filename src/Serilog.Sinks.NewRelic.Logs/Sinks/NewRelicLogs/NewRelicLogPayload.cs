using Newtonsoft.Json;
using Serilog.Events;
using Serilog.Sinks.NewRelic.Logs.Sinks.NewRelicLogs;
using System;
using System.Collections.Generic;

namespace Serilog.Sinks.NewRelic.Logs
{
    public class NewRelicLogPayload
    {
        public NewRelicLogPayload() {}

        public NewRelicLogPayload(string applicationName)
        {
            this.Common.Attributes.Add("application", applicationName);
        }

        [JsonProperty("common")]
        public NewRelicLogCommon Common { get; set; } = new NewRelicLogCommon();

        [JsonProperty("logs")]
        public IList<NewRelicLogItem> Logs { get; set; } = new List<NewRelicLogItem>();
    }

    public class NewRelicLogCommon
    {
        [JsonProperty("attributes")]
        public IDictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();
    }

    public class NewRelicLogItem
    {
        private const string NewRelicLinkingMetadata = "newrelic.linkingmetadata";

        public NewRelicLogItem() {}

        public NewRelicLogItem(LogEvent logEvent, IFormatProvider formatProvider)
        {
            this.Timestamp = logEvent.Timestamp.UtcDateTime.ToUnixTimestamp();
            this.Message = logEvent.RenderMessage(formatProvider);
            this.Attributes.Add("level", logEvent.Level.ToString());
            this.Attributes.Add("stack_trace", logEvent.Exception?.StackTrace ?? "");
            if (logEvent.Exception != null) 
            {
                this.Attributes.Add("exception", logEvent.Exception.ToString() ?? "");
            }

            foreach (var property in logEvent.Properties)
            {
                this.AddProperty(property.Key, property.Value);
            }
        }

        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("attributes")]
        public IDictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();

        private void AddProperty(string key, LogEventPropertyValue value)
        {
            if (key.Equals(NewRelicLinkingMetadata, StringComparison.InvariantCultureIgnoreCase))
            {
                // unroll new relic distributed trace attributes
                if (value is DictionaryValue newRelicProperties)
                {
                    foreach (var property in newRelicProperties.Elements)
                    {
                        this.Attributes.Add(
                            NewRelicPropertyFormatter.Simplify(property.Key).ToString(),
                            NewRelicPropertyFormatter.Simplify(property.Value));
                    }
                }
            }
            else
            {
                this.Attributes.Add(key, NewRelicPropertyFormatter.Simplify(value));
            }
        }
    }
}
