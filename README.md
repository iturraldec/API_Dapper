# Aplicación API web ASP.NET Core, con C# y Dapper, utilizando como frontend paginas razor y blazor

## Descripción

Aplicación API web basada en el framework de Microsoft Asp.Net Core 9.0 y el micro-ORM Dapper.
Muestra por pantalla las categorias y los productos de la base de datos de prueba SQL Server 'Northwind', 
pudiendo realizar CRUD de las marcas de los productos.

## Instalación

Asegurate de:
- Tener instalado los Runtimes de AspNetCore 9.0.10
- Tener instalado el Manejador de base de datos SQL Server
- Tener instalada la base de datos 'Northwind', puedes descargarla [aqui](https://github.com/microsoft/sql-server-samples/tree/master/samples/databases/)
- Agregar la columna 'VersionFila' en la tabla 'Brand' de tipo 'rowversion', para poder ejecutar la opcion de eliminar una marca de forma 'blanda'
- Cambiar la cadena de conexión a la base de datos en el archivo 'Controllers/BrandController.cs' con las credenciales correspondientes a tu manejador

## Uso
Una vez descargada la aplicación podras realizar un CRUD sobre las marcas de las cervezas y consultas de uno-muchos, muchos-uno.
    
## Licencia

GNU GPL
