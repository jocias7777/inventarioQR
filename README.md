# InventarioQR

Sistema web de gestión de inventario y bodega desarrollado en **.NET 8**.

## Módulos principales

- Dashboard operativo
- Productos y variantes
- Inventario por ubicación
- Movimientos (entradas, salidas, transferencias, ajustes)
- Operaciones de bodega
- Reservas de inventario
- Códigos QR (individual y masivo)
- Estructura de bodega (bodega, zona, estantería, nivel, posición)
- Usuarios y roles
- Reportes y auditoría

## Tecnologías

- ASP.NET Core MVC / Razor
- Entity Framework Core
- SQL Server
- Bootstrap + Bootstrap Icons

## Requisitos

- .NET SDK 8.0
- SQL Server

## Configuración rápida

1. Clonar el repositorio  
2. Configurar `ConnectionStrings:DefaultConnection` en `appsettings.json`  
3. Ejecutar migraciones (si aplica)  
4. Ejecutar el proyecto:


## Acceso

La aplicación incluye autenticación y control por roles (Administrador, Bodega, Vendedor).

## Estado

Proyecto en evolución con mejoras continuas de UI/UX y flujo operativo.
