using System;
using System.Text;
using System.Text.Json;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;

namespace Anthos.Samples.BankOfAnthos.Overdraft
{
    /// <summary>
    /// Wraps calls to the bank user service.
    /// </summary>
    public class UserService
    {
        private readonly string _apiAddress;

        public UserService(string baseApiAddress)
        {
            _apiAddress = baseApiAddress;
        }

        public void CreateUser(IBankService.NewUser request)
        {
            string url = $"{_apiAddress}/users";

            var formContent = GetUserFormContent(request);

            var httpClient = new HttpClient();
            var response = httpClient.PostAsync(url, formContent);
            response.Wait();
            var contents = response.Result.Content.ReadAsStringAsync();

            if (response.Result.StatusCode != HttpStatusCode.Created)
                throw new ApplicationException($"Unable to create user: {contents}");
        }

        public string Login(string username, string password)
        {
            string token = null;
            string url = $"{_apiAddress}/login";

            UriBuilder uriBuilder = new UriBuilder(url);
            uriBuilder.Query = $"username={username}&password={password}";
            Uri uri = uriBuilder.Uri;
          
            HttpClient client = new HttpClient();
            var response = client.GetAsync(uri);
            response.Wait();
            if (response.Result.StatusCode != HttpStatusCode.OK)
                throw new ApplicationException($"Unable to get token {token}");

            var doc = JsonDocument.Parse(response.Result.Content.ReadAsStream());
            token = doc.RootElement.GetProperty("token").GetString();

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

            StringContent content = new StringContent(sb.ToString(), UnicodeEncoding.UTF8, 
                "application/x-www-form-urlencoded");

            return content;
        }
    }
}