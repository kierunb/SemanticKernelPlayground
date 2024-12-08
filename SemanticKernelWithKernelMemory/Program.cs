
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.KernelMemory;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemanticKernelPlayground;

Console.WriteLine("Hello, Semantic Kernel and Kernel Memory!");

IConfiguration configuration = GetAppConfiguration();

#region Semantic Kernel Configuration
string apiKey = configuration["azure:api-key"]!;
string deploymentChatName = configuration["azure:deployment-name"]!;
string endpoint = configuration["azure:endpoint"]!;

// Create a kernel with Azure OpenAI chat completion
var kernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion(deploymentChatName, endpoint, apiKey)
    .Build(); 
#endregion

Console.WriteLine("Semantic Kernel created successfully!");

#region Kernel Memory Configuration
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

var kernelMemory = new KernelMemoryBuilder()
    .WithAzureOpenAITextGeneration(azureOpenAITextConfig)
    .WithAzureOpenAITextEmbeddingGeneration(azureOpenAIEmbeddingConfig)
    .Build<MemoryServerless>(); 
#endregion

Console.WriteLine("Kernel Memory created successfully!");

Console.WriteLine("Creating memories...");

await kernelMemory.ImportWebPageAsync("https://en.wikipedia.org/wiki/Notre-Dame_de_Paris");

Console.WriteLine("Memories created...");

var plugin = new MemoryPlugin(kernelMemory, waitForIngestionToComplete: true);
kernel.ImportPluginFromObject(plugin, "memory");

OpenAIPromptExecutionSettings settings = new()
{
    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
};

var prompt = @"
            Question to Kernel Memory: {{$input}}

            Kernel Memory Answer: {{memory.ask $input}}

            If the answer is empty say 'I don't know', otherwise reply.
            ";

string question = "When construction of Notre-Dame de Paris began?";

Console.WriteLine("Searching memory...\n");

var searchResults = await kernelMemory.SearchAsync(question);

Console.WriteLine($"Memories found: {searchResults.Results.Count}\n");

foreach (var result in searchResults.Results)
{
    foreach (var partition in result.Partitions)
    {
        Console.WriteLine($"Text: {partition.Text.Truncate(20)} Relevance: {partition.Relevance}" );
    }
}


KernelArguments arguments = new(settings)
{
    { "input", question },
};


Console.WriteLine("\nInvoking grounded prompt...\n");
var response = await kernel.InvokePromptAsync(prompt, arguments);

Console.WriteLine($"Answer: {response.GetValue<string>()}");
Console.ReadLine();


static IConfiguration GetAppConfiguration()
{
    var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .AddUserSecrets<Program>();
    return builder.Build();
}