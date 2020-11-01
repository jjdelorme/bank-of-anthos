using System;
using System.Text;
using System.Text.Json;
using System.Net;
using System.Net.Http;
using System.Linq;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using System.Collections.Generic;
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
            CreateUserInternal(request);           
            string token = Login(request.username, request.password);
            string accountNum = GetAccountFromToken(token);

            return accountNum;
        }

        private void CreateUserInternal(IBankService.NewUser request)
        {
            string apiAddress = GetApiAddress("USERSERVICE_API_ADDR");
            string url = $"{apiAddress}/users";

            var formContent = GetUserFormContent(request);

            var httpClient = new HttpClient();
            var response = httpClient.PostAsync(url, formContent);
            response.Wait();
            var contents = response.Result.Content.ReadAsStringAsync();

            if (response.Result.StatusCode == HttpStatusCode.Created)
                _logger.Log(LogLevel.Information, $"Account {request.username} created.");
            else 
                throw new ApplicationException($"Unable to create user: {contents}");
        }

        private StringContent GetUserFormContent(IBankService.NewUser request)
        {
            var formContent = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("username", request.username), 
                new KeyValuePair<string, string>("password", request.password),
                new KeyValuePair<string, string>("password-repeat", request.password), 
                new KeyValuePair<string, string>("firstname", request.firstname),
                new KeyValuePair<string, string>("lastname", request.lastname),
                new KeyValuePair<string, string>("birthday", request.birthday.ToShortDateString()),
                new KeyValuePair<string, string>("timezone", request.timezone),
                new KeyValuePair<string, string>("address", request.address),
                new KeyValuePair<string, string>("state", request.state),
                new KeyValuePair<string, string>("zip", request.zip),
                new KeyValuePair<string, string>("ssn", request.ssn)
            };

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < formContent.Count; i++)
            {
                var kv = formContent[i];
                sb.Append($"{kv.Key}={kv.Value}");
                
                if (i != (formContent.Count - 1))
                    sb.Append("&");
            }

            StringContent content = new StringContent(sb.ToString(), UnicodeEncoding.UTF8, 
                "application/x-www-form-urlencoded");

            return content;
        }

        private string Login(string username, string password)
        {
            _logger.Log(LogLevel.Debug, $"Logging in as {username}");

            string token = null;
            string apiAddress = GetApiAddress("USERSERVICE_API_ADDR");
            string url = $"{apiAddress}/login";

            UriBuilder uriBuilder = new UriBuilder(url);
            uriBuilder.Query = $"username={username}&password={password}";
            Uri uri = uriBuilder.Uri;
          
            HttpClient client = new HttpClient();
            var response = client.GetAsync(uri);
            response.Wait();
            if (response.Result.StatusCode != HttpStatusCode.OK)
                throw new ApplicationException($"Unable to get token {token}");

            var doc = JsonDocument.Parse(response.Result.Content.ReadAsStream());
            token = doc.RootElement.GetProperty("token").GetString();

            _logger.Log(LogLevel.Debug, $"Received login token: {token}");

            if (token == null)
                throw new ApplicationException("Unable to get token.");

            return token;           
        }

        private string GetAccountFromToken(string token)
        {
            const string JWT_ACCOUNT_KEY = "acct";
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                tokenHandler.ValidateToken(token, Helpers.GetJwtValidationParameters(_configuration),
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