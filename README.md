# 🚇 Perla Metro - Route Service

Este microservicio gestiona las **rutas del sistema de metro** para el proyecto *Perla Metro*.  
Forma parte de la arquitectura basada en microservicios que incluye servicios de **Products**, **Professors**, **Routes**, entre otros, todos consumidos por la API Main.

---

## 📦 Tecnologías utilizadas
- **.NET 9.0** (ASP.NET Core Web API)
- **Neo4j Aura Free** como base de datos
- **Neo4j.Driver** (driver oficial para .NET)
- **AutoMapper** para el mapeo DTO ↔ Modelo
- **Docker** (opcional para despliegue)

---

## ⚙️ Configuración previa

### 1. Clonar el repositorio
```bash
git clone https://github.com/<tu-usuario>/perla-metro-route-service.git
cd perla-metro-route-service
```
### 2. Variables de entorno
Se debe crear un archivo `.env` en la raíz del proyecto con el siguiente contenido
```bash
NEO4J_URI=neo4j+s://<your-database-id>.databases.neo4j.io
NEO4J_USER=neo4j
NEO4J_PASSWORD=<your-password>
```
### 3. Restaurar dependencias
```bash
dotnet restore
```
### 4. Ejecutar en local
```bash
dotnet run
```
El servicio quedará disponible en: `http://localhost:5000` con documentación de Swagger en `http://localhost:5000/swagger`

---

## 🚀 Deployment en la nube
Este servicio debe ser desplegado en un proveedor cloud gratuito, como **Render**.

## Pasos en Render
### 1. Crear cuenta en Render
### 2. Conectar el repositorio `perla-metro-routes-service`
### 3. Configurar un nuevo **Web Service**:
  - Build Command: `dotnet build`
  - Start Command: `dotnet run --urls http://0.0.0.0:10000`
### 4. Agregar las variables de entorno en el panel de Render:
  `NEO4J_URI`
  `NEO4J_USER`
  `NEO4J_PASSWORD`
### 5. Deploy automático desde la rama `main`.
Render brindará una URL pública como:
```bash
https://perla-metro-route-service.onrender.com
```

---

## 📑 Endpoints principales
### Crear ruta
```bash
POST /api/routes
```
Body:
```bash
{
  "origin": "Antofagasta",
  "destination": "Calama",
  "startTime": "12:00:00",
  "endTime": "15:00:00",
  "stops": ["Sierra Gorda"],
  "status": "Active"
}
```
### Obtener todas las rutas
```bash
GET /api/routes
```
### Obtener ruta por ID
```bash
GET /api/routes/{guid}
```
### Actualizar ruta
```bash
PUT /api/routes/{guid}
```
### Soft delete de ruta
```bash
DELETE /api/routes/{guid}
```
Esto no elimina físicamente la ruta, solo cambia su `status` a `"Inactive"`.

---

## 🛠️ Desarrollo y convenciones
- Se siguen las **conventional commits**:
  - `feat: nueva funcionalidad`
  - `fix: corrección de bug`
  - `docs: cambios en la documentación`
- Todo el código está comentado para mayor claridad.
- Las queries de Neo4j están centralizadas en `RouteQueries.cs`.
