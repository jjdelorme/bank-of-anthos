using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Anthos.Samples.BankOfAnthos.Overdraft
{
    /// <summary>
    /// Abstracts implementation of overdraft credit approval.
    /// </summary>
    public class OverdraftService : IOverdraftService
    {
        private readonly ILogger<OverdraftService> _logger;
        private readonly IBankService _bankService;
        private readonly IConfiguration _configuration;
        private readonly IOverdraftRepository _repository;
        private readonly JwtHelper _jwtHelper;
        private readonly string _localRoutingNum;
            
        public OverdraftService(IConfiguration configuration, ILogger<OverdraftService> logger, 
            IBankService bankService, IOverdraftRepository repository)
        {
            _logger = logger;
            _bankService = bankService;
            _configuration = configuration;
            _repository = repository;
            _jwtHelper = new JwtHelper(_configuration);
            _localRoutingNum = _configuration["LOCAL_ROUTING_NUM"];
        }

        public async Task<long> CreateOverdraftAccountAsync(IOverdraftService.OverdraftRequest request)
        {
            OverdraftAccount account = new OverdraftAccount();
            account.AccountNum = request.AccountNum;
            account.Amount = GetApprovalAmount(request);
            
            if (account.Amount <= 0)
                throw new ApplicationException($"Account {request.AccountNum} not approved for overdraft.");

             string bearerToken = await CreateUserAsync(request);

            JwtHelper jwtHelper = new JwtHelper(_configuration);            
            account.OverdraftAccountNum = jwtHelper.GetAccountFromToken(bearerToken);

            await Task.WhenAll(
                SeedOverdraftAsync(bearerToken, account.OverdraftAccountNum, account.Amount),
                _repository.AddAsync(account)
            );
            
            return account.Amount;
        }

        public async Task<long> GetOverdraftBalanceAsync(string accountNum)
        {
            OverdraftAccount account = await _repository.GetAsync(accountNum);            
            string bearerToken = _jwtHelper.GenerateJwtToken(account.OverdraftAccountNum);
            long balance = await _bankService.GetBalanceAsync(bearerToken, account.OverdraftAccountNum);
            
            return balance;
        }

        /// <summary>
        /// Moves money from overdraft account to customer account.
        /// </summary>
        public async Task CreditAsync(string accountNum, long amount)
        {
            OverdraftAccount account = await _repository.GetAsync(accountNum);   
            string bearerToken = _jwtHelper.GenerateJwtToken(account.OverdraftAccountNum);

            IBankService.Transaction transaction = new IBankService.Transaction(Guid.NewGuid(),
                account.OverdraftAccountNum, _localRoutingNum, accountNum, 
                _localRoutingNum, amount, DateTime.UtcNow
            );
 
            await _bankService.AddTransactionAsync(bearerToken, transaction);
        }

        /// <summary>
        /// Moves money from customer account to overdraft account.
        /// </summary>
        public async Task DebitAsync(string accountNum, long amount)
        {
            OverdraftAccount account = await _repository.GetAsync(accountNum);   
            string bearerToken = _jwtHelper.GenerateJwtToken(accountNum);

            IBankService.Transaction transaction = new IBankService.Transaction(Guid.NewGuid(),
                accountNum, _localRoutingNum, account.OverdraftAccountNum, _localRoutingNum,
                amount, DateTime.UtcNow
            );
            
            await _bankService.AddTransactionAsync(bearerToken, transaction);
        }

        private Task SeedOverdraftAsync(string bearerToken, string accountNum, long amount)
        {
            const string OverdraftFromRoutingNum = "883745001";
            const string OverdraftFromAccountNum = "1099990101";
                        
            IBankService.Transaction transaction = new IBankService.Transaction(Guid.NewGuid(),
                OverdraftFromAccountNum, OverdraftFromRoutingNum, accountNum, 
                _localRoutingNum, amount, DateTime.UtcNow
            );
 
            return _bankService.AddTransactionAsync(bearerToken, transaction);
        }        

        protected virtual long GetApprovalAmount(IOverdraftService.OverdraftRequest request)
        {
            // TODO: Refactor this into configuration.
            const int MinMonthsInJob = 6;
            const int MinIncome = 50000; // $500 per month (note decimals are not used)
            const int PercentMonthly = 10;

            long approvalAmount = 0;
            
            if (request.MonthsInJob >= MinMonthsInJob && 
                request.MonthlyIncome > MinIncome)
            {
                approvalAmount = request.MonthlyIncome / PercentMonthly;
                approvalAmount = (long)(Math.Round(approvalAmount / 100.0) * 100);
                
                _logger.Log(LogLevel.Information, $"Approved {(approvalAmount / 100.0), 0:F2} for {request.AccountNum}");
            }

            return approvalAmount;
        }

        private Task<string> CreateUserAsync(IOverdraftService.OverdraftRequest request)
        {
            const string UserPrefix = "OD_";
            string username = UserPrefix + request.Username;
            string password = "overdraft";

            int maxLength = username.Length >=14 ? 14 : username.Length;

            var user = new IBankService.NewUser(
                username.Substring(0, maxLength),
                password, 
                UserPrefix, 
                request.Fullname,
                DateTime.MinValue,
                "GMT",
                "No Address",
                "NY",
                "00000",
                "000-00-0000");
            
            // Create the user.
            return _bankService.CreateUserAsync(user);
        }
    }
}