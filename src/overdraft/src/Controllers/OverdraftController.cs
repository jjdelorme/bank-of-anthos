using System;
using System.Security.Claims;
using System.Threading.Tasks;
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
        public async Task<IActionResult> Create(Request request)
        {
            if (!ValidateAccount(request.AccountNum))
                return new UnauthorizedResult();

            try
            {
                long amount = await _overdraft.CreateOverdraftAccountAsync(request);
                return Ok(amount);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        /// <summary>
        /// Credits the overdraft account, increasing the amount the customer owes.
        /// </summary>        
        [Authorize]
        [HttpPost("/credit")]
        public async Task<IActionResult> Credit([FromBody]long amount)
        {
            string accountNum = GetAccountNumber();
            try
            {
                await _overdraft.CreditAsync(accountNum, amount);
                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        /// <summary>
        /// Debits the overdraft account, decreasing (of paying off) the amount the customer owes.
        /// </summary>        
        [Authorize]
        [HttpPost("/debit")]
        public async Task<IActionResult> Debit([FromBody]long amount)
        {
            string accountNum = GetAccountNumber();
            try
            {
                await _overdraft.DebitAsync(accountNum, amount);
                return Ok();
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
            }
        }

        /// <summary>
        /// Get available overdraft balance.
        /// </summary>        
        [Authorize]
        [HttpGet("/balance/{accountNum}")]
        public async Task<ActionResult<long>> GetBalance(string accountNum)
        {
            if (!ValidateAccount(accountNum))
                return new UnauthorizedResult();

            try
            {
                long balance = await _overdraft.GetOverdraftBalanceAsync(accountNum);
                return Ok(balance);
            }
            catch (Exception e)
            {
                return StatusCode(500, e.Message);
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
