2D Unity Texture Array Tool - Quick Start Guide

This sample demonstrates how to use the Texture2DArray Toolkit to export and rebuild texture arrays.

GETTING STARTED:
1. Open the tool window: Tools ▸ Texture2DArray Toolkit
2. To export an existing Texture2DArray:
   - Drag your Texture2DArray asset into the "Source Array" field
   - Set export folder path (default: Assets/Exported_T2DArray)
   - Click "Export Slices → PNG"

3. To build a new Texture2DArray:
   - Place your PNG/JPG/TGA images in a folder
   - Set the import folder path in the tool
   - Configure width, height, and other settings
   - Set save path for the new asset
   - Click "Build Texture2DArray from PNG folder"

TIPS:
- Images are processed alphabetically by filename
- The tool automatically configures import settings for source textures
- Use consistent naming (frame_001.png, frame_002.png, etc.) for best results
- Check the Console window for any import warnings or errors

For detailed documentation, see the README.md in the package root.