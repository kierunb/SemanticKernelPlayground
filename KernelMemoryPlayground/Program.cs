
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory;

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
    //.WithSqlServerMemoryDb(sqlConnectionString)
    .Build<MemoryServerless>();

//await memory.ImportDocumentAsync("meeting-transcript.docx", tags: new() { { "user", "Blake" } });

//import a web page
string uri = "https://www.wikiwand.com/pl/articles/Inwazja_Rosji_na_Ukrain%C4%99";
await memory.ImportWebPageAsync(uri, tags: new() { { "user", "Blake" } });
Console.WriteLine("Document imported successfully");

// get answer
//var answer = await memory.AskAsync("Opowiedz o inwazji rosji na Ukrainę");
//Console.WriteLine(answer);

//foreach (var x in answer.RelevantSources)
//{
//    Console.WriteLine(x.SourceUrl != null
//        ? $"  - {x.SourceUrl} [{x.Partitions.First().LastUpdate:D}]"
//        : $"  - {x.SourceName}  - {x.Link} [{x.Partitions.First().LastUpdate:D}]");
//}

var searchResults = await memory.SearchAsync("inwazja rosji na Ukrainę");
foreach (var result in searchResults.Results)
{
    foreach (var partition in result.Partitions)
    {
        Console.WriteLine(partition.Text);
    }
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