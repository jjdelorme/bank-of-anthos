using System;
using System.Text;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace Anthos.Samples.BankOfAnthos.Overdraft
{
    public class JwtHelper
    {
        const string JWT_ACCOUNT_KEY = "acct";

        private readonly IConfiguration _configuration;

        public JwtHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public TokenValidationParameters GetJwtValidationParameters()
        {
            return new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = GetJwtPublicKey(),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
        }

        public string GenerateJwtToken(string accountNum)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim(JWT_ACCOUNT_KEY, accountNum) }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(GetJwtPrivateKey(), SecurityAlgorithms.RsaSha256)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }        

        public string GetAccountFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            tokenHandler.ValidateToken(token, GetJwtValidationParameters(),
                out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var accountNum = jwtToken.Claims.First(x => x.Type == JWT_ACCOUNT_KEY).Value;

            // return account id from JWT token if validation successful
            return accountNum;
        }

        private SecurityKey GetJwtPublicKey()
        {
            string publicKeyPath = _configuration["PUB_KEY_PATH"];
            string secret = System.IO.File.ReadAllText(publicKeyPath);

            if (string.IsNullOrEmpty(secret))
                throw new ApplicationException($"Missing RSA Public Key in {publicKeyPath}");

            string key = StripTags(secret);

            RSA rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(
                source: Convert.FromBase64String(key),
                bytesRead: out int _
            );
            
            return new RsaSecurityKey(rsa);
        }

        private SecurityKey GetJwtPrivateKey()
        {
            string privateKeyPath = _configuration["PRIV_KEY_PATH"];
            string secret = System.IO.File.ReadAllText(privateKeyPath);

            if (string.IsNullOrEmpty(secret))
                throw new ApplicationException($"Missing RSA Private Key in {privateKeyPath}");

            string key = StripTags(secret);

            RSA rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(
                source: Convert.FromBase64String(key),
                bytesRead: out int _
            );
            
            return new RsaSecurityKey(rsa);
        }

        private string StripTags(string key)
        {
            if (key.Contains("PUBLIC KEY"))
            {
                key = key.Replace("-----BEGIN PUBLIC KEY-----", "");
                key = key.Replace("-----END PUBLIC KEY-----", "");
            }
            else
            {
                key = key.Replace("-----BEGIN RSA PRIVATE KEY-----", "");
                key = key.Replace("-----END RSA PRIVATE KEY-----", "");
            }
            key = key.Replace("\r", "");
            key = key.Replace(Environment.NewLine, "");

            return key;
        }
    }
}