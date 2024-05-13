namespace Client.Library.Services.Interfaces;

public interface INavigationService
{
    Task NavigateToPage(string page);
    Task NavigateBack();
    Task RemoveLastPageFromStack();
}