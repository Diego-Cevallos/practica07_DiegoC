// ************************************************************************
// Práctica 07
// Diego Cevallos
// Fecha de realización: 01/06/2026
// Fecha de entrega: 03/06/2026
//
// Resultados:
// 1. Se configuró el cliente para conectarse al servidor en el puerto 8080.
// 2. Se delegó el envío de operaciones a la clase Protocolo.
// 3. Se mantuvo la interfaz gráfica para consultar ingreso, placas y contador.
//
// Conclusiones:
// 1. Se concluye que el cliente queda más simple al usar la clase Protocolo.
// 2. Se concluye que la interfaz permite probar los comandos definidos.
// 3. Se concluye que separar la comunicación facilita futuras modificaciones.
//
// Recomendaciones:
// 1. Se recomienda iniciar el servidor antes de abrir el cliente.
// 2. Se recomienda ingresar placas con tres letras y cuatro números.
// 3. Se recomienda verificar los mensajes de error mostrados por la interfaz.
// ************************************************************************

using System;
using System.Net.Sockets;
using System.Windows.Forms;
using Protocolo;
using ProtocoloAplicacion = Protocolo.Protocolo;

namespace Cliente
{
    public partial class FrmValidador : Form
    {
        // Dirección local del servidor al que se conecta el cliente.
        private const string DIRECCION_SERVIDOR = "127.0.0.1";

        private TcpClient remoto;
        private NetworkStream flujo;

        // Instancia de Protocolo usada para enviar pedidos al servidor.
        private readonly ProtocoloAplicacion protocoloCliente =
            new ProtocoloAplicacion();

        public FrmValidador()
        {
            InitializeComponent();
        }

        // Al cargar el formulario, se conecta al servidor y configura los controles.
        private void FrmValidador_Load(object sender, EventArgs e)
        {
            ConectarServidor();
            ConfigurarControlesIniciales();
        }

        // Envía las credenciales ingresadas para solicitar acceso al sistema.
        private void btnIniciar_Click(object sender, EventArgs e)
        {
            string usuario = txtUsuario.Text;
            string contrasena = txtPassword.Text;

            if (usuario == "" || contrasena == "")
            {
                MessageBox.Show(
                    "Se requiere el ingreso de usuario y contraseña",
                    "ADVERTENCIA");

                return;
            }

            var respuesta = EnviarPedido(
                ProtocoloAplicacion.COMANDO_INGRESO,
                new[] { usuario, contrasena });

            if (respuesta == null)
            {
                MessageBox.Show("Hubo un error", "ERROR");
                return;
            }

            ProcesarRespuestaIngreso(respuesta);
        }

        // Envía los datos del vehículo para consultar el día correspondiente.
        private void btnConsultar_Click(object sender, EventArgs e)
        {
            string modelo = txtModelo.Text;
            string marca = txtMarca.Text;
            string placa = txtPlaca.Text.ToUpperInvariant();

            if (modelo == "" || marca == "" || placa == "")
            {
                MessageBox.Show(
                    "Se requiere ingresar modelo, marca y placa",
                    "ADVERTENCIA");

                return;
            }

            var respuesta = EnviarPedido(
                ProtocoloAplicacion.COMANDO_CALCULO,
                new[] { modelo, marca, placa });

            if (respuesta == null)
            {
                MessageBox.Show("Hubo un error", "ERROR");
                return;
            }

            ProcesarRespuestaCalculo(respuesta);
        }

        // Solicita al servidor el número de consultas realizadas por este cliente.
        private void btnNumConsultas_Click(object sender, EventArgs e)
        {
            var respuesta = EnviarPedido(
                ProtocoloAplicacion.COMANDO_CONTADOR,
                new string[0]);

            if (respuesta == null)
            {
                MessageBox.Show("Hubo un error", "ERROR");
                return;
            }

            if (respuesta.Estado == ProtocoloAplicacion.ESTADO_NOK)
            {
                MessageBox.Show(respuesta.Mensaje, "ERROR");
                return;
            }

            MessageBox.Show(
                "El número de pedidos recibidos en este cliente es " +
                respuesta.Mensaje,
                "INFORMACIÓN");
        }

        // Envía un comando al servidor usando la clase Protocolo.
        private Respuesta EnviarPedido(string comando, string[] parametros)
        {
            if (!ConectarServidor())
            {
                return null;
            }

            Respuesta respuesta =
                protocoloCliente.HazOperacion(comando, parametros, flujo);

            if (respuesta != null &&
                respuesta.Estado == ProtocoloAplicacion.ESTADO_NOK &&
                respuesta.Mensaje.Contains("comunicación"))
            {
                CerrarConexion();
            }

            return respuesta;
        }

        // Establece la conexión TCP con el servidor si aún no existe una conexión activa.
        private bool ConectarServidor()
        {
            if (remoto != null && remoto.Connected && flujo != null)
            {
                return true;
            }

            CerrarConexion();

            try
            {
                remoto = new TcpClient(
                    DIRECCION_SERVIDOR,
                    ProtocoloAplicacion.PUERTO_SERVIDOR);

                flujo = remoto.GetStream();

                return true;
            }
            catch (SocketException ex)
            {
                MessageBox.Show(
                    "No se pudo establecer conexión " + ex.Message,
                    "ERROR");

                return false;
            }
        }

        // Procesa la respuesta del servidor después del intento de ingreso.
        private void ProcesarRespuestaIngreso(Respuesta respuesta)
        {
            bool accesoConcedido =
                respuesta.Estado == ProtocoloAplicacion.ESTADO_OK &&
                respuesta.Mensaje ==
                ProtocoloAplicacion.MENSAJE_ACCESO_CONCEDIDO;

            bool accesoNegado =
                respuesta.Estado == ProtocoloAplicacion.ESTADO_NOK &&
                respuesta.Mensaje ==
                ProtocoloAplicacion.MENSAJE_ACCESO_NEGADO;

            if (accesoConcedido)
            {
                panPlaca.Enabled = true;
                panLogin.Enabled = false;

                MessageBox.Show("Acceso concedido", "INFORMACIÓN");

                txtModelo.Focus();

                return;
            }

            if (accesoNegado)
            {
                panPlaca.Enabled = false;
                panLogin.Enabled = true;

                MessageBox.Show(
                    "Acceso negado por el servidor",
                    "ERROR");

                txtUsuario.Focus();

                return;
            }

            MessageBox.Show(respuesta.Mensaje, "ERROR");
        }

        // Procesa la respuesta del servidor después de consultar una placa.
        private void ProcesarRespuestaCalculo(Respuesta respuesta)
        {
            if (respuesta.Estado == ProtocoloAplicacion.ESTADO_NOK)
            {
                MessageBox.Show(respuesta.Mensaje, "ERROR");
                DesmarcarDias();

                return;
            }

            string[] partes = respuesta.Mensaje.Split(' ');

            if (partes.Length < 2)
            {
                MessageBox.Show(
                    "La respuesta del servidor no tiene el formato esperado",
                    "ERROR");

                DesmarcarDias();

                return;
            }

            byte resultado;

            if (!byte.TryParse(partes[1], out resultado))
            {
                MessageBox.Show(
                    "No se pudo interpretar el indicador del día",
                    "ERROR");

                DesmarcarDias();

                return;
            }

            MessageBox.Show(
                "Se recibió: " + respuesta.Mensaje,
                "INFORMACIÓN");

            MarcarDia(resultado);
        }

        // Deshabilita los controles que no deben usarse al iniciar la aplicación.
        private void ConfigurarControlesIniciales()
        {
            panPlaca.Enabled = false;

            chkLunes.Enabled = false;
            chkMartes.Enabled = false;
            chkMiercoles.Enabled = false;
            chkJueves.Enabled = false;
            chkViernes.Enabled = false;
            chkSabado.Enabled = false;
            chkDomingo.Enabled = false;

            DesmarcarDias();
        }

        // Marca el día correspondiente según el indicador recibido del servidor.
        private void MarcarDia(byte indicadorDia)
        {
            DesmarcarDias();

            switch (indicadorDia)
            {
                case ProtocoloAplicacion.INDICADOR_LUNES:
                    chkLunes.Checked = true;
                    break;

                case ProtocoloAplicacion.INDICADOR_MARTES:
                    chkMartes.Checked = true;
                    break;

                case ProtocoloAplicacion.INDICADOR_MIERCOLES:
                    chkMiercoles.Checked = true;
                    break;

                case ProtocoloAplicacion.INDICADOR_JUEVES:
                    chkJueves.Checked = true;
                    break;

                case ProtocoloAplicacion.INDICADOR_VIERNES:
                    chkViernes.Checked = true;
                    break;

                default:
                    DesmarcarDias();
                    break;
            }
        }

        // Limpia la selección de todos los días.
        private void DesmarcarDias()
        {
            chkLunes.Checked = false;
            chkMartes.Checked = false;
            chkMiercoles.Checked = false;
            chkJueves.Checked = false;
            chkViernes.Checked = false;
            chkSabado.Checked = false;
            chkDomingo.Checked = false;
        }

        // Cierra el flujo y la conexión TCP de forma segura.
        private void CerrarConexion()
        {
            if (flujo != null)
            {
                flujo.Close();
                flujo = null;
            }

            if (remoto != null)
            {
                remoto.Close();
                remoto = null;
            }
        }

        // Cierra la conexión cuando el usuario sale del formulario.
        private void FrmValidador_FormClosing(
            object sender,
            FormClosingEventArgs e)
        {
            CerrarConexion();
        }
    }
}