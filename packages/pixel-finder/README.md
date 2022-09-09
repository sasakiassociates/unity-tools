
![hero image](xx)


A unity package that leverages compute shaders to analyze a `RenderTextures` for specific pixel types.

<br><br>

- [Intro 👋](#intro-)
  - [Features 🤖](#features-)
  - [Pixel Data 📷](#pixel-data-)
  - [Requirements ✔](#requirements-)
- [Installing](#installing)
  - [OpenUPM](#openupm)
  - [Git Package](#git-package)
  - [Git Fork](#git-fork)
- [Examples](#examples)
  - [Code](#code)
  - [Editor](#editor)
- [Sasaki](#sasaki)

<br><br>

## Intro 👋

aaa

### Features 🤖

- The `PixelFinder` object that handles the communication between scene data to the GPU 
- The `PixelFinderLayout` base object that can setup finder objects in different formations (`PixelFinderCube`, `PixelFinderHorizontal`, `PixelFinderOrtho`)     
- The `PixelFinderSystem` object that manages groups of layouts and supports working through a list of points to analyze from.
- Pixel data processed with compute shaders and stored back in the CPU for future use.      
- HLSL code for visualizing data without creating additional CPU overhead
- Handy extension methods for capturing screen shots or normalizing values 


### Pixel Data 📷

The main purpose of this package was to create a system that could move through a series of points and calculate the amount of pixels that matched a givent set of colors and store those colors for some of our analysis tools. Each time the GPU receieves a `RenderTexture` to read through it goes through a set of steps to find the specified colors. 

![pixel match](xx)


When there is a pixel match there is reprojection step to set a value to the pixel in relationship to where it's positioned on the screen. Usually a pixel dead center of a texture would be of more value than one that is off to the side somewhere. 🎯


During runtime the pixel data is stored as an `uint` value type. Once the analysis is complete, the values are casted to an `int` value type to meet CLS Complaint. ✅       


### Requirements ✔
This package requires Unity 2021.x + 

## Installing

This pacakge can be installed in couple of different ways. I recommend for most users going with OpenUPM to add the package to your unity project.


### OpenUPM

installed using [OpenUPM's CLI](https://github.com/openupm/openupm-cli#openupm-cli)

```
$ openupm add com.sasaki.pixelfinder
```

### Git Package

### Git Fork





## Examples
### Code
### Editor


## Sasaki

This package is licensed and supported by Sasaki Associates

![Logo](https://github.com/sasakiassociates/unity-tools/blob/media/sasaki-logo.png?raw=true)
