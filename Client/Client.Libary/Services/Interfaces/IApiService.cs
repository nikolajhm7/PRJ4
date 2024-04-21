namespace Client.Libary.Interfaces;

public interface IApiService
{
    public Task<HttpResponseMessage> MakeApiCall(string endpoint, HttpMethod method, HttpContent content = null);
}