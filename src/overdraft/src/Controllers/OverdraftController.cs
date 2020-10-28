using System;
using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

namespace Anthos.Samples.BankOfAnthos.Overdraft
{
    /// <summary>
    /// Implements Web API for Overdraft protection service.
    /// </summary>
    [ApiController]
    public class OverdraftController : ControllerBase
    {
        private readonly ILogger<OverdraftController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IBankService _bankService;

        public record OverdraftRequest(string AccountNum, int Amount);

        public OverdraftController(ILogger<OverdraftController> logger, IConfiguration configuration, IBankService bankService)
        {
            _logger = logger;
            _configuration = configuration;
            _bankService = bankService;
        }

        [HttpGet("/version")]
        public Version Version()
        {
            return typeof(OverdraftController).Assembly.GetName().Version;
        }

        [HttpGet("/ready")]
        public IActionResult Ready()
        {
            return Ok("ok");
        }

        /// <summary>
        /// Creates an overdraft account.
        /// </summary>
        [Authorize]
        [HttpPost("/create")]
        public string Create(OverdraftRequest request)
        {
            string accountNumber = GetAccountNumber();
            long balance = GetAccountBalance(accountNumber);
            return "ACCOUNT_" + request.AccountNum;
        }

        /// <summary>
        /// Credits the overdraft account, increasing the amount the customer owes.
        /// </summary>        
        [Authorize]
        [HttpPost("/credit")]
        public string Credit()
        {
            return "credited";
        }

        /// <summary>
        /// Debits the overdraft account, decreasing (of paying off) the amount the customer owes.
        /// </summary>        
        [Authorize]
        [HttpPost("/debit")]
        public string Debit()
        {
            return "debited";
        }

        private string GetBearerToken()
        {
            if (this.Request.Headers == null || this.Request.Headers["Authorization"].Count == 0)
                throw new ApplicationException("No authorization header.");
            
            string bearerToken = this.Request.Headers["Authorization"][0];
            bearerToken = bearerToken.Replace("Bearer", "").TrimStart();
            
            return bearerToken;
        }

        private string GetAccountNumber()
        {
            const string claimType = "acct";
            string accountNum = null;

            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var accountClaim = identity.Claims.Where(c => c.Type == claimType).First();
                if (accountClaim != null)
                {
                    accountNum = accountClaim.Value;
                    _logger.Log(LogLevel.Debug, $"Got account claim: {accountNum}");
                }
            }
            
            return accountNum;
        }

        private long GetAccountBalance(string accountNumber)
        {
            const string BALANCES_API_ADDR = "ServiceApi:BALANCES_API_ADDR";
            
            string bearerToken = GetBearerToken();
            string balancesApiAddress = _configuration[BALANCES_API_ADDR];
            return _bankService.GetBalance(bearerToken, accountNumber);
        }
    }
}
