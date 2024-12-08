using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.SqlServer;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.Memory;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SemanticKernelPlayground.Factories;
using SemanticKernelPlayground.Filters;
using SemanticKernelPlayground.Scenarios;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

#pragma warning disable SKEXP0001, SKEXP0010, SKEXP0050, SKEXP0020
Console.WriteLine("> Hello in Semantic Kernel Playground!\n");

try
{
    IConfiguration configuration = GetAppConfiguration();
    LogLevel logLevel = LogLevel.Information;

    #region ConfigureOpenTelemetry
    AppContext.SetSwitch("Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnosticsSensitive", true);
    string otelEndpoint = "http://localhost:4317";

    var resourceBuilder = ResourceBuilder
        .CreateDefault()
        .AddService("SemanticKernelPlayground");
    using var traceProvider = Sdk.CreateTracerProviderBuilder()
        .SetResourceBuilder(resourceBuilder)
        .AddSource("Microsoft.SemanticKernel*")
        .AddOtlpExporter(options => options.Endpoint = new Uri(otelEndpoint))
        .Build();
    using var meterProvider = Sdk.CreateMeterProviderBuilder()
        .SetResourceBuilder(resourceBuilder)
        .AddMeter("Microsoft.SemanticKernel*")
        .AddOtlpExporter(options => options.Endpoint = new Uri(otelEndpoint))
        .Build();
    using var loggerFactory = LoggerFactory.Create(builder =>
    {
        // Add OpenTelemetry as a logging provider
        builder.AddOpenTelemetry(options =>
        {
            options.SetResourceBuilder(resourceBuilder);
            options.AddOtlpExporter(options => options.Endpoint = new Uri(otelEndpoint));
            // Format log messages. This is default to false.
            options.IncludeFormattedMessage = true;
            options.IncludeScopes = true;
        });
        builder.SetMinimumLevel(logLevel);
    });
    #endregion

    var kernel = SemanticKernelFactory.Create(
        AIServiceProvider.AzureOpenAI,
        configuration,
        loggerFactory,
        logLevel: logLevel
    );

    // Semantic Text Memory
    var textEmbeddingGenerationService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();
    var sqlServerStore = new SqlServerMemoryStore(configuration.GetConnectionString("SqlServerVectorDB")!);
    var volatileMemoryStore = new VolatileMemoryStore();
    var memory = new SemanticTextMemory(volatileMemoryStore, textEmbeddingGenerationService);

    Console.WriteLine("Kernel created successfully.");

    // Playground Completion Scenarios
    //await CompletionScenarios.InvokePrompt(kernel);

    //await CompletionScenarios.InvokeSemanticFunction(kernel);

    //await CompletionScenarios.InvokeChatCompletion(kernel);

    //await CompletionScenarios.InvokeNativeCompletion(kernel);

    //await CompletionScenarios.ChatAndNativeFunctions(kernel);

    // await EmbeddingScenarios.CreateEmbedding(kernel);

    //await MemoryStoreScenarios.SemanticKernelMemorySample(kernel, memory);

    await VectorStoreScenarios.VectorStoreSample(kernel);

    //await WebTextSearchScenarios.BingWebSearch(configuration);
    //await WebTextSearchScenarios.SemanticKernelBingWebSearch(configuration, kernel);

    //await AgentScenarios.ChatAgentExample(kernel);
}
catch (Exception ex)
{
    Console.WriteLine("Something went wrong :(");
    Console.WriteLine($"Error: {ex.Message}");
}

static IConfiguration GetAppConfiguration()
{
    var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .AddUserSecrets<Program>();
    return builder.Build();
}