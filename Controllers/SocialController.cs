using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using drportal.Models;
using drportal.Repositories;

namespace drportal.Controllers
{
    public class SocialController : Controller
    {
        private IConfiguration Configuration { get; set; }

        private UserRepository userRepository;

        public SocialController(IConfiguration configuration)
        {
            this.Configuration = configuration;
            this.userRepository = new UserRepository(Configuration["ConnectionString"]);
        }

        [HttpPost]
        [Route("social/login")]
        public IActionResult SocialLogin([FromBody]SocialRequestModel socialRequestModel)
        {
            return new ObjectResult(this.userRepository.SocialRegistration(socialRequestModel));
        }

        [HttpPost]
        [Route("social/test")]
        public IActionResult test([FromBody]string token)
        {
            return new ObjectResult(token);
        }
    }
}
