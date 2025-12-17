// =============================
// Sepet: masa bazlı localStorage
// =============================

function cartKey() {
  return `cart_masa_${window.MASA_ID}`;
}

function loadCart() {
  const raw = localStorage.getItem(cartKey());
  if (!raw) return [];
  try { return JSON.parse(raw); } catch { return []; }
}

function saveCart(items) {
  localStorage.setItem(cartKey(), JSON.stringify(items));
}

function cartAdd(urunId, ad, fiyat) {
  const items = loadCart();
  const found = items.find(x => x.urunId === urunId);
  if (found) found.adet += 1;
  else items.push({ urunId, ad, fiyat, adet: 1 });
  saveCart(items);
  renderCart();
}

function cartRemove(urunId) {
  let items = loadCart();
  items = items.filter(x => x.urunId !== urunId);
  saveCart(items);
  renderCart();
}

function cartInc(urunId) {
  const items = loadCart();
  const it = items.find(x => x.urunId === urunId);
  if (it) it.adet += 1;
  saveCart(items);
  renderCart();
}

function cartDec(urunId) {
  const items = loadCart();
  const it = items.find(x => x.urunId === urunId);
  if (!it) return;
  it.adet -= 1;
  if (it.adet <= 0) {
    cartRemove(urunId);
    return;
  }
  saveCart(items);
  renderCart();
}

function calcTotal(items) {
  return items.reduce((sum, x) => sum + (x.fiyat * x.adet), 0);
}

function renderCart() {
  const items = loadCart();
  document.getElementById("cartCount").innerText = items.reduce((s, x) => s + x.adet, 0);

  const container = document.getElementById("cartItems");
  container.innerHTML = "";

  if (items.length === 0) {
    container.innerHTML = `<div class="alert alert-light">Sepet boş.</div>`;
    document.getElementById("cartTotal").innerText = "0";
    return;
  }

  for (const it of items) {
    const row = document.createElement("div");
    row.className = "border rounded p-2 mb-2";

    row.innerHTML = `
      <div class="d-flex justify-content-between">
        <div>
          <div class="fw-semibold">${it.ad}</div>
          <small class="text-muted">${it.fiyat} ₺</small>
        </div>
        <button class="btn btn-sm btn-outline-danger" onclick="cartRemove(${it.urunId})">Sil</button>
      </div>

      <div class="d-flex justify-content-between align-items-center mt-2">
        <div class="btn-group btn-group-sm" role="group">
          <button class="btn btn-outline-secondary" onclick="cartDec(${it.urunId})">-</button>
          <button class="btn btn-outline-secondary" disabled>${it.adet}</button>
          <button class="btn btn-outline-secondary" onclick="cartInc(${it.urunId})">+</button>
        </div>
        <div><b>${(it.fiyat * it.adet).toFixed(2)}</b> ₺</div>
      </div>
    `;
    container.appendChild(row);
  }

  document.getElementById("cartTotal").innerText = calcTotal(items).toFixed(2);
}

async function checkActiveAdisyon() {
  // MVC endpoint (same-origin): /Menu/AktifAdisyon?masaId=2
  const res = await fetch(`/Menu/AktifAdisyon?masaId=${window.MASA_ID}`);
  if (!res.ok) return null;
  const txt = await res.text();
  if (!txt) return null;
  try { return JSON.parse(txt); } catch { return null; }
}

async function postOrder(items) {
  const payload = {
    masaId: window.MASA_ID,
    urunler: items.map(x => ({ urunId: x.urunId, adet: x.adet }))
  };

  const res = await fetch("/Menu/SiparisVer", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload)
  });

  if (!res.ok) {
    const text = await res.text();
    throw new Error(text || "Sipariş gönderilemedi.");
  }

  return await res.json();
}

// =============================
// 10sn geri sayım modal
// =============================
let timer = null;
let remaining = 10;
let modal = null;

function openCountdown(onFinish) {
  remaining = 10;
  document.getElementById("countdownNumber").innerText = remaining;

  if (!modal) {
    modal = new bootstrap.Modal(document.getElementById("countdownModal"));
  }

  modal.show();

  timer = setInterval(() => {
    remaining -= 1;
    document.getElementById("countdownNumber").innerText = remaining;

    if (remaining <= 0) {
      clearInterval(timer);
      timer = null;
      modal.hide();
      onFinish();
    }
  }, 1000);
}

function cancelCountdown() {
  if (timer) {
    clearInterval(timer);
    timer = null;
  }
  if (modal) modal.hide();
}

// =============================
// init
// =============================
document.addEventListener("DOMContentLoaded", () => {
  renderCart();

  document.getElementById("btnIptal").addEventListener("click", () => {
    cancelCountdown();
  });

  document.getElementById("btnSiparisVer").addEventListener("click", async () => {
    const items = loadCart();
    if (items.length === 0) {
      alert("Sepet boş.");
      return;
    }

    // DOLU masada: gönderim öncesi aktif adisyon kontrolü + onay
    const aktif = await checkActiveAdisyon();
    if (aktif) {
      const ok = confirm("Bu masada aktif adisyon var. Sepetteki ürünleri mevcut adisyona eklemek istiyor musunuz?");
      if (!ok) return;
    }

    openCountdown(async () => {
      try {
        const resp = await postOrder(items);
        saveCart([]);
        renderCart();

        // sepet panelini kapat
        const off = bootstrap.Offcanvas.getInstance(document.getElementById('cartCanvas'));
        if (off) off.hide();

        alert(`Sipariş gönderildi. AdisyonId: ${resp.adisyonId}`);
      } catch (e) {
        alert(e?.message || e);
      }
    });
  });
});
