# AR Blok Evren

Kamerayla çevreyi tarayıp gördüğünü **Minecraft tarzı voksel (blok) dünyaya**
dönüştüren, blok ekleme/kırma yapabildiğin ve farklı **evren temaları** (Minecraft,
Marvel, Çizgi Film, Animasyon) arasında geçiş yapabildiğin bir **Android AR**
uygulaması.

> **Teknoloji:** Unity 2022.3 LTS + AR Foundation 5 (ARCore Depth API)

---

## Nasıl çalışıyor?

1. **Tara**: ARCore'un *environment depth* (derinlik) görüntüsü alınır. Her derinlik
   pikseli 3B dünya noktasına çevrilir (`DepthVoxelizer`) ve bir voksel ızgarasına
   yazılır. Yani telefonu gezdirdikçe çevren bloklaşır.
2. **Blok koy / kır**: Ekrana dokununca bir ışın (raycast) atılır; blok yüzeyine
   dokunursan komşu boşluğa blok eklenir veya dokunduğun blok silinir
   (`ArBlockPlacer`).
3. **Evren değiştir**: Temalar sadece doku (texture) değiştirir; blok geometrisi aynı
   kalır, bu yüzden geçiş anında olur (`ThemeManager`). Dokular çalışma anında
   **prosedürel** üretilir — projede hiçbir üçüncü parti / telifli görsel yoktur.

### Mimari (kod)

```
Assets/Scripts/
  Core/     GameBootstrap  (AR rig'i ve sistemleri koddan kurar)
            AppState, InteractionMode
  AR/       DepthVoxelizer (derinlik -> voksel)
            ArBlockPlacer  (dokunma -> blok koy/kır)
  World/    VoxelWorld, Chunk, ChunkRenderer, ChunkMeshBuilder, Coords, VoxelSettings
  Blocks/   BlockType
  Themes/   ThemeManager, ThemeLibrary, ThemeDefinition, TextureAtlas, FaceDir
  UI/       GameUI, UIFactory
Assets/Shaders/  VoxelUnlit(.Transparent).shader  (doku × köşe rengi)
```

Sahne (`Assets/Scenes/Main.unity`) kasıtlı olarak boştur: her şey `GameBootstrap`
içinde `[RuntimeInitializeOnLoadMethod]` ile koddan oluşturulur.

---

## Kurulum ve APK üretme

1. **Unity Hub** ile **Unity 2022.3 LTS** kur; **Android Build Support**
   (OpenJDK, Android SDK & NDK dahil) modülünü ekle.
2. Bu klasörü Unity ile aç (paketler `Packages/manifest.json`'dan otomatik iner).
3. **Project Settings → XR Plug-in Management → Android** sekmesinde **ARCore**'u
   işaretle. (Bu adım XR sağlayıcı ayar dosyalarını üretir.)
4. **Project Settings → Player**:
   - **Active Input Handling** = *Input System Package (New)* veya *Both*
   - **Minimum API Level** = Android 7.0 (API 24) veya üzeri
   - **Scripting Backend** = IL2CPP, **Target Architectures** = ARM64
5. **File → Build Settings**: `Assets/Scenes/Main.unity`'yi "Scenes In Build"e ekle,
   platformu **Android** yap, **Build** ile APK üret.
6. APK'yı ARCore destekli bir telefona kur.

---

## Önemli notlar / kısıtlar

- **Gerçek tarama yalnızca ARCore destekli fiziksel bir telefonda** anlamlı çalışır.
  Emülatör kamera/derinlik AR'ını gerçekçi çalıştıramaz.
- `DepthVoxelizer`'daki derinlik→dünya dönüşümü ve intrinsics hizalaması cihaza göre
  değişebilir; ilk denemede blokların konumu kaymışsa oradaki ölçek/eksen ayarları
  cihaza göre ince ayar gerektirebilir.
- Blok boyutu ve chunk boyutu `VoxelSettings.cs` içinden ayarlanır
  (`VoxelSize = 0.06 m`).

## Yeni evren (tema) ekleme

`Assets/Scripts/Themes/ThemeLibrary.cs` içine yeni bir `ThemeDefinition` ekle
(bir isim, bir stil ve blok başına renk paleti). UI tema seçici listeyi otomatik
olarak bu listeden üretir.
