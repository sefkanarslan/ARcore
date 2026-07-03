# AR Ölçüm & Alan Planlayıcı

Telefonun kamerasıyla gerçek mekânı **ölçen** (uzunluk, çevre, alan) ve o mekâna
**gerçek boyutlu sanal mobilya yerleştirip düzenlemene** izin veren bir **Android AR**
uygulaması. Emlak, iç mimari, taşınma öncesi yerleşim planı gibi işler için pratiktir.

> **Teknoloji:** Unity 2022.3 LTS + AR Foundation 5 (ARCore)

---

## Nasıl çalışıyor?

Uygulamanın iki modu var; sağ/sol üstteki butonlarla geçiş yaparsın.

### 1) Ölçüm modu
1. **Yüzey tara**: Telefonu yavaşça gezdir; ARCore zemini/duvarı düzlem olarak bulur
   (`ARPlaneManager`) ve hafifçe boyar.
2. **Nokta koy**: Ekrana dokununca parmağın gösterdiği yer bir ışınla (`ARRaycastManager`)
   gerçek dünya noktasına çevrilir ve oraya **anchor** (`ARAnchor`) sabitlenir.
3. **Ölç**: Ardışık noktalar bir çizgiyle bağlanır; her aralığın uzunluğu havada 3B
   etiket olarak, toplam uzunluk da üstteki durum çubuğunda görünür
   (`Vector3.Distance`).
4. **Alan**: **Alan** butonu poligonu kapatır; kapalı alanın **m²** değeri ve **çevresi**
   hesaplanır (`Geometry.PolygonArea` / Newell yöntemi).
5. **Birim**: m / cm / ft / inç arasında anında geçiş (`Units`).

### 2) Plan modu
1. **Katalogdan seç**: Gerçek ölçülerde eşyalar (`FurnitureCatalog`). Her eşya tek bir
   kutu değil, birkaç kutudan oluşan **tanınabilir bir mesh**tir (masa = tabla + 4 ayak,
   koltuk = taban + sırt + kol, yatak = kasa + şilte + yastık…) ve sahnedeki yönlü
   ışıkla gölgelenir (`FurnitureMeshBuilder`).
   - **Zemin eşyaları**: masa, sandalye, koltuk, yatak, dolap, çalışma masası.
   - **Duvar eşyaları**: TV, tablo, raf — **dikey** düzleme asılır, otomatik olarak
     duvara dik hizalanır (`MountType.Wall`).
2. **Yerleştir**: Doğru yüzeye dokun (zemin eşyası yatay, duvar eşyası dikey düzlem).
3. **Düzenle**:
   - Tek parmak sürükle → **taşı** (uygun yüzey boyunca).
   - İki parmak → **döndür + ölçekle** (pinch/twist).
   - Seçili eşya sarı tel kafesle, çakışan eşyalar kırmızıyla işaretlenir.
4. **Çakışma uyarısı**: İki eşya üst üste binince **kırmızı** vurgulanır
   (`Physics.OverlapBox`).
5. **Kaydet / Yükle**: Yerleşim JSON olarak diske yazılır (`LayoutStore`,
   `Application.persistentDataPath/layout.json`).
6. **Foto**: Mevcut AR görünümü PNG olarak dışa aktarılır (`ScreenshotService`).
7. **Rapor**: Ölçümleri + eşya listesini içeren bir özet kartı AR görünümüne bindirilip
   PNG olarak yakalanır ve **Android paylaşım** sayfası açılır (`ReportExporter`,
   `ReportBuilder`, `NativeShare`). Paylaşımda özet metni her zaman gider; görselin
   eklenmesi için uygulamanın `FileProvider`'ı gerekir (yoksa metin paylaşımına düşer).

### Mimari (kod)

Sahne (`Assets/Scenes/Main.unity`) kasıtlı olarak **boştur**: her şey
`GameBootstrap` içinde `[RuntimeInitializeOnLoadMethod]` ile koddan kurulur. Bu sayede
elle düzenlenen bir sahne grafiği yoktur.

```
Assets/Scripts/
  Core/     GameBootstrap      (uygulamayı başlatır)
            AppBootstrapper    (AR rig'i + ışık + sistemleri koddan kurar)
            AppState, AppMode  (paylaşılan durum + olaylar)
  Input/    InputRouter        (dokunma/fare soyutlaması, EnhancedTouch)
  Measure/  MeasurementManager (dokunma -> nokta -> uzunluk/alan + özet erişimi)
            MeasureSegment     (çizgi + 3B mesafe etiketi)
            Units, MeasureUnit (birim dönüşümü)
  Planner/  FurniturePlacer    (yerleştir/seç/taşı/döndür/ölçekle, zemin+duvar, çakışma)
            PlacedFurniture    (kompozit mesh + hacim + seçim/çakışma vurgusu)
            FurnitureMeshBuilder (kutulardan tanınabilir mobilya üretir)
            FurnitureCatalog, FurnitureDefinition, FurnitureKind (kind + MountType)
            LayoutStore, LayoutData (JSON kaydet/yükle)
            ReportBuilder      (özet metni)
  UI/       AppUI, UIFactory   (tüm arayüz koddan)
            ReportExporter     (özet kartı + ekran yakalama + paylaşım)
  Util/     Geometry           (poli-çizgi uzunluğu, poligon alanı/çevresi)
            MaterialFactory    (çalışma anında lit/unlit/transparent materyal)
            WireBox            (seçim tel kafesi)
            ScreenshotService  (PNG dışa aktarma)
            NativeShare        (Android paylaşım)
```

Projede hiçbir üçüncü parti / telifli görsel veya 3B model yoktur; tüm meshler ve
materyaller çalışma anında üretilir.

---

## Kurulum ve APK üretme

1. **Unity Hub** ile **Unity 2022.3 LTS** kur; **Android Build Support**
   (OpenJDK, Android SDK & NDK dahil) modülünü ekle.
2. Bu klasörü Unity ile aç (paketler `Packages/manifest.json`'dan otomatik iner).
3. **Project Settings → XR Plug-in Management → Android** sekmesinde **ARCore**'u
   işaretle.
4. **Project Settings → Player**:
   - **Active Input Handling** = *Input System Package (New)* veya *Both*
   - **Minimum API Level** = Android 7.0 (API 24) veya üzeri
   - **Scripting Backend** = IL2CPP, **Target Architectures** = ARM64
5. **File → Build Settings**: `Assets/Scenes/Main.unity`'yi "Scenes In Build"e ekle,
   platformu **Android** yap, **Build** ile APK üret.
6. APK'yı ARCore destekli bir telefona kur.

> **Editörde deneme:** Cihaz olmadan gerçek AR düzlemleri oluşmaz; ancak Plan modunda
> fare ile tıklama, y=0 zemin düzlemine geri düşerek eşya yerleştirmeyi/taşımayı test
> etmeni sağlar.
