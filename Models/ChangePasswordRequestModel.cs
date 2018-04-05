using System;

namespace drportal.Models {
    public class ChangePasswordRequestModel {
        public Guid UserGuid { get; set; }
        public string ChangePasswordHash { get; set; }
        public string NewPassword { get; set; }
    }
}