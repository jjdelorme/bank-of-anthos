using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Google.Cloud.Firestore;

namespace Anthos.Samples.BankOfAnthos.Overdraft
{
    /// <summary>
    /// Implementation of repository for Firestore.
    /// </summary>
    public class FirestoreOverdraftRepository : IOverdraftRepository
    {
        private readonly string _collectionName;
        private readonly FirestoreDb _db;
        
        public FirestoreOverdraftRepository(IConfiguration configuration)
        {
            _collectionName = configuration["FIRESTORE_COLLECTION"];
            string projectId = configuration["GOOGLE_PROJECT_ID"];
            _db = FirestoreDb.Create(projectId);
        }

        public Task AddAsync(OverdraftAccount account)
        {
            return _db.Collection(_collectionName).AddAsync(account);
        }

        public async Task<OverdraftAccount> GetAsync(string accountNum)
        {
            var documentRef = _db.Collection(_collectionName).Document(accountNum);
            var snapshot = await documentRef.GetSnapshotAsync();

            if (snapshot != null)
                return snapshot.ConvertTo<OverdraftAccount>();
            else
                throw new ApplicationException($"Unable to find overdraft account: {accountNum}");
        }
    }
}