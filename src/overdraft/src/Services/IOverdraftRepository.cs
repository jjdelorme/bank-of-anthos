using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace Anthos.Samples.BankOfAnthos.Overdraft
{
    public interface IOverdraftRepository
    {
        public Task AddAsync(OverdraftAccount account);
        
        public Task<OverdraftAccount> GetAsync(string accountNum);
    }

    [FirestoreData]
    public class OverdraftAccount
    {
        [FirestoreProperty]
        public string AccountNum { get; set; }
        
        [FirestoreProperty]
        public string OverdraftAccountNum { get; set; }
        
        [FirestoreProperty]
        public long Amount { get; set; }        
    }
}