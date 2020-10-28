using System;
using System.Net.Http;

namespace Anthos.Samples.BankOfAnthos.Overdraft
{
    /// <summary>
    /// Wraps REST API calls to the BankOfAnthos services.
    /// </summary>
    public class BankService
    {
        public record Transaction(string FromAccountNum, string FromRoutingNum, string ToAccountNum, string ToRoutingNum, int Amount, DateTime Timestamp);

        private string _bearerToken;

        public BankService(string bearerToken)
        {
            _bearerToken = bearerToken;
        }

        public void AddTransaction(Transaction transaction)
        {
        }

        public long GetBalance(string balancesApiAddress, string accountNum)
        {
            string url = $"{balancesApiAddress}/balances/{accountNum}";
            string response = null;
            using(var httpClient = new HttpClient()) {
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_bearerToken}");
                response = httpClient.GetStringAsync(new Uri(url)).Result;
            }

            return long.Parse(response);
        }
    }
}