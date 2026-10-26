<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="AsistenteIA.ascx.cs" Inherits="Grupo08PrograAvanzada.Componentes.AsistenteIA" %>

<link href="<%= ResolveUrl("~/Content/AsistenteIA.css") %>" rel="stylesheet" type="text/css" />

<div id="asistente" class="asistente"
     data-url="<%= ResolveUrl("~/AsistenteIA.ashx") %>"
     data-usuario="<%: Session["Usuario"] %>">

    <div class="asistente-panel">
        <div class="asistente-header">
            <span class="asistente-avatar" id="asistente-avatar" data-icono="bot"></span>
            <div class="asistente-titulos">
                <span class="asistente-titulo">Asistente IA</span>
                <span class="asistente-estado">
                    <span class="asistente-punto"></span>
                    <span id="asistente-estado-texto">Ollama · conectando…</span>
                </span>
            </div>
            <button type="button" class="asistente-icono-btn" id="asistente-cerrar" aria-label="Cerrar" data-icono="x"></button>
        </div>

        <div class="asistente-mensajes" id="asistente-mensajes" aria-live="polite"></div>

        <div class="asistente-aviso">
            <span data-icono="triangulo"></span>
            <p>El asistente no está disponible. Verificá que Ollama esté corriendo.</p>
        </div>

        <div class="asistente-chips">
            <button type="button" class="asistente-chip" data-enviar="¿Qué lote tuvo más ingresos esta semana?">Lote con más ingresos</button>
            <button type="button" class="asistente-chip" data-enviar="¿Quién está adentro ahora?">¿Quién está adentro?</button>
            <button type="button" class="asistente-chip" data-completar="¿A qué lote entró la patente ">Buscar por patente</button>
        </div>

        <div class="asistente-footer">
            <input type="text" id="asistente-input" class="asistente-input" maxlength="300"
                   placeholder="Preguntale algo sobre el barrio…" autocomplete="off" />
            <button type="button" class="asistente-enviar" id="asistente-enviar" aria-label="Enviar" data-icono="enviar"></button>
        </div>
    </div>

    <button type="button" class="asistente-burbuja" id="asistente-burbuja" aria-label="Abrir asistente">
        <span id="asistente-burbuja-icono" data-icono="bot"></span>
        <span class="asistente-punto asistente-punto-burbuja"></span>
    </button>
</div>

<script src="<%= ResolveUrl("~/Scripts/asistente-ia.js") %>"></script>
