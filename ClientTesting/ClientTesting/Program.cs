using Microsoft.AspNetCore.SignalR.Client;
using System.IO;

var connection = new HubConnectionBuilder()
    .WithUrl("http://localhost:5000/iohub") // Adjust port if different
    .WithAutomaticReconnect()
    .Build();

connection.On<string>("ReceiveData", (message) =>
{
    Console.WriteLine($"[Received] {message}");
});

await connection.StartAsync();
Console.WriteLine("Connected to IOHub");

await connection.InvokeAsync("Register");

// Path to the message.json file
string filePath = "D:\\Code\\GameServerManagementStudio\\ClientTesting\\ClientTesting\\message.json";

// FileSystemWatcher to monitor changes to message.json
var fileWatcher = new FileSystemWatcher
{
    Path = Path.GetDirectoryName(filePath) ?? ".",
    Filter = Path.GetFileName(filePath),
    NotifyFilter = NotifyFilters.LastWrite
};

fileWatcher.Changed += async (sender, e) =>
{
    try
    {
        // Read and send the contents of the file when it is saved
        var fileContents = await File.ReadAllTextAsync(filePath);
        await connection.InvokeAsync("SendToService", fileContents);
        Console.WriteLine($"[File Sent] {fileContents}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Error] Failed to send file contents: {ex.Message}");
    }
};

fileWatcher.EnableRaisingEvents = true;

while (true)
{
    Console.Write("Enter message: ");
    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input)) break;

    if (input.Trim().ToLower() == "send")
    {
        try
        {
            // Read and send the contents of the file when "send" is entered
            var fileContents = await File.ReadAllTextAsync(filePath);
            await connection.InvokeAsync("SendToService", fileContents);
            Console.WriteLine($"[File Sent] {fileContents}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error] Failed to send file contents: {ex.Message}");
        }
    }
    else
    {
        await connection.InvokeAsync("SendToService", input);
    }
}