using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Management;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using System.IO;

namespace FlyerDemoClient
{
    public static class Data
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public static readonly string SchemaEndpoint = "schemas";
        public static readonly string AzureAuthenticationFile = "azureauth.json";

        public static async Task<T> Get<T>(string endpoint) =>
            await JsonSerializer.DeserializeAsync<T>(
                await _httpClient.GetStreamAsync(endpoint));

        public static async Task<T> Post<T>(string endpoint, T data) =>
            await JsonSerializer.DeserializeAsync<T>(
                await (await _httpClient.PostAsync(
                    endpoint,
                    new StringContent(JsonSerializer.Serialize(data))))
                    .Content
                    .ReadAsStreamAsync());

        public static async Task<T> Patch<T>(string endpoint, T data) =>
            await JsonSerializer.DeserializeAsync<T>(
                await (await _httpClient.PatchAsync(
                    endpoint,
                    new StringContent(JsonSerializer.Serialize(data))))
                    .Content
                    .ReadAsStreamAsync());

        static Data()
        {
            _httpClient.BaseAddress = new Uri("http://localhost:7071/api/");
            //_httpClient.BaseAddress = new Uri("http://flyerapi.azurewebsites.net/api/");
            var credentials = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(AzureAuthenticationFile));

            MultipartFormDataContent body = new MultipartFormDataContent
            {
                { new StringContent("grant_type"), "client_credentials" },
                { new StringContent("client_id"), credentials["client_id"] },
                { new StringContent("client_secret"), credentials["client_secret"] },
                { new StringContent("scope"), "https://graph.microsoft.com/.default" }
            };

            HttpRequestMessage msg = new HttpRequestMessage
            {
                RequestUri = new Uri("https://login.microsoftonline.com/klhengrs.onmicrosoft.com/oauth2/v2.0/token"),
                Method = HttpMethod.Post,
                //msg.Headers.Add("Content-Type", "multipart/form-data");
                Content = body
            };
            _httpClient.SendAsync(msg);
            //var dict =JsonSerializer.Deserialize<Dictionary<string, string>>(response.Content.ReadAsStringAsync().Result);
            //_httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", dict["access_token"]);

        }

        public static void AddAuthentication(string bearerToken) => _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
    }
}
