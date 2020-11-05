using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
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
        private readonly HttpClient _httpClient;

        public BankService(IConfiguration configuration, ILogger<BankService> logger, HttpClient httpClient)
        {
            _configuration = configuration;
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task AddTransactionAsync(string bearerToken, IBankService.Transaction transaction)
        {
            string transactionsApiAddress = GetApiAddress("TRANSACTIONS_API_ADDR");
            string url = $"{transactionsApiAddress}/transactions";           

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearerToken}");
            var response = await _httpClient.PostAsJsonAsync(url, transaction);
            var content = await response.Content.ReadAsStringAsync();
            
            _logger.Log(LogLevel.Debug, $"Transaction response: {content}");

            if (!response.IsSuccessStatusCode)
                throw new ApplicationException($"Unable to submit transaction: {content}");
        }

        public async Task<long> GetBalanceAsync(string bearerToken, string accountNum)
        {
            string balancesApiAddress = GetApiAddress("BALANCES_API_ADDR");
            string url = $"{balancesApiAddress}/balances/{accountNum}";

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearerToken}");
            var response = await _httpClient.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new ApplicationException($"Unable to get balance: {content}");            

            return long.Parse(content);
        }

        /// <summary>
        /// Creates a new user and returns the bearer token for the user.
        /// </summary>
        public async Task<string> CreateUserAsync(IBankService.NewUser request)
        {
            UserService user = new UserService(_httpClient, GetApiAddress("USERSERVICE_API_ADDR"));

            await user.CreateUserAsync(request); 
            _logger.Log(LogLevel.Information, $"Account {request.username} created.");

            string token = await user.LoginAsync(request.username, request.password);
            _logger.Log(LogLevel.Debug, $"Logged in as {request.username}.");
            
            if (token == null)
                throw new ApplicationException($"Unable to get token for {request.username}");

            return token;
        }

        /// <summary>
        /// Returns the API address from configurationfor the service, i.e. BALANCES_API_ADDR for balances.
        /// </summary>
        private string GetApiAddress(string key)
        {
            string api = _configuration[key];
            return "http://" + api;
        }        
    }
}