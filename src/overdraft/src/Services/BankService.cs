using System;
using System.Text;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Anthos.Samples.BankOfAnthos.Overdraft
{
    /// <summary>
    /// Wraps REST API calls to the BankOfAnthos services.
    /// </summary>
    public class BankService : IBankService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<BankService> _logger;

        public BankService(IConfiguration configuration, ILogger<BankService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public void AddTransaction(string bearerToken, IBankService.Transaction transaction)
        {
        }

        public long GetBalance(string bearerToken, string accountNum)
        {
            string balancesApiAddress = GetApiAddress("BALANCES_API_ADDR");
            string url = $"{balancesApiAddress}/balances/{accountNum}";
            string response = null;
            using(var httpClient = new HttpClient()) {
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearerToken}");
                response = httpClient.GetStringAsync(new Uri(url)).Result;
            }

            return long.Parse(response);
        }

        public string GetAccountNum(string username, string password)
        {
            string apiAddress = GetApiAddress("USERSERVICE_API_ADDR");
            string url = $"{apiAddress}/login";

            UriBuilder uri = new UriBuilder(url);
            uri.Query = $"username={username}&password={password}";

            var httpClient = new HttpClient();
            var response = httpClient.GetAsync(uri.Uri);
            //response.Result

            return "";
        }

        /// <summary>
        /// Creates a new user and returns the new Account Number.
        /// </summary>
        /// <returns>
        /// AccountNum
        /// </returns>
        public string CreateUser(IBankService.NewUser request)
        {
            string accountNum = "";
            string apiAddress = GetApiAddress("USERSERVICE_API_ADDR");
            string url = $"{apiAddress}/users";

            var formContent = GetFormContent(request);

            Task.Run(async () => {
                var httpClient = new HttpClient();
                var response = await httpClient.PostAsync(url, formContent);
                var contents = await response.Content.ReadAsStringAsync();

                var rawMessage = await response.RequestMessage.Content.ReadAsStringAsync();

                if (response.StatusCode == HttpStatusCode.Created)
                {
                    _logger.Log(LogLevel.Information, $"Account {request.username} created.");
                    accountNum = GetAccountNum(request.username, request.password);
                }
            }).Wait();

            return accountNum;
        }

        /// <sumary>
        /// Returns the API address from configurationfor the service, i.e. BALANCES_API_ADDR for balances.
        /// </summary>
        private string GetApiAddress(string key)
        {
            const string SERVICE_API_SECTION = "ServiceApi";
            string api = _configuration[$"{SERVICE_API_SECTION}:{key}"];
            return "http://" + api;
        }

        private StringContent GetFormContent(IBankService.NewUser request)
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