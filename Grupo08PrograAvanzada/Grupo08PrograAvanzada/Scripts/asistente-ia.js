// Asistente IA (Ollama) del panel de administración. Habla con AsistenteIA.ashx.
// Con USAR_RESPUESTAS_FALSAS = true se simulan las respuestas para probar los estados del diseño sin Ollama.
(function () {
    'use strict';

    var USAR_RESPUESTAS_FALSAS = false;
    var SIMULAR_DESCONECTADO = false;   // para probar el estado 2 del diseño
    var MAX_MENSAJES = 30;

    var BIENVENIDA = {
        Tipo: 'texto',
        Texto: 'Hola, soy el asistente del barrio. Preguntame sobre lotes, personas, vehículos y movimientos.'
    };

    function svg(contenido) {
        return '<svg viewBox="0 0 24 24" aria-hidden="true">' + contenido + '</svg>';
    }

    var ICONOS = {
        bot: svg('<path d="M12 8V4H8"/><rect width="16" height="12" x="4" y="8" rx="2"/><path d="M2 14h2"/><path d="M20 14h2"/><path d="M15 13v2"/><path d="M9 13v2"/>'),
        x: svg('<path d="M18 6 6 18"/><path d="m6 6 12 12"/>'),
        enviar: svg('<path d="m22 2-7 20-4-9-9-4Z"/><path d="M22 2 11 13"/>'),
        wifiOff: svg('<path d="M12 20h.01"/><path d="M8.5 16.43a5 5 0 0 1 7 0"/><path d="M5 12.86a10 10 0 0 1 5.17-2.69"/><path d="M19 12.86a10 10 0 0 0-2-1.52"/><path d="M2 8.82a15 15 0 0 1 4.18-2.64"/><path d="M22 8.82a15 15 0 0 0-11.29-3.76"/><path d="m2 2 20 20"/>'),
        triangulo: svg('<path d="m21.73 18-8-14a2 2 0 0 0-3.48 0l-8 14A2 2 0 0 0 4 21h16a2 2 0 0 0 1.73-3"/><path d="M12 9v4"/><path d="M12 17h.01"/>'),
        alerta: svg('<circle cx="12" cy="12" r="10"/><path d="M12 8v4"/><path d="M12 16h.01"/>'),
        reintentar: svg('<path d="M3 12a9 9 0 0 1 9-9 9.75 9.75 0 0 1 6.74 2.74L21 8"/><path d="M21 3v5h-5"/><path d="M21 12a9 9 0 0 1-9 9 9.75 9.75 0 0 1-6.74-2.74L3 16"/><path d="M8 16H3v5"/>')
    };

    var raiz = document.getElementById('asistente');
    if (!raiz) return;   // no es admin: el componente no se renderizó

    var url = raiz.getAttribute('data-url');
    var claveHistorial = 'asistente-historial:' + raiz.getAttribute('data-usuario');
    var mensajes = document.getElementById('asistente-mensajes');
    var input = document.getElementById('asistente-input');
    var btnEnviar = document.getElementById('asistente-enviar');
    var avatarHeader = document.getElementById('asistente-avatar');
    var burbujaIcono = document.getElementById('asistente-burbuja-icono');
    var estadoTexto = document.getElementById('asistente-estado-texto');
    var chips = raiz.querySelectorAll('.asistente-chip');

    var historial = cargarHistorial();   // [{ rol: 'usuario', texto } | { rol: 'asistente', respuesta }]
    var conectado = false;
    var esperando = false;
    var ultimaPregunta = null;

    // ---------- Íconos ----------

    function ponerIcono(elemento, nombre) {
        elemento.setAttribute('data-icono', nombre);
        elemento.innerHTML = ICONOS[nombre];   // innerHTML solo con SVG fijos, nunca con datos
    }

    raiz.querySelectorAll('[data-icono]').forEach(function (e) {
        ponerIcono(e, e.getAttribute('data-icono'));
    });

    // ---------- Abrir / cerrar ----------

    function abrir() {
        raiz.classList.add('abierto');
        ponerIcono(burbujaIcono, 'x');
        verificarEstado();
        bajarScroll();
        input.focus();
    }

    function cerrar() {
        raiz.classList.remove('abierto');
        ponerIcono(burbujaIcono, 'bot');
    }

    // ---------- Estado de Ollama ----------

    function verificarEstado() {
        pedirEstado()
            .then(ponerEstado)
            .catch(function () { ponerEstado(false); });
    }

    function ponerEstado(ok) {
        conectado = ok;
        raiz.classList.toggle('conectado', ok);
        raiz.classList.toggle('desconectado', !ok);
        estadoTexto.textContent = ok ? 'Ollama · conectado' : 'Ollama · desconectado';
        ponerIcono(avatarHeader, ok ? 'bot' : 'wifiOff');
        input.placeholder = ok ? 'Preguntale algo sobre el barrio…' : 'Asistente no disponible';
        actualizarControles();
    }

    function actualizarControles() {
        var bloqueado = esperando || !conectado;
        input.disabled = bloqueado;
        btnEnviar.disabled = bloqueado;
        chips.forEach(function (c) { c.disabled = bloqueado; });
    }

    // ---------- Enviar ----------

    function enviar(texto) {
        texto = (texto || '').trim();
        if (!texto || esperando || !conectado) return;

        ultimaPregunta = texto;
        input.value = '';
        agregar({ rol: 'usuario', texto: texto });
        preguntar(texto, false);
    }

    // reintento = true vuelve a responder la última pregunta sin agregarla otra vez al historial del servidor
    function preguntar(texto, reintento) {
        esperando = true;
        actualizarControles();
        var pensando = dibujarPensando();

        pedirRespuesta(texto, reintento)
            .catch(function () {
                return { Tipo: 'error', Texto: 'No pude generar una respuesta. Intentá de nuevo.' };
            })
            .then(function (respuesta) {
                pensando.remove();
                esperando = false;
                actualizarControles();
                agregar({ rol: 'asistente', respuesta: respuesta });
                input.focus();
            });
    }

    function agregar(item) {
        mensajes.appendChild(dibujar(item));
        bajarScroll();

        // Los errores no se guardan: el botón Reintentar solo tiene sentido en vivo
        if (item.respuesta && item.respuesta.Tipo === 'error') return;
        historial.push(item);
        historial = historial.slice(-MAX_MENSAJES);
        guardarHistorial();
    }

    // ---------- Dibujar burbujas ----------
    // Todo el texto entra con textContent: nombres y lotes vienen de la base.

    function crear(tag, clase, texto) {
        var e = document.createElement(tag);
        if (clase) e.className = clase;
        if (texto != null) e.textContent = texto;
        return e;
    }

    // El modelo marca el dato principal con **negrita**: se arma con nodos, nunca con innerHTML
    function agregarTexto(contenedor, texto) {
        var partes = String(texto == null ? '' : texto).split('**');
        partes.forEach(function (parte, i) {
            if (!parte) return;
            contenedor.appendChild(i % 2 === 1 ? crear('strong', null, parte) : document.createTextNode(parte));
        });
    }

    function dibujar(item) {
        return item.rol === 'usuario' ? dibujarUsuario(item.texto) : dibujarRespuesta(item.respuesta);
    }

    function dibujarUsuario(texto) {
        var fila = crear('div', 'asistente-fila usuario');
        fila.appendChild(crear('div', 'asistente-msg', texto));
        return fila;
    }

    function filaAsistente(esError) {
        var fila = crear('div', 'asistente-fila' + (esError ? ' error' : ''));
        var avatar = crear('span', 'asistente-avatar');
        avatar.innerHTML = ICONOS[esError ? 'alerta' : 'bot'];
        var contenido = crear('div', 'asistente-contenido');
        fila.appendChild(avatar);
        fila.appendChild(contenido);
        return { fila: fila, contenido: contenido };
    }

    function dibujarRespuesta(r) {
        var esError = r.Tipo === 'error';
        var f = filaAsistente(esError);
        var msg = crear('div', 'asistente-msg');

        if (r.Tipo === 'lista') {
            msg.appendChild(crear('div', 'asistente-msg-titulo', r.Titulo));
            (r.Filas || []).forEach(function (fila) {
                var linea = crear('div', 'asistente-lista-fila');
                linea.appendChild(crear('span', null, fila.Etiqueta));
                linea.appendChild(crear('span', null, fila.Valor));
                msg.appendChild(linea);
            });
        } else {
            // texto, detalle, sin_resultados, fuera_de_tema, falta_dato y error
            var cuerpo = crear('div');
            agregarTexto(cuerpo, r.Texto);
            msg.appendChild(cuerpo);
            (r.Detalles || []).forEach(function (d) {
                msg.appendChild(crear('div', 'asistente-msg-detalle', d));
            });
        }
        f.contenido.appendChild(msg);

        if (esError) {
            var btn = crear('button', 'asistente-reintentar');
            btn.type = 'button';
            btn.innerHTML = ICONOS.reintentar;
            btn.appendChild(document.createTextNode('Reintentar'));
            btn.addEventListener('click', function () {
                if (esperando || !ultimaPregunta) return;
                f.fila.remove();
                preguntar(ultimaPregunta, true);
            });
            f.contenido.appendChild(btn);
        }
        return f.fila;
    }

    function dibujarPensando() {
        var f = filaAsistente(false);
        var msg = crear('div', 'asistente-msg asistente-pensando');
        msg.setAttribute('aria-label', 'Pensando');
        for (var i = 0; i < 3; i++) msg.appendChild(crear('span'));
        f.contenido.appendChild(msg);
        mensajes.appendChild(f.fila);
        bajarScroll();
        return f.fila;
    }

    function bajarScroll() {
        mensajes.scrollTop = mensajes.scrollHeight;
    }

    // ---------- Historial (sessionStorage) ----------

    function cargarHistorial() {
        try { return JSON.parse(sessionStorage.getItem(claveHistorial)) || []; }
        catch (e) { return []; }
    }

    function guardarHistorial() {
        try { sessionStorage.setItem(claveHistorial, JSON.stringify(historial)); }
        catch (e) { /* sin storage el chat igual funciona, solo no se recuerda */ }
    }

    // ---------- Comunicación con el handler ----------

    function pedirEstado() {
        if (USAR_RESPUESTAS_FALSAS) return Promise.resolve(!SIMULAR_DESCONECTADO);

        return fetch(url + '?accion=estado', { credentials: 'same-origin' })
            .then(function (r) { return r.ok ? r.json() : { conectado: false }; })
            .then(function (d) { return d.conectado === true; });
    }

    function pedirRespuesta(texto, reintento) {
        if (USAR_RESPUESTAS_FALSAS) return respuestaFalsa(texto);

        return fetch(url + '?accion=' + (reintento ? 'reintentar' : 'preguntar'), {
            method: 'POST',
            credentials: 'same-origin',
            headers: { 'Content-Type': 'application/json; charset=utf-8' },
            body: JSON.stringify({ pregunta: texto })
        }).then(function (r) {
            if (r.ok) return r.json();

            // 400 (pregunta vacía o larga) y 403 (sesión vencida) traen un mensaje para mostrar
            return r.json()
                .catch(function () { return {}; })
                .then(function (d) {
                    if (d && d.error) return { Tipo: 'error', Texto: d.error };
                    throw new Error('HTTP ' + r.status);
                });
        });
    }

    // Respuestas de prueba: cada palabra clave muestra un estado del diseño
    function respuestaFalsa(texto) {
        var t = texto.toLowerCase();
        var r;

        if (t.indexOf('error') >= 0) {
            r = null;
        } else if (t.indexOf('xx999xx') >= 0) {
            r = { Tipo: 'sin_resultados', Texto: 'No encontré ingresos de la patente XX999XX.' };
        } else if (t.indexOf('patente') >= 0 && !/[a-z]{2,3}\s?\d{3}\s?[a-z]{0,2}/.test(t.replace('patente', ''))) {
            r = { Tipo: 'falta_dato', Texto: '¿De qué patente? Escribila completa, por ejemplo AB123CD.' };
        } else if (t.indexOf('patente') >= 0) {
            r = {
                Tipo: 'detalle',
                Texto: 'La patente AB123CD ingresó al Lote 14 el 18/10 a las 09:42.',
                Detalles: [
                    'Conducía: María Fernández · Vehículo de: Juan Pérez',
                    'Titular del lote: Juan Pérez · Guardia: Ramón Gutiérrez (Garita Norte)'
                ]
            };
        } else if (t.indexOf('adentro') >= 0) {
            r = {
                Tipo: 'lista',
                Titulo: 'Personas adentro ahora',
                Filas: [
                    { Etiqueta: 'Fernández, María', Valor: 'Lote 14 · 2 h' },
                    { Etiqueta: 'Gómez, Lucas', Valor: 'Lote 7 · 45 min' }
                ]
            };
        } else if (t.indexOf('lote') >= 0 || t.indexOf('ingreso') >= 0) {
            r = {
                Tipo: 'lista',
                Titulo: 'Lotes con más ingresos esta semana',
                Filas: [
                    { Etiqueta: '1. Lote 14', Valor: '23 ingresos' },
                    { Etiqueta: '2. Lote 7', Valor: '19 ingresos' },
                    { Etiqueta: '3. Lote 22', Valor: '16 ingresos' },
                    { Etiqueta: '4. Lote 3', Valor: '14 ingresos' },
                    { Etiqueta: '5. Lote 11', Valor: '11 ingresos' }
                ]
            };
        } else if (t.indexOf('resumen') >= 0) {
            r = { Tipo: 'texto', Texto: 'Esta semana hubo 112 ingresos de 64 personas distintas. El lote con más ingresos fue el Lote 14.' };
        } else {
            r = { Tipo: 'fuera_de_tema', Texto: 'Solo puedo responder sobre lotes, personas, vehículos y movimientos del barrio.' };
        }

        return new Promise(function (ok, falla) {
            setTimeout(function () { r ? ok(r) : falla(new Error('simulado')); }, 1500);
        });
    }

    // ---------- Eventos ----------

    document.getElementById('asistente-burbuja').addEventListener('click', function () {
        if (raiz.classList.contains('abierto')) cerrar(); else abrir();
    });

    document.getElementById('asistente-cerrar').addEventListener('click', cerrar);

    btnEnviar.addEventListener('click', function () { enviar(input.value); });

    input.addEventListener('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();   // por las dudas: nunca disparar un postback
            enviar(input.value);
        }
    });

    chips.forEach(function (chip) {
        chip.addEventListener('click', function () {
            var completar = chip.getAttribute('data-completar');
            if (completar) {
                input.value = completar;
                input.focus();
                input.setSelectionRange(completar.length, completar.length);
            } else {
                enviar(chip.getAttribute('data-enviar'));
            }
        });
    });

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && raiz.classList.contains('abierto')) cerrar();
    });

    // ---------- Inicio ----------

    mensajes.appendChild(dibujarRespuesta(BIENVENIDA));
    historial.forEach(function (item) { mensajes.appendChild(dibujar(item)); });
    verificarEstado();   // pinta el puntito de la burbuja aunque el panel esté cerrado
})();
