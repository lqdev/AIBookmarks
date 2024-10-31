using Microsoft.Extensions.AI;
using Azure.AI.Inference;
using Azure;
using ChatRole = Microsoft.Extensions.AI.ChatRole;

IChatClient client =
    new ChatCompletionsClient(
        endpoint: new Uri("https://models.inference.ai.azure.com"), 
        new AzureKeyCredential(Environment.GetEnvironmentVariable("GITHUB_TOKEN")))
        .AsChatClient("gpt-4o-mini");

var filePaths = Directory.GetFiles("data");

var systemPrompt = 
    """
    You are an AI assistant that extracts underlined, highlighted, and marked passages from book page images.

    When passages have a natural continuation between pages, merge them and assign the page number where the first passage starts.
    """;

var passages = new List<AIBookmark>();

foreach(var path in filePaths)
{
    var file = await File.ReadAllBytesAsync(path);
    var messages = new List<ChatMessage>
    {
        new ChatMessage(ChatRole.System, systemPrompt),
        new ChatMessage(ChatRole.User, new AIContent[] {
            new ImageContent(file, "image/jpeg"),
            new TextContent("Extract the marked passages from the image"),
        })
    };

    var response = await client.CompleteAsync<List<AIBookmark>>(messages, options: new ChatOptions {Temperature = 0.1f});

    passages.AddRange(response.Result);
}

var sortedPassages = 
    passages
        .OrderBy(p => p.PageNumber)
        .Select(p => $"> {p.Text} (pg. {p.PageNumber})");


foreach(var passage in sortedPassages)
{
    Console.WriteLine(passage);
    Console.WriteLine("");
}


class AIBookmark
{
    public string Text {get;set;}

    public int PageNumber {get;set;}
}