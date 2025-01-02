# Procedural Terrain with Diamond Square in Unity

![Image](https://github.com/bentoBAUX/Procedural-Terrain-with-Diamond-Square-in-Unity/blob/master/Assets/Images/Image.jpg)

## Overview
In this project, I implemented the Diamond-Square Algorithm to generate procedural terrain in Unity. The algorithm is a fractal-based technique that creates realistic, randomised heightmaps, perfect for terrains in games and simulations.

To make the tool easier to use, I designed a custom Unity Editor Window. This allows users to configure and generate terrain directly within the editor, removing the need for manual scripting and making the workflow more intuitive.

The tool is functional, but I don’t consider it asset-ready yet. One significant reason is the absence of a loading bar to provide user feedback during terrain generation. Additionally, I feel the UI could be improved to enhance usability. Future revisits would be necessary to refine this project, adding features and improvements to transform it into a polished and user-friendly asset.

## How does the algorithm work?
On an abstract level, the Diamond-Square Algorithm works as follows:

1. **Initialisation**: Begin with a square grid, assigning random values to the corner points.
2. **Diamond Step**: For each square, calculate the centre point by averaging the corners and adding random noise.
3. **Square Step**: For each diamond, compute the midpoints of the edges by averaging adjacent points and adding random noise.
4. **Repeat**: Halve the step size and repeat until the grid is fully filled.
   
This iterative process results in natural-looking terrain featuring both smooth and rugged details.

## Features
- **Customisable Terrain**: Modify height, roughness, and resolution to your liking.
- **Efficient Implementation**: Dynamically generates terrain at runtime.
- **Integration Ready**: Easily integrates with Unity's terrain system.
- **Expandable Design**: Straightforward to adapt for different procedural generation needs.

## Licence
This project is licensed under the MIT Licence. See the LICENSE file for details.

