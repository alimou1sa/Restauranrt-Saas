using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSaaS.Application.DTOs.RefreshTokens.RefreshTokensRequest
{
    public class LogoutRequest
    {
        [Required] public string RefreshToken { get; set; } = null!;
    }
}
