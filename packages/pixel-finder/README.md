
![hero image](xx)


A unity package that leverages compute shaders to analyze a `RenderTextures` for specific pixel types.

<br><br>

- [Intro 👋](#intro-)
  - [Features 🤖](#features-)
  - [The Pixel Finder](#the-pixel-finder)
  - [Pixel Data 📷](#pixel-data-)
  - [Layouts and Systems](#layouts-and-systems)
- [Installing](#installing)
  - [OpenUPM](#openupm)
  - [Git Package](#git-package)
  - [Git Fork](#git-fork)
- [Examples](#examples)
  - [Running with the editor](#running-with-the-editor)
  - [Using a Point Cloud](#using-a-point-cloud)
  - [Extending the System or Layouts](#extending-the-system-or-layouts)
  - [Using the UI](#using-the-ui)
- [Sasaki](#sasaki)

<br><br>

## Intro 👋

<br><br>

### Features 🤖

- The `PixelFinder` object that handles the communication between scene data to the GPU 
- The `PixelFinderLayout` base object that can setup finder objects in different formations (`PixelFinderCube`, `PixelFinderHorizontal`, `PixelFinderOrtho`)     
- The `PixelFinderSystem` object that manages groups of layouts and supports working through a list of points to analyze from.
- Pixel data processed with compute shaders and stored back in the CPU for future use.      
- HLSL code for visualizing data without creating additional CPU overhead
- Handy extension methods for capturing screen shots or normalizing values 

<br><br>

### The Pixel Finder

The main purpose of this package was to create a system that could move through a series of points and calculate the amount of pixels that matched a givent set of colors and store those colors for some of our analysis tools. The `PixelFinder` object is the main component in this package as it brings all of the magic from the GPU to CPU. It Each time the GPU receieves a `RenderTexture` to read through it goes through a set of steps to find the specified colors. 


### Pixel Data 📷

When there is a pixel match there is reprojection step to set a value to the pixel in relationship to where it's positioned on the screen. Usually a pixel dead center of a texture would be of more value than one that is off to the side somewhere. 🎯


![pixel match](xx)


During runtime the pixel data is stored as an `uint` value type. Once the analysis is complete, the values are casted to an `int` value type to meet CLS Complaint. ✅       


### Layouts and Systems
How the system works and reports data.


Some info about how the layouts types work.

Layout types to go in detail about
- Cube
- Horizontal
- Noraml
- Ortho

<br><br>


## Installing

This package requires Unity 2021.x + 

This pacakge can be installed in couple of different ways. I recommend for most users going with OpenUPM to add the package to your unity project.


### OpenUPM

installed using [OpenUPM's CLI](https://github.com/openupm/openupm-cli#openupm-cli)

```
$ openupm add com.sasaki.pixelfinder
```

### Git Package

how to link the file with git URL

### Git Fork

how to fork this repo and referencing to disk


## Examples

Here are some handy examples that show how to use the package in your own unity project

### Running with the editor
info about setting up a scene!

### Using a Point Cloud
info about using a point cloud

### Extending the System or Layouts
info about coding up some stuff!

### Using the UI  
info about using raw images and data types


## Sasaki

This package is licensed and supported by Sasaki Associates

![Logo](https://github.com/sasakiassociates/unity-tools/blob/media/sasaki-logo.png?raw=true)
