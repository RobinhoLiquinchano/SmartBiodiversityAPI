# Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia los archivos del proyecto y restaura dependencias
# Asegúrate de que los nombres de las carpetas coincidan exactamente con tu repositorio
COPY SmartBiodiversityUtn/SmartBiodiversityUtn.csproj SmartBiodiversityUtn/
COPY SmartBiodiversityUtnModels/SmartBiodiversityUtnModels.csproj SmartBiodiversityUtnModels/
RUN dotnet restore SmartBiodiversityUtn/SmartBiodiversityUtn.csproj

# Copia el resto del código y publica
COPY . .
RUN dotnet publish SmartBiodiversityUtn/SmartBiodiversityUtn.csproj -c Release -o /app/publish --no-restore

# Etapa de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

# Configuración de puerto para Render
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Nombre del archivo DLL generado (normalmente el nombre del proyecto API)
ENTRYPOINT ["dotnet", "SmartBiodiversityUtn.dll"]