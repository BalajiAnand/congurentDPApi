using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using drportal.Models;
using drportal.Helpers;
using System.Data;
using drportal.Repositories;
using drportal.Enums;

namespace drportal.Controllers
{
    public class AuthController : Controller
    {
        private IConfiguration Configuration { get; set; }

        private UserRepository userRepository;

        public AuthController(IConfiguration configuration)
        {
            this.Configuration = configuration;
            this.userRepository = new UserRepository(Configuration.GetValue<string>("ConnectionString"));
        }

        [HttpPost]
        [Route("auth/login")]
        public IActionResult Login(LoginRequestModel loginRequest)
        {
            var authenticatedUser = userRepository.AuthenticateUser(loginRequest);
            if (authenticatedUser != null)
            {
                var token = Crypto.GenerateToken(authenticatedUser, Configuration.GetValue<string>("AppSecret"));
                return this.Json(new {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    User = authenticatedUser
                });
            }

            return Unauthorized();
        }

        [HttpPost]
        [Route("auth/register")]
        public IActionResult Register(RegistrationRequestModel registrationRequest)
        {
            registrationRequest.Gender = Gender.Unspecified;
            return new ObjectResult(this.userRepository.RegisterUser(registrationRequest));
        }

        [HttpPost]
        [Route("auth/forgotpassword")]
        public IActionResult ForgotPassword(ForgotPasswordRequestModel forgotPasswordRequest)
        {
            var userModel = userRepository.GetUser(forgotPasswordRequest.Email);

            if (userModel != null)
            {
                if (userModel.DOB.Equals(forgotPasswordRequest.DOB))
                {
                    string forgotHash;
                    if (userModel.ForgotPasswordExpiry != null && DateTime.Now <= userModel.ForgotPasswordExpiry)
                    {
                        forgotHash = userModel.ForgotPasswordHash;
                    }
                    else {
                        forgotHash = Crypto.CreateForgotPasswordHash(userModel.UserGuid, Configuration.GetValue<string>("EncryptionKey"));
                        userRepository.UpdateForgotPasswordHash(userModel.UserGuid, forgotHash);
                    }

                    // var mailSetting = Configuration.GetValue<MailSettings>("Mail");
                    var mailSetting = new MailSettings() {
                        Host = Configuration.GetValue<string>("Mail:Host"),
                        Port = Configuration.GetValue<int>("Mail:Port"),
                        UserName = Configuration.GetValue<string>("Mail:UserName"),
                        Password = Configuration.GetValue<string>("Mail:Password"),
                        WhitelistedRecipients = Configuration.GetValue<string>("Mail:WhitelistedRecipients").Split(';')
                    };

                    new MailHelper(mailSetting, Configuration.GetValue<string>("WebApplicationUrl"))
                        .SendForgotPasswordMail(userModel.Email, userModel.Name, forgotHash);
                    return Ok();
                }
            }

            return BadRequest();
        }

        [HttpPost]
        [Route("auth/unhashforgotpassword")]
        public IActionResult UnhashForgotPassword(string forgotPasswordHash)
        {
            var user = userRepository.GetUser(Crypto.UnhashForgotPassword(forgotPasswordHash, Configuration.GetValue<string>("EncryptionKey")));

            if (DateTime.Now <= user.ForgotPasswordExpiry) {
                return this.Json(user);
            }

            return BadRequest();
        }

        [HttpPost]
        [Route("auth/changepassword")]
        public IActionResult ChangePassword(ChangePasswordRequestModel changePasswordRequest)
        {
            if (userRepository.ChangePassword(changePasswordRequest)) {
                return Ok();
            }

            return BadRequest();
        }

        [HttpGet]
        [Route("auth/isAuthenticated")]
        [Authorize]
        public IActionResult CheckAuthentication()
        {
            return new ObjectResult("Authenticated!");
        }
    }
}

