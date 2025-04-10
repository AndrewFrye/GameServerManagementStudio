using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameServerManagementStudio.Data;
using GameServerManagementStudio.Data.JSON.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;


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

        _connection.On<string>("ReceiveData",
            async message => { await OnDataRecieved(message); });
    }

    [ObservableProperty] string _commandInput = String.Empty;
    
    private List<string> _consoleLines = new();
    public async Task OnDataRecieved(string packet)
    {
        var message = JsonConvert.DeserializeObject<MessageEntity>(packet);
        if (message == null)
            return;
        else
        {
            switch (message.Command)
            {
                case "Log":
                    _consoleLines.Add(message.Message);
                    while (_consoleLines.Count > 100)
                        _consoleLines.RemoveAt(0);
                    
                    ConsoleText = String.Join("\n", _consoleLines);
                    break;
                case "ReturnServerInfos":
                    await ServerInfosRecieved(message.Message);
                    break;
            }
        }
    }
    
    private async Task ServerInfosRecieved(string infos)
    {
        var serverInfos = JsonConvert.DeserializeObject<List<IGameInfoEntity>>(infos);
        if (serverInfos == null)
            return;
        
        SelectServerSource.Clear();
        _serverInfos.Clear();
        foreach (var serverInfo in serverInfos)
        {
            SelectServerSource.Add(serverInfo.InstanceId);
            _serverInfos.Add(serverInfo.InstanceId, serverInfo);
        }
    }
    

    [RelayCommand]
    public async Task Send()
    {
        if (!IsConnected)
        {
            ConsoleText += "[Error] Not connected to IOHub\n";
            return;
        }

        if (String.IsNullOrWhiteSpace(CommandInput))
        {
            return;
        }
        
        try
        {
            var message = await EncodeCommand(CommandInput);
            await _connection.InvokeAsync("SendToService", message);
        }
        catch (Exception ex)
        {
            ConsoleText += $"[Error] Failed to send command: {ex.Message}\n";
        }
    }

    private async Task<string> EncodeCommand(string command)
    {
        var message = new MessageEntity()
        {
            Command = "Command",
            Source = "Client",
            Message = command
        };

        return await Task.Run(() => JsonConvert.SerializeObject(message));
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
            await FetchServerInfos();
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

    public ObservableCollection<string> SelectServerSource { get; set; } = new();
    private Dictionary<string, IGameInfoEntity> _serverInfos = new();
    [ObservableProperty] private string _selectedServerString = String.Empty;
    [ObservableProperty] private IGameInfoEntity _selectedServer;
    
    [RelayCommand]
    public async Task FetchServerInfos()
    {
        if (!IsConnected)
        {
            ConsoleText += "[Error] Not connected to IOHub\n";
            return;
        }

        try
        {
            var message = new MessageEntity
            {
                Command = "FetchServerInfos",
                Source = "Client"
            };
            await _connection.InvokeAsync("SendToService", JsonConvert.SerializeObject(message));
        }
        catch (Exception ex)
        {
            ConsoleText += $"[Error] Failed to fetch server infos: {ex.Message}\n";
        }
    }
}