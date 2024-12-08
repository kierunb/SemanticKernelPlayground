using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using OllamaSharp.Models.Chat;
using System.Text;

#pragma warning disable SKEXP0070
Console.WriteLine("Hello in Semantic Kernel with LLMs!");



StringBuilder chatPrompt = new("""
                            <message role="system">You are a librarian, expert about books</message>
                            <message role="user">Hi, I'm looking for book suggestions</message>
                            """);

var kernel = Kernel.CreateBuilder()
    .AddOllamaChatCompletion(
        endpoint: new Uri("http://localhost:11434"),
        modelId: "phi3")
.Build();

var functionResult = await kernel.InvokePromptAsync(chatPrompt.ToString());


var messageContent = functionResult.GetValue<ChatMessageContent>();


Console.WriteLine("Reply from the model:");
Console.WriteLine(messageContent?.ToString() ?? "no response");

