<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="OlvideContrasenia.aspx.cs" Inherits="Grupo08PrograAvanzada.OlvideContrasenia" %>

<!DOCTYPE html>

<html>
    <head runat="server">
        <title> Barrio Cerrado I - Recuperar contraseña</title>
        <webopt:bundlereference runat="server" path="~/Content/css" />
        <link href="Content/Site.css" rel="stylesheet" />
        <link href="~/favicon.ico" rel="shortcut icon" type="image/x-icon" />
    </head>
    <body class="login-page">
        <form id="form1" runat="server">

            <uc:Card ID="cardrecuperar" runat="server">
                <Contenido>

                    <h2> Barrio Cerrado I </h2>
                    <p> Recuperar contraseña </p>

                    <asp:Label ID="lbl_error" runat="server" CssClass="input-error" Visible="false" />

                    <div id="paso_correo" runat="server">
                        <p class="page-sub">
                            Ingresá el correo de tu cuenta y te mandamos un código para elegir
                            una contraseña nueva.
                        </p>
                        <uc:Input ID="input_correo" runat="server" Label="Correo" Type="Email" />
                        <uc:Button ID="btn_enviar_codigo" runat="server" TextButton="Enviarme el código"
                            TypeButton="Save" OnButtonClick="btn_enviar_codigo_ButtonClick" />
                    </div>

                    <div id="paso_codigo" runat="server" visible="false">
                        <asp:Label ID="lbl_aviso_envio" runat="server" CssClass="page-sub" />
                        <uc:Input ID="input_codigo" runat="server" Label="Código de 6 dígitos" Type="Text" />
                        <uc:Input ID="input_password" runat="server" Label="Contraseña nueva" Type="Password" />
                        <uc:Input ID="input_password_repetir" runat="server" Label="Repetir contraseña nueva" Type="Password" />
                        <p class="page-sub">
                            Mínimo 8 caracteres, combinando letras y números.
                        </p>
                        <uc:Button ID="btn_restablecer" runat="server" TextButton="Cambiar mi contraseña"
                            TypeButton="Save" OnButtonClick="btn_restablecer_ButtonClick" />
                    </div>

                    <div id="paso_listo" runat="server" visible="false">
                        <p class="page-sub">
                            Listo, tu contraseña quedó cambiada. Ya podés iniciar sesión con la nueva.
                        </p>
                    </div>

                    <p class="page-sub"><a href="Login.aspx">Volver al inicio de sesión</a></p>

                </Contenido>
            </uc:Card>

        </form>
    </body>
</html>
