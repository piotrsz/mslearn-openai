// Implicit using statements are included
using System.Text;
using System.ClientModel;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Azure;

// Add Azure OpenAI packages
// Add Azure OpenAI packages
using Azure.AI.OpenAI;
using OpenAI.Chat;

// Build a config object and retrieve user settings.
class ChatMessageLab
{

static string? oaiEndpoint;
static string? oaiKey;
static string? oaiDeploymentName;
    static void Main(string[] args)
{
IConfiguration config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

oaiEndpoint = config["AzureOAIEndpoint"];
oaiKey = config["AzureOAIKey"];
oaiDeploymentName = config["AzureOAIDeploymentName"];

//Initialize messages list
// Initialize messages list
Console.WriteLine("\nAdding grounding context from grounding.txt");
string groundingText = System.IO.File.ReadAllText("grounding.txt");
var messagesList = new List<ChatMessage>()
{
    new UserChatMessage(groundingText),
};
do {
    // Pause for system message update
    Console.WriteLine("-----------\nPausing the app to allow you to change the system prompt.\nPress any key to continue...");
    Console.ReadKey();
    
    Console.WriteLine("\nUsing system message from system.txt");
    string systemMessage = System.IO.File.ReadAllText("system.txt"); 
    systemMessage = systemMessage.Trim();

    Console.WriteLine("\nEnter user message or type 'quit' to exit:");
    string userMessage = Console.ReadLine() ?? "";
    userMessage = userMessage.Trim();
    
    if (systemMessage.ToLower() == "quit" || userMessage.ToLower() == "quit")
    {
        break;
    }
    else if (string.IsNullOrEmpty(systemMessage) || string.IsNullOrEmpty(userMessage))
    {
        Console.WriteLine("Please enter a system and user message.");
        continue;
    }
    else
    {
        // Format and send the request to the model
// Format and send the request to the model
messagesList.Add(new SystemChatMessage(systemMessage));
messagesList.Add(new UserChatMessage(userMessage));
GetResponseFromOpenAI(messagesList);
    }
} while (true);

}

// Define the function that gets the response from Azure OpenAI endpoint
// Define the function that gets the response from Azure OpenAI endpoint
private static void GetResponseFromOpenAI(List<ChatMessage> messagesList)
{   
    Console.WriteLine("\nSending prompt to Azure OpenAI endpoint...\n\n");

    if(string.IsNullOrEmpty(oaiEndpoint) || string.IsNullOrEmpty(oaiKey) || string.IsNullOrEmpty(oaiDeploymentName) )
    {
        Console.WriteLine("Please check your appsettings.json file for missing or incorrect values.");
        return;
    }

// Configure the Azure OpenAI client
// Configure the Azure OpenAI client
AzureOpenAIClient azureClient = new (new Uri(oaiEndpoint), new ApiKeyCredential(oaiKey));
ChatClient chatClient = azureClient.GetChatClient(oaiDeploymentName);


// Get response from Azure OpenAI
// Get response from Azure OpenAI
ChatCompletionOptions chatCompletionOptions = new ChatCompletionOptions()
{
    Temperature = 0.7f,
    MaxOutputTokenCount = 800
};

ChatCompletion completion = chatClient.CompleteChat(
    messagesList,
    chatCompletionOptions
);

Console.WriteLine($"{completion.Role}: {completion.Content[0].Text}");
messagesList.Add(new AssistantChatMessage(completion.Content[0].Text));



}

}
