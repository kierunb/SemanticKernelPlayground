using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemanticKernelPlayground.Plugins;

namespace SemanticKernelPlayground.Scenarios;

public static class CompletionScenarios
{
    public async static Task InvokePrompt(Kernel kernel)
    {
        Console.WriteLine("\nPrompting the kernel...");
        var response = await kernel.InvokePromptAsync("Why is the sky blue in one sentence?");
        Console.WriteLine(response);
    }

    public async static Task InvokePromptWithParameters(Kernel kernel)
    {
        Console.WriteLine("\nPrompting the kernel with parameters...");
        KernelArguments arguments = new() { { "topic", "dogs" } };
        var response = await kernel.InvokePromptAsync("Write a joke about {{$topic}}", arguments);
        Console.WriteLine(response.GetValue<string>());
    }

    public async static Task InvokeSemanticFunction(Kernel kernel)
    {
        Console.WriteLine("\nInvoking Semantic Function...");
        // Function defined using few-shot design pattern
        string promptTemplate =
            @"
            Generate a creative reason or excuse for the given event.
            Be creative and be funny. Let your imagination run wild.

            Event: I am running late.
            Excuse: I was being held ransom by giraffe gangsters.

            Event: I haven't been to the gym for a year
            Excuse: I've been too busy training my pet dragon.

            Event: {{$input}}
            ";

        var excuseFunction = kernel.CreateFunctionFromPrompt(
            promptTemplate,
            new OpenAIPromptExecutionSettings()
            {
                MaxTokens = 100,
                Temperature = 0.8,
                TopP = 1,
            }
        );

        var result = await kernel.InvokeAsync(
            excuseFunction,
            new() { ["input"] = "I missed the F1 final race" }
        );
        Console.WriteLine(result.GetValue<string>());

    }

    public async static Task InvokeChatCompletion(Kernel kernel)
    {
        Console.WriteLine("\nChat completion demo");
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        ChatHistory history = [];
        history.AddUserMessage("What is the generative AI?");

        var response = chatCompletionService.GetStreamingChatMessageContentsAsync(
            chatHistory: history,
            kernel: kernel
        );

        await foreach (var chunk in response)
        {
            Console.Write(chunk);
        }
    }

    // function calling
    public async static Task InvokeNativeCompletion(Kernel kernel)
    {
        Console.WriteLine("\nInvoking native function demo");
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        kernel.Plugins.AddFromType<DatePlugin>("Date");

        OpenAIPromptExecutionSettings openAIPromptExecutionSettings =
            new() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };

        Console.Write("User > ");
        string? userInput = Console.ReadLine();

        var result = await chatCompletionService.GetChatMessageContentAsync(
            userInput!,
            executionSettings: openAIPromptExecutionSettings,
            kernel: kernel
        );
        Console.WriteLine($"Assistant > {result}");
    }

    public async static Task ChatAndNativeFunctions(Kernel kernel)
    {
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

        kernel.Plugins.AddFromType<LightsPlugin>("Lights");

        // Enable planning

        OpenAIPromptExecutionSettings openAIPromptExecutionSettings =
            new() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() };

        // Create a history store the conversation
        var history = new ChatHistory();

        // Initiate a back-and-forth chat
        string? userInput = string.Empty;
        do
        {
            // Collect user input
            Console.Write("User > ");
            userInput = Console.ReadLine();

            // Add user input
            history.AddUserMessage(userInput);

            // Get the response from the AI
            var result = await chatCompletionService.GetChatMessageContentAsync(
                history,
                executionSettings: openAIPromptExecutionSettings,
                kernel: kernel
            );

            // Print the results
            Console.WriteLine("Assistant > " + result);

            // Add the message from the agent to the chat history
            history.AddMessage(result.Role, result.Content ?? string.Empty);
        } while (userInput is not null);
    }
}
