using SmartBiodiversityUtnModels.DTOs.Bitacora;

namespace SmartBiodiversityUtn.Services
{
    /// <summary>
    /// Helper seguro para registrar acciones en la bitácora.
    /// No lanza excepciones si el usuario no existe.
    /// </summary>
    public static class BitacoraHelper
    {
        public static async Task RegistrarAccionAsync(
            IBitacoraService bitacoraService,
            string idUsuario,
            string accion,
            string? detalle = null)
        {
            if (bitacoraService == null || string.IsNullOrWhiteSpace(idUsuario))
                return;

            try
            {
                await bitacoraService.CreateBitacoraAsync(new CreateBitacoraRequest
                {
                    IdUsuarioBit = idUsuario,
                    AccionBit = accion,
                    DetalleBit = detalle
                });
            }
            catch
            {
                // Silenciamos el error para que no rompa el flujo principal
                // (el usuario "SYSTEM" no existe, por ejemplo)
            }
        }
    }
}