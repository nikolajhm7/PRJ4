using System.Threading.Tasks;

namespace Client.UI.Services.Interfaces;

interface IAuthenticationService
{
    public Task<bool> IsUserAuthenticated();
}