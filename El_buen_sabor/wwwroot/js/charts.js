window.charts = {

    top: null,
    hours: null,
    money: null,
    weekly: null,

    create: function (id, type, labels, data, valueType) {

        const canvas = document.getElementById(id);
        const ctx = canvas.getContext("2d");

        let background = "#4F46E5";

        // Degradado para gráfico de líneas
        if (type === "line") {
            const gradient = ctx.createLinearGradient(0, 0, 0, 350);
            gradient.addColorStop(0, "rgba(79,70,229,0.35)");
            gradient.addColorStop(1, "rgba(79,70,229,0.02)");
            background = gradient;
        }

        return new Chart(ctx, {
            type: type,

            data: {
                labels: labels,
                datasets: [{
                    data: data,

                    backgroundColor: type === "bar"
                        ? "rgba(79,70,229,0.85)"
                        : background,

                    borderColor: "#4F46E5",
                    borderWidth: 3,

                    borderRadius: type === "bar" ? 10 : 0,
                    borderSkipped: false,

                    fill: type === "line",

                    tension: 0.45,

                    pointRadius: type === "line" ? 4 : 0,
                    pointHoverRadius: 7,
                    pointBackgroundColor: "#4F46E5",
                    pointBorderColor: "#ffffff",
                    pointBorderWidth: 2,

                    hoverBackgroundColor: "#4338CA"
                }]
            },

            options: {

                responsive: true,
                maintainAspectRatio: false,

                interaction: {
                    intersect: false,
                    mode: "index"
                },

                animation: {
                    duration: 900,
                    easing: "easeOutQuart"
                },

                plugins: {

                    legend: {
                        display: false
                    },

                    tooltip: {
                        backgroundColor: "#111827",
                        titleColor: "#ffffff",
                        bodyColor: "#E5E7EB",
                        borderColor: "#374151",
                        borderWidth: 1,
                        cornerRadius: 10,
                        padding: 12,
                        displayColors: false,
                        callbacks: {
                            label: function (context) {
                                const value = context.parsed.y;

                                if (valueType === "currency") {
                                    return new Intl.NumberFormat("es-AR", {
                                        style: "currency",
                                        currency: "ARS",
                                        maximumFractionDigits: 0
                                    }).format(value);
                                }

                                if (valueType === "units") {
                                    return `${value} ${value === 1 ? "unidad vendida" : "unidades vendidas"}`;
                                }

                                return `${value} ${value === 1 ? "factura generada" : "facturas generadas"}`;
                            }
                        }
                    }

                },

                scales: {

                    x: {

                        grid: {
                            display: false
                        },

                        ticks: {
                            color: "#6B7280",
                            font: {
                                size: 12,
                                weight: "500"
                            }
                        }

                    },

                    y: {

                        beginAtZero: true,

                        grid: {
                            color: "rgba(107,114,128,0.12)",
                            drawBorder: false
                        },

                        ticks: {
                            color: "#6B7280",
                            font: {
                                size: 12
                            }
                        }

                    }

                }

            }

        });

    },

    renderAll: function (tpL, tpV, hL, hV, mL, mV, hSL, hSV) {

        if (this.top) this.top.destroy();
        if (this.hours) this.hours.destroy();
        if (this.money) this.money.destroy();
        if (this.weekly) this.weekly.destroy();

        this.top = this.create(
            "topProductsChart",
            "bar",
            tpL,
            tpV,
            "units"
        );

        this.hours = this.create(
            "hoursChart",
            "line",
            hL,
            hV,
            "invoices"
        );

        this.money = this.create(
            "moneyProductsChart",
            "bar",
            mL,
            mV,
            "currency"
        );

        this.weekly = this.create(
            "weeklyHoursChart",
            "bar",
            hSL,
            hSV,
            "invoices"
        );
    }

};
