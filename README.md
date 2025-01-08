# Procedural Terrain with Diamond Square in Unity

![Image](https://github.com/bentoBAUX/Procedural-Terrain-with-Diamond-Square-in-Unity/blob/master/Assets/Images/Image.jpg)

## Overview
In this project, I implemented the Diamond-Square Algorithm to generate procedural terrains in Unity. The algorithm is a fractal-based technique that creates realistic, randomised heightmaps, perfect for terrains in games and simulations.

To make the tool easier to use, I designed a custom Unity Editor Window. This allows users to configure and generate terrain directly within the editor, removing the need for manual scripting and making the workflow more intuitive.

The tool is functional, but I don’t consider it asset-ready yet. One significant reason is the absence of a loading bar to provide user feedback during terrain generation. Additionally, I feel the UI could be improved to enhance usability. Future revisits would be necessary to refine this project, adding features and improvements to transform it into a polished and user-friendly asset.

## How does the algorithm work?

![Diagram](https://github.com/bentoBAUX/Procedural-Terrain-with-Diamond-Square-in-Unity/blob/master/Assets/Images/DiamondSquare.jpg)
*Image credit: Christopher Ewin on Wikipedia*

On an **abstract** level, the Diamond-Square Algorithm works as follows:

1. **Initialisation**: Begin with a square grid, assigning random values to the corner points.
2. **Diamond Step**: For each square, calculate the centre point by averaging the corners and adding random noise.
3. **Square Step**: For each diamond, compute the midpoints of the edges by averaging adjacent points and adding random noise.
4. **Repeat**: Halve the step size and repeat until the grid is fully filled.
   
This iterative process results in natural-looking terrain featuring both smooth and rugged details.

On a **technical** level, the Diamond-Square Algorithm works as follows:

The terrain is generated by computing its heightmap, a 2D array where each cell corresponds to a specific (x, y) coordinate on the terrain, and its value represents the elevation (z) at that point.

1. **Grid Initialization**: Grid Size Calculation: The algorithm requires a grid size of $2^n + 1$, $n \in \mathbb{N}$, where $n$ determines the resolution of the terrain. The user provides $n$ via the resolution slider, which directly sets the grid size. This ensures the size is always valid and eliminates the need for manual adjustment or correction.
  
   Random values are then assigned to the four corner points of the grid.

2. **Iterative Refinement**:

   - **Diamond Step**: For each square, the centre point (diamond) is computed as the average of the four corner points, with an added random offset proportional to the current scale. This introduces controlled roughness.
   - **Square Step**: For each diamond, the midpoints of the square’s edges are computed by averaging adjacent points, again adding a random offset. Edge wrapping or boundary handling ensures the grid remains valid.
     
3. **Noise Reduction**: After each iteration, the scale of the random offset is reduced, typically halving with each step, to refine the terrain and add finer details.

4. **Termination**: The process repeats until the grid is fully populated, with progressively smaller steps producing increasingly detailed terrain.

5. **Export**: The final heightmap is exported to an OBJ file, generating a 3D terrain mesh. Each vertex is assigned:

   - **Position**: Calculated based on the (x, y) grid coordinates and heightmap values.
   - **Normal Vector**: Derived from the gradient of the heightmap for accurate lighting calculations.
   - **Texture Coordinates**: Mapped linearly based on grid positions for applying materials.
     
However, **vertex tangents**, necessary for effects like normal mapping, are not computed in this implementation. The script for this can be found [here](https://github.com/bentoBAUX/Procedural-Terrain-with-Diamond-Square-in-Unity/blob/master/Assets/Scripts/DiamondSquare.cs).

## Usage
To use this project, follow these steps:

1. **Clone the Repository**: Clone the project and open it in Unity.
2. **Access the Generator**: Navigate to Tools > Diamond Square Generator to open the editor window.
3. **Recommended Settings**: For the best results, keep the default settings and set the resolution to 10. Generating with this resolution may take some time.
4. **View the Generated Asset**: While the generation is in progress, you can spam Alt+Tab to switch between windows. Once completed, the generated object will appear in your Assets folder.

This workflow basically imports a new asset into your project each time you generate a terrain. While functional, this isn't optimal. Future updates to this project will focus on enhancing the editor's usability for a more streamlined experience.

## Licence
This project is licensed under the MIT Licence. See the [LICENSE](https://github.com/bentoBAUX/Procedural-Terrain-with-Diamond-Square-in-Unity/blob/master/LICENSE) file for details.

