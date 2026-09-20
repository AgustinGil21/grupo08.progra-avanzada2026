<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Grupo08PrograAvanzada.Login" %>

<!DOCTYPE html>

<html>
    <head runat="server">
        <meta charset="utf-8" />
        <title> Barrio Cerrado I - Login</title>
        <webopt:bundlereference runat="server" path="~/Content/css" />
        <link href="Content/Site.css" rel="stylesheet" />
        <link href="~/favicon.ico" rel="shortcut icon" type="image/x-icon" />
    </head>
    <body class="login-page">
        <form id="form1" runat="server">
            
            <uc:Card ID="cardlogin" runat="server">
                <Contenido>
                    
                    <h2> Barrio Cerrado I </h2>
                    <p> Login </p>

                    <div id="login_datos1" runat="server">
                        <asp:Label ID="lbl_error_login" runat="server" CssClass="input-error" Visible="false" />
                        <uc:Input ID="input_usuario" runat="server" Label="Correo" Type="Email" />
                        <uc:Input ID="input_password" runat="server" Label="Contraseña" Type="Password" />
                        <uc:Button ID="btn_login_1" runat="server" TextButton="Iniciar Sesión" 
                            TypeButton="Save" OnButtonClick="btn_login_1_ButtonClick" />

                        <p class="page-sub">
                            <a href="OlvideContrasenia.aspx">Olvidé mi contraseña</a>
                        </p>
                        <p class="page-sub">
                            ¿Todavía no tenés usuario? <a href="Registrarse.aspx">Creá el tuyo</a>
                        </p>
                    </div>

                    <div id="login_datos_guardia" runat="server" visible="false">
                        <asp:Label ID="lbl_error_guardia" runat="server" CssClass="input-error" Visible="false" />
                        <uc:DropdownList ID="ddl_garita" Titulo="Garita" runat="server" />
                        <uc:DropdownList ID="ddl_turno" Titulo="Turno" runat="server" />
                        <uc:Button ID="btn_login_2" TextButton="Iniciar Turno" 
                            TypeButton="Save" runat="server" OnButtonClick="btn_login_2_ButtonClick" />
                    </div>

                </Contenido>
            </uc:Card>

        </form>
    </body>
</html>
