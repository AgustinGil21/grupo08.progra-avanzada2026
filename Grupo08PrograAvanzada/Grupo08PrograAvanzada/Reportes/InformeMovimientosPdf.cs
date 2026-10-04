using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;

namespace Grupo08PrograAvanzada.Reportes
{
    public enum TipoInforme
    {
        Persona,
        Categoria,
        Lote,
        Fechas
    }

    public class InformeMovimientosPdf
    {
        private static readonly BaseColor ColorTexto = new BaseColor(17, 24, 39);
        private static readonly BaseColor ColorGris = new BaseColor(107, 114, 128);
        private static readonly BaseColor ColorLinea = new BaseColor(229, 231, 235);
        private static readonly BaseColor ColorBanda = new BaseColor(243, 244, 246);
        private static readonly BaseColor ColorZebra = new BaseColor(249, 250, 251);

        private class Columna
        {
            public string Titulo;
            public string Campo;
            public float Ancho;

            public Columna(string titulo, string campo, float ancho)
            {
                Titulo = titulo;
                Campo = campo;
                Ancho = ancho;
            }
        }

        public static byte[] Generar(TipoInforme tipo, DataTable movimientos, string nombreBarrio,
            string domicilio, string generadoPor, IList<KeyValuePair<string, string>> filtros)
        {
            if (movimientos == null)
            {
                throw new Exception("No se pudo generar el informe: no hay datos de movimientos.");
            }

            try
            {
                using (var ms = new MemoryStream())
                {
                    var doc = new Document(PageSize.A4, 40f, 40f, 40f, 55f);
                    var writer = PdfWriter.GetInstance(doc, ms);
                    writer.PageEvent = new PieDePagina(nombreBarrio + " \u00B7 Informe confidencial, solo administradores");

                    doc.Open();

                    doc.Add(ArmarCabecera(nombreBarrio, domicilio, generadoPor));

                    var fTitulo = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, Font.NORMAL, ColorTexto);
                    var titulo = new Paragraph("Informe de movimientos por " + NombreTipo(tipo), fTitulo);
                    titulo.SpacingBefore = 14f;
                    titulo.SpacingAfter = 8f;
                    doc.Add(titulo);

                    if (filtros != null && filtros.Count > 0)
                    {
                        doc.Add(ArmarBandaFiltros(filtros));
                    }

                    doc.Add(ArmarTabla(tipo, movimientos));
                    doc.Add(ArmarTotales(movimientos));

                    doc.Close();
                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo generar el PDF del informe: " + ex.Message);
            }
        }

        private static string NombreTipo(TipoInforme tipo)
        {
            switch (tipo)
            {
                case TipoInforme.Persona: return "persona";
                case TipoInforme.Categoria: return "categor\u00EDa";
                case TipoInforme.Lote: return "lote";
                default: return "rango de fechas";
            }
        }

        private static List<Columna> ColumnasDe(TipoInforme tipo)
        {
            var fecha = new Columna("Fecha y hora", "fecha_hora", 17f);
            var persona = new Columna("Persona", "Persona", 22f);
            var documento = new Columna("Documento", "Documento", 17f);
            var categoria = new Columna("Categor\u00EDa", "Categoria", 14f);
            var lote = new Columna("Lote", "Lote", 11f);
            var garita = new Columna("Garita", "Garita", 13f);
            var movimiento = new Columna("Movimiento", "Movimiento", 12f);

            switch (tipo)
            {
                case TipoInforme.Persona:
                    return new List<Columna> { fecha, categoria, lote, garita, movimiento };
                case TipoInforme.Categoria:
                    return new List<Columna> { fecha, persona, documento, lote, garita, movimiento };
                case TipoInforme.Lote:
                    return new List<Columna> { fecha, persona, documento, categoria, garita, movimiento };
                default:
                    return new List<Columna> { fecha, persona, documento, categoria, lote, garita, movimiento };
            }
        }

        private static PdfPTable ArmarCabecera(string nombreBarrio, string domicilio, string generadoPor)
        {
            var fNombre = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, Font.NORMAL, ColorTexto);
            var fChico = FontFactory.GetFont(FontFactory.HELVETICA, 9, Font.NORMAL, ColorGris);

            var tabla = new PdfPTable(2);
            tabla.WidthPercentage = 100f;
            tabla.SetWidths(new float[] { 60f, 40f });

            var izquierda = new Phrase();
            izquierda.Add(new Chunk(nombreBarrio ?? "", fNombre));
            if (!string.IsNullOrWhiteSpace(domicilio))
            {
                izquierda.Add(new Chunk("\n" + domicilio, fChico));
            }

            var celdaIzq = new PdfPCell(izquierda);
            celdaIzq.Border = Rectangle.NO_BORDER;
            celdaIzq.PaddingBottom = 8f;
            tabla.AddCell(celdaIzq);

            string texto = "Generado: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm")
                         + "\nPor: " + (generadoPor ?? "");
            var celdaDer = new PdfPCell(new Phrase(texto, fChico));
            celdaDer.Border = Rectangle.NO_BORDER;
            celdaDer.PaddingBottom = 8f;
            celdaDer.HorizontalAlignment = Element.ALIGN_RIGHT;
            tabla.AddCell(celdaDer);

            return tabla;
        }

        private static PdfPTable ArmarBandaFiltros(IList<KeyValuePair<string, string>> filtros)
        {
            var fEtiqueta = FontFactory.GetFont(FontFactory.HELVETICA, 9, Font.NORMAL, ColorGris);
            var fValor = FontFactory.GetFont(FontFactory.HELVETICA, 9, Font.NORMAL, ColorTexto);

            var frase = new Phrase();
            foreach (var f in filtros)
            {
                frase.Add(new Chunk(f.Key + ": ", fEtiqueta));
                frase.Add(new Chunk(f.Value + "      ", fValor));
            }

            var celda = new PdfPCell(frase);
            celda.Border = Rectangle.NO_BORDER;
            celda.Padding = 8f;

            var tabla = new PdfPTable(1);
            tabla.WidthPercentage = 100f;
            tabla.SpacingAfter = 12f;
            tabla.AddCell(celda);
            return tabla;
        }

        private static PdfPTable ArmarTabla(TipoInforme tipo, DataTable movimientos)
        {
            var columnas = ColumnasDe(tipo);
            var fHeader = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 8.5f, Font.NORMAL, ColorTexto);
            var fCelda = FontFactory.GetFont(FontFactory.HELVETICA, 8.5f, Font.NORMAL, ColorTexto);

            var tabla = new PdfPTable(columnas.Count);
            tabla.WidthPercentage = 100f;
            var anchos = new float[columnas.Count];
            for (int i = 0; i < columnas.Count; i++)
            {
                anchos[i] = columnas[i].Ancho;
            }
            tabla.SetWidths(anchos);
            tabla.HeaderRows = 1;

            foreach (var col in columnas)
            {
                var h = new PdfPCell(new Phrase(col.Titulo, fHeader));
                h.Border = Rectangle.NO_BORDER;
                h.Padding = 6f;
                tabla.AddCell(h);
            }

            if (movimientos.Rows.Count == 0)
            {
                var vacio = new PdfPCell(new Phrase("No hay movimientos para los filtros elegidos.", fCelda));
                vacio.Colspan = columnas.Count;
                vacio.Border = Rectangle.BOTTOM_BORDER;
                vacio.BorderColor = ColorLinea;
                vacio.Padding = 10f;
                tabla.AddCell(vacio);
                return tabla;
            }

            int n = 0;
            foreach (DataRow fila in movimientos.Rows)
            {
                BaseColor fondo = null;
                foreach (var col in columnas)
                {
                    var c = new PdfPCell(new Phrase(Texto(fila, col.Campo), fCelda));
                    c.Border = Rectangle.BOTTOM_BORDER;
                    c.BorderColor = ColorLinea;
                    c.BorderWidth = 0.5f;
                    c.Padding = 5f;
                    if (fondo != null)
                    {
                        c.BackgroundColor = fondo;
                    }
                    tabla.AddCell(c);
                }
                n++;
            }

            return tabla;
        }

        private static Paragraph ArmarTotales(DataTable movimientos)
        {
            int ingresos = 0;
            int egresos = 0;
            foreach (DataRow fila in movimientos.Rows)
            {
                string mov = Texto(fila, "Movimiento");
                if (mov == "Ingreso") ingresos++;
                else if (mov == "Egreso") egresos++;
            }

            var fBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, Font.NORMAL, ColorTexto);
            var p = new Paragraph();
            p.SpacingBefore = 8f;
            p.Add(new Chunk("Total de movimientos: " + movimientos.Rows.Count + "      ", fBold));
            p.Add(new Chunk("Ingresos: " + ingresos + "      ", fBold));
            p.Add(new Chunk("Egresos: " + egresos, fBold));
            return p;
        }

        private static string Texto(DataRow fila, string campo)
        {
            if (!fila.Table.Columns.Contains(campo) || fila[campo] == DBNull.Value)
            {
                return "";
            }

            if (campo == "fecha_hora")
            {
                return Convert.ToDateTime(fila[campo]).ToString("dd/MM/yyyy HH:mm");
            }

            return Convert.ToString(fila[campo]);
        }

        private class PieDePagina : PdfPageEventHelper
        {
            private const float AnchoTotal = 12f;
            private readonly string _textoIzquierda;
            private PdfTemplate _total;
            private BaseFont _bf;

            public PieDePagina(string textoIzquierda)
            {
                _textoIzquierda = textoIzquierda;
            }

            public override void OnOpenDocument(PdfWriter writer, Document document)
            {
                _bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                _total = writer.DirectContent.CreateTemplate(AnchoTotal, 12f);
            }

            public override void OnEndPage(PdfWriter writer, Document document)
            {
                PdfContentByte cb = writer.DirectContent;
                float y = document.BottomMargin - 20f;
                float izq = document.LeftMargin;
                float der = document.PageSize.Width - document.RightMargin;

                cb.SetLineWidth(0.5f);
                cb.SetColorStroke(ColorLinea);
                cb.MoveTo(izq, y + 12f);
                cb.LineTo(der, y + 12f);
                cb.Stroke();

                cb.BeginText();
                cb.SetFontAndSize(_bf, 8f);
                cb.SetColorFill(ColorGris);
                cb.SetTextMatrix(izq, y);
                cb.ShowText(_textoIzquierda);
                cb.EndText();

                string texto = "P\u00E1gina " + writer.PageNumber + " de ";
                float ancho = _bf.GetWidthPoint(texto, 8f);
                cb.BeginText();
                cb.SetFontAndSize(_bf, 8f);
                cb.SetColorFill(ColorGris);
                cb.SetTextMatrix(der - AnchoTotal - ancho, y);
                cb.ShowText(texto);
                cb.EndText();

                cb.AddTemplate(_total, der - AnchoTotal, y);
            }

            public override void OnCloseDocument(PdfWriter writer, Document document)
            {
                _total.BeginText();
                _total.SetFontAndSize(_bf, 8f);
                _total.SetColorFill(ColorGris);
                _total.SetTextMatrix(0f, 0f);
                _total.ShowText((writer.PageNumber - 1).ToString());
                _total.EndText();
            }
        }
    }
}