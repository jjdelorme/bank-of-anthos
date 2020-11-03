using System;
using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Request = Anthos.Samples.BankOfAnthos.Overdraft.IOverdraftService.OverdraftRequest;

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
        private readonly IOverdraftService _overdraft;

        public OverdraftController(ILogger<OverdraftController> logger, 
            IConfiguration configuration, IOverdraftService overdraft)
        {
            _logger = logger;
            _configuration = configuration;
            _overdraft = overdraft;
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
        /// Processes overdraft application and creates account if approved.
        /// </summary>
        /// <returns>
        /// Overdraft limit if approved, 0 if not.
        /// </returns>
        [Authorize]
        [HttpPost("/create")]
        public IActionResult Create(Request request)
        {
            if (!ValidateAccount(request))
                return new UnauthorizedResult();

            long amount = _overdraft.CreateOverdraftAccount(request);

            return Ok(amount);
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

        /// <summary>
        /// Get available overdraft balance.
        /// </summary>        
        [Authorize]
        [HttpGet("/balance")]
        public long GetBalance()
        {
            string accountNum = GetAccountNumber();
            var task = _overdraft.GetOverdraftBalanceAsync(accountNum);
            task.Wait();
            return task.Result;
        }

        private bool ValidateAccount(Request request)
        {
            string accountNumber = GetAccountNumber();
            if (request.AccountNum != accountNumber)
            {
                _logger.Log(LogLevel.Warning, "Account does not match claim.");
                return false;
            }
            else
            {
                return true;
            }
        }

        private string GetBearerToken()
        {
            if (this.Request?.Headers == null || 
                this.Request.Headers["Authorization"].Count == 0)
            {
                _logger.Log(LogLevel.Warning, "No authorization header.");
                return null;
            }
            
            string bearerToken = this.Request.Headers["Authorization"][0];
            bearerToken = bearerToken.Replace("Bearer", "").TrimStart();
            
            return bearerToken;
        }

        private string GetAccountNumber()
        {
            const string claimType = "acct";
            string accountNum = null;

            var identity = HttpContext?.User.Identity as ClaimsIdentity;
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
    }
}
