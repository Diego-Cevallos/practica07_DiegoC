// ************************************************************************
// Práctica 07
// Diego Cevallos
// Fecha de realización: 01/06/2026
// Fecha de entrega: 03/06/2026
//
// Resultados:
// 1. Se creó la clase Protocolo para centralizar la comunicación.
// 2. Se implementó HazOperacion para enviar pedidos y recibir respuestas.
// 3. Se implementó ResolverPedido para procesar los comandos del servidor.
//
// Conclusiones:
// 1. Se concluye que la clase Protocolo organiza mejor la lógica del sistema.
// 2. Se concluye que Pedido y Respuesta permiten estructurar los mensajes.
// 3. Se concluye que centralizar la lógica facilita el mantenimiento.
//
// Recomendaciones:
// 1. Se recomienda validar los mensajes antes de procesarlos.
// 2. Se recomienda usar constantes simbólicas para evitar valores sin significado.
// 3. Se recomienda probar los comandos INGRESO, CALCULO y CONTADOR.
// ************************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace Protocolo
{
    public class Pedido
    {
        public string Comando { get; set; }
        public string[] Parametros { get; set; }

        // Convierte el mensaje recibido desde la red en un objeto Pedido.
        public static Pedido Procesar(string mensaje)
        {
            if (string.IsNullOrWhiteSpace(mensaje))
            {
                return new Pedido
                {
                    Comando = string.Empty,
                    Parametros = new string[0]
                };
            }

            string[] partes = mensaje.Split(
                new char[] { ' ' },
                StringSplitOptions.RemoveEmptyEntries);

            return new Pedido
            {
                Comando = partes[0].ToUpperInvariant(),
                Parametros = partes.Skip(1).ToArray()
            };
        }

        // Convierte el objeto Pedido en texto para enviarlo por el flujo de red.
        public override string ToString()
        {
            if (Parametros == null || Parametros.Length == 0)
            {
                return Comando;
            }

            return $"{Comando} {string.Join(" ", Parametros)}";
        }
    }

    public class Respuesta
    {
        public string Estado { get; set; }
        public string Mensaje { get; set; }

        // Convierte el mensaje recibido desde el servidor en un objeto Respuesta.
        public static Respuesta Procesar(string mensaje)
        {
            if (string.IsNullOrWhiteSpace(mensaje))
            {
                return new Respuesta
                {
                    Estado = Protocolo.ESTADO_NOK,
                    Mensaje = "Respuesta vacía"
                };
            }

            string[] partes = mensaje.Split(
                new char[] { ' ' },
                2,
                StringSplitOptions.RemoveEmptyEntries);

            return new Respuesta
            {
                Estado = partes[0].ToUpperInvariant(),
                Mensaje = partes.Length > 1 ? partes[1] : string.Empty
            };
        }

        // Convierte el objeto Respuesta en texto para enviarlo por la red.
        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(Mensaje))
            {
                return Estado;
            }

            return $"{Estado} {Mensaje}";
        }
    }

    public class Protocolo
    {
        public const int PUERTO_SERVIDOR = 8080;
        public const int TAMANO_BUFFER = 1024;

        public const string ESTADO_OK = "OK";
        public const string ESTADO_NOK = "NOK";

        public const string COMANDO_INGRESO = "INGRESO";
        public const string COMANDO_CALCULO = "CALCULO";
        public const string COMANDO_CONTADOR = "CONTADOR";

        public const string MENSAJE_ACCESO_CONCEDIDO = "ACCESO_CONCEDIDO";
        public const string MENSAJE_ACCESO_NEGADO = "ACCESO_NEGADO";

        public const byte INDICADOR_LUNES = 0b00100000;
        public const byte INDICADOR_MARTES = 0b00010000;
        public const byte INDICADOR_MIERCOLES = 0b00001000;
        public const byte INDICADOR_JUEVES = 0b00000100;
        public const byte INDICADOR_VIERNES = 0b00000010;

        private const int CANTIDAD_PARAMETROS_INGRESO = 2;
        private const int CANTIDAD_PARAMETROS_CALCULO = 3;
        private const int INDICE_USUARIO = 0;
        private const int INDICE_CLAVE = 1;
        private const int INDICE_PLACA = 2;
        private const int INDICE_ULTIMO_DIGITO = 6;
        private const int OPCIONES_ACCESO = 2;
        private const int VALOR_ACCESO_CONCEDIDO = 0;

        private const string USUARIO_ADMINISTRADOR = "root";
        private const string CLAVE_ADMINISTRADOR = "admin20";
        private const string FORMATO_PLACA = @"^[A-Z]{3}[0-9]{4}$";

        private static readonly Random generadorAleatorio = new Random();
        private static readonly object bloqueoAleatorio = new object();

        private readonly Dictionary<string, int> conteoSolicitudesClientes =
            new Dictionary<string, int>();

        private readonly object bloqueoClientes = new object();

        // Crea un pedido con comando y parámetros para enviarlo al servidor.
        public Respuesta HazOperacion(
            string comando,
            string[] parametros,
            NetworkStream flujo)
        {
            Pedido pedido = new Pedido
            {
                Comando = comando.ToUpperInvariant(),
                Parametros = parametros ?? new string[0]
            };

            return HazOperacion(pedido, flujo);
        }

        // Envía un pedido desde el cliente hacia el servidor y espera una respuesta.
        private Respuesta HazOperacion(Pedido pedido, NetworkStream flujo)
        {
            if (pedido == null || flujo == null || !flujo.CanRead || !flujo.CanWrite)
            {
                return CrearRespuestaError("No hay conexión disponible");
            }

            try
            {
                byte[] bufferTx = Encoding.UTF8.GetBytes(pedido.ToString());

                flujo.Write(bufferTx, 0, bufferTx.Length);

                byte[] bufferRx = new byte[TAMANO_BUFFER];
                int bytesRx = flujo.Read(bufferRx, 0, bufferRx.Length);

                if (bytesRx <= 0)
                {
                    return CrearRespuestaError("No se recibió respuesta del servidor");
                }

                string mensajeRx = Encoding.UTF8.GetString(bufferRx, 0, bytesRx);

                return Respuesta.Procesar(mensajeRx);
            }
            catch (IOException)
            {
                return CrearRespuestaError("Error de comunicación con el servidor");
            }
            catch (SocketException)
            {
                return CrearRespuestaError("Error de socket durante la comunicación");
            }
        }

        // Procesa el mensaje recibido por el servidor y obtiene la respuesta.
        public Respuesta ResolverPedido(string mensaje, string direccionCliente)
        {
            Pedido pedido = Pedido.Procesar(mensaje);

            return ResolverPedido(pedido, direccionCliente);
        }

        // Selecciona la operación que debe ejecutarse según el comando recibido.
        private Respuesta ResolverPedido(Pedido pedido, string direccionCliente)
        {
            if (pedido == null)
            {
                return CrearRespuestaError("Pedido no válido");
            }

            switch (pedido.Comando)
            {
                case COMANDO_INGRESO:
                    return ResolverIngreso(pedido);

                case COMANDO_CALCULO:
                    return ResolverCalculo(pedido, direccionCliente);

                case COMANDO_CONTADOR:
                    return ResolverContador(direccionCliente);

                default:
                    return CrearRespuestaError("Comando no reconocido");
            }
        }

        // Valida las credenciales enviadas por el cliente.
        private Respuesta ResolverIngreso(Pedido pedido)
        {
            string[] parametros = pedido.Parametros ?? new string[0];

            if (parametros.Length != CANTIDAD_PARAMETROS_INGRESO)
            {
                return CrearRespuestaError(MENSAJE_ACCESO_NEGADO);
            }

            bool credencialesValidas =
                parametros[INDICE_USUARIO] == USUARIO_ADMINISTRADOR &&
                parametros[INDICE_CLAVE] == CLAVE_ADMINISTRADOR;

            if (!credencialesValidas)
            {
                return CrearRespuestaError(MENSAJE_ACCESO_NEGADO);
            }

            if (AccesoConcedidoAleatorio())
            {
                return new Respuesta
                {
                    Estado = ESTADO_OK,
                    Mensaje = MENSAJE_ACCESO_CONCEDIDO
                };
            }

            return CrearRespuestaError(MENSAJE_ACCESO_NEGADO);
        }

        // Valida la placa recibida y calcula el indicador del día correspondiente.
        private Respuesta ResolverCalculo(Pedido pedido, string direccionCliente)
        {
            string[] parametros = pedido.Parametros ?? new string[0];

            if (parametros.Length != CANTIDAD_PARAMETROS_CALCULO)
            {
                return CrearRespuestaError("Parámetros incompletos");
            }

            string placa = parametros[INDICE_PLACA].ToUpperInvariant();

            if (!ValidarPlaca(placa))
            {
                return CrearRespuestaError("Placa no válida");
            }

            byte indicadorDia = ObtenerIndicadorDia(placa);

            IncrementarContadorCliente(direccionCliente);

            return new Respuesta
            {
                Estado = ESTADO_OK,
                Mensaje = $"{placa} {indicadorDia}"
            };
        }

        // Devuelve la cantidad de solicitudes válidas realizadas por el cliente.
        private Respuesta ResolverContador(string direccionCliente)
        {
            int cantidadSolicitudes = ObtenerCantidadSolicitudes(direccionCliente);

            if (cantidadSolicitudes == 0)
            {
                return CrearRespuestaError("No hay solicitudes previas");
            }

            return new Respuesta
            {
                Estado = ESTADO_OK,
                Mensaje = cantidadSolicitudes.ToString()
            };
        }

        // Verifica que la placa tenga tres letras seguidas de cuatro números.
        private bool ValidarPlaca(string placa)
        {
            return !string.IsNullOrWhiteSpace(placa) &&
                   Regex.IsMatch(placa, FORMATO_PLACA);
        }

        // Obtiene el indicador binario del día según el último dígito de la placa.
        private byte ObtenerIndicadorDia(string placa)
        {
            int ultimoDigito = int.Parse(placa.Substring(INDICE_ULTIMO_DIGITO, 1));

            switch (ultimoDigito)
            {
                case 1:
                case 2:
                    return INDICADOR_LUNES;

                case 3:
                case 4:
                    return INDICADOR_MARTES;

                case 5:
                case 6:
                    return INDICADOR_MIERCOLES;

                case 7:
                case 8:
                    return INDICADOR_JUEVES;

                case 9:
                case 0:
                    return INDICADOR_VIERNES;

                default:
                    return 0;
            }
        }

        // Mantiene la lógica original de acceso aleatorio del servidor.
        private bool AccesoConcedidoAleatorio()
        {
            lock (bloqueoAleatorio)
            {
                return generadorAleatorio.Next(OPCIONES_ACCESO) ==
                       VALOR_ACCESO_CONCEDIDO;
            }
        }

        // Incrementa el contador de solicitudes del cliente actual.
        private void IncrementarContadorCliente(string direccionCliente)
        {
            if (string.IsNullOrWhiteSpace(direccionCliente))
            {
                return;
            }

            lock (bloqueoClientes)
            {
                if (conteoSolicitudesClientes.ContainsKey(direccionCliente))
                {
                    conteoSolicitudesClientes[direccionCliente]++;
                }
                else
                {
                    conteoSolicitudesClientes[direccionCliente] = 1;
                }
            }
        }

        // Obtiene el número de solicitudes registradas para un cliente.
        private int ObtenerCantidadSolicitudes(string direccionCliente)
        {
            if (string.IsNullOrWhiteSpace(direccionCliente))
            {
                return 0;
            }

            lock (bloqueoClientes)
            {
                if (conteoSolicitudesClientes.ContainsKey(direccionCliente))
                {
                    return conteoSolicitudesClientes[direccionCliente];
                }

                return 0;
            }
        }

        // Crea una respuesta de error con estado NOK y el mensaje recibido.
        private Respuesta CrearRespuestaError(string mensaje)
        {
            return new Respuesta
            {
                Estado = ESTADO_NOK,
                Mensaje = mensaje
            };
        }
    }
}