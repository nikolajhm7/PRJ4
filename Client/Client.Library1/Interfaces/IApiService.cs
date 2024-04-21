using System.Net.Http;
using System.Threading.Tasks;

namespace Client.UI.Services.Interfaces;

public interface IApiService
{
    public Task<HttpResponseMessage> MakeApiCall(string endpoint, HttpMethod method, HttpContent content = null);
}