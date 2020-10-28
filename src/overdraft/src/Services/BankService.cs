using System;
using System.Net.Http;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Anthos.Samples.BankOfAnthos.Overdraft
{
    /// <summary>
    /// Wraps REST API calls to the BankOfAnthos services.
    /// </summary>
    public class BankService : IBankService
    {
        public record Transaction(string FromAccountNum, string FromRoutingNum, string ToAccountNum, string ToRoutingNum, int Amount, DateTime Timestamp);
        private readonly IConfiguration _configuration;

        public BankService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void AddTransaction(string bearerToken, Transaction transaction)
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

        /// <sumary>
        /// Returns the configuration value for the service, i.e.ServiceApi:BALANCES_API_ADDR for balances.
        /// </summary>
        private string GetApiAddress(string key)
        {
            const string SERVICE_API_SECTION = "ServiceApi";
            return _configuration[$"{SERVICE_API_SECTION}:{key}"];
        }
    }
}