// index.js - Lógica para la página POS
let cart = [];
let selectedItemIndex = -1;

async function simulateScan() {
    try {
        const products = await window.chrome.webview.hostObjects.ServicioComercio.ObtenerProductosAsync();
        if (products.length > 0) {
            const randomProduct = products[Math.floor(Math.random() * products.length)];
            addToCart(randomProduct);
        } else {
            alert('No hay productos en inventario. Agrega algunos primero.');
        }
    } catch (e) {
        alert('Error conectando con la base de datos: ' + e);
    }
}

async function addByCode() {
    const code = document.getElementById('manualCode').value;
    if (code) {
        try {
            const product = await window.chrome.webview.hostObjects.ServicioComercio.ObtenerProductoPorCodigoAsync(code);
            if (product) {
                addToCart(product);
            } else {
                alert('Producto no encontrado');
            }
        } catch (e) {
            alert('Error: ' + e);
        }
    }
}

function addToCart(product) {
    const existing = cart.find(item => item.Id === product.Id);
    if (existing) {
        existing.Quantity += 1;
    } else {
        cart.push({ ...product, Quantity: 1 });
    }
    updateCartDisplay();
}

function updateCartDisplay() {
    const cartDiv = document.getElementById('cart');
    let html = '';
    let total = 0;
    cart.forEach((item, index) => {
        const itemTotal = item.Precio * item.Quantity;
        total += itemTotal;
        html += `<div class="list-group-item d-flex justify-content-between align-items-center ${selectedItemIndex === index ? 'bg-light' : ''}" onclick="selectItem(${index})">
            ${item.Nombre} - $${item.Precio} x ${item.Quantity} = $${itemTotal.toFixed(2)}
            <button class="btn btn-sm btn-danger" onclick="removeItem(${index})">X</button>
        </div>`;
    });
    cartDiv.innerHTML = html;
    document.getElementById('total').textContent = total.toFixed(2);
}

function selectItem(index) {
    selectedItemIndex = index;
    updateSelectedDisplay();
    updateCartDisplay();
}

function updateSelectedDisplay() {
    const display = document.getElementById('selectedDisplay');
    if (selectedItemIndex >= 0) {
        display.textContent = `Cantidad de ${cart[selectedItemIndex].Nombre}: ${cart[selectedItemIndex].Quantity}`;
    } else {
        display.textContent = 'Selecciona un artículo para editar cantidad';
    }
}

function removeItem(index) {
    cart.splice(index, 1);
    if (selectedItemIndex >= index) selectedItemIndex = -1;
    updateSelectedDisplay();
    updateCartDisplay();
}

function appendNumber(num) {
    const activeElement = document.activeElement;
    if (activeElement && activeElement.tagName === 'INPUT') {
        activeElement.value += num;
    } else if (selectedItemIndex >= 0) {
        const current = cart[selectedItemIndex].Quantity.toString();
        const newQty = parseInt(current + num);
        if (!isNaN(newQty) && newQty > 0) {
            cart[selectedItemIndex].Quantity = newQty;
            updateSelectedDisplay();
            updateCartDisplay();
        }
    }
}

function clearQuantity() {
    const activeElement = document.activeElement;
    if (activeElement && activeElement.tagName === 'INPUT') {
        activeElement.value = '';
    } else if (selectedItemIndex >= 0) {
        cart[selectedItemIndex].Quantity = 1;
        updateSelectedDisplay();
        updateCartDisplay();
    }
}

function removeSelected() {
    if (selectedItemIndex >= 0) {
        cart.splice(selectedItemIndex, 1);
        selectedItemIndex = -1;
        updateSelectedDisplay();
        updateCartDisplay();
    }
}

async function checkout() {
    if (cart.length > 0) {
        try {
            const sale = { Items: cart.map(item => ({ ProductId: item.Id, Quantity: item.Quantity, Price: item.Precio })) };
            await window.chrome.webview.hostObjects.ServicioComercio.AgregarVentaAsync(sale.Items);
            alert('Venta registrada!');
            cart = [];
            selectedItemIndex = -1;
            updateSelectedDisplay();
            updateCartDisplay();
        } catch (e) {
            alert('Error en checkout: ' + e);
        }
    } else {
        alert('Carrito vacío');
    }
}