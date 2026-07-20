using System;
using System.Collections.Generic;
using System.Text;

namespace SmartBiodiversityUtnModels.DTOs.Account
{
    public class PasswordResetRequest
    {
        public string Email { get; set; } = string.Empty;
    }
}
