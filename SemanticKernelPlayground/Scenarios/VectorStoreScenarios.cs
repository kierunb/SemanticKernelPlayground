using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Embeddings;
using System.Text.Json;

namespace SemanticKernelPlayground.Scenarios;

#pragma warning disable SKEXP0001

public static class VectorStoreScenarios
{
    public static async Task VectorStoreSample(Kernel kernel)
    {
        var vectorStore = new InMemoryVectorStore();

        // Get a collection instance using vector store
        var collection = vectorStore.GetCollection<ulong, Glossary>("skglossary");

        await collection.CreateCollectionIfNotExistsAsync();

        var glossaryEntries = new List<Glossary>()
        {
            new()
            {
                Key = 1,
                Term = "API",
                Definition = "Application Programming Interface. A set of rules and specifications that allow software components to communicate and exchange data."
            },
            new()
            {
                Key = 2,
                Term = "Connectors",
                Definition = "Connectors allow you to integrate with various services provide AI capabilities, including LLM, AudioToText, TextToAudio, Embedding generation, etc."
            },
            new()
            {
                Key = 3,
                Term = "RAG",
                Definition = "Retrieval Augmented Generation - a term that refers to the process of retrieving additional data to provide as context to an LLM to use when generating a response (completion) to a user's question (prompt)."
            }
        };

        // generate embeddings
        var textEmbeddingGenerationService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();

        var tasks = glossaryEntries.Select(entry => Task.Run(async () =>
        {
            entry.DefinitionEmbedding = await textEmbeddingGenerationService.GenerateEmbeddingAsync(entry.Definition);
        }));

        await Task.WhenAll(tasks);

        // upsert records
        await foreach (var key in collection.UpsertBatchAsync(glossaryEntries))
        {
            Console.WriteLine("Key: {key} upserted.");
        }

        // get records by key

        var options = new GetRecordOptions() { IncludeVectors = true };

        Console.WriteLine("\n\nGetting records by key...");
        await foreach (var record in collection.GetBatchAsync(keys: [1, 2, 3], options))
        {
            Console.WriteLine($"Key: {record.Key}");
            Console.WriteLine($"Term: {record.Term}");
            Console.WriteLine($"Definition: {record.Definition}");
            Console.WriteLine($"Definition Embedding: {JsonSerializer.Serialize(record.DefinitionEmbedding).Truncate(60)}");
        }

        Console.WriteLine("\n\nPerforming a search...");

        var searchString = "I want to learn more about Connectors";
        var searchVector = await textEmbeddingGenerationService.GenerateEmbeddingAsync(searchString);

        var searchResult = await collection.VectorizedSearchAsync(searchVector);

        await foreach (var result in searchResult.Results)
        {
            Console.WriteLine($"Search score: {result.Score}");
            Console.WriteLine($"Key: {result.Record.Key}");
            Console.WriteLine($"Term: {result.Record.Term}");
            Console.WriteLine($"Definition: {result.Record.Definition}");
            Console.WriteLine("=========");
        }
    }
}

public sealed class Glossary
{
    [VectorStoreRecordKey]
    public ulong Key { get; set; }

    [VectorStoreRecordData]
    public string Term { get; set; } = string.Empty;

    [VectorStoreRecordData]
    public string Definition { get; set; } = string.Empty;

    [VectorStoreRecordVector(Dimensions: 1536)]
    public ReadOnlyMemory<float> DefinitionEmbedding { get; set; }
}
