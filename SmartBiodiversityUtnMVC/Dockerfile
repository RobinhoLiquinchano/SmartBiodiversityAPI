# ===================== Etapa de compilación =====================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia los .csproj y restaura dependencias (MVC + proyecto de modelos compartido).
# Los nombres de carpeta deben coincidir con tu repositorio.
COPY SmartBiodiversityUtnMVC/SmartBiodiversityUtnMVC.csproj SmartBiodiversityUtnMVC/
COPY SmartBiodiversityUtnModels/SmartBiodiversityUtnModels.csproj SmartBiodiversityUtnModels/
RUN dotnet restore SmartBiodiversityUtnMVC/SmartBiodiversityUtnMVC.csproj

# Copia el resto del código y publica SOLO el MVC
COPY . .
RUN dotnet publish SmartBiodiversityUtnMVC/SmartBiodiversityUtnMVC.csproj -c Release -o /app/publish --no-restore

# ===================== Etapa de ejecución =====================
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

# Puerto para Render
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SmartBiodiversityUtnMVC.dll"]
