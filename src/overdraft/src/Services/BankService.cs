using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
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
            string transactionsApiAddress = GetApiAddress("TRANSACTIONS_API_ADDR");
            string url = $"{transactionsApiAddress}/transactions";           

            JsonContent content = JsonContent.Create<IBankService.Transaction>(transaction);

            var httpClient = new HttpClient();
            var response = httpClient.PostAsync(url, content);
            var contents = response.Result.Content.ReadAsStringAsync();

            if (response.Result.StatusCode != HttpStatusCode.Created)
                throw new ApplicationException($"Unable to submit transaction.");
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

        /// <summary>
        /// Creates a new user and returns the new Account Number.
        /// </summary>
        /// <returns>
        /// AccountNum
        /// </returns>
        public string CreateUser(IBankService.NewUser request)
        {
            string apiAddress = GetApiAddress("USERSERVICE_API_ADDR");
            UserService user = new UserService(apiAddress);

            user.CreateUser(request);     
            _logger.Log(LogLevel.Information, $"Account {request.username} created.");

            string token = user.Login(request.username, request.password);
            _logger.Log(LogLevel.Debug, $"Logged in as {request.username}.");
            
            JwtHelper jwtHelper = new JwtHelper(_configuration);            
            string accountNum = jwtHelper.GetAccountFromToken(token);

            if (accountNum == null)
                throw new ApplicationException($"Unable to get account number from token for {request.username}");

            return accountNum;
        }

        /// <summary>
        /// Returns the API address from configurationfor the service, i.e. BALANCES_API_ADDR for balances.
        /// </summary>
        private string GetApiAddress(string key)
        {
            const string SERVICE_API_SECTION = "ServiceApi";
            string api = _configuration[$"{SERVICE_API_SECTION}:{key}"];
            return "http://" + api;
        }        
    }
}