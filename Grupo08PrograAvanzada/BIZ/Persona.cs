using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIZ
{
    public class Persona
    {
        private const int ID_OBJETO_VIGENTE = 5;

        public static DataSet ListarCategorias()
        {
            var ds = new DataSet();
            string q = @"SELECT * FROM Categoria";

            try
            {
                var CN = Conexion.AbrirConexion();

                using (var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
                {
                    using (var da = new System.Data.SqlClient.SqlDataAdapter(cmd))
                    {
                        da.Fill(ds);
                    }
                }

                return ds;
            }
            catch (Exception ex)
            {
                throw new Exception("Error al listar categorías: " + ex.Message);
            }
        }

        public static DataTable BuscarPadron(string texto, string idCategoria)
        {
            var dt = new DataTable();
            string q = @"
            SELECT  p.id_persona,
                    c.nombre + ' ' + c.apellido AS Persona,
                    c.nro_documento,
                    ISNULL(STUFF((SELECT ', ' + cat.descripcion
                                  FROM PersonaCategoria pc
                                  INNER JOIN Categoria cat ON cat.id_categoria = pc.id_categoria
                                  WHERE pc.id_persona = p.id_persona
                                  ORDER BY cat.descripcion
                                  FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'), 1, 2, ''), '') AS Categoria,
                    o.descripcion AS Estado
            FROM Persona p
            INNER JOIN Cuenta c ON c.id_cuenta = p.id_cuenta
            LEFT JOIN Estado e ON e.id_estado = c.id_estado
            LEFT JOIN Objeto o ON o.id_objeto = e.id_objeto
            WHERE (@texto = ''
                   OR c.nombre LIKE '%' + @texto + '%'
                   OR c.apellido LIKE '%' + @texto + '%'
                   OR c.nro_documento LIKE '%' + @texto + '%')
              AND (@idCategoria = '' OR EXISTS (
                        SELECT 1
                        FROM PersonaCategoria pc2
                        WHERE pc2.id_persona = p.id_persona
                          AND CAST(pc2.id_categoria AS varchar(10)) = @idCategoria))
            ORDER BY c.apellido, c.nombre;";

            try
            {
                var CN = Conexion.AbrirConexion();
                using (var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@texto", texto ?? "");
                    cmd.Parameters.AddWithValue("@idCategoria", idCategoria ?? "");

                    using (var da = new System.Data.SqlClient.SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo buscar en el padron: " + ex.Message);
            }
        }

        public static DataTable BuscarPersonaPorDocumento(string nroDocumento)
        {
            var dt = new DataTable();
            string q = @"
            SELECT  p.id_persona,
                    c.apellido,
                    c.nombre,
                    pa.descripcion AS Nacionalidad,
                    ISNULL(STUFF((SELECT ', ' + cat.descripcion
                                  FROM PersonaCategoria pc
                                  INNER JOIN Categoria cat ON cat.id_categoria = pc.id_categoria
                                  WHERE pc.id_persona = p.id_persona
                                  ORDER BY cat.descripcion
                                  FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'), 1, 2, ''), '') AS Categoria
            FROM Persona p
            INNER JOIN Cuenta c ON c.id_cuenta = p.id_cuenta
            LEFT JOIN Pais pa   ON pa.id_pais  = c.id_pais
            WHERE c.nro_documento = @nroDocumento;";

            try
            {
                var CN = Conexion.AbrirConexion();
                using (var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@nroDocumento", nroDocumento ?? "");

                    using (var da = new System.Data.SqlClient.SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo buscar la persona por documento: " + ex.Message);
            }
        }

        public static string ObtenerFotoReferencia(string nroDocumento)
        {
            string q = @"
            SELECT TOP 1 p.foto_referencia
            FROM Persona p
            INNER JOIN Cuenta c ON c.id_cuenta = p.id_cuenta
            WHERE c.nro_documento = @nroDocumento;";

            try
            {
                using (var CN = Conexion.AbrirConexion())
                using (var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@nroDocumento", nroDocumento ?? "");
                    object valor = cmd.ExecuteScalar();
                    return valor == null || valor == DBNull.Value ? null : Convert.ToString(valor);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo obtener la foto de la persona: " + ex.Message);
            }
        }

        public static DataTable ListarCategoriasDePersona(int idPersona)
        {
            var dt = new DataTable();
            string q = @"
            SELECT cat.id_categoria, cat.descripcion
            FROM PersonaCategoria pc
            INNER JOIN Categoria cat ON cat.id_categoria = pc.id_categoria
            WHERE pc.id_persona = @idPersona
              AND cat.descripcion <> 'Propietario'
            ORDER BY cat.descripcion;";

            try
            {
                var CN = Conexion.AbrirConexion();
                using (var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@idPersona", idPersona);

                    using (var da = new System.Data.SqlClient.SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudieron listar las categorías de la persona: " + ex.Message);
            }
        }

        // El COALESCE es para que un campo vacio no pise un dato que ya estaba cargado.
        public static void ActualizarDatosDeContacto(int idPersona, string telefono, string fotoReferencia)
        {
            string q = @"
            SET XACT_ABORT ON;
            BEGIN TRANSACTION;

                UPDATE c
                SET c.telefono = COALESCE(@telefono, c.telefono)
                FROM Cuenta c
                INNER JOIN Persona p ON p.id_cuenta = c.id_cuenta
                WHERE p.id_persona = @idPersona;

                UPDATE Persona
                SET foto_referencia = COALESCE(@fotoReferencia, foto_referencia)
                WHERE id_persona = @idPersona;

            COMMIT TRANSACTION;";

            try
            {
                var CN = Conexion.AbrirConexion();
                using (var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@idPersona", idPersona);
                    cmd.Parameters.AddWithValue("@telefono",
                        string.IsNullOrWhiteSpace(telefono) ? (object)DBNull.Value : telefono);
                    cmd.Parameters.AddWithValue("@fotoReferencia",
                        string.IsNullOrWhiteSpace(fotoReferencia) ? (object)DBNull.Value : fotoReferencia);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudieron actualizar los datos de contacto: " + ex.Message);
            }
        }

        public static bool TieneLicenciaVigente(int idPersona)
        {
            string q = @"
            SELECT TOP 1 1
            FROM PersonaLicencia
            WHERE id_persona = @idPersona
              AND vigencia_licencia >= CAST(GETDATE() AS date);";

            try
            {
                var CN = Conexion.AbrirConexion();
                using (var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@idPersona", idPersona);
                    return cmd.ExecuteScalar() != null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo comprobar la licencia: " + ex.Message);
            }
        }

        public static DataTable BuscarVisitasIngresadasHoy(string textoBusqueda)
        {
            var dt = new DataTable();
            string q = @"
            SELECT  m.id_movimiento,
                    p.id_persona,
                    c.apellido + ', ' + c.nombre AS Persona,
                    c.nro_documento,
                    m.fecha_hora_ingreso,
                    ISNULL(lo.descripcion, '') AS Lote,
                    o.descripcion AS Estado
            FROM Movimiento m
            INNER JOIN Persona p ON p.id_persona = m.id_persona
            INNER JOIN Cuenta c  ON c.id_cuenta  = p.id_cuenta
            LEFT JOIN Lote lo    ON lo.id_lote   = m.id_lote
            LEFT JOIN Estado e   ON e.id_estado  = m.id_estado
            LEFT JOIN Objeto o   ON o.id_objeto  = e.id_objeto
            WHERE CAST(m.fecha_hora_ingreso AS date) = CAST(GETDATE() AS date)
              AND (@texto = '' OR c.nro_documento LIKE '%' + @texto + '%')
            ORDER BY m.fecha_hora_ingreso DESC;";

            try
            {
                var CN = Conexion.AbrirConexion();
                using (var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@texto", textoBusqueda ?? "");

                    using (var da = new System.Data.SqlClient.SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }

                return dt;
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudieron buscar las visitas de hoy: " + ex.Message);
            }
        }

        public static int DarDeAltaLicencia(int idPersona, string nroLicencia, DateTime vigenciaLicencia)
        {
            string q = @"
            SET XACT_ABORT ON;
            BEGIN TRANSACTION;

                DECLARE @idLicencia INT = (SELECT ISNULL(MAX(id_persona_licencia), 0) + 1 FROM PersonaLicencia);
                SET IDENTITY_INSERT PersonaLicencia ON;
                INSERT INTO PersonaLicencia (id_persona_licencia, id_persona, id_estado, nro_licencia,
                    vigencia_licencia, fecha_alta)
                VALUES (@idLicencia, @idPersona, @idEstado, @nroLicencia, @vigenciaLicencia, GETDATE());
                SET IDENTITY_INSERT PersonaLicencia OFF;

            COMMIT TRANSACTION;

            SELECT @idLicencia;";

            try
            {
                var CN = Conexion.AbrirConexion();
                using (var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@idPersona", idPersona);
                    cmd.Parameters.AddWithValue("@nroLicencia", nroLicencia);
                    cmd.Parameters.AddWithValue("@vigenciaLicencia", vigenciaLicencia);
                    cmd.Parameters.AddWithValue("@idEstado", ID_OBJETO_VIGENTE);
                    return (int)cmd.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudo dar de alta la licencia: " + ex.Message);
            }
        }

        public static int DarDeAltaPersonaConCategorias(string nombre, string apellido, int idPais, string telefono,
            DateTime fechaNacimiento, int idTipoDocumento, string nroDocumento, string fotoReferencia,
            List<int> idCategorias)
        {
            int idPersona = Auth.DarDeAltaPersona(nombre, apellido, idPais, telefono, fechaNacimiento, idTipoDocumento, nroDocumento, fotoReferencia);

            if (idCategorias == null || idCategorias.Count == 0)
            {
                return idPersona;
            }

            string q = @"
            IF NOT EXISTS (SELECT 1 FROM PersonaCategoria
                           WHERE id_persona = @idPersona AND id_categoria = @idCategoria)
                INSERT INTO PersonaCategoria (id_persona, id_categoria)
                VALUES (@idPersona, @idCategoria);";

            try
            {
                var CN = Conexion.AbrirConexion();
                using (var cmd = new System.Data.SqlClient.SqlCommand(q, CN))
                {
                    cmd.Parameters.AddWithValue("@idPersona", idPersona);
                    var pIdCategoria = cmd.Parameters.Add("@idCategoria", System.Data.SqlDbType.Int);

                    foreach (int idCategoria in idCategorias)
                    {
                        pIdCategoria.Value = idCategoria;
                        cmd.ExecuteNonQuery();
                    }
                }

                return idPersona;
            }
            catch (Exception ex)
            {
                throw new Exception("No se pudieron asignar las categorías: " + ex.Message);
            }
        }
    }
}
