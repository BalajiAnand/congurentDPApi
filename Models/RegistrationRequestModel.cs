using System;
using drportal.Enums;

namespace drportal.Models {
    public class RegistrationRequestModel {
        
        public Guid? UserGuid { get; set; }
        public string Name { get; set; }
        public string DOB { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public Gender? Gender { get; set; }
    }
}

