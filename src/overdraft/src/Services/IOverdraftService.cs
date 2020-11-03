using System;
using System.Threading.Tasks;

namespace Anthos.Samples.BankOfAnthos.Overdraft
{
    /// <summary>
    /// Abstracts implementation of overdraft credit approval.
    /// </summary>
    public interface IOverdraftService
    {
        public record OverdraftRequest(string AccountNum, string Username, 
            string Fullname, long MonthlyIncome, int MonthsInJob);

        /// <summary>
        /// Creates an overdraft account if request meets business rules.
        /// </summary>
        /// <returns>
        /// Overdraft amount or 0 if not qualified.
        /// </returns>
        Task<long> CreateOverdraftAccountAsync(OverdraftRequest request);

        /// <summary>
        /// Gets the current overdraft balance.
        /// </summary>
        Task<long> GetOverdraftBalanceAsync(string accountNum);
    }
}