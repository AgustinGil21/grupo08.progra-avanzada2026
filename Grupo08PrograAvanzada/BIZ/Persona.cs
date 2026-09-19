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
