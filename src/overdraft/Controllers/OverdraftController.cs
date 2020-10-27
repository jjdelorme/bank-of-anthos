using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Anthos.Samples.BankOfAnthos.Overdraft
{
    [ApiController]
    public class OverdraftController : ControllerBase
    {
        private readonly ILogger<OverdraftController> _logger;

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

        [HttpPost("/create")]
        public string Create()
        {
            return "ACCOUNT_XXXX";
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
