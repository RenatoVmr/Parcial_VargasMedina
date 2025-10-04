# Usar imagen oficial de .NET 8 Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Usar imagen de .NET 8 SDK para build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivo de proyecto y restaurar dependencias
COPY ["Parcial_VargasMedina.csproj", "."]
RUN dotnet restore "Parcial_VargasMedina.csproj"

# Copiar todo el código fuente
COPY . .
WORKDIR "/src"

# Compilar y publicar la aplicación
RUN dotnet build "Parcial_VargasMedina.csproj" -c Release -o /app/build
RUN dotnet publish "Parcial_VargasMedina.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Imagen final
FROM base AS final
WORKDIR /app

# Copiar la aplicación compilada
COPY --from=build /app/publish .

# Crear directorio para base de datos con permisos
RUN mkdir -p /app/data

# Crear usuario no-root para seguridad
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# Configurar variables de entorno
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/app.db"

# Volumen para persistir la base de datos
VOLUME ["/app/data"]

# Punto de entrada
ENTRYPOINT ["dotnet", "Parcial_VargasMedina.dll"]