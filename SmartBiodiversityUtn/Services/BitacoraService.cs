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
                throw new Exception("Usuario no encontrado para registrar en bitácora.");

            var bitacora = new Bitacora
            {
                IdLog = "LOG-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
                IdUsuarioBit = request.IdUsuarioBit,
                IdRolesBit = usuario.IdRolesU,
                AccionBit = request.AccionBit,
                DetalleBit = request.DetalleBit,
                FechaBit = DateTime.UtcNow
            };

            context.Bitacora.Add(bitacora);
            await context.SaveChangesAsync();

            return MapToResponse(bitacora, usuario);
        }

        public async Task<IEnumerable<BitacoraResponse>> GetAllBitacorasAsync()
        {
            var bitacoras = await context.Bitacora
                .Include(b => b.Usuario)
                    .ThenInclude(u => u.Rol)
                .OrderByDescending(b => b.FechaBit)
                .ToListAsync();

            return bitacoras.Select(b => MapToResponse(b, b.Usuario)).ToList();
        }

        public async Task<IEnumerable<BitacoraResponse>> GetBitacorasByUsuarioAsync(string idUsuario)
        {
            var bitacoras = await context.Bitacora
                .Include(b => b.Usuario)
                    .ThenInclude(u => u.Rol)
                .Where(b => b.IdUsuarioBit == idUsuario)
                .OrderByDescending(b => b.FechaBit)
                .ToListAsync();

            return bitacoras.Select(b => MapToResponse(b, b.Usuario)).ToList();
        }

        private BitacoraResponse MapToResponse(Bitacora bitacora, Usuario usuario)
        {
            return new BitacoraResponse
            {
                IdLog = bitacora.IdLog,
                IdUsuarioBit = bitacora.IdUsuarioBit,
                NombreUsuario = $"{usuario.Nombres} {usuario.Apellidos}",
                IdRolesBit = bitacora.IdRolesBit,
                NombreRol = usuario.Rol?.NombreRol ?? "Sin rol",
                AccionBit = bitacora.AccionBit,
                DetalleBit = bitacora.DetalleBit,
                FechaBit = bitacora.FechaBit
            };
        }
    }
}