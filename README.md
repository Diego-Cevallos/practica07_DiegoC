\# Práctica 07 - Gestión de Versiones e Integración Continua



\## Proyecto AD\_PA01\_2024A - Aplicaciones Distribuidas



\---



\## Autor



\*\*Diego Cevallos\*\*



\---



\## Descripción



Este repositorio contiene el proyecto \*\*AD\_PA01\_2024A\*\*, clonado desde el repositorio base indicado en la Práctica 07 de la asignatura \*\*Aplicaciones Distribuidas\*\*.



El proyecto fue modificado en \*\*Visual Studio 2022\*\* para aplicar gestión de versiones con \*\*GitHub\*\*. Además, se realizaron cambios en el código del cliente, servidor y protocolo, colocando encabezados, comentarios y aplicando convenciones de programación.



\---



\## Objetivo del Proyecto



Modificar el proyecto clonado para centralizar la lógica de comunicación entre cliente y servidor mediante una nueva clase \*\*Protocolo\*\*.



El proyecto permite:



\* Clonar un repositorio desde GitHub en Visual Studio.

\* Modificar el código fuente del cliente, servidor y protocolo.

\* Crear una nueva clase \*\*Protocolo\*\*.

\* Implementar los métodos \*\*HazOperacion\*\* y \*\*ResolverPedido\*\* dentro de la clase \*\*Protocolo\*\*.

\* Usar las clases \*\*Pedido\*\* y \*\*Respuesta\*\* dentro del proyecto Protocolo.

\* Evitar que cliente y servidor implementen directamente la lógica de comunicación.

\* Subir los cambios realizados a un repositorio personal de GitHub.



\---



\## Tecnologías Utilizadas



\* C#

\* .NET Framework

\* Windows Forms

\* TCP Sockets

\* Visual Studio 2022

\* Git

\* GitHub



\---



\## Estructura del Proyecto



```text

practica07\_DiegoC

├── Cliente

│   └── Interfaz gráfica que envía pedidos al servidor.

│

├── Servidor

│   └── Aplicación que recibe pedidos y responde usando Protocolo.

│

└── Protocolo

&#x20;   └── Proyecto que contiene Pedido, Respuesta y la clase Protocolo.

```



\---



\## Cambios Realizados



\### Proyecto Protocolo



Se creó la clase \*\*Protocolo\*\*, encargada de centralizar la lógica principal de comunicación.



En esta clase se implementaron los métodos:



\* \*\*HazOperacion\*\*

\* \*\*ResolverPedido\*\*



También se mantuvieron las clases:



\* \*\*Pedido\*\*

\* \*\*Respuesta\*\*



Estas clases permiten estructurar los mensajes enviados entre cliente y servidor.



\### Proyecto Cliente



Se modificó el cliente para que utilice la clase \*\*Protocolo\*\* al enviar pedidos al servidor.



El cliente ya no implementa directamente el método \*\*HazOperacion\*\*, sino que delega esa responsabilidad al proyecto \*\*Protocolo\*\*.



\### Proyecto Servidor



Se modificó el servidor para que utilice la clase \*\*Protocolo\*\* al resolver los pedidos recibidos.



El servidor ya no implementa directamente el método \*\*ResolverPedido\*\*, sino que usa la clase \*\*Protocolo\*\* para procesar los comandos.



También se corrigió el mensaje mostrado en consola para que coincida con el puerto utilizado por el servidor.



\---



\## Cómo Ejecutar el Proyecto



1\. Clonar el repositorio:



```bash

git clone https://github.com/diego-cevallos/practica07\_DiegoC.git

```



2\. Abrir el proyecto en \*\*Visual Studio 2022\*\*.



3\. Abrir la solución del proyecto.



4\. Compilar la solución.



5\. Ejecutar primero el proyecto \*\*Servidor\*\*.



6\. Ejecutar después el proyecto \*\*Cliente\*\*.



7\. Ingresar las credenciales de prueba:



```text

Usuario: root

Contraseña: admin20

```



8\. Realizar una consulta de placa con el siguiente formato:



```text

ABC1234

```



La placa debe tener tres letras y cuatro números.



\---



\## Pruebas Realizadas



Se realizaron pruebas para verificar:



\* Compilación correcta de la solución.

\* Conexión entre cliente y servidor.

\* Envío de credenciales desde el cliente.

\* Respuesta del servidor mediante la clase Protocolo.

\* Consulta de placas válidas.

\* Validación de placas incorrectas.

\* Funcionamiento del contador de solicitudes.



\---



\## Gestión de Versiones



Los cambios fueron registrados mediante Git y subidos a GitHub desde Visual Studio 2022.



Se utilizó GitHub para:



\* Crear el repositorio.

\* Registrar commits.

\* Subir los cambios del proyecto.

\* Mantener el historial de modificaciones.



\---



\## Estado del Proyecto



✅ Proyecto clonado

✅ Código modificado

✅ Clase Protocolo implementada

✅ Cliente actualizado

✅ Servidor actualizado

✅ Comentarios agregados

✅ README agregado

✅ Cambios preparados para subir a GitHub



