using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Configuration;

namespace WebClient.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty] private string _greeting = "Welcome to Avalonia!";
    
    private readonly IConfiguration _configuration;
    public MainViewModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }
}