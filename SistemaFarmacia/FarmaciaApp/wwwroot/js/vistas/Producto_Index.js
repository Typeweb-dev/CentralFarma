const MODELO_BASE = {
    idProducto: 0,
    codigoBarra: "",
    marca: "",
    descripcion: "",
    idCategoria: 0,
    stock: 0,
    urlImagen: "",
    precio: 0,
    esActivo: ""
};

let tablaData;
let filaSeleccionada;

$(document).ready(function () {
    cargarCategorias();
    inicializarTabla();
});

function cargarCategorias() {
    fetch("/Categoria/Lista")
        .then(response => response.ok ? response.json() : Promise.reject(response))
        .then(responseJson => {
            if (responseJson.data.length > 0) {
                responseJson.data.forEach((item) => {
                    $("#cboCategoria").append(
                        $("<option>").val(item.idCategoria).text(item.descripcion)
                    );
                });
            }
        });
}

function inicializarTabla() {
    if ($.fn.DataTable.isDataTable('#tbdata')) {
        $('#tbdata').DataTable().clear().destroy();
    }

    tablaData = $('#tbdata').DataTable({
        responsive: true,
        destroy: true,
        ajax: {
            url: '/Producto/Lista',
            type: "GET",
            datatype: "json",
            error: function (xhr, error, thrown) {
                console.error("Error al cargar productos:", xhr.responseText);
            }
        },
        columns: [
            { data: "idProducto", visible: false, searchable: false },
            {
                data: "urlImagen", render: function (data) {
                    return `<img style="height:60px" src="${data}" class="rounded mx-auto d-block"/>`;
                }
            },
            { data: "codigoBarra" },
            { data: "marca" },
            { data: "descripcion" },
            { data: "nombreCategoria" },
            { data: "stock" },
            { data: "precio" },
            {
                data: "esActivo", render: function (data) {
                    return data == 1
                        ? `<span class="badge badge-info">Activo</span>`
                        : `<span class="badge badge-danger">No Activo</span>`;
                }
            },
            {
                defaultContent:
                    '<button class="btn btn-primary btn-editar btn-sm mr-2"><i class="fas fa-pencil-alt"></i></button>' +
                    '<button class="btn btn-danger btn-eliminar btn-sm"><i class="fas fa-trash-alt"></i></button>',
                orderable: false,
                searchable: false,
                width: "80px"
            }
        ],
        order: [[0, "desc"]],
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Productos',
                exportOptions: {
                    columns: [2, 3, 4, 5, 6]
                }
            },
            'pageLength'
        ],
        language: {
            url: "https://cdn.datatables.net/plug-ins/1.11.5/i18n/es-ES.json"
        }
    });
}

// Mostrar modal
function mostrarModal(modelo = MODELO_BASE) {
    $("#txtId").val(modelo.idProducto);
    $("#txtCodigoBarra").val(modelo.codigoBarra);
    $("#txtMarca").val(modelo.marca);
    $("#txtDescripcion").val(modelo.descripcion);
    $("#cboCategoria").val(modelo.idCategoria || $("#cboCategoria option:first").val());
    $("#txtStock").val(modelo.stock);
    $("#txtPrecio").val(modelo.precio);
    $("#cboEstado").val(modelo.esActivo);
    $("#txtImagen").val("");
    $("#imgProducto").attr("src", modelo.urlImagen);
    $("#modalData").modal("show");
}

$("#btnNuevo").click(function () {
    mostrarModal();
});

$("#btnGuardar").click(function () {
    const inputs = $("input.input-validar").serializeArray();
    const inputs_sin_valor = inputs.filter((item) => item.value.trim() == "");

    if (inputs_sin_valor.length > 0) {
        const mensaje = `Debe completar el campo: "${inputs_sin_valor[0].name}"`;
        toastr.warning("", mensaje);
        $(`input[name="${inputs_sin_valor[0].name}"]`).focus();
        return;
    }

    const modelo = structuredClone(MODELO_BASE);
    modelo.idProducto = parseInt($("#txtId").val());
    modelo.codigoBarra = $("#txtCodigoBarra").val();
    modelo.marca = $("#txtMarca").val();
    modelo.descripcion = $("#txtDescripcion").val();
    modelo.idCategoria = $("#cboCategoria").val();
    modelo.stock = $("#txtStock").val();
    modelo.precio = $("#txtPrecio").val();
    modelo.esActivo = $("#cboEstado").val();

    const inputFoto = document.getElementById("txtImagen");
    const formData = new FormData();

    formData.append("imagen", inputFoto.files[0]);
    formData.append("modelo", JSON.stringify(modelo));

    $("#modalData").find("div.modal-content").LoadingOverlay("show");

    const url = modelo.idProducto == 0 ? "/Producto/Crear" : "/Producto/Editar";
    const method = modelo.idProducto == 0 ? "POST" : "PUT";

    fetch(url, {
        method: method,
        body: formData
    })
        .then(response => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide");
            return response.ok ? response.json() : Promise.reject(response);
        })
        .then(responseJson => {
            if (responseJson.estado) {
                if (modelo.idProducto == 0) {
                    tablaData.row.add(responseJson.objeto).draw(false);
                } else {
                    tablaData.row(filaSeleccionada).data(responseJson.objeto).draw(false);
                    filaSeleccionada = null;
                }
                $("#modalData").modal("hide");
                swal("¡Listo!", modelo.idProducto == 0 ? "El producto fue creado" : "El producto fue modificado", "success");
            } else {
                swal("¡Lo sentimos!", responseJson.mensaje, "error");
            }
        });
});

// Editar
$("#tbdata tbody").on("click", ".btn-editar", function () {
    filaSeleccionada = $(this).closest("tr").hasClass("child")
        ? $(this).closest("tr").prev()
        : $(this).closest("tr");

    const data = tablaData.row(filaSeleccionada).data();
    mostrarModal(data);
});

// Eliminar
$("#tbdata tbody").on("click", ".btn-eliminar", function () {
    let fila = $(this).closest("tr").hasClass("child")
        ? $(this).closest("tr").prev()
        : $(this).closest("tr");

    const data = tablaData.row(fila).data();

    swal({
        title: "¿Estás seguro?",
        text: `Eliminar el producto "${data.descripcion}"`,
        type: "warning",
        showCancelButton: true,
        confirmButtonClass: "btn-danger",
        confirmButtonText: "Sí, eliminar",
        cancelButtonText: "No, cancelar",
        closeOnConfirm: false,
        closeOnCancel: true
    }, function (respuesta) {
        if (respuesta) {
            $(".showSweetAlert").LoadingOverlay("show");

            fetch(`/Producto/Eliminar?IdProducto=${data.idProducto}`, {
                method: "DELETE"
            })
                .then(response => {
                    $(".showSweetAlert").LoadingOverlay("hide");
                    return response.ok ? response.json() : Promise.reject(response);
                })
                .then(responseJson => {
                    if (responseJson.estado) {
                        tablaData.row(fila).remove().draw();
                        swal("¡Listo!", "El producto fue eliminado", "success");
                    } else {
                        swal("¡Lo sentimos!", responseJson.mensaje, "error");
                    }
                });
        }
    });
});
