using Microsoft.EntityFrameworkCore;
using SmartBiodiversityUtn.Data;
using SmartBiodiversityUtnModels.DTOs.Bitacora;
using SmartBiodiversityUtnModels.Entities;

namespace SmartBiodiversityUtn.Services
{
    public class BitacoraService(SmartBiodiversityUtnContext context) : IBitacoraService
    {
        public async Task<BitacoraResponse> CreateBitacoraAsync(CreateBitacoraRequest request)
        {
            var usuario = await context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.IdUsuario == request.IdUsuarioBit);

            if (usuario == null)
                throw new Exception("Usuario no encontrado.");

            var bitacora = new Bitacora
            {
                IdLog = "LOG-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
                IdUsuarioBit = usuario.IdUsuario,
                IdRolesBit = usuario.IdRolesU,
                AccionBit = request.AccionBit,
                DetalleBit = request.DetalleBit,
                FechaBit = DateTime.UtcNow // Excelente práctica usar UtcNow para logs
            };

            context.Bitacora.Add(bitacora);
            await context.SaveChangesAsync();

            return MapToResponse(bitacora); // Llamada síncrona
        }

        public async Task<IEnumerable<BitacoraResponse>> GetAllBitacorasAsync()
        {
            var bitacoras = await context.Bitacora
                .Include(b => b.Usuario)
                .Include(b => b.Rol)
                .OrderByDescending(b => b.FechaBit) // Cumple HU-01
                .ToListAsync();

            return bitacoras.Select(MapToResponse); // Mapeo limpio usando LINQ
        }

        public async Task<IEnumerable<BitacoraResponse>> GetBitacorasByUsuarioAsync(string idUsuario)
        {
            var bitacoras = await context.Bitacora
                .Include(b => b.Usuario)
                .Include(b => b.Rol)
                .Where(b => b.IdUsuarioBit == idUsuario)
                .OrderByDescending(b => b.FechaBit)
                .ToListAsync();

            return bitacoras.Select(MapToResponse);
        }

        private BitacoraResponse MapToResponse(Bitacora bitacora)
        {
            return new BitacoraResponse
            {
                IdLog = bitacora.IdLog,
                IdUsuarioBit = bitacora.IdUsuarioBit,
                NombreUsuario = bitacora.Usuario != null ? $"{bitacora.Usuario.Nombres} {bitacora.Usuario.Apellidos}" : "Desconocido",
                IdRolesBit = bitacora.IdRolesBit,
                NombreRol = bitacora.Rol?.NombreRol ?? "Sin rol",
                AccionBit = bitacora.AccionBit,
                DetalleBit = bitacora.DetalleBit,
                FechaBit = bitacora.FechaBit
            };
        }
    }
}