using System;
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
            return "ACCOUNT_" + request.AccountNum;
        }

        [HttpPost("/credit")]
        public string Credit()
        {
            return "credited";
        }

        [HttpPost("/debit")]
        public string Debit()
        {
            return "debited";
        }
    }
}
