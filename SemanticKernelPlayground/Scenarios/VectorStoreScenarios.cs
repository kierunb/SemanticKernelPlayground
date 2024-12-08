using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Embeddings;
using Qdrant.Client;
using System.Text.Json;

namespace SemanticKernelPlayground.Scenarios;

#pragma warning disable SKEXP0001

public static class VectorStoreScenarios
{
    public static async Task VectorMemoryStoreSample(Kernel kernel)
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

    public static async Task VectorQdrantStoreSample(Kernel kernel)
    {
        // docker run -d --name qdrant -p 6333:6333 -p 6334:6334 qdrant/qdrant:latest
        // Qdrant dashboard: http://localhost:6333/dashboard
        // Create a Qdrant VectorStore object
        var vectorStore = new QdrantVectorStore(new QdrantClient("localhost"));
        var textEmbeddingGenerationService = kernel.GetRequiredService<ITextEmbeddingGenerationService>();

        // Choose a collection from the database and specify the type of key and record stored in it via Generic parameters.
        var collection = vectorStore.GetCollection<ulong, Hotel>("skhotels");

        // Create the collection if it doesn't exist yet.
        await collection.CreateCollectionIfNotExistsAsync();

        // Upsert a record.
        string descriptionText = "A place where everyone can be happy.";
        ulong hotelId = 1;

        // Create a record and generate a vector for the description using your chosen embedding generation implementation.
        await collection.UpsertAsync(new Hotel
        {
            HotelId = hotelId,
            HotelName = "Hotel Happy",
            Description = descriptionText,
            DescriptionEmbedding = await GenerateEmbeddingAsync(descriptionText),
            Tags = new[] { "luxury", "pool" }
        });

        // Retrieve the upserted record.
        Hotel? retrievedHotel = await collection.GetAsync(hotelId);

        // Embedding generation method
        async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string textToVectorize)
        {
            return await textEmbeddingGenerationService.GenerateEmbeddingAsync(textToVectorize);
        }

        // Generate a vector for your search text, using your chosen embedding generation implementation.
        ReadOnlyMemory<float> searchVector = await GenerateEmbeddingAsync("I'm looking for a hotel where customer happiness is the priority.");

        // Do the search.
        var searchResult = await collection.VectorizedSearchAsync(searchVector, new() { Top = 1 });

        // Inspect the returned hotel.
        await foreach (var record in searchResult.Results)
        {
            Console.WriteLine("Found hotel description: " + record.Record.Description);
            Console.WriteLine("Found record score: " + record.Score);
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

public class Hotel
{
    [VectorStoreRecordKey]
    public ulong HotelId { get; set; }

    [VectorStoreRecordData(IsFilterable = true)]
    public string HotelName { get; set; } = string.Empty;

    [VectorStoreRecordData(IsFullTextSearchable = true)]
    public string Description { get; set; } = string.Empty;

    [VectorStoreRecordVector(Dimensions: 4)]
    public ReadOnlyMemory<float>? DescriptionEmbedding { get; set; }

    [VectorStoreRecordData(IsFilterable = true)]
    public string[] Tags { get; set; } = [];
}
