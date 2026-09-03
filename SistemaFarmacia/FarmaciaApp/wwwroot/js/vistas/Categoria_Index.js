const MODELO_BASE = {
    idCategoria: 0,
    descripcion: "",
    esActivo: 1
};

let tablaData;
let filaSeleccionada;

$(document).ready(function ()
{

    tablaData = $('#tbdata').DataTable(
    {
        responsive: true,
        "ajax": 
        {
            "url": '/Categoria/Lista',
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "idCategoria", "visible": false, "searchable": false },
            { "data": "descripcion" },
            {
                "data": "esActivo", render: function (data)
                {
                    return data == 1
                        ? `<span class="badge badge-info">Activo</span>`
                        : `<span class="badge badge-danger">No Activo</span>`;
                }
            },
            {
                "defaultContent":
                    '<button class="btn btn-primary btn-editar btn-sm mr-2"><i class="fas fa-pencil-alt"></i></button>' +
                    '<button class="btn btn-danger btn-eliminar btn-sm"><i class="fas fa-trash-alt"></i></button>',
                "orderable": false,
                "searchable": false,
                "width": "80px"
            }
        ],
        order: [[0, "desc"]],
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Categorias',
                exportOptions: {
                    columns: [1, 2]
                }
            },
            'pageLength'
        ],
        language: {
            url: "https://cdn.datatables.net/plug-ins/1.11.5/i18n/es-ES.json"
        },
    });
});

// mostrar modal
function mostrarModal(modelo = MODELO_BASE)
{
    $("#txtId").val(modelo.idCategoria);
    $("#txtDescripcion").val(modelo.descripcion);
    $("#cboEstado").val(modelo.esActivo);
    $("#modalData").modal("show");
}


$("#btnNuevo").click(function () {

    mostrarModal();
});


$("#btnGuardar").click(function ()
{
    if ($("#txtDescripcion").val().trim() === "")
    {
        toastr.warning("", "Debe completar el campo : descripción");
        $("#txtDescripcion").focus();
        return;
    }

    const modelo = structuredClone(MODELO_BASE);
    modelo.idCategoria = parseInt($("#txtId").val());
    modelo.descripcion = $("#txtDescripcion").val();
    modelo.esActivo = parseInt($("#cboEstado").val());

    $("#btnGuardar").prop("disabled", true);
    $("#modalData").find("div.modal-content").LoadingOverlay("show");

    const url = modelo.idCategoria === 0 ? "/Categoria/Crear" : "/Categoria/Editar";
    const method = modelo.idCategoria === 0 ? "POST" : "PUT";

    fetch(url,
    {
        method: method,
        headers: { "Content-Type": "application/json; charset=utf-8" },
        body: JSON.stringify(modelo)
    })
    .then(response =>
    {
        $("#modalData").find("div.modal-content").LoadingOverlay("hide");
        $("#btnGuardar").prop("disabled", false);
        return response.ok ? response.json() : Promise.reject(response);
    })
    .then(responseJson =>
    {
        if (responseJson.estado)
        {
            if (modelo.idCategoria === 0)
            {
                tablaData.row.add(responseJson.objeto).draw(false);
                swal("¡Listo!", "La categoría fue creada", "success");
            } else {
                tablaData.row(filaSeleccionada).data(responseJson.objeto).draw(false);
                filaSeleccionada = null;
                swal("¡Listo!", "La categoría fue modificada", "success");
            }
            $("#modalData").modal("hide");
        } else
        {
            swal("¡Lo sentimos!", responseJson.mensaje, "error");
        }
    });
});

function obtenerFilaOriginal(boton) {
    let tr = $(boton).closest("tr");
    return tr.hasClass("child") ? tr.prev() : tr;
}

// Editar
$("#tbdata tbody").on("click", ".btn-editar", function ()
{
    filaSeleccionada = obtenerFilaOriginal(this);
    const data = tablaData.row(filaSeleccionada).data();
    mostrarModal(data);
});

// Eliminar
$("#tbdata tbody").on("click", ".btn-eliminar", function ()
{
    const fila = obtenerFilaOriginal(this);
    const data = tablaData.row(fila).data();

    swal({
        title: "¿Estás seguro?",
        text: `Eliminar la categoría "${data.descripcion}"`,
        type: "warning",
        showCancelButton: true,
        confirmButtonClass: "btn-danger",
        confirmButtonText: "Sí, eliminar",
        cancelButtonText: "No, cancelar",
        closeOnConfirm: false,
        closeOnCancel: true
    }, function (respuesta)
    {
        if (respuesta)
        {
            $(".showSweetAlert").LoadingOverlay("show");

            fetch(`/Categoria/Eliminar?IdCategoria=${data.idCategoria}`, {
                method: "DELETE"
            })
            .then(response =>
            {
                $(".showSweetAlert").LoadingOverlay("hide");
                return response.ok ? response.json() : Promise.reject(response);
            })
            .then(responseJson => {
                if (responseJson.estado)
                {
                    tablaData.row(fila).remove().draw();
                    swal("¡Listo!", "La categoría fue eliminada", "success");
                } else {
                    swal("¡Lo sentimos!", responseJson.mensaje, "error");
                }
            });
        }
    });
});