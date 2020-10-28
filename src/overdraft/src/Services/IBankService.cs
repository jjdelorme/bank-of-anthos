using System;
using System.Net.Http;

namespace Anthos.Samples.BankOfAnthos.Overdraft
{
    /// <summary>
    /// Wraps REST API calls to the BankOfAnthos services.
    /// </summary>
    public interface IBankService
    {
        void AddTransaction(string bearerToken, BankService.Transaction transaction);
        long GetBalance(string bearerToken, string accountNum);
    }
}