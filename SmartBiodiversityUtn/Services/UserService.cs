using Microsoft.EntityFrameworkCore;
using SmartBiodiversityUtn.Data;
using SmartBiodiversityUtnModels.DTOs.Account;
using SmartBiodiversityUtnModels.DTOs.Bitacora;

namespace SmartBiodiversityUtn.Services;

public class UserService : IUserService
{
    private readonly SmartBiodiversityUtnContext _context;
    private readonly IBitacoraService _bitacoraService;

    public UserService(SmartBiodiversityUtnContext context, IBitacoraService bitacoraService)
    {
        _context = context;
        _bitacoraService = bitacoraService;
    }

    public async Task<UserProfileResponse?> GetProfileAsync(string idUsuario)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

        if (usuario == null)
            return null;

        return new UserProfileResponse
        {
            IdUsuario = usuario.IdUsuario,
            Nombres = usuario.Nombres,
            Apellidos = usuario.Apellidos,
            Correo = usuario.Correo,
            Estado = usuario.Estado,
            NombreRol = usuario.Rol.NombreRol,
            FechaRegistro = usuario.FechaRegistro
        };
    }

    public async Task<bool> UpdateProfileAsync(string idUsuario, UpdateProfileRequest request)
    {
        var usuario = await _context.Usuarios.FindAsync(idUsuario);

        if (usuario == null)
            return false;

        var nombresAnterior = usuario.Nombres;
        var apellidosAnterior = usuario.Apellidos;

        usuario.Nombres = request.Nombres;
        usuario.Apellidos = request.Apellidos;

        _context.Usuarios.Update(usuario);
        var result = await _context.SaveChangesAsync() > 0;

        if (result)
        {
            await _bitacoraService.CreateBitacoraAsync(new CreateBitacoraRequest
            {
                IdUsuarioBit = idUsuario,
                AccionBit = "ACTUALIZAR_PERFIL",
                DetalleBit = $"Actualizó su perfil: {nombresAnterior} {apellidosAnterior} → {request.Nombres} {request.Apellidos}"
            });
        }

        return result;
    }

    public async Task<IEnumerable<UserListResponse>> GetAllUsersAsync()
    {
        var usuarios = await _context.Usuarios
            .Include(u => u.Rol)
            .Include(u => u.Aportes)
            .OrderByDescending(u => u.FechaRegistro)
            .ToListAsync();

        return usuarios.Select(u => new UserListResponse
        {
            IdUsuario = u.IdUsuario,
            Nombres = u.Nombres,
            Apellidos = u.Apellidos,
            Correo = u.Correo,
            Estado = u.Estado,
            NombreRol = u.Rol.NombreRol,
            FechaRegistro = u.FechaRegistro,
            TotalAportes = u.Aportes.Count
        });
    }

    public async Task<UserListResponse?> GetUserByIdAsync(string idUsuario)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Rol)
            .Include(u => u.Aportes)
            .FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

        if (usuario == null)
            return null;

        return new UserListResponse
        {
            IdUsuario = usuario.IdUsuario,
            Nombres = usuario.Nombres,
            Apellidos = usuario.Apellidos,
            Correo = usuario.Correo,
            Estado = usuario.Estado,
            NombreRol = usuario.Rol.NombreRol,
            FechaRegistro = usuario.FechaRegistro,
            TotalAportes = usuario.Aportes.Count
        };
    }
}
