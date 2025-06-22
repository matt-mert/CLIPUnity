# CLIPUnity

Offline, on-device image indexing and search for Unity using OpenAI’s CLIP model.

[![Unity 2021.3+](https://img.shields.io/badge/Unity-2021.3%2B-blue.svg)](#)  
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)  
[![GitHub Release](https://img.shields.io/github/v/release/matt-mert/CLIPUnity?label=latest%20release)](https://github.com/matt-mert/CLIPUnity/releases)

---

## 🚀 Features

- **Offline image indexing** with CLIP (ViT-B/32)
- **Unity Editor integration**
    - Preprocess folder of sprites → `index.pt`
    - Live search window with threshold & Top-K
    - Search through your images with prompts!
- **One-click initial setup**: auto-download prebuilt binaries from GitHub Releases
- **UPM-friendly**: assembly definitions, namespace isolation
- **Cross-platform**: Windows & macOS support

---

## 📦 Installation

1. Open Unity (2021.3 LTS or newer).
2. Window → Package Manager → **+** → *Add package from git URL…*
3. Enter: https://github.com/matt-mert/CLIPUnity.git
4. Click **Add**.

---

## ⚙️ Initial Setup

After install, go to Tools → CLIP Installer. Click **Initial Setup** to:

1. Download the correct `clip_tool` pre-built binary for your OS.
2. It will be placed in  
   `Assets/StreamingAssets/clipunity-vX.Y.Z/[windows|macos]/`.
3. Refresh the AssetDatabase if you don't see it

Remove existing binaries and re-run Initial Setup after updating to a new version tag.

---

## 🛠️ Usage

### Editor: Preprocess Sprites (Required)

Open **Tools → CLIP Editor** and click **Process Images** (with your folder):

```csharp
// Editor-only
CLIP.ProcessImages("Assets/MySprites");
```

Indexing may take a while. An index.pt file will be created at Application.persistentDataPath.

Note that you need to index the images again if you add/remove items from the folder.

### Live Search (Sample)

In the same window:

1. Click **Start Search Session**
2. Wait for 🟢 “Ready”
3. Enter a prompt, adjust Top K & Threshold
4. See thumbnail results update live as you type

### Runtime Example (C#)

```csharp
// In your game code:
var runtime = new CLIPRuntime(threshold: 0.6f);
runtime.Start();
string[] matches = runtime.Query("blue-haired warriors", 10);
runtime.Stop();

// `matches` contains the filenames of images that match your prompt.
```

## 🎯 Requirements

- Unity 2021.3 LTS or newer
- .NET 4.x Equivalent scripting runtime
- Internet access only required for the one-time initial download of the CLIP binaries; all searching is offline.

## 🤝 Contributing

Thank you for your interest <3

1. Fork this repository and clone.
2. Create a feature branch:

    ```bash
    git checkout -b feat/your-feature
    ```

3. Make your changes, commit, and push:

    ```bash
    git commit -am "Add feature X"
    git push origin feat/your-feature
    ```

4. Open a Pull Request and describe your improvements.

Please adhere to the existing code style.

## ⚖️ License

This project is licensed under the MIT License. See [LICENSE](https://github.com/matt-mert/CLIPUnity/blob/master/LICENSE.md) for full details.

## 👤 Author

Made with ❤️ by [matt-mert](https://github.com/matt-mert)