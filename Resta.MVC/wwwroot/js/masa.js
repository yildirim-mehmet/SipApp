const sepetKey = `masa_${masaId}_sepet`;
let MASA_DOLU = false;

document.addEventListener("DOMContentLoaded", async () => {
    await loadMasaDurum();
    await loadMenu();
});

async function loadMasaDurum() {
    const res = await fetch(`/api/Masa/${masaId}/durum`);
    const data = await res.json();

    const badge = document.getElementById("masaDurum");

    if (data.durum === "dolu") {
        MASA_DOLU = true;
        badge.className = "badge bg-danger";
        badge.innerText = "DOLU";
    } else {
        MASA_DOLU = false;
        badge.className = "badge bg-success";
        badge.innerText = "BOŞ";
    }
}
