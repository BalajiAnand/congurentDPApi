using System;
using drportal.Enums;
using Newtonsoft.Json;

namespace drportal.Models {
    public class UserModel {
        public Guid UserGuid { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime DOB { get; set; }
        public Gender Gender { get; set; }

        [JsonIgnore]
        public string ForgotPasswordHash { get; set; }
        [JsonIgnore]
        public DateTime? ForgotPasswordExpiry { get; set; }
    }
}