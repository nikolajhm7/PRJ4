namespace Client.Libary.Services.Interfaces;

public interface INavigationService
{
    Task NavigateToPage(string page);
    Task NavigateBack();
}