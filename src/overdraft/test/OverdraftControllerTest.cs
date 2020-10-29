using System;
using Anthos.Samples.BankOfAnthos.Overdraft;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Anthos.Samples.BankOfAnthos.Overdraft.Tests
{
    public class OverdraftControllerTest
    {
        private readonly OverdraftController _controller; 

        public OverdraftControllerTest()
        {
            var loggerMock = new Mock<ILogger<OverdraftController>>();

            var configMock = new Mock<IConfiguration>();
            
            var bankServiceMock = new Mock<IBankService>();
            bankServiceMock.Setup(s => s.GetBalance(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(1000);

            _controller = new OverdraftController(loggerMock.Object, configMock.Object, bankServiceMock.Object);
        }
     
        [Fact]
        public void Version()
        {
            var version = typeof(OverdraftController).Assembly.GetName().Version;
            Assert.Equal(version, _controller.Version());
        }

        [Fact]
        public void Ready()
        {
            Assert.True(_controller.Ready() is Microsoft.AspNetCore.Mvc.OkObjectResult);
        }

        [Fact]
        public void Create()
        {
            // Dummy data for now.
            var request = new OverdraftController.OverdraftRequest("9999", 500);

            Assert.True(_controller.Create(request) == 
                "ACCOUNT_9999", "Should return account id");
        }   
    }
}
