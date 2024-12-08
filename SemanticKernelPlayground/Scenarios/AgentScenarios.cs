using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;

#pragma warning disable SKEXP0110 
namespace SemanticKernelPlayground.Scenarios;

// https://learn.microsoft.com/en-us/semantic-kernel/frameworks/agent/?pivots=programming-language-csharp
public class AgentScenarios
{
    public static async Task ChatAgentExample(Kernel kernel)
    {

        ChatCompletionAgent agent =
        new()
        {
            Name = "SummarizationAgent",
            Instructions = "Summarize user input",
            Kernel = kernel
        };
        ChatHistory chat = [];

        string userInput = "Can you summarize this text for me?";
        string text = "\nNewton’s first law states that if a body is at rest or moving at a constant speed in a straight line, " +
            "it will remain at rest or keep moving in a straight line at constant speed unless it is acted upon by a force. " +
            "In fact, in classical Newtonian mechanics, there is no important distinction between rest and uniform motion in a straight line; " +
            "they may be regarded as the same state of motion seen by different observers, " +
            "one moving at the same velocity as the particle and the other moving at constant velocity with respect to the particle. " +
            "This postulate is known as the law of inertia.";

        chat.Add(new ChatMessageContent(AuthorRole.User, $"{userInput} {text}"));

        // Generate the agent response(s)
        await foreach (ChatMessageContent response in agent.InvokeAsync(chat))
        {
            Console.WriteLine(response);
        }
    }
}
