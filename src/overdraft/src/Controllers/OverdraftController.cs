using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Request = Anthos.Samples.BankOfAnthos.Overdraft.IOverdraftService.OverdraftRequest;
using Google.Cloud.Diagnostics.Common;

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
        public ActionResult<string> Version()
        {
            string version = _configuration["VERSION"];
            return Ok(version);
        }

        [HttpGet("/ready")]
        public ActionResult<string> Ready()
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
        public async Task<IActionResult> Create([FromServices] IManagedTracer tracer, Request request)
        {
            using (tracer.StartSpan(nameof(Create)))
            {
                _logger.LogDebug($"TraceID: {tracer.GetCurrentTraceId()}");

                if (!ValidateAccount(request.AccountNum))
                    return new UnauthorizedResult();

                long amount = await _overdraft.CreateOverdraftAccountAsync(request);
                return Ok(amount);
            }
        }

        /// <summary>
        /// Credits the overdraft account, increasing the amount the customer owes.
        /// </summary>        
        [Authorize]
        [HttpPost("/credit")]
        public async Task<IActionResult> Credit([FromServices] IManagedTracer tracer, [FromBody]long amount)
        {
            using (tracer.StartSpan(nameof(Credit)))
            {            
                string accountNum = GetAccountNumber();
                await _overdraft.CreditAsync(accountNum, amount);
                return Ok();
            }
        }

        /// <summary>
        /// Debits the overdraft account, decreasing (of paying off) the amount the customer owes.
        /// </summary>        
        [Authorize]
        [HttpPost("/debit")]
        public async Task<IActionResult> Debit([FromServices] IManagedTracer tracer, [FromBody]long amount)
        {
            using (tracer.StartSpan(nameof(Debit)))
            {            
                string accountNum = GetAccountNumber();
                await _overdraft.DebitAsync(accountNum, amount);
                return Ok();
            }
        }

        /// <summary>
        /// Get available overdraft balance.
        /// </summary>        
        [Authorize]
        [HttpGet("/balance/{accountNum}")]
        public async Task<ActionResult<long>> GetBalance([FromServices] IManagedTracer tracer, string accountNum)
        {
            using (tracer.StartSpan(nameof(GetBalance)))
            {                        
                if (!ValidateAccount(accountNum))
                    return new UnauthorizedResult();

                long balance = await _overdraft.GetOverdraftBalanceAsync(accountNum);
                return Ok(balance);
            }
        }

        private bool ValidateAccount(string accountNumber)
        {
            string jwtAccountNumber = GetAccountNumber();
            if (jwtAccountNumber != accountNumber)
            {
                _logger.Log(LogLevel.Warning, $"Account {accountNumber} does not match claim acct:{jwtAccountNumber}.");
                return false;
            }
            else
            {
                return true;
            }
        }

        private string GetAccountNumber()
        {
            return JwtHelper.GetAccountFromHttpContext(this.HttpContext);
        }
    }
}
