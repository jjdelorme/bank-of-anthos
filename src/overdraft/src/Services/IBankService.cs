using System;
using System.Threading.Tasks;

namespace Anthos.Samples.BankOfAnthos.Overdraft
{
    /// <summary>
    /// Wraps REST API calls to the BankOfAnthos services.
    /// </summary>
    public interface IBankService
    {
        public record Transaction(Guid Uuid, string FromAccountNum, string FromRoutingNum, string ToAccountNum, string ToRoutingNum, long Amount, DateTime Timestamp);

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

        Task AddTransactionAsync(string bearerToken, Transaction transaction);
        Task<long> GetBalanceAsync(string bearerToken, string accountNum);
        Task<string> CreateUserAsync(NewUser request);
    }
}