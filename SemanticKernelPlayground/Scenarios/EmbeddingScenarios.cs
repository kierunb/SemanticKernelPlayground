using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Embeddings;

namespace SemanticKernelPlayground.Scenarios;

#pragma warning disable SKEXP0001

public static class EmbeddingScenarios
{
    public static async Task CreateEmbedding(Kernel kernel)
    {
        var textEmbeddingGenerationService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();


        IList<ReadOnlyMemory<float>> embeddings =
            await textEmbeddingGenerationService.GenerateEmbeddingsAsync(
            [
                "With text embedding generation, you can use an AI model to generate vectors (aka embeddings)",
                "These vectors encode the semantic meaning of the text in such a way that mathematical equations can be used on two vectors to compare the similiarty of the original text"
            ]);

        //display the embeddings
        foreach (var embedding in embeddings)
        {
            Console.WriteLine(embedding.ToString());
            foreach (var scalar in embedding.ToArray())
            {
                Console.Write($"{scalar}, ");
            }
        }
    }
}
