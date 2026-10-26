using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace BIZ
{
    // Herramientas que el modelo puede elegir (tool calling de Ollama). El modelo solo elige una y
    // completa sus parametros; los datos salen siempre de AsistenteConsultas.
    public class AsistenteHerramientas
    {
        public const string LotesMasIngresos = "lotes_mas_ingresos";
        public const string MovimientosVehiculo = "movimientos_vehiculo";
        public const string MovimientosPersona = "movimientos_persona";
        public const string CategoriasMasFrecuentes = "categorias_mas_frecuentes";
        public const string PersonasAdentro = "personas_adentro";
        public const string IngresosALote = "ingresos_a_lote";
        public const string HorariosYGaritas = "horarios_y_garitas";
        public const string ResumenPeriodo = "resumen_periodo";

        private static readonly string[] PERIODOS = { "hoy", "ayer", "semana", "mes", "todo" };

        private const int TopPorDefecto = 5;
        private const int TopMaximo = 10;
        private const int TopMovimientos = 5;
        // Para que el modelo chico no se pierda con listas largas
        private const int MaximoFilasParaElModelo = 10;
        private const int FranjasPico = 3;

        public class ResultadoHerramienta
        {
            // true: Texto son los datos para que el modelo redacte la respuesta.
            // false: Texto ya es la respuesta final (falta un dato o no hubo resultados).
            public bool HayDatos { get; set; }
            public string Tipo { get; set; }
            public string Texto { get; set; }
        }

        // Definicion de las herramientas en el formato que pide Ollama
        public static JArray Definiciones()
        {
            return new JArray
            {
                CrearDefinicion(LotesMasIngresos,
                    "Ranking de los lotes que mas ingresos (visitas) recibieron. " +
                    "Para preguntas como 'que lotes reciben mas visitas' o 'cual es el lote mas visitado'.",
                    new JObject
                    {
                        ["periodo"] = ParametroPeriodo(),
                        ["top"] = Parametro("integer", "Cuantos lotes mostrar, entre 1 y 10. Si no lo dice, no lo completes.")
                    }),

                CrearDefinicion(MovimientosVehiculo,
                    "Ultimos ingresos y egresos de un vehiculo buscado por su patente: cuando entro y salio, " +
                    "a que lote fue, quien manejaba, de quien es el vehiculo y que guardia lo registro.",
                    new JObject
                    {
                        ["patente"] = Parametro("string", "Patente del vehiculo tal como la escribio el usuario. Si no la dijo, no la completes."),
                        ["periodo"] = ParametroPeriodo()
                    }),

                CrearDefinicion(MovimientosPersona,
                    "Ultimos ingresos y egresos de una persona buscada por DNI o por nombre y apellido: " +
                    "cuando entro y salio, a que lote fue, por que garita y con que vehiculo.",
                    new JObject
                    {
                        ["persona"] = Parametro("string", "DNI o nombre/apellido de la persona tal como lo escribio el usuario."),
                        ["periodo"] = ParametroPeriodo()
                    }),

                CrearDefinicion(CategoriasMasFrecuentes,
                    "Que categorias de personas ingresan mas (visitante, proveedor, obrero, propietario), " +
                    "con la cantidad de ingresos y el porcentaje.",
                    new JObject
                    {
                        ["periodo"] = ParametroPeriodo()
                    }),

                CrearDefinicion(PersonasAdentro,
                    "Personas que estan adentro del barrio en este momento: ingresaron y todavia no salieron.",
                    new JObject()),

                CrearDefinicion(IngresosALote,
                    "Quien entro a un lote determinado: personas, categoria, patente y guardia que lo registro.",
                    new JObject
                    {
                        ["lote"] = Parametro("string", "Numero o nombre del lote tal como lo escribio el usuario. Si no lo dijo, no lo completes."),
                        ["periodo"] = ParametroPeriodo()
                    }),

                CrearDefinicion(HorariosYGaritas,
                    "Horarios pico y garitas con mas ingresos: en que franjas horarias y por que garitas entra mas gente.",
                    new JObject
                    {
                        ["periodo"] = ParametroPeriodo()
                    }),

                CrearDefinicion(ResumenPeriodo,
                    "Resumen general de los movimientos: total de ingresos y egresos, personas y vehiculos distintos, " +
                    "lote y categoria con mas ingresos y cuantas personas hay adentro ahora. Usala tambien para " +
                    "preguntas generales como 'como viene la semana', 'como estuvo hoy' o 'cuanto movimiento hubo'.",
                    new JObject
                    {
                        ["periodo"] = ParametroPeriodo()
                    })
            };
        }

        // Ollama manda los argumentos como objeto, pero algunos modelos los mandan como texto JSON.
        // Los nombres se limpian porque los modelos chicos a veces devuelven "periodo: " o "Periodo".
        public static Dictionary<string, string> LeerParametros(JToken argumentos)
        {
            var parametros = new Dictionary<string, string>();

            if (argumentos == null || argumentos.Type == JTokenType.Null)
                return parametros;

            if (argumentos.Type == JTokenType.String)
            {
                string texto = ((string)argumentos).Trim();
                if (!texto.StartsWith("{"))
                    return parametros;

                argumentos = JObject.Parse(texto);
            }

            var objeto = argumentos as JObject;
            if (objeto == null)
                return parametros;

            foreach (var propiedad in objeto.Properties())
            {
                string clave = SinTildes(propiedad.Name).Trim().TrimEnd(':').Trim().ToLowerInvariant();
                string valor = LeerValor(propiedad.Value);

                if (clave.Length > 0 && valor.Length > 0 && !parametros.ContainsKey(clave))
                    parametros.Add(clave, valor);
            }

            return parametros;
        }

        public static bool Existe(string nombre)
        {
            switch (nombre)
            {
                case LotesMasIngresos:
                case MovimientosVehiculo:
                case MovimientosPersona:
                case CategoriasMasFrecuentes:
                case PersonasAdentro:
                case IngresosALote:
                case HorariosYGaritas:
                case ResumenPeriodo:
                    return true;
                default:
                    return false;
            }
        }

        // preguntaAnterior es la ultima pregunta del usuario antes de esta, para las repreguntas
        // (ej. el asistente pidio la patente y el usuario solo escribe "AB123CD").
        public static ResultadoHerramienta Ejecutar(ConsultaElegida consulta, string preguntaActual, string preguntaAnterior)
        {
            VerificarParametros(consulta, preguntaActual, preguntaAnterior);

            switch (consulta.Nombre)
            {
                case LotesMasIngresos:
                    return EjecutarLotesMasIngresos(consulta);
                case MovimientosVehiculo:
                    return EjecutarMovimientosVehiculo(consulta);
                case MovimientosPersona:
                    return EjecutarMovimientosPersona(consulta);
                case CategoriasMasFrecuentes:
                    return EjecutarCategoriasMasFrecuentes(consulta);
                case PersonasAdentro:
                    return EjecutarPersonasAdentro();
                case IngresosALote:
                    return EjecutarIngresosALote(consulta);
                case HorariosYGaritas:
                    return EjecutarHorariosYGaritas(consulta);
                case ResumenPeriodo:
                    return EjecutarResumenPeriodo(consulta);
                default:
                    throw new Exception("La herramienta '" + consulta.Nombre + "' no existe.");
            }
        }

        // Los modelos chicos completan parametros que el usuario no dijo (un periodo cualquiera o una
        // patente de ejemplo). Solo se acepta lo que de verdad aparece en lo que escribio el usuario.
        private static void VerificarParametros(ConsultaElegida consulta, string preguntaActual, string preguntaAnterior)
        {
            if (consulta.Parametros == null)
                consulta.Parametros = new Dictionary<string, string>();

            // El periodo sale de la pregunta actual; si no lo menciona, cada herramienta usa su valor por defecto
            string periodo = PeriodoMencionado(preguntaActual);
            if (periodo != null)
                consulta.Parametros["periodo"] = periodo;
            else
                consulta.Parametros.Remove("periodo");

            string texto = (preguntaActual ?? "") + " " + (preguntaAnterior ?? "");

            if (!MencionaPatente(consulta.Parametro("patente"), texto))
                consulta.Parametros.Remove("patente");

            if (!MencionaPersona(consulta.Parametro("persona"), texto))
                consulta.Parametros.Remove("persona");

            if (!MencionaLote(consulta.Parametro("lote"), texto))
                consulta.Parametros.Remove("lote");
        }

        private static string PeriodoMencionado(string pregunta)
        {
            string texto = SinTildes(pregunta ?? "").ToLowerInvariant();

            if (Regex.IsMatch(texto, @"\bhoy\b")) return "hoy";
            if (Regex.IsMatch(texto, @"\bayer\b")) return "ayer";
            if (Regex.IsMatch(texto, @"\bsemana\b")) return "semana";
            if (Regex.IsMatch(texto, @"\bmes\b")) return "mes";
            if (Regex.IsMatch(texto, @"\b(todo|siempre|historico|historial|en total)\b")) return "todo";

            return null;
        }

        // "ab 123 cd" y "AB-123-CD" cuentan como la misma patente
        private static bool MencionaPatente(string patente, string texto)
        {
            string buscada = SoloLetrasYNumeros(patente).ToUpperInvariant();
            return buscada.Length >= 4 && SoloLetrasYNumeros(texto).ToUpperInvariant().Contains(buscada);
        }

        private static bool MencionaPersona(string persona, string texto)
        {
            string documento = (persona ?? "").Replace(".", "").Replace(" ", "");

            if (documento.Length > 0 && documento.All(char.IsDigit))
                return (texto ?? "").Replace(".", "").Contains(documento);

            string textoLimpio = SinTildes(texto ?? "").ToLowerInvariant();
            string[] palabras = SinTildes(persona ?? "").ToLowerInvariant()
                .Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

            return palabras.Length > 0 && palabras.All(p => textoLimpio.Contains(p));
        }

        // Si el lote tiene un numero alcanza con que el numero este en el texto ("lote 9" o solo "9")
        private static bool MencionaLote(string lote, string texto)
        {
            if (string.IsNullOrWhiteSpace(lote))
                return false;

            Match numero = Regex.Match(lote, @"\d+");

            if (numero.Success)
                return Regex.IsMatch(texto ?? "", @"(?<!\d)" + numero.Value + @"(?!\d)");

            return SoloLetrasYNumeros(texto).ToLowerInvariant().Contains(SoloLetrasYNumeros(lote).ToLowerInvariant());
        }

        private static string SoloLetrasYNumeros(string texto)
        {
            return new string((texto ?? "").Where(char.IsLetterOrDigit).ToArray());
        }

        private static ResultadoHerramienta EjecutarLotesMasIngresos(ConsultaElegida consulta)
        {
            string periodo = Periodo(consulta, "todo");
            DateTime desde, hasta;
            AsistenteConsultas.RangoDePeriodo(periodo, out desde, out hasta);

            DataTable dt = AsistenteConsultas.LotesConMasIngresos(desde, hasta, Top(consulta));

            if (dt.Rows.Count == 0)
                return SinResultados("No hubo ingresos a ningun lote " + TextoPeriodo(periodo) + ".");

            return Datos("Ranking de lotes con mas ingresos", periodo, desde, hasta, dt);
        }

        private static ResultadoHerramienta EjecutarMovimientosVehiculo(ConsultaElegida consulta)
        {
            string patente = AsistenteConsultas.NormalizarPatente(consulta.Parametro("patente"));

            if (patente == "")
                return FaltaDato("Decime la patente del vehiculo que queres consultar.");

            string periodo = Periodo(consulta, "todo");
            DateTime desde, hasta;
            AsistenteConsultas.RangoDePeriodo(periodo, out desde, out hasta);

            DataTable dt = AsistenteConsultas.MovimientosDeVehiculo(patente, desde, hasta, TopMovimientos);

            if (dt.Rows.Count == 0)
                return SinResultados("No encontre movimientos del vehiculo " + patente + " " + TextoPeriodo(periodo) + ".");

            return Datos("Ultimos movimientos del vehiculo " + patente + " (los mas recientes primero)",
                periodo, desde, hasta, dt);
        }

        private static ResultadoHerramienta EjecutarMovimientosPersona(ConsultaElegida consulta)
        {
            string persona = consulta.Parametro("persona");

            if (persona == "")
                return FaltaDato("Decime el DNI o el nombre de la persona que queres consultar.");

            string periodo = Periodo(consulta, "todo");
            DateTime desde, hasta;
            AsistenteConsultas.RangoDePeriodo(periodo, out desde, out hasta);

            DataTable dt = AsistenteConsultas.MovimientosDePersona(persona, desde, hasta, TopMovimientos);

            if (dt.Rows.Count == 0)
                return SinResultados("No encontre movimientos de " + persona + " " + TextoPeriodo(periodo) + ".");

            return Datos("Ultimos movimientos de las personas que coinciden con '" + persona + "' (los mas recientes primero)",
                periodo, desde, hasta, dt);
        }

        private static ResultadoHerramienta EjecutarCategoriasMasFrecuentes(ConsultaElegida consulta)
        {
            string periodo = Periodo(consulta, "todo");
            DateTime desde, hasta;
            AsistenteConsultas.RangoDePeriodo(periodo, out desde, out hasta);

            DataTable dt = AsistenteConsultas.CategoriasMasFrecuentes(desde, hasta);

            if (dt.Rows.Count == 0)
                return SinResultados("No hubo ingresos " + TextoPeriodo(periodo) + ".");

            return Datos("Categorias con mas ingresos. Una persona con varias categorias cuenta en cada una, " +
                "por eso los porcentajes pueden sumar mas de 100%", periodo, desde, hasta, dt);
        }

        private static ResultadoHerramienta EjecutarPersonasAdentro()
        {
            DataTable dt = AsistenteConsultas.PersonasAdentro();

            if (dt.Rows.Count == 0)
                return SinResultados("No hay nadie adentro del barrio en este momento.");

            return Datos("Personas que estan adentro del barrio ahora (las que llevan mas tiempo primero)",
                null, DateTime.MinValue, DateTime.MinValue, dt);
        }

        private static ResultadoHerramienta EjecutarIngresosALote(ConsultaElegida consulta)
        {
            string lote = consulta.Parametro("lote");

            if (lote == "")
                return FaltaDato("Decime el lote que queres consultar.");

            string periodo = Periodo(consulta, "todo");
            DateTime desde, hasta;
            AsistenteConsultas.RangoDePeriodo(periodo, out desde, out hasta);

            DataTable dt = AsistenteConsultas.IngresosALote(lote, desde, hasta, Top(consulta));

            if (dt.Rows.Count == 0)
                return SinResultados("No encontre ingresos al lote " + lote + " " + TextoPeriodo(periodo) +
                    ". Revisa que el numero de lote sea correcto.");

            return Datos("Ultimos ingresos al lote " + lote + " (los mas recientes primero)", periodo, desde, hasta, dt);
        }

        private static ResultadoHerramienta EjecutarHorariosYGaritas(ConsultaElegida consulta)
        {
            string periodo = Periodo(consulta, "todo");
            DateTime desde, hasta;
            AsistenteConsultas.RangoDePeriodo(periodo, out desde, out hasta);

            DataSet ds = AsistenteConsultas.HorariosYGaritasPico(desde, hasta);

            if (ds.Tables[0].Rows.Count == 0)
                return SinResultados("No hubo ingresos " + TextoPeriodo(periodo) + ".");

            // Solo las franjas pico: con todas, el modelo chico mezcla el orden
            int franjas = Math.Min(FranjasPico, ds.Tables[0].Rows.Count);

            var texto = new StringBuilder();
            texto.AppendLine(Encabezado("Horarios pico y garitas con mas ingresos", periodo, desde, hasta));
            texto.AppendLine("Las " + franjas + " franjas horarias con mas ingresos, de mayor a menor:");
            texto.Append(FilasComoTexto(ds.Tables[0], franjas));
            texto.AppendLine("Garitas con mas ingresos, de mayor a menor:");
            texto.Append(FilasComoTexto(ds.Tables[1], MaximoFilasParaElModelo));

            return new ResultadoHerramienta { HayDatos = true, Tipo = MensajeChat.TipoTexto, Texto = texto.ToString() };
        }

        private static ResultadoHerramienta EjecutarResumenPeriodo(ConsultaElegida consulta)
        {
            string periodo = Periodo(consulta, "semana");
            DateTime desde, hasta;
            AsistenteConsultas.RangoDePeriodo(periodo, out desde, out hasta);

            DataRow fila = AsistenteConsultas.ResumenDelPeriodo(desde, hasta);

            if (Convert.ToInt32(fila["TotalIngresos"]) == 0 && Convert.ToInt32(fila["TotalEgresos"]) == 0)
                return SinResultados("No hubo ingresos ni egresos " + TextoPeriodo(periodo) +
                    ". Adentro ahora hay " + fila["AdentroAhora"] + " personas.");

            var texto = new StringBuilder();
            texto.AppendLine(Encabezado("Resumen de movimientos del barrio", periodo, desde, hasta));

            foreach (DataColumn columna in fila.Table.Columns)
            {
                texto.AppendLine(NombreLegible(columna.ColumnName) + ": " + Formatear(columna.ColumnName, fila[columna]));
            }

            return new ResultadoHerramienta { HayDatos = true, Tipo = MensajeChat.TipoTexto, Texto = texto.ToString() };
        }

        private static ResultadoHerramienta Datos(string titulo, string periodo, DateTime desde, DateTime hasta, DataTable dt)
        {
            int cantidad = Math.Min(dt.Rows.Count, MaximoFilasParaElModelo);

            // La cantidad explicita hace que el modelo nombre todos los resultados y no solo el primero
            var texto = new StringBuilder();
            texto.AppendLine(Encabezado(titulo, periodo, desde, hasta));
            texto.AppendLine("Cantidad de resultados: " + cantidad + ".");
            texto.Append(FilasComoTexto(dt, MaximoFilasParaElModelo));

            // El recordatorio va al final porque es lo que mas atiende un modelo chico
            if (cantidad > 1)
                texto.AppendLine("La respuesta tiene que nombrar los " + cantidad + " resultados, uno por linea.");

            return new ResultadoHerramienta { HayDatos = true, Tipo = MensajeChat.TipoTexto, Texto = texto.ToString() };
        }

        private static ResultadoHerramienta SinResultados(string texto)
        {
            return new ResultadoHerramienta { HayDatos = false, Tipo = MensajeChat.TipoSinResultados, Texto = texto };
        }

        private static ResultadoHerramienta FaltaDato(string texto)
        {
            return new ResultadoHerramienta { HayDatos = false, Tipo = MensajeChat.TipoFaltaDato, Texto = texto };
        }

        private static string Encabezado(string titulo, string periodo, DateTime desde, DateTime hasta)
        {
            if (periodo == null)
                return "Consulta: " + titulo + ".";

            if (periodo == "todo")
                return "Consulta: " + titulo + ". Periodo: todo el registro.";

            // hasta es exclusivo, por eso se muestra el dia anterior
            return "Consulta: " + titulo + ". Periodo: " + periodo + " (del " + desde.ToString("dd/MM/yyyy") +
                " al " + hasta.AddDays(-1).ToString("dd/MM/yyyy") + ").";
        }

        // Cada fila en una linea "Columna: valor; Columna: valor", que el modelo chico entiende bien.
        // Se usa ";" y no "|" para que, si el modelo copia la linea tal cual, igual se lea bien.
        private static string FilasComoTexto(DataTable dt, int maximo)
        {
            var texto = new StringBuilder();
            int cantidad = Math.Min(dt.Rows.Count, maximo);

            for (int i = 0; i < cantidad; i++)
            {
                DataRow fila = dt.Rows[i];
                var partes = dt.Columns.Cast<DataColumn>()
                    .Select(c => NombreLegible(c.ColumnName) + ": " + Formatear(c.ColumnName, fila[c]));

                texto.AppendLine("- " + string.Join("; ", partes));
            }

            if (dt.Rows.Count > cantidad && maximo == MaximoFilasParaElModelo)
                texto.AppendLine("Hay " + (dt.Rows.Count - cantidad) + " resultados mas que no se muestran.");

            return texto.ToString();
        }

        private static string Formatear(string columna, object valor)
        {
            if (valor == null || valor == DBNull.Value)
                return columna == "FechaEgreso" ? "todavia no salio, sigue adentro" : "sin dato";

            if (valor is DateTime)
                return ((DateTime)valor).ToString("dd/MM/yyyy HH:mm");

            string texto = Convert.ToString(valor, CultureInfo.InvariantCulture).Trim();

            if (texto == "")
                return columna == "Patente" ? "a pie" : "sin dato";

            if (columna == "Porcentaje")
                return texto + "%";

            if (columna == "HorasAdentro")
            {
                decimal horas = Convert.ToDecimal(valor);
                return horas >= 24
                    ? (int)(horas / 24) + " dias y " + (int)(horas % 24) + " horas"
                    : texto + " horas";
            }

            return texto;
        }

        // Etiquetas para el texto que lee el modelo. Con "Fecha egreso" el modelo chico a veces decia
        // que no habia salido; con "Salio el" no se confunde. Las columnas de los DataTable no cambian.
        private static readonly Dictionary<string, string> ETIQUETAS = new Dictionary<string, string>
        {
            { "FechaIngreso", "Entro el" },
            { "FechaEgreso", "Salio el" },
            { "DuenioVehiculo", "Duenio del vehiculo" },
            { "TitularLote", "Titular del lote" },
            { "Guardia", "Guardia que lo registro" },
            { "HorasAdentro", "Tiempo adentro" },
            { "TotalIngresos", "Total de ingresos" },
            { "TotalEgresos", "Total de egresos" },
            { "PersonasDistintas", "Personas distintas que entraron" },
            { "VehiculosDistintos", "Vehiculos distintos que entraron" },
            { "LoteTop", "Lote con mas ingresos" },
            { "IngresosLoteTop", "Ingresos de ese lote" },
            { "CategoriaTop", "Categoria con mas ingresos" },
            { "AdentroAhora", "Personas adentro ahora mismo" }
        };

        // Sin etiqueta propia: "PersonasDistintas" -> "Personas distintas"
        private static string NombreLegible(string columna)
        {
            string etiqueta;
            if (ETIQUETAS.TryGetValue(columna, out etiqueta))
                return etiqueta;

            string texto = Regex.Replace(columna, "(?<=[a-z])(?=[A-Z])", " ").ToLowerInvariant();
            return char.ToUpper(texto[0]) + texto.Substring(1);
        }

        private static string Periodo(ConsultaElegida consulta, string porDefecto)
        {
            string periodo = SinTildes(consulta.Parametro("periodo")).Trim().ToLowerInvariant();
            return PERIODOS.Contains(periodo) ? periodo : porDefecto;
        }

        private static string TextoPeriodo(string periodo)
        {
            switch (periodo)
            {
                case "hoy": return "hoy";
                case "ayer": return "ayer";
                case "semana": return "esta semana";
                case "mes": return "este mes";
                default: return "en todo el registro";
            }
        }

        // Si pide mas de 10 se corta en 10
        private static int Top(ConsultaElegida consulta)
        {
            int top;
            if (!int.TryParse(consulta.Parametro("top"), NumberStyles.Integer, CultureInfo.InvariantCulture, out top))
                return TopPorDefecto;

            return Math.Max(1, Math.Min(TopMaximo, top));
        }

        private static JObject CrearDefinicion(string nombre, string descripcion, JObject propiedades)
        {
            return new JObject
            {
                ["type"] = "function",
                ["function"] = new JObject
                {
                    ["name"] = nombre,
                    ["description"] = descripcion,
                    ["parameters"] = new JObject
                    {
                        ["type"] = "object",
                        ["properties"] = propiedades,
                        // Ninguno es obligatorio para el modelo: si falta uno necesario se le pide al usuario,
                        // asi el modelo no inventa una patente o un lote
                        ["required"] = new JArray()
                    }
                }
            };
        }

        private static JObject ParametroPeriodo()
        {
            var parametro = Parametro("string",
                "Periodo a consultar: hoy, ayer, semana (desde el lunes), mes (desde el dia 1) o todo. " +
                "No calcules fechas. Si el usuario no lo dice, no lo completes.");
            parametro["enum"] = new JArray(PERIODOS);
            return parametro;
        }

        private static JObject Parametro(string tipo, string descripcion)
        {
            return new JObject
            {
                ["type"] = tipo,
                ["description"] = descripcion
            };
        }

        private static string LeerValor(JToken valor)
        {
            if (valor == null || valor.Type == JTokenType.Null)
                return "";

            // Los numeros con InvariantCulture para no tener problemas con la coma decimal
            if (valor.Type == JTokenType.Integer || valor.Type == JTokenType.Float)
                return Convert.ToString(((JValue)valor).Value, CultureInfo.InvariantCulture);

            return valor.ToString().Trim();
        }

        private static string SinTildes(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return "";

            string descompuesto = texto.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (char c in descompuesto)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
