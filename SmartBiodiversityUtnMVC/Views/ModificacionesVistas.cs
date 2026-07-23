// =====================================================================
//  MODIFICACIÓN EN Index.cshtml (Dashboard)
//  
//  En el modal de registro de especies del Dashboard, agrega un 
//  <select> de facultad antes del botón "Registrar":
//  
//  Busca el <form> con asp-action="EspecieCreate" y agrega esto
//  DENTRO del form, justo antes del input de imagen:
// =====================================================================

/*
<div class="form-group" style="margin-bottom:16px;">
    <label style="font-weight:bold;margin-bottom:6px;display:block;">Facultad (opcional)</label>
    <select name="FacultadId" id="FacultadId" 
            style="width:100%;padding:12px;border:1px solid #e5e7eb;border-radius:10px;font-size:14px;">
        <option value="">-- Sin vincular a facultad --</option>
    </select>
</div>
*/

// =====================================================================
//  AGREGA ESTE SCRIPT al final del Index.cshtml (antes del </body>)
//  para cargar las facultades en el select del modal:
// =====================================================================

/*
<script>
    // Cargar facultades para el selector del modal de crear especie
    (async function cargarFacultades() {
        try {
            const resp = await fetch('https://smartbiodiversityapi.onrender.com/api/Facultades');
            if (!resp.ok) return;
            const facultades = await resp.json();
            const select = document.getElementById('FacultadId');
            if (!select) return;
            facultades.forEach(f => {
                const opt = document.createElement('option');
                opt.value = f.idFacultad;
                opt.textContent = f.nombre;
                select.appendChild(opt);
            });
        } catch(e) { console.warn('No se pudieron cargar facultades:', e); }
    })();
</script>
*/

// =====================================================================
//  MODIFICACIÓN EN Flora.cshtml y Fauna.cshtml
//  
//  En ambos formularios de creación (FloraCreate y FaunaCreate),
//  agrega el mismo <select> de facultad antes del botón "Registrar".
//  El campo name="FacultadId" se enlazará automáticamente al DTO.
// =====================================================================
