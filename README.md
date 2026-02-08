# Refinery 🏭

Refinery, ham ve yapılandırılmamış e-posta verilerini işleyerek (ingest), yapay zeka (AI) destekli analizlerle anlamlı ve yönetilebilir iş biletlerine (ticket) dönüştüren, yüksek performanslı ve dağıtık bir arka plan işleme sistemidir.

**Durum:** Geliştirme Aşamasında (v0.1 Alpha)

## Proje Amacı
Müşterilerden gelen e-postalar genellikle karmaşık, düzensiz ve eksik bilgiler içerir. Refinery, bu "ham petrolü" alır ve işleyerek:

- **Analiz Eder:** İçeriği anlar, özet çıkarır.
- **Sınıflandırır:** Teknik, Satış, Fatura vb. kategorilere ayırır.
- **Önceliklendirir:** Aciliyet durumunu (Low/Medium/High) belirler.
- **Yapılandırır:** İsim, telefon gibi eksik bilgileri tespit eder ve JSON formatında sunar.

## Mimari
Clean Architecture prensiplerine bağlı, ölçeklenebilir Microservices (Worker Services) yapısı:

- **Ingest Worker (Producer):** (Yakında) IMAP/posta sunucularını dinleyerek ham e-postayı alır.
- **Redis Streams:** Mesaj kuyruğu ve yüksek hacim tamponu.
- **Refinement Worker (Consumer):** Kuyruktan veriyi alır, AI ile işler ve ticket üretir.
- **AI Engine:** Google Gemini (via Microsoft Semantic Kernel) ile metin analizi.

## Teknoloji Yığını

- .NET 9 (Worker Services)
- Redis (Streams & Consumer Groups)
- Google Gemini AI (LLM)
- Microsoft Semantic Kernel
- Entity Framework Core (yakında)

## Proje Yapısı

```
Refinery/
├── src/
│   ├── Refinery.Core
│   ├── Refinery.Infrastructure.Redis
│   ├── Refinery.Infrastructure.Ai
│   ├── Refinery.Infrastructure.Data (WIP)
│   ├── Refinery.IngestWorker
│   └── Refinery.RefinementWorker
└── Refinery.sln
```

## Kurulum ve Çalıştırma

Gereksinimler:

- .NET 9 SDK
- Docker (veya mevcut bir Redis)
- Google AI Studio API Key

1) Redis'i Docker ile başlatın:

```bash
docker run -d -p 6379:6379 --name refinery-redis redis
```

2) Konfigürasyon

`src/Refinery.RefinementWorker/appsettings.Development.json` dosyasını düzenleyin ve Gemini API anahtarınızı girin:

```json
  "Gemini": {
    "ApiKey": "SENIN_API_KEYIN",
    "ModelId": "gemini-2.0-flash"
  }
```

3) Worker'ı çalıştırın:

```bash
dotnet run --project src/Refinery.RefinementWorker
```

4) Test (Simülasyon)

Redis CLI ile manuel bir e-posta ekleyip Worker çıktısını izleyin:

```bash
XADD ticket_emails * Subject "Login Sorunu" Body "Sisteme giriş yapamıyorum, hata kodu 500 alıyorum. Acil yardım. Kullanıcı: ahmet" Sender "ahmet@test.com" Recipient "support@refinery.com"
```

5) Worker Konsolunda Göreceğiniz Çıktı:

```bash
info: Refinery.RefinementWorker.Worker[0]
      [MAIL ALINDI] Gönderen: ahmet@test.com | Konu: Login Sorunu
info: Refinery.RefinementWorker.Worker[0]
      ------------------------------------------------
info: Refinery.RefinementWorker.Worker[0]
      [AI ANALİZİ TAMAMLANDI]
info: Refinery.RefinementWorker.Worker[0]
      Kategori : Technical
info: Refinery.RefinementWorker.Worker[0]
      Aciliyet : High
info: Refinery.RefinementWorker.Worker[0]
      Özet     : Kullanıcı 500 hatası nedeniyle sisteme giriş yapamıyor.
info: Refinery.RefinementWorker.Worker[0]
      Eksik Bilgi: True (Telefon numarası eksik)
info: Refinery.RefinementWorker.Worker[0]
      ------------------------------------------------
info: Refinery.RefinementWorker.Worker[0]
      [KAYIT BAŞARILI] Ticket ID: a1b2c3d4-e5f6-7890-1234-567890abcdef DB'ye yazıldı.
```

## Yol Haritası (Roadmap)

- [x] Solution ve Katmanlı Mimari Kurulumu
- [x] Redis Streams Altyapısı (Producer/Consumer)
- [x] Google Gemini AI Entegrasyonu (Semantic Kernel)
- [ ] EF Core & LocalDB ile Kalıcılık (Persistence)
- [ ] IMAP Entegrasyonu (Gerçek E-posta Dinleme)
- [ ] Dead Letter Queue (Hata Yönetimi)
- [ ] Dashboard / UI (Ticket Görüntüleme)

## Notlar

- Proje şu an alpha aşamasındadır; persist katmanı ve gerçek e-posta entegrasyonu üzerinde çalışılmaktadır.
- Gemini API kullanımınız için Google AI Studio erişimi ve uygun kota/anahtar gereklidir.

---

Hazırlayan: Refinery Takımı
