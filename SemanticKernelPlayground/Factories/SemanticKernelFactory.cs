using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using SemanticKernelPlayground.Filters;

namespace SemanticKernelPlayground.Factories;

public enum AIServiceProvider
{
    AzureOpenAI,
    OpenAI
}

public static class SemanticKernelFactory
{
    public static Kernel Create(
        AIServiceProvider aiServiceProvider,
        IConfiguration configuraton,
        ILoggerFactory loggerFactory,
        LogLevel logLevel = LogLevel.Trace)
    {

        IKernelBuilder builder = Kernel.CreateBuilder();

        if (loggerFactory is null)
        {
            // console output
            builder.Services.AddLogging(services => services.AddConsole().SetMinimumLevel(logLevel));
        }
        else
        {
            builder.Services.AddSingleton(loggerFactory);
        }

        // enable filters
        //builder.Services.AddSingleton<IFunctionInvocationFilter, LoggingFilter>();

        ConfigureAIProvider(builder, configuraton, aiServiceProvider);

        return builder.Build();
    }

    private static void ConfigureAIProvider(IKernelBuilder builder, IConfiguration configuration, AIServiceProvider aiServiceProvider)
    {
        switch (aiServiceProvider)
        {
            case AIServiceProvider.AzureOpenAI:
                builder.Services.AddAzureOpenAIChatCompletion(
                    deploymentName: configuration["azure:deployment-name"]!,
                    endpoint: configuration["azure:endpoint"]!,
                    apiKey: configuration["azure:api-key"]!
                );

                #pragma warning disable SKEXP0010
                builder.Services.AddAzureOpenAITextEmbeddingGeneration(
                    deploymentName: configuration["azure:embedding-deployment-name"]!,
                    endpoint: configuration["azure:endpoint"]!,
                    apiKey: configuration["azure:api-key"]!
                );

                break;

            case AIServiceProvider.OpenAI:
                builder.Services.AddOpenAIChatCompletion(
                    modelId: configuration["openai:model-id"]!,
                    apiKey: configuration["openai:api-key"]!
                );
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(aiServiceProvider), aiServiceProvider, null);
        }
    }
}
