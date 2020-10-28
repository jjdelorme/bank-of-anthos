using System;
using Anthos.Samples.BankOfAnthos.Overdraft;
using Xunit;

namespace Anthos.Samples.BankOfAnthos.Overdraft.Tests
{
    public class OverdraftControllerTest
    {
        // TODO: Mock logger & configuration objects here.
        static OverdraftController controller = new OverdraftController(null, null, null);
     
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
            var request = new OverdraftController.OverdraftRequest("9999", 500);

            Assert.True(controller.Create(request) == 
                "ACCOUNT_9999", "Should return account id");
        }   
    }
}
