<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Registrarse.aspx.cs" Inherits="Grupo08PrograAvanzada.Registrarse" %>

<!DOCTYPE html>

<html>
    <head runat="server">
        <meta charset="utf-8" />
        <title> Barrio Cerrado I - Crear usuario</title>
        <webopt:bundlereference runat="server" path="~/Content/css" />
        <link href="Content/Site.css" rel="stylesheet" />
        <link href="~/favicon.ico" rel="shortcut icon" type="image/x-icon" />
    </head>
    <body class="login-page">
        <form id="form1" runat="server">

            <uc:Card ID="cardregistro" runat="server">
                <Contenido>

                    <h2> Barrio Cerrado I </h2>
                    <p> Crear usuario </p>

                    <asp:Label ID="lbl_error" runat="server" CssClass="input-error" Visible="false" />

                    <div id="paso_documento" runat="server">
                        <p class="page-sub">
                            Tu documento ya tiene que estar cargado en el padrón del barrio.
                            Si no aparece, pedile el alta a un administrador.
                        </p>
                        <uc:Input ID="input_documento" runat="server" Label="Número de documento" Type="Text" />
                        <uc:Button ID="btn_verificar" runat="server" TextButton="Continuar"
                            TypeButton="Save" OnButtonClick="btn_verificar_ButtonClick" />
                    </div>

                    <div id="paso_credenciales" runat="server" visible="false">
                        <asp:Label ID="lbl_persona" runat="server" CssClass="page-sub" />
                        <uc:Input ID="input_correo" runat="server" Label="Correo" Type="Email" />
                        <uc:Input ID="input_password" runat="server" Label="Contraseña" Type="Password" />
                        <uc:Input ID="input_password_repetir" runat="server" Label="Repetir contraseña" Type="Password" />
                        <p class="page-sub">
                            Mínimo 8 caracteres, combinando letras y números.
                        </p>
                        <uc:Button ID="btn_crear" runat="server" TextButton="Crear usuario"
                            TypeButton="Save" OnButtonClick="btn_crear_ButtonClick" />
                    </div>

                    <div id="paso_listo" runat="server" visible="false">
                        <p class="page-sub">
                            Listo, tu usuario quedó creado. Todavía no tenés permisos asignados:
                            un administrador te tiene que habilitar antes de que puedas usar el sistema.
                        </p>
                    </div>

                    <p class="page-sub"><a href="Login.aspx">Volver al inicio de sesión</a></p>

                </Contenido>
            </uc:Card>

        </form>
    </body>
</html>
