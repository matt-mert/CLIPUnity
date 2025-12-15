# CLIP for Unity

Offline, on-device image indexing and search for Unity using OpenAI’s CLIP model.

[![Unity 2021.3+](https://img.shields.io/badge/Unity-2021.3%2B-blue.svg)](#)  
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://github.com/matt-mert/CLIPUnity/blob/master/LICENSE.md)  
[![GitHub Release](https://img.shields.io/github/v/release/matt-mert/CLIPUnity?label=latest%20release)](https://github.com/matt-mert/CLIPUnity/releases)

---

## Features

- **Offline image indexing** with [CLIP](https://github.com/openai/CLIP) (ViT-B/32)
- **Unity Editor integration**
    - Preprocess folder of sprites → `index.pt`
    - Live search with threshold & Top-K
    - Works both for editor-time and runtime
    - Search through your images with prompts!
- **One-click initial setup**: auto-download prebuilt binaries from GitHub Releases
- **UPM-friendly**: assembly definitions, namespace isolation
- **Cross-platform**: Windows & macOS support

---

## Install via OpenUPM

- This package is available on [OpenUPM](https://openupm.com/packages/com.mattmert.clipunity/) package registry.
- Using this method, you can easily receive updates as they are released.
- If you have [openupm-cli](https://github.com/openupm/openupm-cli) installed, run the following command in your project's directory:

```
openupm add com.mattmert.clipunity
```

- For updating, you can use the package manager UI or run the following command with the version:

```
openupm add com.mattmert.clipunity@X.X.X
```

- When updating, make sure you remove all `clipunity-*` directories under the StreamingAssets folder.

---

## Install via UPM (Package Manager UI)

- Window → Package Manager → **+** → *Add package from git URL…*
- Enter: https://github.com/matt-mert/CLIPUnity.git
- Click **Add**.


- For updating, you can use the package manager UI or enter a specific version like:

    - https://github.com/matt-mert/CLIPUnity.git#X.X.X


- When updating, make sure you remove all `clipunity-*` directories under the StreamingAssets folder.

---

## Install via UPM (Manually)

- Open the `Packages/manifest.json` file in your project. Then add this package to the `dependencies` block:

```json
  {
    "dependencies": {
      ...
      "com.mattmert.clipunity": "https://github.com/matt-mert/CLIPUnity.git",
      ...
    }
  }
  ```

- For updating, you can use the package manager UI or enter #X.X.X suffix

- When updating, make sure you remove all `clipunity-*` directories under the StreamingAssets folder.

---

## Initial Setup

After install, go to Tools → CLIP Installer. Click **Initial Setup** to:

- Download the correct `clip_tool` pre-built binary for your OS.
- It will be placed in  
   `Assets/StreamingAssets/clipunity-vX.Y.Z/[windows|macos]/`.
- Refresh the AssetDatabase if you don't see it

⚠️⚠️⚠️ Remove existing binaries (under the StreamingAssets folder) and re-run Initial Setup after updating to a new version tag.

---

## Usage

### Editor: Preprocess Sprites (Required)

Open **Tools → CLIP Editor** and click **Process Images** (with your folder):

```csharp
// Editor-only
CLIP.ProcessImages("Assets/MySprites");
```

Indexing may take a while. An index.pt file will be created at Application.persistentDataPath.

Note that you need to index the images again if you add/remove items from the folder.

### Live Search (Editor Window)

In the same window:

- Click **Start Search Session**
- Wait for 🟢 “Ready”
- Enter a prompt, adjust Top K & Threshold
- See thumbnail results update live as you type

### Runtime Example (C#)

```csharp
// In your game code:
var runtime = new CLIPRuntime(threshold: 0.6f);
runtime.Start();
string[] matches = runtime.Query("cute animals", 10);
runtime.Stop();

// `matches` contains the filenames of images that match your prompt.
```

## Requirements

- Unity 2021.3 LTS or newer
- .NET 4.x Equivalent scripting runtime
- Internet access only required for the one-time initial download of the CLIP binaries; all searching is offline.

## Contributing

Thank you for your interest <3

- Fork this repository and clone.
- Create a feature branch:

    ```bash
    git checkout -b feat/your-feature
    ```

- Make your changes, commit, and push:

    ```bash
    git commit -am "Add feature X"
    git push origin feat/your-feature
    ```

- Open a Pull Request and describe your improvements.

Please adhere to the existing code style.

## License

This project is licensed under the MIT License. See [LICENSE](https://github.com/matt-mert/CLIPUnity/blob/master/LICENSE.md) for full details.

## Author

Made with ❤️ by [matt-mert](https://github.com/matt-mert)
