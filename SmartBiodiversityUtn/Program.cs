using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using SmartBiodiversityUtn.Data;
using SmartBiodiversityUtn.Services;
using SmartBiodiversityUtnModels.Entities;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


// === CONFIGURACIÓN DE BASE DE DATOS ===
builder.Services.AddDbContext<SmartBiodiversityUtnContext>(options =>
{
    var envConnection = Environment.GetEnvironmentVariable("DefaultConnection");

    if (!string.IsNullOrWhiteSpace(envConnection))
    {
        options.UseNpgsql(envConnection);
        Console.WriteLine("Base de datos configurada mediante variable de entorno");
    }
    else
    {
        var localConnection = builder.Configuration.GetConnectionString("SmartBiodiversityUtnContext");

        if (string.IsNullOrWhiteSpace(localConnection))
        {
            localConnection = builder.Configuration.GetConnectionString("DefaultConnection");
        }

        if (string.IsNullOrWhiteSpace(localConnection))
        {
            throw new InvalidOperationException("No se encontró ninguna cadena de conexión válida en appsettings.json ni en variables de entorno.");
        }

        options.UseNpgsql(localConnection);
        Console.WriteLine("Base de datos configurada mediante appsettings.json");
    }
});

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// SERVICIOS 
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Servicios de autenticación
builder.Services.AddScoped<IAuthServices, AuthServices>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IEspecieService, EspecieService>();
builder.Services.AddHttpClient<IMultimediaService, MultimediaService>();
builder.Services.AddHttpClient<IAporteService, AporteService>();
builder.Services.AddScoped<IAvisoService, AvisoService>();
builder.Services.AddHttpClient<IEmailService, EmailService>();
builder.Services.AddScoped<IBitacoraService, BitacoraService>();
builder.Services.AddScoped<IUserService, UserService>();

// ====== NUEVO: Servicio de Facultades ======
builder.Services.AddScoped<IFacultadService, FacultadService>();

// JWT AUTHENTICATION 
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["AppSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["AppSettings:Audience"],
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Token"]!)),
            ValidateIssuerSigningKey = true,
        };
    });


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors("AllowAll");

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.Servers = new[]
    {
        new Scalar.AspNetCore.ScalarServer("https://smartbiodiversityapi.onrender.com")
    };
});
app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();

// SEMILLA DE ROLES Y USUARIO ADMIN 
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<SmartBiodiversityUtnContext>();
        var passwordHasher = new PasswordHasher<Usuario>();


        context.Database.Migrate();

        // === SEED ROLES ===
        var rolesPorDefecto = new List<Rol>
        {
            new Rol { IdRoles = "1", NombreRol = "Administrador" },
            new Rol { IdRoles = "2", NombreRol = "Visitante" },

        };

        foreach (var nuevoRol in rolesPorDefecto)
        {
            if (!context.Roles.Any(r => r.NombreRol == nuevoRol.NombreRol))
            {
                context.Roles.Add(nuevoRol);
            }
        }
        await context.SaveChangesAsync();

        // === SEED USUARIO ADMINISTRADOR POR DEFECTO ===
        var adminRole = await context.Roles
            .FirstOrDefaultAsync(r => r.NombreRol == "Administrador");

        if (adminRole != null &&
            !await context.Usuarios.AnyAsync(u => u.Correo == "admin@smartbiodiversity.com"))
        {
            var adminUser = new Usuario
            {
                IdUsuario = "ADM-" + Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
                Nombres = "Administrador",
                Apellidos = "Sistema",
                Correo = "admin@smartbiodiversity.com",
                Password = passwordHasher.HashPassword(null!, "Admin123*"),
                Estado = "Activo",
                FechaRegistro = DateTime.UtcNow,
                IntentosFallidos = 0,
                IdRolesU = adminRole.IdRoles
            };

            context.Usuarios.Add(adminUser);
            await context.SaveChangesAsync();
        }

        // ====== NUEVO: SEED FACULTADES UTN ======
        // ====== SEED FACULTADES UTN (coordenadas corregidas) ======
        var facultadesSeed = new List<Facultad>
        {
            new Facultad
            {
                IdFacultad = "FAC-UTN001",
                NumeroFac = 1,
                NombreFac = "FACAE",
                Latitud = 0.356538,
                Longitud = -78.110797,
                DescripcionFac = "Facultad de Ciencias Administrativas y Económicas - Campus El Olivo"
            },
            new Facultad
            {
                IdFacultad = "FAC-UTN002",
                NumeroFac = 2,
                NombreFac = "FECYT",
                Latitud = 0.357397,
                Longitud = -78.111033,
                DescripcionFac = "Facultad de Educación, Ciencia y Tecnología - Campus El Olivo"
            },
            new Facultad
            {
                IdFacultad = "FAC-UTN003",
                NumeroFac = 3,
                NombreFac = "FICAYA",
                Latitud = 0.358352,
                Longitud = -78.111259,
                DescripcionFac = "Facultad de Ingeniería en Ciencias Agropecuarias y Ambientales - Campus El Olivo"
            },
            new Facultad
            {
                IdFacultad = "FAC-UTN004",
                NumeroFac = 4,
                NombreFac = "FICA",
                Latitud = 0.358813,
                Longitud = -78.111248,
                DescripcionFac = "Facultad de Ingeniería en Ciencias Aplicadas - Campus El Olivo"
            },
            new Facultad
            {
                IdFacultad = "FAC-UTN005",
                NumeroFac = 5,
                NombreFac = "FCCSS",
                Latitud = 0.358786,
                Longitud = -78.111768,
                DescripcionFac = "Facultad de Ciencias de la Salud - Campus El Olivo"
            },
            new Facultad
            {
                IdFacultad = "FAC-UTN006",
                NumeroFac = 6,
                NombreFac = "CAI",
                Latitud = 0.358405,
                Longitud = -78.111811,
                DescripcionFac = "Centro Académico de Idiomas - Campus El Olivo"
            },
            new Facultad
            {
                IdFacultad = "FAC-UTN007",
                NumeroFac = 7,
                NombreFac = "POSGRADO",
                Latitud = 0.358373,
                Longitud = -78.112364,
                DescripcionFac = "Facultad de Posgrado - Campus El Olivo"
            },
            new Facultad
            {
                IdFacultad = "FAC-UTN008",
                NumeroFac = 8,
                NombreFac = "DBU",
                Latitud = 0.359065,
                Longitud = -78.110443,
                DescripcionFac = "Departamento De Bienestar Universitario- Campus El Olivo"
            }

        };

        foreach (var fac in facultadesSeed)
        {
            if (!await context.Facultades.AnyAsync(f => f.IdFacultad == fac.IdFacultad))
            {
                context.Facultades.Add(fac);
            }
        }
        await context.SaveChangesAsync();


    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error al crear los roles o el usuario administrador por defecto.");
    }
}
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
