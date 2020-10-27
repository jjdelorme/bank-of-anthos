using System;
using Anthos.Samples.BankOfAnthos.Overdraft;
using Xunit;

namespace Anthos.Samples.BankOfAnthos.Overdraft.Tests
{
    public class OverdraftControllerTest
    {
        static OverdraftController controller = new OverdraftController(null);
     
        [Fact]
        public void Version()
        {
            var version = typeof(OverdraftController).Assembly.GetName().Version;
            Assert.Equal(version, controller.Version());
        }

        [Fact]
        public void Ready()
        {
            Assert.True(controller.Ready() is Microsoft.AspNetCore.Mvc.OkObjectResult);
        }

        [Fact]
        public void Create()
        {
            // Dummy data for now.
            Assert.True(controller.Create() == "ACCOUNT_XXXX", "Should return account id");
        }   
    }
}
