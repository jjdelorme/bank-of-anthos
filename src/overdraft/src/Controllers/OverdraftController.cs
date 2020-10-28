using System;
using System.Security.Claims;
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
            var bearer = this.Request.Headers["Authorization"][0];
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                foreach(var claim in identity.Claims)
                    _logger.Log(LogLevel.Debug, $"{claim.Type}: {claim.Value}");
            }

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
    }
}
