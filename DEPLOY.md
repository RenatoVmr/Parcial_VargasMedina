# Portal Académico - Despliegue en Render con Docker

## 🐳 Despliegue usando Docker en Render.com

### 📋 Variables de Entorno Requeridas:

```bash
ASPNETCORE_ENVIRONMENT=Production
PORT=8080
ConnectionStrings__DefaultConnection=Data Source=/app/data/app.db
ConnectionStrings__Redis=[REDIS_URL_DE_RENDER]
```

### 🚀 Pasos para Deploy en Render:

#### 1. **Crear Redis Instance (Obligatorio):**
   - Ve a tu Dashboard de Render
   - Click en "New" → "Redis"
   - Nombre: `portal-academico-redis`
   - Plan: Starter (gratis)
   - Copia la **Redis URL** generada

#### 2. **Crear Web Service con Docker:**
   - Click en "New" → "Web Service"
   - Conectar tu repositorio GitHub
   - Configuración:
     ```
     Name: portal-academico
     Environment: Docker
     Region: Oregon (US West)
     Branch: master (o deploy)
     ```

#### 3. **Configurar Variables de Entorno:**
   ```
   ASPNETCORE_ENVIRONMENT=Production  
   PORT=8080
   ConnectionStrings__Redis=redis://[TU-REDIS-URL-AQUI]
   ```
   
   ⚠️ **Importante:** Reemplaza `[TU-REDIS-URL-AQUI]` con la URL real de tu Redis instance

#### 4. **Configuración Automática:**
   - ✅ Render detectará automáticamente el `Dockerfile`
   - ✅ La base de datos SQLite se inicializará automáticamente
   - ✅ Los datos de ejemplo se cargarán automáticamente

### Credenciales del Sistema:
- **Coordinador:** `admin@portal.edu` / `Admin123!`

### Funcionalidades Principales:
- ✅ Sistema de autenticación con Identity
- ✅ Catálogo de cursos con filtros
- ✅ Sistema de inscripciones con validaciones  
- ✅ Cache Redis (60s) para optimización
- ✅ Sesiones para último curso visitado
- ✅ Panel completo de coordinador
- ✅ CRUD de cursos y gestión de matrículas

### Base de Datos:
- SQLite (incluida en el proyecto)
- Se inicializa automáticamente con datos de ejemplo
- Migrations aplicadas automáticamente

### Tecnologías:
- ASP.NET Core 8 MVC
- Entity Framework Core + SQLite
- ASP.NET Core Identity  
- Redis (StackExchange.Redis)
- Bootstrap 5 + Font Awesome