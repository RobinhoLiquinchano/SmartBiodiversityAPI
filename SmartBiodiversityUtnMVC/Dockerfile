# Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia los archivos del proyecto y restaura dependencias
# Se incluye el proyecto MVC y el proyecto de Modelos compartido
COPY SmartBiodiversityUtnMVC/SmartBiodiversityUtnMVC.csproj SmartBiodiversityUtnMVC/
COPY SmartBiodiversityUtnModels/SmartBiodiversityUtnModels.csproj SmartBiodiversityUtnModels/
RUN dotnet restore SmartBiodiversityUtnMVC/SmartBiodiversityUtnMVC.csproj

# Copia el resto del código y publica
COPY . .
RUN dotnet publish SmartBiodiversityUtnMVC/SmartBiodiversityUtnMVC.csproj -c Release -o /app/publish --no-restore

# Etapa de ejecución
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

# Configuración de puerto para Render
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Nombre del archivo DLL generado correspondiente al proyecto MVC
ENTRYPOINT ["dotnet", "SmartBiodiversityUtnMVC.dll"]