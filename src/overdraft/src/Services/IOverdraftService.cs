using System;
using System.Net.Http;

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
        long CreateOverdraftAccount(OverdraftRequest request);
    }
}