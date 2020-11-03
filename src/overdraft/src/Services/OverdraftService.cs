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

        public OverdraftService(IConfiguration configuration, ILogger<OverdraftService> logger, 
            IBankService bankService, IOverdraftRepository repository)
        {
            _logger = logger;
            _bankService = bankService;
            _configuration = configuration;
            _repository = repository;
            _jwtHelper = new JwtHelper(_configuration);
        }

        public long CreateOverdraftAccount(IOverdraftService.OverdraftRequest request)
        {
            OverdraftAccount account = new OverdraftAccount();
            account.AccountNum = request.AccountNum;
            account.Amount = GetApprovalAmount(request);
            
            if (account.Amount > 0)
            {
                account.OverdraftAccountNum = CreateUser(request);
                DepositOverdraft(account.OverdraftAccountNum, account.Amount);
                _repository.AddAsync(account).Wait();
            }
            else
            {
                _logger.Log(LogLevel.Information, $"Account {request.AccountNum} not approved for overdraft.");
            }

            return account.Amount;
        }

        public async Task<long> GetOverdraftBalanceAsync(string accountNum)
        {
            OverdraftAccount account = await _repository.GetAsync(accountNum);
            
            string bearerToken = _jwtHelper.GenerateJwtToken(account.OverdraftAccountNum);
            long balance = _bankService.GetBalance(bearerToken, account.OverdraftAccountNum);
            
            return balance;
        }

        protected virtual long GetApprovalAmount(IOverdraftService.OverdraftRequest request)
        {
            // TODO: Refactor this into configuration.
            const int minMonthsInJob = 6;
            const int minIncome = 50000; // $500 per month (note decimals are not used)
            const int percentMonthly = 10;

            long approvalAmount = 0;
            
            if (request.MonthsInJob >= minMonthsInJob && 
                request.MonthlyIncome > minIncome)
            {
                approvalAmount = request.MonthlyIncome / percentMonthly;
                approvalAmount = (long)(Math.Round(approvalAmount / 100.0) * 100);
                
                _logger.Log(LogLevel.Information, $"Approved {0:0.##, approvalAmount/100} for {request.AccountNum}");
            }

            return approvalAmount;
        }

        private string CreateUser(IOverdraftService.OverdraftRequest request)
        {
            const string userPrefix = "OD_";
            string username = userPrefix + request.Username;
            string password = "overdraft";

            int maxLength = username.Length >=14 ? 14 : username.Length-1;

            var user = new IBankService.NewUser(
                username.Substring(0, maxLength),
                password, 
                userPrefix, 
                request.Fullname,
                DateTime.MinValue,
                "GMT",
                "No Address",
                "NY",
                "00000",
                "000-00-0000");
            
            // Create the user.
            string accountNum = _bankService.CreateUser(user);

            return accountNum;
        }

        private void DepositOverdraft(string overdraftAccountNum, long amount)
        {
            const string OVERDRAFT_ROUTING_NUM = "883745001";
            const string OVERDRAFT_ACCOUNT_NUM = "1099990101";
            
            string localRoutingNumber = _configuration["LOCAL_ROUTING_NUMBER"];
            string bearerToken = _jwtHelper.GenerateJwtToken(overdraftAccountNum);

            IBankService.Transaction transaction = new IBankService.Transaction(Guid.NewGuid(),
                OVERDRAFT_ACCOUNT_NUM, OVERDRAFT_ROUTING_NUM, overdraftAccountNum, 
                localRoutingNumber, amount, DateTime.UtcNow
            );
 
            _bankService.AddTransaction(bearerToken, transaction);
        }
    }
}