using FluentAssertions;
using Resta.API.Tests.DTOs;
using Resta.API.Tests.Fixtures;
using Resta.API.Tests.Helpers;
using Xunit;

namespace Resta.API.Tests.Flows;

public class FullFlowTests : IClassFixture<ApiTestFixture>
{
    private readonly HttpClient _client;

    public FullFlowTests(ApiTestFixture fixture)
    {
        _client = fixture.Client;
    }

    [Fact]
    public async Task Full_Restaurant_Flow_Should_Work()
    {
        // 1️⃣ Bölüm oluştur
        //var bolum = await _client.PostAsync<dynamic>("/api/Bolum", new
        //{
        //    ad = "Alakart",
        //    kod = "ALAKART",
        //    aktif = true
        //});

        //int bolumId = (int)bolum.id;
        var bolum = await _client.PostAsync<IdResponseDto>(
        "/api/Bolum",
        new
        {
            ad = "Alakart",
            kod = "ALAKART",
            aktif = true
        });

        int bolumId = bolum.Id;


        // 2️⃣ Masa oluştur
        var masa = await _client.PostAsync<IdResponseDto>(
    "/api/Masa",
    new
    {
        bolumId,
        kod = "M1",
        ad = "Masa 1"
    });

        int masaId = masa.Id;


        // 3️⃣ Aktif adisyon YOK (API: Ok(null) döndürmeli)
        var aktifAdisyon = await _client.GetJsonAsync<object?>($"/api/Masa/{masaId}/aktif-adisyon");
        aktifAdisyon.Should().BeNull();


        // 4️⃣ Menü çek
        var menu = await _client.GetJsonAsync<List<object>>($"/api/Menu/masa/{masaId}");
        menu.Should().NotBeNull();


        // 5️⃣ Sipariş ver → adisyon otomatik açılmalı
        var siparis = await _client.PostAsync<SiparisVerResponseDto>(
    "/api/Siparis/ver",
    new
    {
        masaId,
        urunler = new[]
        {
            new { urunId = 1, adet = 1 }
        }
    });

        int adisyonId = siparis.AdisyonId;


        // 6️⃣ Aktif adisyon artık VAR
        // 6️⃣ Aktif adisyon artık VAR
        var aktif = await _client.GetJsonAsync<AdisyonMiniDto>($"/api/Masa/{masaId}/aktif-adisyon");
        aktif.Id.Should().Be(adisyonId);


        // 7️⃣ Ödeme al → adisyon kapanmalı
        var odeme = await _client.PostAsync<dynamic>("/api/Odeme", new
        {
            adisyonId,
            tip = "masada"
        });

        // 8️⃣ Masa artık BOŞ
        // 8️⃣ Masa artık BOŞ
        var durum = await _client.GetJsonAsync<MasaDurumDto>($"/api/Masa/{masaId}/durum");


        durum.Durum.Should().Be("bos");

    }
}
