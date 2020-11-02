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
            const string JWT_SECRET_NAME = "Jwt:PublicKey";
            string secret = _configuration[JWT_SECRET_NAME];

            if (string.IsNullOrEmpty(secret))
                throw new ApplicationException($"Missing JWT Key: {JWT_SECRET_NAME}");

            byte[] bytes = Convert.FromBase64String(secret);
            string key = Encoding.UTF8.GetString(bytes);
            key = StripTags(key);

            RSA rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(
                source: Convert.FromBase64String(key),
                bytesRead: out int _
            );
            
            return new RsaSecurityKey(rsa);
        }

        private SecurityKey GetJwtPrivateKey()
        {
            const string JWT_PRIVATE_KEY = "Jwt:PrivateKey";
            string secret = _configuration[JWT_PRIVATE_KEY];

            byte[] bytes = Convert.FromBase64String(secret);
            string key = Encoding.UTF8.GetString(bytes);
            key = StripTags(key);

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