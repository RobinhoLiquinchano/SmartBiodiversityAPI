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
    // 1. Intentar obtener desde variable de entorno (usualmente definido en Render)
    var envConnection = Environment.GetEnvironmentVariable("DefaultConnection");

    if (!string.IsNullOrWhiteSpace(envConnection))
    {
        options.UseNpgsql(envConnection);
        Console.WriteLine("Base de datos configurada mediante variable de entorno");
    }
    else
    {
        // 2. Si no, buscar en appsettings.json
        // Primero buscamos tu clave específica
        var localConnection = builder.Configuration.GetConnectionString("SmartBiodiversityUtnContext");

        // Si no existe, probamos una alternativa genérica por si acaso
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

// SERVICIOS 
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Servicios de autenticación
builder.Services.AddScoped<IAuthServices, AuthServices>();
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<IEspecieService, EspecieService>();
builder.Services.AddHttpClient<IMultimediaService, MultimediaService>();
builder.Services.AddScoped<IMultimediaService, MultimediaService>();
builder.Services.AddScoped<IAporteService, AporteService>();
builder.Services.AddScoped<IAvisoService, AvisoService>();
builder.Services.AddScoped<IEmailService, EmailService>();
// === BITÁCORA ===
builder.Services.AddScoped<IBitacoraService, BitacoraService>();

// ====================== JWT AUTHENTICATION ======================
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

// ====================== CORS ======================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMVC", policy =>
    {
        policy.WithOrigins("https://localhost:7265")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.MapGet("/", () => Results.Redirect("/scalar/v1")).ExcludeFromDescription();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ====================== SEMILLA DE ROLES Y USUARIO ADMIN ======================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<SmartBiodiversityUtnContext>();
        var passwordHasher = new PasswordHasher<Usuario>();

        // === SEED ROLES ===
        var rolesPorDefecto = new List<Rol>
        {
            new Rol { IdRoles = "1", NombreRol = "Administrador" },
            new Rol { IdRoles = "2", NombreRol = "Visitante" },
            new Rol { IdRoles = "3", NombreRol = "Usuario Registrado" }
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
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error al crear los roles o el usuario administrador por defecto.");
    }
}

app.Run();
