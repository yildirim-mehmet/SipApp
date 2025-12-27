let seciliMasaId = null;
let seciliAdisyonId = null;

async function loadSiparisler() {
    const res = await fetch(`/Ekran/Siparisler?ekranId=${window.EKRAN_ID}`);
    const data = await res.json();

    // data: kalem listesi bekliyoruz
    renderFromSiparisList(data || []);
}

function renderFromSiparisList(list) {
    // SOL: gelen siparişler (son gelen üstte)
    const sol = document.getElementById("solListe");
    sol.innerHTML = "";

    const sorted = [...list].sort((a, b) => new Date(b.zaman || b.siparisVerilmeZamani) - new Date(a.zaman || a.siparisVerilmeZamani));

    if (sorted.length === 0) {
        sol.innerHTML = `<div class="text-muted p-2">Sipariş yok.</div>`;
    } else {
        for (const x of sorted.slice(0, 50)) {
            sol.innerHTML += `
        <div class="border rounded p-2 mb-2">
          <div><b>Masa:</b> ${x.masaId} - ${x.masaAdi ?? ""}</div>
          <div><b>Adisyon:</b> ${x.adisyonId ?? x.adisyonId}</div>
          <div class="small text-muted">${x.urunAdi} x${x.adet}</div>
        </div>`;
        }
    }

    // SAĞ: masalar (gruplu)
    const grouped = {};
    for (const x of list) {
        grouped[x.masaId] = grouped[x.masaId] || { masaId: x.masaId, masaAdi: x.masaAdi ?? "", adet: 0, sonZaman: x.zaman || x.siparisVerilmeZamani, adisyonId: x.adisyonId };
        grouped[x.masaId].adet += 1;
        const z = new Date(x.zaman || x.siparisVerilmeZamani);
        if (z > new Date(grouped[x.masaId].sonZaman)) grouped[x.masaId].sonZaman = z;
    }

    const masalar = Object.values(grouped).sort((a, b) => new Date(b.sonZaman) - new Date(a.sonZaman));
    const grid = document.getElementById("masaGrid");
    grid.innerHTML = "";

    if (masalar.length === 0) {
        grid.innerHTML = `<div class="text-muted">Aktif masa yok.</div>`;
        return;
    }

    for (const m of masalar) {
        grid.innerHTML += `
      <button class="btn btn-outline-primary w-100 text-start mb-2"
              onclick="selectMasa(${m.masaId})">
        <div class="d-flex justify-content-between">
          <div><b>${m.masaId}</b> ${m.masaAdi}</div>
          <span class="badge bg-danger">${m.adet}</span>
        </div>
        <div class="small text-muted">Son: ${new Date(m.sonZaman).toLocaleTimeString()}</div>
      </button>`;
    }
}

async function selectMasa(masaId) {
    seciliMasaId = masaId;

    const res = await fetch(`/Ekran/MasaAdisyon?masaId=${masaId}`);
    if (!res.ok) {
        document.getElementById("adisyonDetay").innerHTML = `<div class="text-muted">Aktif adisyon yok.</div>`;
        return;
    }

    const data = await res.json();
    renderAdisyon(data);
}

function renderAdisyon(data) {
    // data: { adisyonId, masaId, masaAdi, toplam, kalemler:[] }
    seciliAdisyonId = data.adisyonId;

    document.getElementById("adisyonBaslik").innerText =
        `Masa ${data.masaId} ${data.masaAdi ?? ""} • Adisyon ${data.adisyonId}`;

    document.getElementById("toplamTutar").innerText = (data.toplam ?? 0).toFixed(2);

    document.getElementById("btnKapat").disabled = false;
    document.getElementById("btnIptal").disabled = false;
    document.getElementById("btnYazdir").disabled = false;

    const body = document.getElementById("adisyonDetay");
    body.innerHTML = "";

    const kalemler = [...(data.kalemler || [])]
        .sort((a, b) => new Date(b.zaman) - new Date(a.zaman)); // son sipariş üstte

    if (kalemler.length === 0) {
        body.innerHTML = `<div class="text-muted">Kalem yok.</div>`;
        return;
    }

    for (const k of kalemler) {
        const durumTxt = k.siparisDurumu === null ? "Onay Bekliyor"
            : k.siparisDurumu === 0 ? "Hazırlanıyor"
                : k.siparisDurumu === 1 ? "Masada"
                    : "İptal";

        body.innerHTML += `
      <div class="border rounded p-2 mb-2">
        <div class="d-flex justify-content-between">
          <div>
            <b>${k.urunAdi}</b> x${k.adet}
            <div class="small text-muted">${durumTxt}</div>
          </div>
          <div class="btn-group btn-group-sm">
            <button class="btn btn-outline-warning" onclick="setDurum(${k.kalemId},0)">Haz.</button>
            <button class="btn btn-outline-success" onclick="setDurum(${k.kalemId},1)">Masada</button>
            <button class="btn btn-outline-danger" onclick="setDurum(${k.kalemId},2)">İptal</button>
          </div>
        </div>
      </div>`;
    }
}

async function setDurum(kalemId, durum) {
    await fetch(`/Ekran/KalemDurum?kalemId=${kalemId}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(durum)
    });

    // UI refresh
    if (seciliMasaId) await selectMasa(seciliMasaId);
    await loadSiparisler();
}

// butonlar
document.addEventListener("DOMContentLoaded", async () => {
    await loadSiparisler();

    document.getElementById("btnKapat").addEventListener("click", async () => {
        if (!seciliAdisyonId) return;
        await fetch(`/Ekran/AdisyonKapat?adisyonId=${seciliAdisyonId}`, { method: "POST" });
        seciliMasaId = null;
        seciliAdisyonId = null;
        document.getElementById("adisyonDetay").innerHTML = `<div class="text-muted">Adisyon kapatıldı.</div>`;
        await loadSiparisler();
    });

    document.getElementById("btnIptal").addEventListener("click", async () => {
        if (!seciliAdisyonId) return;
        await fetch(`/Ekran/AdisyonIptal?adisyonId=${seciliAdisyonId}`, { method: "POST" });
        seciliMasaId = null;
        seciliAdisyonId = null;
        document.getElementById("adisyonDetay").innerHTML = `<div class="text-muted">Adisyon iptal edildi.</div>`;
        await loadSiparisler();
    });

    document.getElementById("btnYazdir").addEventListener("click", () => {
        if (!seciliAdisyonId) return;
        window.open(`/Ekran/Yazdir?adisyonId=${seciliAdisyonId}`, "_blank");
    });

    // ✅ SignalR bağlan (API hub)
    const conn = new signalR.HubConnectionBuilder()
        .withUrl(window.API_HUB_URL)
        .withAutomaticReconnect()
        .build();

    conn.on("YeniSiparis", async (msg) => {
        // sadece bu ekran için geldiyse
        if (msg.ekranId == window.EKRAN_ID) {
            const badge = document.getElementById("badgeYeni");
            badge.innerText = (parseInt(badge.innerText || "0") + 1).toString();
            await loadSiparisler();
            if (seciliMasaId) await selectMasa(seciliMasaId);
        }
    });

    conn.on("SiparisDurumDegisti", async (_) => {
        await loadSiparisler();
        if (seciliMasaId) await selectMasa(seciliMasaId);
    });

    await conn.start();
    await conn.invoke("JoinEkranGroup", window.EKRAN_ID.toString());
});






// SON EKLENEN
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/siparis")
    .build();

connection.on("YeniSiparis", data => {
    const card = document.querySelector(`#masa-${data.masaId}`);
    if (!card) return;

    card.classList.add("highlight");
    setTimeout(() => card.classList.remove("highlight"), 2000);
});

connection.start();




//son mantıklı düzenlemeler ama yukarısı riziko
conn.on("AdisyonKapandi", async (msg) => {
    // Eğer seçili adisyon kapandıysa paneli temizle
    if (seciliAdisyonId && msg.adisyonId == seciliAdisyonId) {
        seciliMasaId = null;
        seciliAdisyonId = null;

        document.getElementById("adisyonBaslik").innerText = "Adisyon kapandı";
        document.getElementById("adisyonDetay").innerHTML =
            `<div class="text-muted">Bu adisyon kapatıldı/iptal edildi.</div>`;
        document.getElementById("toplamTutar").innerText = "0.00";

        document.getElementById("btnKapat").disabled = true;
        document.getElementById("btnIptal").disabled = true;
        document.getElementById("btnYazdir").disabled = true;
    }

    // Listeyi yenile (sol/sağ paneller otomatik düşer)
    await loadSiparisler();
});
