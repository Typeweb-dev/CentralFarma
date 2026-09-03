$(document).ready(function () {

    $("div.container-fluid").LoadingOverlay("show");

    fetch("/DashBoard/ObtenerResumen")
    .then(response => {
         $("div.container-fluid").LoadingOverlay("hide");
        return response.ok ? response.json() : Promise.reject(response);
    })
    .then(responseJson => {

        if (responseJson.estado) {

            // Mostrar datos para las tarjetas 
            let d = responseJson.objeto

            $("#totalVenta").text(d.totalVentas)
            $("#totalIngresos").text(d.totalIngresos)
            $("#totalProductos").text(d.totalProductos)
            $("#totalCategorias").text(d.totalCategorias)

            // Obtener valores y text para GBarra
            let barchar_labels;
            let barchar_data;

            if (d.ventasUltimaSemana.length > 0) {
                barchar_labels = d.ventasUltimaSemana.map((item) => { return item.fecha })
                barchar_data = d.ventasUltimaSemana.map((item) => { return item.total })
            }
            else {
                barchar_labels = ["Sin Resultados"]
                barchar_data = [0]
            }

            // Obtener valores y text para GPastel

            let Paschar_labels;
            let Paschar_data;


            if (d.productosTopUltimaSemana.length > 0) {
                Paschar_labels = d.productosTopUltimaSemana.map((item) => { return item.producto })
                Paschar_data = d.productosTopUltimaSemana.map((item) => { return item.cantidad })
            }
            else {
                Paschar_labels = ["Sin Resultados"]
                Paschar_data = [0]
            }


            // GBarra
            let controlVenta = document.getElementById("chartVentas");
            let myBarChart = new Chart(controlVenta, {
                type: 'bar',
                data: {
                    labels: barchar_labels,
                    datasets: [{
                        label: "Cantidad",
                        backgroundColor: "#4e73df",
                        hoverBackgroundColor: "#2e59d9",
                        borderColor: "#4e73df",
                        data: barchar_data,
                    }],
                },
                options: {
                    maintainAspectRatio: false,
                    legend: {
                        display: false
                    },
                    scales: {
                        xAxes: [{
                            gridLines: {
                                display: false,
                                drawBorder: false
                            },
                            maxBarThickness: 50,
                        }],
                        yAxes: [{
                            ticks: {
                                min: 0,
                                maxTicksLimit: 5
                            }
                        }],
                    },
                }
            });


            // GPastel
            let controlProducto = document.getElementById("chartProductos");
            let myPieChart = new Chart(controlProducto, {
                type: 'doughnut',
                data: {
                    labels: Paschar_labels,
                    datasets: [{
                        data: Paschar_data,
                        backgroundColor: ['#4e73df', '#1cc88a', '#36b9cc', "#FF785B"],
                        hoverBackgroundColor: ['#2e59d9', '#17a673', '#2c9faf', "#FF5733"],
                        hoverBorderColor: "rgba(234, 236, 244, 1)",
                    }],
                },
                options: {
                    maintainAspectRatio: false,
                    tooltips: {
                        backgroundColor: "rgb(255,255,255)",
                        bodyFontColor: "#858796",
                        borderColor: '#dddfeb',
                        borderWidth: 1,
                        xPadding: 15,
                        yPadding: 15,
                        displayColors: false,
                        caretPadding: 10,
                    },
                    legend: {
                        display: true
                    },
                    cutoutPercentage: 80,
                },
            });
        }
    })
})