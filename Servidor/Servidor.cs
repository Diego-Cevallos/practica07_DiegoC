// ************************************************************************
// Práctica 07
// Diego Cevallos
// Fecha de realización: 01/06/2026
// Fecha de entrega: 03/06/2026
//
// Resultados:
// 1. Se configuró el servidor TCP para escuchar conexiones en el puerto 8080.
// 2. Se delegó la resolución de pedidos a la clase Protocolo.
// 3. Se corrigió el mensaje del puerto mostrado por consola.
//
// Conclusiones:
// 1. Se concluye que el servidor queda más modular al usar la clase Protocolo.
// 2. Se concluye que la atención de clientes mediante hilos permite varias conexiones.
// 3. Se concluye que separar la lógica del servidor mejora el mantenimiento.
//
// Recomendaciones:
// 1. Se recomienda ejecutar primero el servidor antes de iniciar el cliente.
// 2. Se recomienda verificar que el puerto 8080 no esté ocupado.
// 3. Se recomienda revisar la consola del servidor durante las pruebas.
// ************************************************************************

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ProtocoloAplicacion = Protocolo.Protocolo;

namespace Servidor
{
    class Servidor
    {
        private static TcpListener escuchador;

        // Instancia compartida de Protocolo usada para resolver los pedidos recibidos.
        private static readonly ProtocoloAplicacion protocoloServidor =
            new ProtocoloAplicacion();

        static void Main(string[] args)
        {
            try
            {
                // Crea el servidor TCP y lo deja escuchando en el puerto configurado.
                escuchador = new TcpListener(
                    IPAddress.Any,
                    ProtocoloAplicacion.PUERTO_SERVIDOR);

                escuchador.Start();

                Console.WriteLine(
                    "Servidor inició en el puerto {0}...",
                    ProtocoloAplicacion.PUERTO_SERVIDOR);

                while (true)
                {
                    // Acepta una conexión entrante de un cliente.
                    TcpClient cliente = escuchador.AcceptTcpClient();

                    Console.WriteLine(
                        "Cliente conectado, puerto: {0}",
                        cliente.Client.RemoteEndPoint.ToString());

                    // Crea un hilo para atender al cliente sin detener el servidor principal.
                    Thread hiloCliente = new Thread(ManipuladorCliente);
                    hiloCliente.Start(cliente);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine(
                    "Error de socket al iniciar el servidor: " + ex.Message);
            }
            finally
            {
                escuchador?.Stop();
            }
        }

        private static void ManipuladorCliente(object obj)
        {
            TcpClient cliente = obj as TcpClient;
            NetworkStream flujo = null;

            if (cliente == null)
            {
                return;
            }

            try
            {
                // Obtiene el flujo de red para leer pedidos y enviar respuestas.
                flujo = cliente.GetStream();

                byte[] bufferRx =
                    new byte[ProtocoloAplicacion.TAMANO_BUFFER];

                int bytesRx;

                while ((bytesRx = flujo.Read(
                    bufferRx,
                    0,
                    bufferRx.Length)) > 0)
                {
                    // Lee el mensaje enviado por el cliente.
                    string mensajeRx =
                        Encoding.UTF8.GetString(bufferRx, 0, bytesRx);

                    Console.WriteLine("Se recibió: " + mensajeRx);

                    // Obtiene la dirección del cliente para registrar sus solicitudes.
                    string direccionCliente =
                        cliente.Client.RemoteEndPoint.ToString();

                    // Usa la clase Protocolo para resolver el pedido recibido.
                    var respuesta = protocoloServidor.ResolverPedido(
                        mensajeRx,
                        direccionCliente);

                    Console.WriteLine("Se envió: " + respuesta);

                    byte[] bufferTx =
                        Encoding.UTF8.GetBytes(respuesta.ToString());

                    // Envía la respuesta procesada al cliente.
                    flujo.Write(bufferTx, 0, bufferTx.Length);
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine(
                    "Error de socket al manejar el cliente: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "Error general al manejar el cliente: " + ex.Message);
            }
            finally
            {
                // Libera los recursos de red cuando finaliza la atención del cliente.
                flujo?.Close();
                cliente?.Close();
            }
        }
    }
}
