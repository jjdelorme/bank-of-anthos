using System;
using System.Net.Http;

namespace Anthos.Samples.BankOfAnthos.Overdraft
{
    /// <summary>
    /// Wraps REST API calls to the BankOfAnthos services.
    /// </summary>
    public class BankService
    {
        private string _bearerToken;

        public BankService(string bearerToken)
        {
            _bearerToken = bearerToken;
        }

        public void AddTransaction(OverdraftController.Transaction transaction)
        {
        }

        public long GetBalance(string balancesApiUri, string accountNum)
        {
            string url = $"{balancesApiUri}/balances/{accountNum}";
            string response = null;
            using(var httpClient = new HttpClient()) {
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_bearerToken}");
                response = httpClient.GetStringAsync(new Uri(url)).Result;
            }

            return long.Parse(response);
        }
    }
}