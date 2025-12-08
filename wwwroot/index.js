document.addEventListener("DOMContentLoaded", () => {
    console.log("DOM Loaded - Script executing");
    console.log("WebView2 available:", !!(window.chrome && window.chrome.webview));

    // Función genérica para enviar mensaje a WebView2
    function sendMessage(action, codigo = "") {
        console.log("sendMessage called:", action, codigo);
        if (window.chrome && window.chrome.webview) {
            const message = { Action: action, Codigo: codigo };
            window.chrome.webview.postMessage(JSON.stringify(message));
            console.log("Message sent to C#:", message);
        } else {
            console.log("WebView2 no detectado:", action, codigo);
        }
    }

    // Botones del footer
    const footerButtons = document.querySelectorAll("footer button");
    console.log("Footer buttons found:", footerButtons.length);
    footerButtons.forEach(btn => {
        btn.addEventListener("click", () => {
            const action = btn.dataset.action;
            console.log("Footer button clicked:", action);
            sendMessage(action);
        });
    });

    // Botones de sidebar
    const sidebarButtons = {
        btnInventory: "Inventario",
        btnSales: "Ventas",
        btnSettings: "Ajustes"
    };

    Object.keys(sidebarButtons).forEach(id => {
        const el = document.getElementById(id);
        console.log("Sidebar button", id, "found:", !!el);
        if (el) {
            el.addEventListener("click", () => {
                console.log("Sidebar button clicked:", id);
                sendMessage(sidebarButtons[id]);
            });
        }
    });

    // Tabla de productos - Selección y Borrado
    // Tabla de productos - Selección y Borrado
    const productsTable = document.getElementById("productsTable");
    let selectedRow = null;

    productsTable.addEventListener("click", e => {
        // Borrado
        if (e.target.classList.contains("deleteRow")) {
            const row = e.target.closest("tr");
            const codigo = row.querySelector("td input").value;
            sendMessage("delete_item", codigo);
            // row.remove(); // Don't remove immediately, wait for C# update
            return;
        }

        // Selección de fila
        const row = e.target.closest("tr");
        if (row && row.parentElement.tagName === "TBODY") {
            if (selectedRow) {
                selectedRow.classList.remove("selected");
            }
            selectedRow = row;
            selectedRow.classList.add("selected");

            const codigo = row.querySelector("td input").value;
            sendMessage("SelectItem", codigo);
        }
    });

    // Teclado numérico
    const numericPad = document.querySelector(".numeric-pad");
    if (numericPad) {
        numericPad.addEventListener("click", e => {
            if (e.target.tagName === "BUTTON") {
                const value = e.target.textContent;
                sendMessage("NumPad", value);
            }
        });
    }

    // Listen for C# updates
    if (window.chrome && window.chrome.webview) {
        window.chrome.webview.addEventListener('message', event => {
            const msg = event.data;
            // msg might be JSON string or object depending on how C# sends it.
            // MainWindow sends `PostWebMessageAsJson`, so it arrives as object (if parsed) or string.
            // Usually `event.data` is the object if `PostWebMessageAsJson` is used.

            if (msg.Type === 'UpdateSaleList') {
                renderSaleTable(msg.Data);
            }
        });
    }

    function renderSaleTable(items) {
        const tbody = productsTable.querySelector("tbody");
        tbody.innerHTML = "";
        items.forEach(item => {
            const tr = document.createElement("tr");
            // Highlight if selected? We lose selection on re-render unless we track it.
            // For now, let's just render.
            tr.innerHTML = `
                <td><input type="text" value="${item.Product.Codigo}" readonly></td>
                <td><input type="text" value="${item.Product.Nombre}" readonly></td>
                <td><input type="number" value="${item.UnitPrice.toFixed(2)}" readonly></td>
                <td><input type="number" value="21" readonly></td>
                <td><input type="number" value="${item.TotalPrice.toFixed(2)}" readonly></td>
                <td><button class="deleteRow">❌</button></td>
            `;
            tbody.appendChild(tr);
        });
    }
});
