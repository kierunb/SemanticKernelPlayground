
using Azure.AI.OpenAI;
using Azure;
using OpenAI.Chat;

Console.WriteLine("Hello, Azure Open AI SDK!");

#region settings
string endpoint = "https://azure-open-ai-sweden-bk.openai.azure.com/";
string key = "ff6d756998ea471db1192263d18b47ca";
#endregion


AzureOpenAIClient azureClient = new(
    new Uri(endpoint),
    new AzureKeyCredential(key));

// This must match the custom deployment name you chose for your model
ChatClient chatClient = azureClient.GetChatClient("gpt-4o");

var response = await chatClient.CompleteChatAsync("Jak ugotować żurek?");

Console.WriteLine(response.Value.Content[0].Text);