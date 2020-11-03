using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;

namespace Anthos.Samples.BankOfAnthos.Overdraft
{
    /// <summary>
    /// Helper class to wraps calls to the bank's user service.
    /// </summary>
    public class UserService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiAddress;

        public UserService(HttpClient httpClient, string baseApiAddress)
        {
            _httpClient = httpClient;
            _apiAddress = baseApiAddress;
        }

        public async Task CreateUserAsync(IBankService.NewUser request)
        {
            string url = $"{_apiAddress}/users";

            var formContent = GetUserFormContent(request);

            var response = await _httpClient.PostAsync(url, formContent);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new ApplicationException($"Unable to create user: {content}");
        }

        public async Task<string> LoginAsync(string username, string password)
        {
            const string TOKEN_PROPERTY = "token";
            string url = $"{_apiAddress}/login";

            UriBuilder uriBuilder = new UriBuilder(url);
            uriBuilder.Query = $"username={username}&password={password}";
            Uri uri = uriBuilder.Uri;
          
            var response = await _httpClient.GetAsync(uri);
            if (!response.IsSuccessStatusCode)
                throw new ApplicationException($"Unable to get token for {username}");

            var doc = JsonDocument.Parse(response.Content.ReadAsStream());
            string token = doc.RootElement.GetProperty(TOKEN_PROPERTY).GetString();

            if (token == null)
                throw new ApplicationException("Unable to get token.");

            return token;           
        }

        private StringContent GetUserFormContent(IBankService.NewUser request)
        {
            var formContent = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("username", request.username), 
                new KeyValuePair<string, string>("password", request.password),
                new KeyValuePair<string, string>("password-repeat", request.password), 
                new KeyValuePair<string, string>("firstname", request.firstname),
                new KeyValuePair<string, string>("lastname", request.lastname),
                new KeyValuePair<string, string>("birthday", request.birthday.ToShortDateString()),
                new KeyValuePair<string, string>("timezone", request.timezone),
                new KeyValuePair<string, string>("address", request.address),
                new KeyValuePair<string, string>("state", request.state),
                new KeyValuePair<string, string>("zip", request.zip),
                new KeyValuePair<string, string>("ssn", request.ssn)
            };

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < formContent.Count; i++)
            {
                var kv = formContent[i];
                sb.Append($"{kv.Key}={kv.Value}");
                
                if (i != (formContent.Count - 1))
                    sb.Append("&");
            }

            // TODO: test this with FormUrlEncodedContent should be a better fit?
            StringContent content = new StringContent(sb.ToString(), UnicodeEncoding.UTF8, 
                "application/x-www-form-urlencoded");

            return content;
        }
    }
}