using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using OpenAI.VectorStores;

#pragma warning disable SKEXP0001, SKEXP0010, SKEXP0050
namespace SemanticKernelPlayground.Scenarios;
public static class MemoryStoreScenarios
{
    public static async Task SemanticKernelMemorySample(Kernel kernel, SemanticTextMemory memory)
    {
        string memoryCollectionName = "aboutMe";

        await memory.SaveInformationAsync(memoryCollectionName, id: "info1", text: "My name is Andrea");
        await memory.SaveInformationAsync(
            memoryCollectionName,
            id: "info2",
            text: "I currently work as a tourist operator"
        );
        await memory.SaveInformationAsync(
            memoryCollectionName,
            id: "info3",
            text: "I currently live in Seattle and have been living there since 2005"
        );
        await memory.SaveInformationAsync(
            memoryCollectionName,
            id: "info4",
            text: "I visited France and Italy five times since 2015"
        );
        await memory.SaveInformationAsync(
            memoryCollectionName,
            id: "info5",
            text: "My family is from New York"
        );

        var questions = new[]
        {
            "what is my name?",
            "where do I live?",
            "where is my family from?",
            "where have I travelled?",
            "what do I do for work?",
        };

        foreach (var q in questions)
        {
            var response = await memory.SearchAsync(memoryCollectionName, q).FirstOrDefaultAsync();
            Console.WriteLine("Q: " + q);
            Console.WriteLine($"A: {response?.Metadata.Text} (Relevance: {response?.Relevance.ToString()})");
        }
    }
}

