using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using drportal.Models;
using Microsoft.IdentityModel.Tokens;

namespace drportal.Helpers {
    public class Crypto {
        public static JwtSecurityToken GenerateToken(UserModel user, string appSecret)
        {
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSecret));

            var claims = new Claim[] {
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Exp, $"{new DateTimeOffset(DateTime.Now.AddDays(1)).ToUnixTimeSeconds()}")
            };

            return new JwtSecurityToken(new JwtHeader(
                new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256)),
                new JwtPayload(claims));
        }
        
        public static string GenerateSHA256String(string password)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new System.Text.StringBuilder();
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (byte theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            return hash.ToString();
        }

        public static string EncryptString(string text, string key) {
            byte[] inputArray = UTF8Encoding.UTF8.GetBytes(text);  
            TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider();  
            tripleDES.Key = UTF8Encoding.UTF8.GetBytes(key);
            tripleDES.Mode = CipherMode.ECB;
            tripleDES.Padding = PaddingMode.PKCS7;
            tripleDES.BlockSize = 64;
            ICryptoTransform cTransform = tripleDES.CreateEncryptor();  
            byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);  
            tripleDES.Clear();  
            return Convert.ToBase64String(resultArray, 0, resultArray.Length); 
        }

        public static string DecryptString(string text, string key) {
            byte[] inputArray = Convert.FromBase64String(text);  
            TripleDESCryptoServiceProvider tripleDES = new TripleDESCryptoServiceProvider();  
            tripleDES.Key = UTF8Encoding.UTF8.GetBytes(key);  
            tripleDES.Mode = CipherMode.ECB;  
            tripleDES.Padding = PaddingMode.PKCS7;
            tripleDES.BlockSize =64;
            ICryptoTransform cTransform = tripleDES.CreateDecryptor();  
            byte[] resultArray = cTransform.TransformFinalBlock(inputArray, 0, inputArray.Length);  
            tripleDES.Clear();   
            return UTF8Encoding.UTF8.GetString(resultArray); 
        }

        public static string CreateForgotPasswordHash(Guid userGuid, string key) {
            return EncryptString(Convert.ToString(userGuid) + "." + Guid.NewGuid().ToString(), key);
        }

        public static Guid UnhashForgotPassword(string hash, string key) {
            var unhashed = DecryptString(hash, key);
            if (unhashed.Contains(".")) {
                return Guid.Parse(unhashed.Substring(0, unhashed.LastIndexOf(".")));
            }
            
            return Guid.Empty;
        }
    }
}