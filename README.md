# CommerceBoost

App de escritorio para gestión de comercio usando WPF con WebView2.

## Tecnologías
- Backend: C# (WPF)
- Frontend: HTML/CSS (con Bootstrap para UI moderna)

## Estructura
- `Models/`: Clases de datos (Product, Sale, Customer)
- `Services/`: Lógica de negocio (CommerceService)
- `wwwroot/`: Archivos HTML/CSS del frontend

## Cómo ejecutar
1. Asegúrate de tener .NET 8 instalado.
2. Ejecuta `dotnet run` en la raíz del proyecto.
3. La app se abrirá con la interfaz web embebida.

## Próximos pasos
- Conectar el frontend con el backend vía JavaScript interop en WebView2.
- Agregar base de datos (ej. SQLite).
- Implementar funcionalidades CRUD.