using System;
using System.Text;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;

namespace Anthos.Samples.BankOfAnthos.Overdraft
{
    public class Helpers
    {
        public static TokenValidationParameters GetJwtValidationParameters(IConfiguration configuration)
        {
            string secret = GetJwtPublicKey(configuration);

            return new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = GetJwtKey(secret),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
        }

        private static string GetJwtPublicKey(IConfiguration configuration)
        {
            const string JWT_SECRET_NAME = "JwtSecret";
            string secret = configuration[JWT_SECRET_NAME];

            if (string.IsNullOrEmpty(secret))
                throw new ApplicationException($"Missing JWT Key: {JWT_SECRET_NAME}");
            
            return secret;
        }

        private static SecurityKey GetJwtKey(string secret)
        {
            byte[] bytes = Convert.FromBase64String(secret);
            string publicKey = Encoding.UTF8.GetString(bytes);

            publicKey = publicKey.Replace("-----BEGIN PUBLIC KEY-----", "");
            publicKey = publicKey.Replace("-----END PUBLIC KEY-----", "");
            publicKey = publicKey.Replace("\r", "");
            publicKey = publicKey.Replace(Environment.NewLine, "");

            RSA rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(
                source: Convert.FromBase64String(publicKey),
                bytesRead: out int _
            );
            
            return new RsaSecurityKey(rsa);
        }        
    }
}