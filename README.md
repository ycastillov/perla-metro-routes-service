# 🚇 Perla Metro - Routes Service

Este microservicio gestiona las **rutas del sistema de transporte subterráneo** para el proyecto *Perla Metro*.  
Forma parte de la **arquitectura orientada a servicios (SOA)** con monolito distribuido que incluye servicios de **Users**, **Tickets**, **Routes** y **Stations**, todos consumidos por la API Main.

---

## 🏗️ Arquitectura y Patrón de Diseño

- **Arquitectura**: SOA (Service-Oriented Architecture) con monolito distribuido
- **Patrón de diseño**: Repository Pattern con Dependency Injection
- **Comunicación**: RESTful API con intercambio de datos JSON
- **Separación de responsabilidades**: Cada servicio maneja su propio dominio de datos

---

## 📦 Tecnologías utilizadas

- **.NET 9.0** (ASP.NET Core Web API)
- **Neo4j Aura Free** como base de datos en la nube
- **Neo4j.Driver 5.28.3** (driver oficial para .NET)
- **AutoMapper 12.0.1** para el mapeo DTO ↔ Modelo
- **FluentValidation** para validación de datos
- **DotNetEnv** para manejo de variables de entorno
- **Docker** 

---

## ⚙️ Configuración previa

### 1. Clonar el repositorio
```bash
git clone https://github.com/ycastillov/perla-metro-route-service.git
cd perla-metro-route-service
```

### 2. Variables de entorno
Crear un archivo `.env` en la raíz del proyecto:

```bash
# Neo4j Aura Configuration
NEO4J_URI=neo4j+s://458869f7.databases.neo4j.io
NEO4J_USER=458869f7
NEO4J_PASSWORD=e_mWCiqvF_DFDFQKgOvPwLKpWmvzRqzXGmqiae3lHvs

# Environment
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://localhost:5000
```

### 3. Restaurar dependencias
```bash
dotnet restore
```

### 4. Ejecutar localmente
```bash
dotnet run
```

El servicio estará disponible en:
- **API**: `http://localhost:5000`
- **Swagger UI**: `http://localhost:5000/swagger`
- **Health Check**: `http://localhost:5000/health`

---

## 🚀 Deployment en Render

### Configuración en Render:
1. **Conectar repositorio**: `perla-metro-routes-service`
2. **Build Command**: `dotnet publish -c Release -o out`
3. **Start Command**: `dotnet out/PerlaMetro-RouteService.dll`
4. **Environment**: Production

### Variables de entorno en Render:
```bash
NEO4J_URI=neo4j+s://458869f7.databases.neo4j.io
NEO4J_USER=458869f7
NEO4J_PASSWORD=_mWCiqvF_DFDFQKgOvPwLKpWmvzRqzXGmqiae3lHvs
ASPNETCORE_ENVIRONMENT=Production
```

### URL del servicio desplegado:
```
https://perla-metro-routes-service-wf9c.onrender.com
```

---

## 📑 API Endpoints

### 🔍 Información del servicio
```http
GET /
```
Respuesta: Información básica del servicio y endpoints disponibles.

### 💚 Health Check
```http
GET /health
```
Respuesta: Estado del servicio y conectividad.

### 🛣️ Gestión de Rutas

#### Crear ruta
```http
POST /api/routes
Content-Type: application/json

{
  "origin": "Estación Central",
  "destination": "Estación Norte",
  "startTime": "06:00:00",
  "endTime": "06:45:00",
  "stops": ["Estación Intermedia 1", "Estación Intermedia 2"]
}
```

#### Obtener todas las rutas
```http
GET /api/routes
```
**Nota**: Solo disponible para usuarios con rol Administrador.

#### Obtener ruta por ID
```http
GET /api/routes/{guid}
```

#### Actualizar ruta
```http
PUT /api/routes/{guid}
Content-Type: application/json

{
  "origin": "Estación Central Actualizada",
  "destination": "Estación Norte",
  "startTime": "06:15:00",
  "endTime": "07:00:00",
  "stops": ["Nueva Estación Intermedia"]
}
```

#### Eliminar ruta (Soft Delete)
```http
DELETE /api/routes/{guid}
```
**Importante**: Implementa SOFT DELETE - marca la ruta como inactiva preservando la trazabilidad.

---

## 🔒 Seguridad y Validaciones

- **Validación de duplicados**: No permite rutas idénticas
- **Soft Delete**: Preserva trazabilidad mediante eliminación lógica
- **Filtros de visualización**: Excluye rutas inactivas en consultas públicas
- **CORS**: Configurado para permitir comunicación con API Main
- **Validaciones de negocio**: Horarios coherentes, estaciones válidas

---

## 🗄️ Estructura de Base de Datos (Neo4j)

### Nodos:
- **Route**: Representa una ruta del sistema
  - `id` (UUID)
  - `origin` (string)
  - `destination` (string)
  - `startTime` (TimeSpan)
  - `endTime` (TimeSpan)
  - `stops` (array de strings)
  - `status` (Active/Inactive)
  - `createdAt` (DateTime)
  - `updatedAt` (DateTime)

---

## 🛠️ Desarrollo

### Conventional Commits
```bash
feat: agregar endpoint para filtrar rutas por estado
fix: corregir validación de horarios
docs: actualizar documentación de API
refactor: mejorar estructura de queries Neo4j
```

### Estructura del proyecto
```
PerlaMetro-RouteService/
├── Src/
│   ├── Controllers/          # Controladores REST
│   ├── Models/              # Modelos de dominio
│   ├── DTOs/                # Data Transfer Objects
│   ├── Repositories/        # Patrón Repository
│   ├── Interfaces/          # Interfaces y contratos
│   ├── Mappings/            # Perfiles de AutoMapper
│   ├── Infrastructure/      # Conexión DB y servicios
│   └── Validators/          # Validaciones FluentValidation
├── appsettings.json
├── Program.cs
├── Dockerfile
└── README.md
```

### Pruebas locales
```bash
# Compilar
dotnet build

# Ejecutar
dotnet run

# Ver documentación
# Abrir http://localhost:5000/swagger en el navegador
```

---

## 🔄 Integración con API Main

Este servicio será consumido por la **API Main** que coordina todas las operaciones entre servicios. La API Main no posee base de datos propia y actúa como orquestador de servicios.

### Endpoints expuestos para API Main:
- Gestión completa de rutas (CRUD)
- Validación de rutas para emisión de tickets
- Consultas de disponibilidad de rutas
