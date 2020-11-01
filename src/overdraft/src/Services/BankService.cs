using System;
using System.Net.Http;
using System.Linq;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Anthos.Samples.BankOfAnthos.Overdraft
{
    /// <summary>
    /// Wraps REST API calls to the BankOfAnthos services.
    /// </summary>
    public class BankService : IBankService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<BankService> _logger;

        public BankService(IConfiguration configuration, ILogger<BankService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public void AddTransaction(string bearerToken, IBankService.Transaction transaction)
        {
        }

        public long GetBalance(string bearerToken, string accountNum)
        {
            string balancesApiAddress = GetApiAddress("BALANCES_API_ADDR");
            string url = $"{balancesApiAddress}/balances/{accountNum}";
            string response = null;
            using(var httpClient = new HttpClient()) {
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {bearerToken}");
                response = httpClient.GetStringAsync(new Uri(url)).Result;
            }

            return long.Parse(response);
        }

        /// <summary>
        /// Creates a new user and returns the new Account Number.
        /// </summary>
        /// <returns>
        /// AccountNum
        /// </returns>
        public string CreateUser(IBankService.NewUser request)
        {
            string apiAddress = GetApiAddress("USERSERVICE_API_ADDR");
            UserService user = new UserService(apiAddress);

            user.CreateUser(request);     
            _logger.Log(LogLevel.Information, $"Account {request.username} created.");

            string token = user.Login(request.username, request.password);
            string accountNum = GetAccountFromToken(token);

            return accountNum;
        }

        private string GetAccountFromToken(string token)
        {
            const string JWT_ACCOUNT_KEY = "acct";
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                tokenHandler.ValidateToken(token, JwtHelper.GetJwtValidationParameters(_configuration),
                    out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var accountNum = jwtToken.Claims.First(x => x.Type == JWT_ACCOUNT_KEY).Value;

                // return account id from JWT token if validation successful
                return accountNum;
            }
            catch
            {
                _logger.Log(LogLevel.Error, "Token validation failed.");
                throw new ApplicationException("Token validation failed.");
            }
        }

        /// <summary>
        /// Returns the API address from configurationfor the service, i.e. BALANCES_API_ADDR for balances.
        /// </summary>
        private string GetApiAddress(string key)
        {
            const string SERVICE_API_SECTION = "ServiceApi";
            string api = _configuration[$"{SERVICE_API_SECTION}:{key}"];
            return "http://" + api;
        }        
    }
}