# Unity AR Accordion
A Unity application for mobile devices using ARFoundation to augment a picture by presenting its components in AR space. 

## Requirements
Currently supporting only iOS.

- Unity >= 2019.1
- Packages (Preview)
  - AR Foundation >= 2.2.0 (preview.2)
  - ARKit XR Plugin >= 2.1.0 (preview.5)
  - Post Processing >= 2.2.1
- XCode 10
- iOS >= 11.3

## Usage
1. Print or project the reference image.
2. Define the physical size of the image in the Reference Image Library. 
3. Build Settings -> Switch to platform iOS.
4. Build and run the application.

## Troubleshooting
- There is a problem using depth-of-field post processing on several platforms. To fix this, apply the bugfix from https://github.com/Unity-Technologies/PostProcessing/commit/997e0c143ebfc817521a7f65170bb95dabb5e9b7 in `Packages/com.unity.postprocessing/PostProcessing/Runtime/Effects/DepthOfField.cs`

## Distribution WRM
- Build project for XCode, open in XCode
- Check, adjust signing with Distribution certificates (set PP)
- Go to 'General / App Icons and Launch Images / App Icons Source'; click on the right arrow; link 'AppIcon-1024x1024.png' at AppIcon 1024 drop zone located at the bottom of the sheet.
- Go to 'Info' and remove 'Application does not run in background' from Info.plist options
