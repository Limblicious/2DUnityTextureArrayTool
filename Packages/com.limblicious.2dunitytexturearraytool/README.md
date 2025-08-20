# 2D Unity Texture Array Tool

A Unity editor tool for exporting slices from Texture2DArrays to PNG images and rebuilding Texture2DArrays from individual image files.

## Features

- **Export Texture2DArray to PNGs**: Extract individual slices from a Texture2DArray asset and save them as PNG files
- **Build Texture2DArray from PNGs**: Create a new Texture2DArray asset from a folder of PNG/JPG/TGA images
- **Configurable settings**: Control texture format, mipmaps, color space, filtering, and more
- **Automatic texture import handling**: Ensures proper import settings for source images

## Installation

### Via Git URL (Recommended)

1. Open Unity Package Manager (`Window ▸ Package Manager`)
2. Click the `+` button and select `Add package from git URL...`
3. Enter the following URL:
   ```
   https://github.com/limblicious/2d-unity-texture-array-tool.git#1.0.0
   ```

### Via .unitypackage

Download the latest `.unitypackage` from the [Releases](https://github.com/limblicious/2d-unity-texture-array-tool/releases) page and import it into your project.

## Usage

1. Open the tool window via `Tools ▸ Texture2DArray Toolkit`

### Exporting Texture2DArray to PNGs

1. Drag your Texture2DArray asset into the "Source Array" field
2. Set the "Export To Folder" path (defaults to `Assets/Exported_T2DArray`)
3. Click "Export Slices → PNG"

The tool will create numbered PNG files (`slice_000.png`, `slice_001.png`, etc.) in the specified folder.

### Building Texture2DArray from PNGs

1. Place your PNG/JPG/TGA images in a folder (defaults to `Assets/Edited_T2DArray`)
2. Configure the array settings:
   - **Width/Height**: Target dimensions for the array
   - **Generate Mipmaps**: Whether to create mipmaps
   - **Texture Format**: Internal format for the array
   - **Linear**: Color space (unchecked = sRGB)
   - **Filter/Wrap modes**: Sampling settings
   - **Anisotropic filtering**: Quality settings
3. Set the "Save As" path for the new Texture2DArray asset
4. Click "Build Texture2DArray from PNG folder"

The tool will automatically configure import settings for your source images and create a new Texture2DArray asset.

## Requirements

- Unity 2022.3.22f1 or later
- Compatible with URP, HDRP, and Built-in Render Pipeline

## License

MIT License - see [LICENSE](LICENSE) file for details.