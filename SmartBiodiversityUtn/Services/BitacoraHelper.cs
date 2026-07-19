using SmartBiodiversityUtnModels.DTOs.Bitacora;
using System.Security.Claims;

namespace SmartBiodiversityUtn.Services
{
    public static class BitacoraHelper
    {
        public static async Task RegistrarAccionAsync(
            this IBitacoraService bitacoraService,
            IHttpContextAccessor httpContextAccessor,
            string accion,
            string? detalle = null)
        {
            var userId = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userEmail = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(userId))
                return; 

            await bitacoraService.CreateBitacoraAsync(new CreateBitacoraRequest
            {
                IdUsuarioBit = userId,
                AccionBit = accion,
                DetalleBit = detalle ?? $"{accion} realizada por {userEmail ?? userId}"
            });
        }

        public static async Task RegistrarAccionComoAsync(
            this IBitacoraService bitacoraService,
            string idUsuario,
            string accion,
            string? detalle = null)
        {
            await bitacoraService.CreateBitacoraAsync(new CreateBitacoraRequest
            {
                IdUsuarioBit = idUsuario,
                AccionBit = accion,
                DetalleBit = detalle
            });
        }
    }
}
