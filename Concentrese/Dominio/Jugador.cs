using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ClienteConcentrese
{
    class Jugador
    {
        public const int SIN_CONECTAR = 0;
        public const int EN_TURNO = 1;
        public const int ESPERANDO_TURNO_OPONENTE = 3;
        public const string JUGADOR = "JUGADOR";
        public const string JUGADA = "JUGADA";
        public const string FALLO = "FALLO";
        public const string PAREJA = "PAREJA";
        public const string PRIMERA_JUGADA = "PRIMERA_JUGADA";
        public const string SEGUNDA_JUGADA= "SEGUNDA_JUGADA";

        public string Jugada { get; set; }
        public string Nombre { get; set; }
        public string Host { get; set; }
        public int Puerto { get; set; }

        public int EstadoJuego { get; set; }

        private TcpClient cliente;
        private NetworkStream flujo;
        public Tablero TableroJuego;

        public Jugador(string host, int puerto, string nombre)
        {
            Nombre = nombre;
            Host = host;
            Puerto = puerto;
            cliente = new TcpClient();
            Jugada = PRIMERA_JUGADA;
        }

        public void Conectar()
        {
            try
            {
                cliente.Connect(Host, Puerto);
                flujo = cliente.GetStream();
                IniciarEncuentro();
            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine(ane.Message);
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void IniciarEncuentro()
        {
            string lineaJugador = new StringBuilder().Append(JUGADOR)
                                                    .Append(":")
                                                    .Append(Nombre)
                                                    .ToString();
            EnviarDatos(lineaJugador);

            string datosJugador1 = RecibirDatos();
            string datosJugador2 = RecibirDatos();

            string numerosTablero = RecibirDatos();
            InicializarTablero(numerosTablero);

            string turno = RecibirDatos();
            if (turno.Equals("1"))
                EstadoJuego = EN_TURNO;
            else
                EstadoJuego = ESPERANDO_TURNO_OPONENTE;
        }


        private void InicializarTablero(string numerosTablero)
        {
            //string[] strNumeros = numeroTablero.Split(',');
            //int[] numeros = new int[strNumeros.Length];
            //for(int i = 0; i < numeros.Length; i++)
            //{
            //    numeros[i] = int.Parse(strNumeros[i]);
            //}
            int[] numeros = Array.ConvertAll(numerosTablero.Split(','), s => int.Parse(s));
            TableroJuego = new Tablero(numeros);
        }

        private void EnviarDatos(string datos)
        {
            byte[] msg = Encoding.ASCII.GetBytes(datos);
            flujo.Write(msg, 0, msg.Length);
        }

        private string RecibirDatos()
        {
            string datos = null;
            Byte[] bytes = new Byte[1024];
            int i = flujo.Read(bytes, 0, bytes.Length);
            datos = Encoding.ASCII.GetString(bytes, 0, i);
            return datos;
        }
        private void jugar(int casilla)
        {
            
            if (EstadoJuego == EN_TURNO)
            {
                if(Jugada == PRIMERA_JUGADA)
                {
                    Primerajugada(casilla);
                    string mensaje = new StringBuilder().Append(JUGADA).Append(":").Append(casilla).ToString();
                    EnviarDatos(mensaje);

                }
                else
                {
                    bool resultado = Segundajugada(casilla);
                    string mensaje = new StringBuilder().Append(JUGADA).Append(":").Append(casilla).ToString();
                    EnviarDatos(mensaje);
                    string mensajeResultado = resultado ? PAREJA : FALLO;
                    EnviarDatos(mensajeResultado);
                }
            }

            else if (EstadoJuego == ESPERANDO_TURNO_OPONENTE)
            {
                if(Jugada == PRIMERA_JUGADA)
                {
                    int jugada = int.Parse(RecibirDatos().Split(':')[1]);
                    TableroJuego.SeleccionarPrimeraCasilla(jugada);
                }
                else
                {
                    int jugada = int.Parse(RecibirDatos().Split(':')[1]);
                    TableroJuego.SeleccionarSegundaCasilla(jugada);
                    string resultado = RecibirDatos();
                }
            }
         
           
        }
        private void Primerajugada(int casilla)
        {
            TableroJuego.SeleccionarPrimeraCasilla(casilla);
            Jugada = SEGUNDA_JUGADA;
        }
        
        private bool Segundajugada(int casilla)
        {
            Jugada = PRIMERA_JUGADA;
            EstadoJuego = EstadoJuego == ESPERANDO_TURNO_OPONENTE ? EN_TURNO : ESPERANDO_TURNO_OPONENTE ;
            return TableroJuego.SeleccionarSegundaCasilla(casilla);
        }
    }
}
