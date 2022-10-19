Console.WriteLine("Paste your NewRelic LicenseKey and press <enter>:");
var licenseKey = Console.ReadLine();

var logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.ColoredConsole(
        outputTemplate: "{Timestamp:HH:mm:ss} ({ThreadId}) [{Level}] {Message}{NewLine}{Exception}")
    .WriteTo.NewRelicLogs(
        applicationName: "NewRelicLogsSinkDev",
        licenseKey: licenseKey)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .Enrich.WithNewRelicLogsInContext()
    .CreateLogger();

logger
    .ForContext("SampleTransaction", "trans1")
    .Information("Message in a transaction");

const string template = "This is a simple {Level} message {Val}";
logger.Verbose(template, LogEventLevel.Verbose, (int)LogEventLevel.Verbose);
logger.Debug(template, LogEventLevel.Debug, (int)LogEventLevel.Debug);
logger.Information(template, LogEventLevel.Information, (int)LogEventLevel.Information);
logger.Warning(template, LogEventLevel.Warning, (int)LogEventLevel.Warning);

// Adding a custom transaction

using (logger.BeginTimedOperation("Time a thread sleep for 2 seconds."))
{
    Thread.Sleep(1000);
    using (logger.BeginTimedOperation("And inside we try a Task.Delay for 2 seconds."))
    {
        Task.Delay(2000).Wait();
    }
    Thread.Sleep(1000);
}

using (logger.BeginTimedOperation("Using a passed in identifier", "test-loop"))
{
    // ReSharper disable once NotAccessedVariable
    var a = "";
    for (var i = 0; i < 1000; i++)
    {
        a += "b";
    }
}

// Exceed a limit
using (logger.BeginTimedOperation("This should execute within 1 second.", null, LogEventLevel.Debug, TimeSpan.FromSeconds(1)))
{
    Thread.Sleep(1100);
}

// Gauge
var queue = new Queue<int>();
var gauge = logger.GaugeOperation("queue", "item(s)", () => queue.Count);

gauge.Write();

queue.Enqueue(20);

gauge.Write();

queue.Dequeue();

gauge.Write();

// Counter
var counter = logger.CountOperation("counter", "operation(s)", true, LogEventLevel.Debug, resolution: 2);
counter.Increment();
counter.Increment();
counter.Increment();
counter.Decrement();

// Throw Exception
try
{
    throw new ApplicationException("This is an exception raised to test the New Relic API");
}
catch (Exception ex)
{
    logger.Error(ex, "Error whilst testing the Serilog.Sinks.NewRelic.Logs.Sample");
    logger.Error("A templated test message notifying of an error. Value {val}", 1);

    // adding complex object
    LogContext.PushProperty("ComplexDictionary", GetComplexDictionary());
    logger.Error("Test with complex object and dictionary");
}

logger.Dispose();

Console.WriteLine("Press a key to exit.");
Console.ReadKey(true);


static IDictionary<string, object> GetComplexDictionary()
    => new Dictionary<string, object>
    {
        { "property1", "WORKS!" },
        { "property2", 123 },
        { "property3", new Dictionary<string, object>
        {
            { "subproperty1", "blah blah" },
            { "subproperty2", new Dictionary<string, object>
            {
                { "subsubproperty1", Guid.NewGuid() },
                { "subproperty2", new Dictionary<string, object>
                {
                    { "subsubproperty1", DateTime.UtcNow },
                    { "subsubproperty2", new Dictionary<string, object>
                    {
                        { "subsubsubproperty1", 10.500F }
                    }}
                }}
            }}
        }}
    };
