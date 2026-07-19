using System;
using System.Collections.Generic;
using System.Text;

namespace SmartBiodiversityUtnModels.DTOs.Account
{
    public class RefreshTokenRequestDto
    {
        public string UserId { get; set; } = string.Empty;
        public required string RefreshToken { get; set; }
    }
}
