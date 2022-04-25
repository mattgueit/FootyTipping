using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FootyTipping.Client.Helpers;
using FootyTipping.Shared;
using Microsoft.AspNetCore.Components;

namespace FootyTipping.Client.Services
{

    public interface IHttpService
    {
        Task Delete(string uri);
        Task<T> Delete<T>(string uri);
        Task<T> Get<T>(string uri);
        Task Post(string uri, object value);
        Task<T> Post<T>(string uri, object value);
        Task Put(string uri, object value);
        Task<T> Put<T>(string uri, object value);
    }

    public class HttpService : IHttpService
    {
        private HttpClient _httpClient;
        private NavigationManager _navigationManager;
        private ILocalStorageService _localStorageService;

        public HttpService(HttpClient httpClient,
            NavigationManager navigationManager,
            ILocalStorageService localStorageService
        )
        {
            _httpClient = httpClient;
            _navigationManager = navigationManager;
            _localStorageService = localStorageService;
        }

        public async Task Delete(string uri)
        {
            var request = this.CreateRequest(HttpMethod.Delete, uri);

            await this.SendRequest(request);
        }

        public async Task<T> Delete<T>(string uri)
        {
            var request = this.CreateRequest(HttpMethod.Delete, uri);

            return await this.SendRequest<T>(request);
        }

        public async Task<T> Get<T>(string uri)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);

            return await this.SendRequest<T>(request);
        }

        public async Task Post(string uri, object value)
        {
            var request = this.CreateRequest(HttpMethod.Post, uri, value);

            await this.SendRequest(request);
        }

        public async Task<T> Post<T>(string uri, object value)
        {
            var request = this.CreateRequest(HttpMethod.Post, uri, value);

            return await this.SendRequest<T>(request);
        }

        public async Task Put(string uri, object value)
        {
            var request = this.CreateRequest(HttpMethod.Put, uri, value);

            await this.SendRequest(request);
        }

        public async Task<T> Put<T>(string uri, object value)
        {
            var request = this.CreateRequest(HttpMethod.Put, uri, value);

            return await this.SendRequest<T>(request);
        }

        private HttpRequestMessage CreateRequest(HttpMethod method, string uri, object value = null)
        {
            var request = new HttpRequestMessage(method, uri);

            if (value != null)
                request.Content = new StringContent(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json");

            return request;
        }

        private async Task SendRequest(HttpRequestMessage request)
        {
            await this.AddJwtHeader(request);

            // send request
            using var response = await _httpClient.SendAsync(request);

            // auto logout on 401 response
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _navigationManager.NavigateTo("account/logout");
                return;
            }

            await this.HandleErrors(response);
        }

        private async Task<T> SendRequest<T>(HttpRequestMessage request)
        {
            await this.AddJwtHeader(request);

            // send request
            using var response = await _httpClient.SendAsync(request);

            // auto logout on 401 response
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _navigationManager.NavigateTo("account/logout");
                return default;
            }

            await this.HandleErrors(response);

            var options = new JsonSerializerOptions();
            options.PropertyNameCaseInsensitive = true;
            options.Converters.Add(new StringConverter());

            return await response.Content.ReadFromJsonAsync<T>(options);
        }

        private async Task AddJwtHeader(HttpRequestMessage request)
        {
            // add jwt auth header if user is logged in and request is to the api url
            var user = await _localStorageService.GetItem<User>("user");
            var isApiUrl = !request.RequestUri.IsAbsoluteUri;

            if (user != null && isApiUrl)
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", user.Token);
        }

        private async Task HandleErrors(HttpResponseMessage response)
        {
            // throw exception on error response
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                throw new Exception(error["message"]);
            }
        }
    }
}
