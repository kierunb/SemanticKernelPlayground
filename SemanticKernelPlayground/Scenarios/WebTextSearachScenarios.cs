using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using System.Text;

#pragma warning disable SKEXP0050
namespace SemanticKernelPlayground.Scenarios;
public static class WebTextSearchScenarios
{
    public async static Task BingWebSearch(IConfiguration configuration)
    {
        var textSearch = new BingTextSearch(apiKey: configuration["bing:api-key"]!);

        var query = "What is the Semantic Kernel?";

        var searchResults = await textSearch.SearchAsync(query, new() { Top = 4 });
        await foreach (string result in searchResults.Results)
        {
            Console.WriteLine(result);
        }
    }

    public async static Task SemanticKernelBingWebSearch(IConfiguration configuration, Kernel kernel)
    {
        var textSearch = new BingTextSearch(apiKey: configuration["bing:api-key"]!);
        var searchQuery = "Najnowsze informacje o cenach prądu w Polsce";
        var searchResults = await textSearch.SearchAsync(searchQuery, new() { Top = 10 });
        var stringBuilder = new StringBuilder();
        await foreach (string result in searchResults.Results)
        {
            stringBuilder.AppendLine(result);
        }

        Console.WriteLine("Search results:\n");
        Console.WriteLine(stringBuilder.ToString());

        // Build a text search plugin with Bing search and add to the kernel
        //var searchPlugin = textSearch.CreateWithSearch("SearchPlugin");
        //kernel.Plugins.Add(searchPlugin);

        //var prompt = "{{SearchPlugin.Search $query}}. {{$query}}";
        //var promptQuery = "Co możesz mi powiedzieć o najnwowszych cenach prądu w Polsce?";
        var prompt = "{{$searchResults}}. {{$promptQuery}}";

        KernelArguments arguments = new() { { "searchResults", stringBuilder.ToString() }, { "promptQuery", searchQuery } };
        Console.WriteLine("Invoking prompt with search results:\n");
        Console.WriteLine(await kernel.InvokePromptAsync(prompt, arguments));
    }
}
