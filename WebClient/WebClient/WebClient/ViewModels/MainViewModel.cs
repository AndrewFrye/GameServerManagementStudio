using System;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.SignalR.Client;


namespace WebClient.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty] private string _consoleText = String.Empty;
    [ObservableProperty] private bool _isConnected = false;

    private readonly IConfiguration _configuration;
    private readonly HubConnection _connection;

    public MainViewModel(IConfiguration configuration)
    {
        _configuration = configuration;
        
        _connection = new HubConnectionBuilder()
            .WithUrl($"http://{_configuration["HostName"]}:{_configuration["Port"]}/iohub")
            .WithAutomaticReconnect()
            .Build();

        _connection.On<string>("ReceiveData", message =>
        {
            ConsoleText += $"[Received] {message}{Environment.NewLine}";
        });
    }

    [RelayCommand]
    public async Task Send()
    {
        if (!IsConnected)
        {
            ConsoleText += "[Error] Not connected to IOHub\n";
            return;
        }

        try
        {
            await SendFileContents();
        }
        catch (Exception ex)
        {
            ConsoleText += $"[Error] Failed to send file contents: {ex.Message}\n";
        }
    }
    
    [RelayCommand]
    public async Task Connect()
    {
        try
        {
            await _connection.StartAsync();
            await _connection.InvokeAsync("Register");
            IsConnected = true;
            ConsoleText += "Connected to IOHub\n";
        }
        catch (Exception ex)
        {
            ConsoleText += $"[Error] Failed to connect: {ex.Message}\n";
        }
    }
    
    [RelayCommand]
    public async Task Disconnect()
    {
        try
        {
            await _connection.StopAsync();
            IsConnected = false;
            ConsoleText += "Disconnected from IOHub\n";
        }
        catch (Exception ex)
        {
            ConsoleText += $"[Error] Failed to disconnect: {ex.Message}\n";
        }
    }
    
    private async Task SendFileContents()
    {
        string _filePath = "D:\\Code\\GameServerManagementStudio\\ClientTesting\\ClientTesting\\message.json";
        
        try
        {
            var fileContents = await File.ReadAllTextAsync(_filePath);
            await _connection.InvokeAsync("SendToService", fileContents);
            ConsoleText += $"[File Sent] {fileContents}\n";
        }
        catch (Exception ex)
        {
            ConsoleText += $"[Error] Failed to send file contents: {ex.Message}\n";
        }
    }
}