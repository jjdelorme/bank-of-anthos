using System;

namespace Anthos.Samples.BankOfAnthos.Overdraft
{
    /// <summary>
    /// Wraps REST API calls to the BankOfAnthos services.
    /// </summary>
    public interface IBankService
    {
        public record Transaction(string FromAccountNum, string FromRoutingNum, string ToAccountNum, string ToRoutingNum, long Amount, DateTime Timestamp);

        public record NewUser(string username,
            string password,
            string firstname,
            string lastname,
            DateTime birthday,
            string timezone,
            string address,
            string state,
            string zip,
            string ssn);

        void AddTransaction(string bearerToken, Transaction transaction);
        long GetBalance(string bearerToken, string accountNum);
        string CreateUser(NewUser request);
    }
}