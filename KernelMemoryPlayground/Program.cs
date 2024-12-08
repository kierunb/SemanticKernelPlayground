
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.Handlers;
using SemanticKernelPlayground;

Console.WriteLine("Hello in Semantic Memory Playground");

var configuration = GetAppConfiguration();

var sqlConnectionString = configuration.GetConnectionString("SqlServerEmbeddings");

var azureOpenAITextConfig = new AzureOpenAIConfig
{
    Endpoint = configuration["azure:endpoint"]!,
    APIKey = configuration["azure:api-key"]!,
    Deployment = configuration["azure:deployment-name"]!,
    Auth = AzureOpenAIConfig.AuthTypes.APIKey,
};

var azureOpenAIEmbeddingConfig = new AzureOpenAIConfig
{
    Endpoint = configuration["azure:endpoint"]!,
    APIKey = configuration["azure:api-key"]!,
    Deployment = configuration["azure:embedding-deployment-name"]!,
    Auth = AzureOpenAIConfig.AuthTypes.APIKey,
};

var memory = new KernelMemoryBuilder()
    .WithAzureOpenAITextGeneration(azureOpenAITextConfig)
    .WithAzureOpenAITextEmbeddingGeneration(azureOpenAIEmbeddingConfig) 
    .WithSqlServerMemoryDb(configuration.GetConnectionString("SqlServerEmbeddingsDB")!)
    .Build<MemoryServerless>();

// run as service/container/in cloud: https://github.com/microsoft/kernel-memory
// memory web client: https://github.com/microsoft/kernel-memory/blob/main/examples/001-dotnet-WebClient/Program.cs
// bash/curl: https://microsoft.github.io/kernel-memory/quickstart/bash

// build your own processing pipeline: https://github.com/microsoft/kernel-memory/blob/main/examples/004-dotnet-serverless-custom-pipeline/Program.cs
//Console.WriteLine("\nRegistering pipeline handlers...");

//.WithoutDefaultHandlers() // remove default handlers, added manually below
//memory.Orchestrator.AddHandler<TextExtractionHandler>("extract_text");
//memory.Orchestrator.AddHandler<TextPartitioningHandler>("split_text_in_partitions");
//memory.Orchestrator.AddHandler<GenerateEmbeddingsHandler>("generate_embeddings");
//memory.Orchestrator.AddHandler<SummarizationHandler>("summarize");
//memory.Orchestrator.AddHandler<SaveRecordsHandler>("save_memory_records");

Console.WriteLine("\nImporting documents...\n");

// import document samples: https://github.com/microsoft/kernel-memory/blob/main/examples/002-dotnet-Serverless/Program.cs
// security/tags: https://microsoft.github.io/kernel-memory/security/filters

// import word document
await memory.ImportDocumentAsync(@"docs\Notre-Dame-Seminary.docx", tags: new() { { "topic", "notre dame" } });

//import a web page
string uri = "https://en.wikipedia.org/wiki/Notre-Dame_de_Paris";
await memory.ImportWebPageAsync(uri, tags: new() { { "topic", "notre dame" } });

Console.WriteLine("Documents imported successfully");

//foreach (var x in answer.RelevantSources)
//{
//    Console.WriteLine(x.SourceUrl != null
//        ? $"  - {x.SourceUrl} [{x.Partitions.First().LastUpdate:D}]"
//        : $"  - {x.SourceName}  - {x.Link} [{x.Partitions.First().LastUpdate:D}]");
//}

string query1 = "When construction of Notre-Dame de Paris began?";
string query2 = "Tell me about seminary building";

Console.WriteLine("\nSearching...\n");

var searchResults = await memory.SearchAsync(query1);
foreach (var result in searchResults.Results)
{
    foreach (var partition in result.Partitions)
    {
        Console.WriteLine($"Text: {partition.Text.Truncate(20)} Relevance: {partition.Relevance}");
    }
}

Console.WriteLine("\nGetting Answers...\n");

Console.WriteLine("Question: " + query1);
var answer1 = await memory.AskAsync(query1);
Console.WriteLine(answer1);

Console.WriteLine("Question: " + query2);
var answer2 = await memory.AskAsync(query2);
Console.WriteLine(answer2);

static IConfiguration GetAppConfiguration()
{
    var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .AddUserSecrets<Program>();
    return builder.Build();
}