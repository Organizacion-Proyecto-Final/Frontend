window.listPrinters = async function () {

    try {

        if (!qz.websocket.isActive()) {
            await qz.websocket.connect();
        }

        return await qz.printers.find();

    } catch (err) {

        console.error(err);
        return [];

    }

};


window.printTicket = async function (printerName, factura) {

    try {

        if (!qz.websocket.isActive()) {
            await qz.websocket.connect();
        }

        let config = printerName
            ? qz.configs.create(printerName)
            : qz.configs.create();

        // ===== ARMADO DEL TICKET =====

        let ticket = "";

        // Inicializar impresora
        ticket += "\x1B\x40";

        // Centrar + Negrita
        ticket += "\x1B\x61\x01";
        ticket += "\x1D\x21\x11"; // doble ancho + doble alto
        ticket += "FastRestaurant\n";

        // Volver a tamaño normal
        ticket += "\x1D\x21\x00";
        // Opcional: quitar negrita
        ticket += "\x1B\x45\x00";

        ticket += "================================\n";

        // Volver a izquierda
        ticket += "\x1B\x61\x00";

        ticket += "Factura : " + factura.id + "\n";
        ticket += "Mesa    : " + factura.tableNumber + "\n";
        const utcDate = /(?:Z|[+-]\d{2}:\d{2})$/i.test(factura.date)
            ? factura.date
            : factura.date + "Z";
        const argentinaDate = new Intl.DateTimeFormat("es-AR", {
            timeZone: "America/Argentina/Buenos_Aires",
            day: "2-digit",
            month: "2-digit",
            year: "numeric",
            hour: "2-digit",
            minute: "2-digit",
            hour12: false
        }).format(new Date(utcDate));

        ticket += "Fecha   : " + argentinaDate + "\n";

        ticket += "================================\n";
        ticket += "CANT  PRODUCTO\n";
        ticket += "--------------------------------\n";

        // Productos
        factura.details.forEach(item => {

            const subtotal = item.quantity * item.price;

            // Primera línea
            ticket += `${item.quantity} x ${item.productName}\n`;

            // Segunda línea (alineada)
            ticket += `      $${item.price.toFixed(2)} x ${item.quantity} = $${subtotal.toFixed(2)}\n\n`;

        });

        ticket += "--------------------------------\n";

        // Total centrado y en negrita
        ticket += "\x1B\x45\x01";
        ticket += "TOTAL: $" + factura.total.toFixed(2) + "\n";
        ticket += "\x1B\x45\x00";

        ticket += "================================\n";

        // Centrar despedida
        ticket += "\x1B\x61\x01";
        ticket += "¡GRACIAS POR ELEGIRNOS!\n";
        ticket += "Los esperamos nuevamente\n";
        ticket += "\x1B\x61\x00";

        // Alimentar papel
        ticket += "\n\n\n\n";

        // Corte (si la impresora lo soporta)
        ticket += "\x1D\x56\x00";

        const data = [{
            type: "raw",
            format: "plain",
            data: ticket
        }];

        await qz.print(config, data);

        return true;

    } catch (err) {

        console.error(err);
        return false;

    }

};
