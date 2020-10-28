using System;
using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace Anthos.Samples.BankOfAnthos.Overdraft
{
    [ApiController]
    public class OverdraftController : ControllerBase
    {
        private readonly ILogger<OverdraftController> _logger;
        public record OverdraftRequest(string AccountNum, int Amount);
        public record Transaction(string FromAccountNum, string FromRoutingNum, string ToAccountNum, string ToRoutingNum, int Amount, DateTime Timestamp);
        public OverdraftController(ILogger<OverdraftController> logger)
        {
            _logger = logger;
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

        [Authorize]
        [HttpPost("/create")]
        public string Create(OverdraftRequest request)
        {
            string accountNumber = GetAccountNumber();
            return "ACCOUNT_" + request.AccountNum;
        }

        [Authorize]
        [HttpPost("/credit")]
        public string Credit()
        {
            return "credited";
        }

        [Authorize]
        [HttpPost("/debit")]
        public string Debit()
        {
            return "debited";
        }

        private string GetAccountNumber()
        {
            const string claimType = "acct";
            if (this.Request.Headers == null || this.Request.Headers["Authorization"].Count == 0)
            {
                _logger.Log(LogLevel.Debug, "No authorization header.");
                return null;
            }

            string accountNum = null;
            var bearer = this.Request.Headers["Authorization"][0];
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var accountClaim = identity.Claims.Where(c => c.Type == claimType).First();
                if (accountClaim != null)
                {
                    accountNum = accountClaim.Value;
                    _logger.Log(LogLevel.Debug, $"Got acount claim: {accountNum}");
                }
            }
            
            return accountNum;
        }
    }
}
