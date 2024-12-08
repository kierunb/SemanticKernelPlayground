# SemanticKernel Playground

## Aspire Dashboard setup

[Setup and run Aspire Dashboard in standalone mode.](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/dashboard/standalone?tabs=powershell#start-the-dashboard)

```powershell
docker run -it -d `
    -p 18888:18888 `
    -p 4317:18889 `
    --name aspire-dashboard `
    mcr.microsoft.com/dotnet/aspire-dashboard:9.0
```

**Setup Qdrant vector database container**
```powershell
docker run -d --name qdrant -p 6333:6333 -p 6334:6334 qdrant/qdrant:latest
```

## AzureOpenAI/OpenAI setup

Provide keys and endpoints using appsetting.json or user secrets.

```powershell
dotnet user-secrets init

dotnet user-secrets set "azure:deployment-name" "gpt-4o"
dotnet user-secrets set "azure:embedding-deployment-name" "text-embedding-3-large"
dotnet user-secrets set "azure:endpoint" "<value>"
dotnet user-secrets set "azure:api-key" "<value>"

dotnet user-secrets set "openai:model-id" "<value>"
dotnet user-secrets set "openai:api-key" "<value>"

dotnet user-secrets set "bing:api-key" "<value>"
```

## Semantic Kernel samples:
[Samples repo](https://github.com/microsoft/semantic-kernel/tree/main/dotnet/samples)

- Chat Completion: OpenAI_ChatCompletion
- Chat Completion with Vision: OpenAI_ChatCompletionWithVision
- Function Calling: AutoInvokeKernelFunctionsAsync

## References

OpenAI SDK:
- https://learn.microsoft.com/en-us/azure/ai-services/openai/overview
- https://learn.microsoft.com/en-us/azure/ai-services/openai/chatgpt-quickstart

Semantic Kernel:
- https://learn.microsoft.com/en-us/semantic-kernel/overview/
- https://github.com/microsoft/semantic-kernel/tree/main
- https://github.com/microsoft/semantic-kernel/tree/main/dotnet/samples/Concepts
- https://github.com/microsoft/semantic-kernel/tree/main/dotnet/samples

RAG:
- https://learn.microsoft.com/en-us/azure/search/retrieval-augmented-generation-overview

Kernel Memory:
- https://microsoft.github.io/kernel-memory/
- https://github.com/microsoft/kernel-memory
- https://github.com/microsoft/kernel-memory/blob/main/examples/002-dotnet-Serverless/Program.cs
- https://microsoft.github.io/kernel-memory/security/filters

Kernel Memory + .NET Aspire sample:
- https://nikiforovall.github.io/dotnet/ai/2024/09/04/typical-rag-dotnet.html

SK + SM (Microsoft.KernelMemory.SemanticKernelPlugin):
- https://www.developerscantina.com/p/semantic-kernel-memory/

TODO:
https://learn.microsoft.com/en-us/semantic-kernel/frameworks/agent/examples/example-chat-agent?pivots=programming-language-csharp

## Additional Topics

### Presidio

Presidio (Origin from Latin praesidium ‘protection, garrison’) helps to ensure sensitive data is properly managed and 
governed. It provides fast identification and anonymization modules for private entities in text and images 
such as credit card numbers, names, locations, social security numbers, bitcoin wallets, US phone numbers, 
financial data and more.

- [GitHub](https://github.com/microsoft/presidio)
- [Demo](https://huggingface.co/spaces/presidio/presidio_demo)
- [Docs](https://microsoft.github.io/presidio/)

### GraphRag

Retrieval-Augmented Generation (RAG) is a technique to search for information based on a user query and 
provide the results as reference for an AI answer to be generated. 
This technique is an important part of most LLM-based tools and the majority of RAG 
approaches use vector similarity as the search technique. 
GraphRAG uses LLM-generated knowledge graphs to provide substantial improvements in question-and-answer 
performance when conducting document analysis of complex information. 
This builds upon our recent research, which points to the power of prompt augmentation 
when performing discovery on private datasets. Here, we define private dataset as data that the 
LLM is not trained on and has never seen before, such as an enterprise’s proprietary research, 
business documents, or communications. Baseline RAG was created to help solve this problem, 
but we observe situations where baseline RAG performs very poorly

- [GitHub](https://github.com/microsoft/graphrag)
- [Article](https://www.microsoft.com/en-us/research/blog/graphrag-unlocking-llm-discovery-on-narrative-private-data/)
- [Docs](https://github.com/microsoft/graphrag/blob/main/RAI_TRANSPARENCY.md#what-is-graphrag)