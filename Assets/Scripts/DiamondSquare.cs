using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

public class DiamondSquare
{
    private readonly int _size;
    private readonly float _xScale;
    private readonly float _yScale;
    private readonly float _heightScale;
    private readonly float _roughness;

    private readonly bool _obj;
    private readonly float[,] _heightmap;

    public DiamondSquare(int size, float xScale, float yScale, float heightScale, float roughness, bool obj)
    {
        _size = size;
        _xScale = xScale;
        _yScale = yScale;
        _heightScale = heightScale;
        _roughness = roughness;
        _obj = obj;
        _heightmap = new float[size, size];
    }
    public void GenerateHeightmap(string outputPath = null)
    {
        InitialiseCorners();
        int tileSize = _size - 1;

        float scale = 1.0f;

        while (tileSize > 1)
        {
            int half = tileSize / 2;

            // Diamond Step: Calculate the centre point of each square
            for (int x = half; x < _size; x += tileSize)
            {
                for (int y = half; y < _size; y += tileSize)
                {
                    DiamondStep(x, y, tileSize, scale);
                }
            }

            // Square Step: Fill in the edge midpoints of the grid
            // The two loops below handle the staggered nature of the grid:
            // - The first loop processes rows where the midpoints are offset by half a step.
            // - The second loop processes rows where the midpoints align with the grid.

            // First pass: Handles horizontally offset rows
            for (int y = 0; y < _size; y += tileSize)
            {
                for (int x = half; x < _size; x += tileSize)
                {
                    SquareStep(x, y, tileSize, scale);
                }
            }

            // Second pass: Handles vertically aligned rows
            for (int y = half; y < _size; y += tileSize)
            {
                for (int x = 0; x< _size; x += tileSize)
                {
                    SquareStep(x, y, tileSize, scale);
                }
            }

            // Adjust the scale of random noise for finer details
            if (_roughness == 1.0f)
            {
                scale /= 2.0f;
            }
            else
            {
                scale /= (float)Math.Pow(2.0, _roughness);
            }

            // Halve the tile size for the next iteration
            tileSize /= 2;
        }

        // Export the heightmap to an OBJ file if required
        if (_obj && !string.IsNullOrEmpty(outputPath))
        {
            WriteToFile(outputPath);
        }
    }

    private void InitialiseCorners()
    {
        _heightmap[0, 0] = Random.Range(0f, 1f);
        _heightmap[0, _size - 1] = Random.Range(0f, 1f);
        _heightmap[_size - 1, 0] = Random.Range(0f, 1f);
        _heightmap[_size - 1, _size - 1] = Random.Range(0f, 1f);
    }

    private void DiamondStep(int x, int y, int tileSize, float scale)
    {
        int half = tileSize / 2;

        float sum = 0;
        int count = 0;

        // Boolean checks to handle edge cases dynamically
        // These ensure that only valid neighbours are considered, avoiding out-of-bounds errors.
        bool left = x - half >= 0;      // Is there a valid point to the left?
        bool right = x + half < _size;  // Is there a valid point to the right?
        bool up = y - half >= 0;        // Is there a valid point above?
        bool down = y + half < _size;   // Is there a valid point below?

        // Add contributions from neighbours only if they are within bounds

        if (up && left)                 // Top-left corner
        {
            count++;
            sum += _heightmap[y - half, x - half];
        }

        if (up && right)                // Top-right corner
        {
            count++;
            sum += _heightmap[y - half, x + half];
        }

        if (down && left)               // Bottom-left corner
        {
            count++;
            sum += _heightmap[y + half, x - half];
        }

        if (down && right)              // Bottom-right corner
        {
            count++;
            sum += _heightmap[y + half, x + half];
        }

        // Compute the new value at the center point
        // Average the valid neighbors and add random noise scaled by the current iteration's scale
        _heightmap[y, x] = sum / count + NextGaussian() * scale;
    }

    private void SquareStep(int x, int y, int tileSize, float scale)
    {
        int half = tileSize / 2;

        float sum = 0;
        int count = 0;

        // Check and add the left neighbor if within bounds
        if (x - half >= 0)
        {
            sum += _heightmap[y, x - half];
            count++;
        }

        // Check and add the right neighbor if within bounds
        if (x + half < _size)
        {
            sum += _heightmap[y, x + half];
            count++;
        }

        // Check and add the top neighbor if within bounds
        if (y - half >= 0)
        {
            sum += _heightmap[y - half, x];
            count++;
        }

        // Check and add the bottom neighbor if within bounds
        if (y + half < _size)
        {
            sum += _heightmap[y + half, x];
            count++;
        }

        // Compute the new value at the edge midpoint
        // Average the valid neighbors and add random noise scaled by the current iteration's scale
        _heightmap[y, x] = sum / count + NextGaussian() * scale;
    }

    private float NextGaussian()
    {
        // https://stackoverflow.com/a/218600/9656601
        float stdDev = 1f;
        float mean = 0f;
        float u1 = 1.0f - Random.Range(0f, 1f);
        float u2 = 1.0f - Random.Range(0f, 1f);
        float randStdNormal = (float)(Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2));
        float randNormal = mean + stdDev * randStdNormal;

        return randNormal;
    }

    public void WriteToFile(string filename)
    {
        try
        {
            // Open a StreamWriter to write the OBJ file
            using (StreamWriter writer = new StreamWriter(filename, false, Encoding.ASCII))
            {
                // Write the vertex positions to the OBJ file
                WriteVertexPositions(writer);

                // Write the vertex normals for lighting calculations
                WriteVertexNormals(writer);

                // Write the texture coordinates for material mapping
                WriteVertexTexCoords(writer);

                // Write the face definitions (triangles) to the OBJ file
                // The .obj format uses 1-based indexing, so we add 1 to all indices
                for (int y = 0; y < _size - 1; y++)                 // Iterate over the grid rows
                {
                    for (int x = 0; x < _size - 1; x++)             // Iterate over the grid columns
                    {
                        // Define the four corner vertices of the current grid cell
                        int idx0 = y * _size + x + 1;               // Top-left
                        int idx1 = y * _size + (x + 1) + 1;         // Top-right
                        int idx2 = (y + 1) * _size + (x + 1) + 1;   // Bottom-right
                        int idx3 = (y + 1) * _size + x + 1;         // Bottom-left

                        // Write two triangles to form a square
                        writer.WriteLine($"f {idx0}/{idx0}/{idx0} {idx1}/{idx1}/{idx1} {idx2}/{idx2}/{idx2}");
                        writer.WriteLine($"f {idx0}/{idx0}/{idx0} {idx2}/{idx2}/{idx2} {idx3}/{idx3}/{idx3}");
                    }
                }
            }
        }
        catch (Exception e)
        {
            // Catch any exceptions during file writing and log the error message
            Console.WriteLine(e.Message);
        }
    }

    private void WriteVertexPositions(StreamWriter writer)
    {
        // Iterate through every point in the heightmap
        for (int y = 0; y < _size; y++) // Loop through rows
        {
            for (int x = 0; x < _size; x++) // Loop through columns
            {
                // Calculate the x-coordinate in world space
                // Normalized to [-1, 1] based on the grid size, then scaled by _xScale
                float xpos = _xScale * ((float)x / (_size - 1) * 2.0f - 1.0f);

                // Calculate the y-coordinate in world space
                // Normalized to [-1, 1] based on the grid size, then scaled by _yScale
                float ypos = _yScale * ((float)y / (_size - 1) * 2.0f - 1.0f);

                // Retrieve the height (z-coordinate) from the heightmap and scale it
                float zpos = _heightScale * _heightmap[y, x];

                // Write the vertex to the OBJ file
                // Format: "v x y z", where x, y, and z are the vertex positions
                // CultureInfo.InvariantCulture ensures consistent decimal formatting (e.g., '.' for decimal separator)
                writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "v {0} {1} {2}", xpos, zpos, -ypos));
            }
        }
    }

    private void WriteVertexNormals(StreamWriter writer)
    {
        // Calculate scaling factors for the x and y axes
        // These adjust the gradients based on the height-to-scale ratio and the grid resolution
        float scalingX = _heightScale / _xScale * (_size - 1.0f) / 2.0f;
        float scalingY = _heightScale / _yScale * (_size - 1.0f) / 2.0f;

        // Iterate over each point in the heightmap
        for (int y = 0; y < _size; y++) // Loop through rows
        {
            for (int x = 0; x < _size; x++) // Loop through columns
            {
                // Fetch neighboring height values or use the current point if at an edge
                float left = (x > 0) ? _heightmap[y, x - 1] : _heightmap[y, x];
                float right = (x < _size - 1) ? _heightmap[y, x + 1] : _heightmap[y, x];
                float top = (y > 0) ? _heightmap[y - 1, x] : _heightmap[y, x];
                float bottom = (y < _size - 1) ? _heightmap[y + 1, x] : _heightmap[y, x];

                // Determine the horizontal and vertical distance for gradient calculation
                // Use 2.0f for interior points and 1.0f for edge points
                float dx = (x > 0 && x < _size - 1) ? 2.0f : 1.0f;
                float dy = (y > 0 && y < _size - 1) ? 2.0f : 1.0f;

                // Calculate the gradients in the x and y directions
                float gradientX = (right - left) / dx;
                float gradientY = (bottom - top) / dy;

                // Compute the components of the normal vector
                // Negate the gradients for correct orientation
                float nx = -gradientX * scalingX;
                float ny = -gradientY * scalingY;
                float nz = 1.0f; // The z-component is 1 to ensure the normal points outward

                // Normalize the normal vector to ensure unit length
                float normalLength = (float)Math.Sqrt(nx * nx + ny * ny + nz * nz);

                nx /= normalLength;
                ny /= normalLength;
                nz /= normalLength;

                // Write the normal vector to the OBJ file
                // Format: "vn nx ny nz", where nx, ny, nz are the normalized components
                writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "vn {0} {1} {2}", nx, nz, -ny)); // nz then -ny because of Unity's coordinate system
            }
        }
    }

    private void WriteVertexTexCoords(StreamWriter writer)
    {
        // Iterate over each point in the heightmap
        for (int y = 0; y < _size; y++) // Loop through rows
        {
            for (int x = 0; x < _size; x++) // Loop through columns
            {
                // Calculate the texture coordinates (u, v)
                // u: Horizontal coordinate, normalized to [0, 1]
                float u = (float)x / (_size - 1);

                // v: Vertical coordinate, normalized to [0, 1]
                float v = (float)y / (_size - 1);

                // Write the texture coordinates to the OBJ file
                // Format: "vt u v", where u and v are the normalized coordinates
                writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "vt {0} {1}", u, v));
            }
        }
    }

    public float[,] GetHeightMap()
    {
        return _heightmap;
    }

}