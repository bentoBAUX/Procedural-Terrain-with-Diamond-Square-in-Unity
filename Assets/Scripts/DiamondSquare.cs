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
        Trace.Assert(IsPowerOfTwo(size));
        _size = size;
        _xScale = xScale;
        _yScale = yScale;
        _heightScale = heightScale;
        _roughness = roughness;
        _obj = obj;
        _heightmap = new float[size, size];
        GenerateHeightmap();
    }

    private bool IsPowerOfTwo(int x)
    {
        return (x != 0) && ((x & (x - 1)) == 0);
    }

    public void GenerateHeightmap()
    {
        InitialiseCorners();
        int tileSize = _size - 1;

        float scale = 1.0f;

        while (tileSize > 1)
        {
            int half = tileSize / 2;

            // Diamond Step
            for (int x = half; x < _size; x += tileSize)
            {
                for (int y = half; y < _size; y += tileSize)
                {
                    DiamondStep(x, y, tileSize, scale);
                }
            }

            // Square Step
            for (int y = 0; y < _size; y += tileSize)
            {
                for (int x = half; x < _size; x += tileSize)
                {
                    SquareStep(x, y, tileSize, scale);
                }
            }

            for (int y = half; y < _size; y += tileSize)
            {
                for (int x = 0; x< _size; x += tileSize)
                {
                    SquareStep(x, y, tileSize, scale);
                }
            }

            if (_roughness == 1.0f)
            {
                scale /= 2.0f;
            }
            else
            {
                scale /= (float)Math.Pow(2.0, _roughness);
            }

            tileSize /= 2;
        }

        if(_obj) WriteToFile(Application.dataPath + "/Terrain.obj");

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

        bool left = x - half >= 0;
        bool right = x + half < _size;
        bool up = y - half >= 0;
        bool down = y + half < _size;

        if (up && left)
        {
            count++;
            sum += _heightmap[y - half, x - half];
        }

        if (up && right)
        {
            count++;
            sum += _heightmap[y - half, x + half];
        }

        if (down && left)
        {
            count++;
            sum += _heightmap[y + half, x - half];
        }

        if (down && right)
        {
            count++;
            sum += _heightmap[y + half, x + half];
        }

        _heightmap[y, x] = sum / count + NextGaussian() * scale;
    }

    private void SquareStep(int x, int y, int tileSize, float scale)
    {
        int half = tileSize / 2;

        float sum = 0;
        int count = 0;

        if (x - half >= 0)
        {
            sum += _heightmap[y, x - half];
            count++;
        }

        if (x + half < _size)
        {
            sum += _heightmap[y, x + half];
            count++;
        }

        if (y - half >= 0)
        {
            sum += _heightmap[y - half, x];
            count++;
        }

        if (y + half < _size)
        {
            sum += _heightmap[y + half, x];
            count++;
        }

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
            using (StreamWriter writer = new StreamWriter(filename, false, Encoding.ASCII))
            {
                WriteVertexPositions(writer);
                WriteVertexNormals(writer);
                WriteVertexTexCoords(writer);

                // Write all triangle indices. Indices in the .obj format start at 1.
                for (int y = 0; y < _size - 1; y++)
                {
                    for (int x = 0; x < _size - 1; x++)
                    {
                        int idx0 = y * _size + x + 1;
                        int idx1 = y * _size + (x + 1) + 1;
                        int idx2 = (y + 1) * _size + (x + 1) + 1;
                        int idx3 = (y + 1) * _size + x + 1;

                        writer.WriteLine($"f {idx0}/{idx0}/{idx0} {idx1}/{idx1}/{idx1} {idx2}/{idx2}/{idx2}");
                        writer.WriteLine($"f {idx0}/{idx0}/{idx0} {idx2}/{idx2}/{idx2} {idx3}/{idx3}/{idx3}");
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }

    private void WriteVertexPositions(StreamWriter writer)
    {
        for (int y = 0; y < _size; y++)
        {
            for (int x = 0; x < _size; x++)
            {
                float xpos = _xScale * ((float)x / (_size - 1) * 2.0f - 1.0f);
                float ypos = _yScale * ((float)y / (_size - 1) * 2.0f - 1.0f);
                float zpos = _heightScale * _heightmap[y, x];
                writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "v {0} {1} {2}", xpos, zpos, -ypos));
            }
        }
    }

    private void WriteVertexNormals(StreamWriter writer)
    {
        float scalingX = _heightScale / _xScale * (_size - 1.0f) / 2.0f;
        float scalingY = _heightScale / _yScale * (_size - 1.0f) / 2.0f;

        for (int y = 0; y < _size; y++)
        {
            for (int x = 0; x < _size; x++)
            {
                float left = (x > 0) ? _heightmap[y, x - 1] : _heightmap[y, x];
                float right = (x < _size - 1) ? _heightmap[y, x + 1] : _heightmap[y, x];
                float top = (y > 0) ? _heightmap[y - 1, x] : _heightmap[y, x];
                float bottom = (y < _size - 1) ? _heightmap[y + 1, x] : _heightmap[y, x];

                float dx = (x > 0 && x < _size - 1) ? 2.0f : 1.0f;
                float dy = (y > 0 && y < _size - 1) ? 2.0f : 1.0f;

                float gradientX = (right - left) / dx;
                float gradientY = (bottom - top) / dy;

                float nx = -gradientX * scalingX;
                float ny = -gradientY * scalingY;
                float nz = 1.0f;

                float normalLength = (float)Math.Sqrt(nx * nx + ny * ny + nz * nz);

                nx /= normalLength;
                ny /= normalLength;
                nz /= normalLength;

                writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "vn {0} {1} {2}", nx, nz, -ny));
            }
        }
    }

    private void WriteVertexTexCoords(StreamWriter writer)
    {
        for (int y = 0; y < _size; y++)
        {
            for (int x = 0; x < _size; x++)
            {
                float u = (float)x / (_size - 1);
                float v = (float)y / (_size - 1);
                writer.WriteLine(string.Format(CultureInfo.InvariantCulture, "vt {0} {1}", u, v));
            }
        }
    }

    public float[,] GetHeightMap()
    {
        return _heightmap;
    }

}