
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class ContourMap : MonoBehaviour
{
    // Creates contour map from terrain heightmap data as Texture2D

    // use default colours and optional parameter numberOfBands
    public static Texture2D FromTerrain(Terrain terrain, int numberOfBands = 20)
    {
        return FromTerrain(terrain, numberOfBands, Color.black, Color.clear);
    }

    // define all parameters
    public static Texture2D FromTerrain(Terrain terrain, int numberOfBands, Color bandColor, Color bkgColor)
    {
        //sea level
        float seaLevel = 0.06f;

        // dimensions
        int width = terrain.terrainData.heightmapResolution;
        int height = terrain.terrainData.heightmapResolution;

        // heightmap data
        float[,] heightmap = terrain.terrainData.GetHeights(0, 0, width, height);

        // Create Output Texture2D with heightmap dimensions
        Texture2D topoMap = new Texture2D(width, height);
        topoMap.anisoLevel = 16;

        // array for storing colours to be applied to texture
        Color[] colourArray = new Color[width * height];
        string tag = terrain.tag;

        if (tag.Equals("rainforest"))
            bkgColor = new Color(0.01568628f, 0.9960785f, 0.07450981f, 0);
        else if (tag.Equals("temperateforest"))
            bkgColor = new Color(0.01568628f, 0.7450981f, 0.345098f, 0);
        else if(tag.Equals("desert"))
            bkgColor = new Color(0.9843138f, 0.8352942f, 0.01960784f, 0);
        else if (tag.Equals("tundra"))
            bkgColor = new Color(0.5843138f, 0.9960785f, 0.9764706f, 0);
        else if (tag.Equals("taiga"))
            bkgColor = new Color(0.1019608f, 0.4745098f, 0.3568628f, 0);
        else if (tag.Equals("grassland"))
            bkgColor = new Color(0.9019608f, 0.9843138f, 0.4784314f, 0);
        else if (tag.Equals("savanna"))
            bkgColor = new Color(0.9882354f, 0.6039216f, 0.01568628f, 0);
        else if (tag.Equals("freshwater"))
            bkgColor = new Color(0.145098f, 0.145098f, 0.9725491f, 0);
        else if (tag.Equals("ice"))
            bkgColor = new Color(0.9882354f, 0.9921569f, 0.9882354f, 0);
        else
            bkgColor = Color.clear;

        // Set background
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if(heightmap[y,x] > seaLevel)
                    colourArray[(y * width) + x] = bkgColor;
                else
                    colourArray[(y * width) + x] = new Color(0.4901961f, 0.7803922f, 0.9333334f, 0);
            }
        }

        colourArray = ContourMap.RotateNinety(height, width, colourArray, 2);

        // Initial Min/Max values for normalized terrain heightmap values
        float minHeight = 1f;
        float maxHeight = 0;

        // Find lowest and highest points
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (minHeight > heightmap[y, x])
                {
                    minHeight = heightmap[y, x];
                }
                if (maxHeight < heightmap[y, x])
                {
                    maxHeight = heightmap[y, x];
                }
            }
        }

        // Create height band list
        float bandDistance = (maxHeight - minHeight) / (float)numberOfBands; // Number of height bands to create

        List<float> bands = new List<float>();

        // Get ranges
        float r = minHeight + bandDistance;
        while (r < maxHeight)
        {
            bands.Add(r);
            r += bandDistance;
        }

        // Create slice buffer
        bool[,] slice = new bool[width, height];

        // Draw bands
        for (int b = 0; b < bands.Count; b++)
        {
            // Get Slice
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (heightmap[y, x] >= bands[b])
                    {
                        slice[x, y] = true;
                    }
                    else
                    {
                        slice[x, y] = false;
                    }
                }
            }

            // Detect edges on slice and write to output
            for (int y = 1; y < height - 1; y++)
            {
                for (int x = 1; x < width - 1; x++)
                {
                    if (slice[x, y] == true && heightmap[y, x] > seaLevel)
                    {
                        if (
                            slice[x - 1, y] == false ||
                            slice[x + 1, y] == false ||
                            slice[x, y - 1] == false ||
                            slice[x, y + 1] == false)
                        {
                            // heightmap is read y,x from bottom left
                            // texture is read x,y from top left
                            // magic equation to find correct array index
                            int ind = ((height - y - 1) * width) + (width - x - 1);

                            colourArray[ind] = bandColor;
                        }
                    }
                }
            }

        }

        // apply colour array to texture
        topoMap.SetPixels(colourArray);
        topoMap.Apply();

        // Return result
        return topoMap;
    }

    public static Color[] RotateNinety(int height, int width, Color[] c, int r)
    {
        Color[] temp = c;
        Color[] o = new Color[c.Length];
        if (r < 1)
            o = c;
        else
            for (int i = 0; i < r; i++)
            {
                o = new Color[c.Length];
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        o[(x * width) + (height - 1 - y)] = temp[(y * width) + x];
                    }
                }
                temp = o;
            }

        return o;
    }

}



/*
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class ContourMap
{
    //Creates contour map from Raw 16bpp heightmap data as Texture2D
    //Returns null on failure
    //If neither width nor height specified, then it's POT size will be guessed
    public static Texture2D FromRawHeightmap16bpp(string fileName, int width = 0, int height = 0)
    {
        if (!File.Exists(fileName))
        {
            Debug.Log("Heightmap not found " + fileName);
            return null;
        }

        //dimensions
        int _width = width;
        int _height = height;

        Color32 bandColor = new Color32(255, 255, 255, 255);
        Color32 bkgColor = Color.clear;//new Color32(0, 0, 0, 0);

        //Output
        Texture2D topoMap;

        //Read raw 16bit heightmap
        byte[] rawBytes = System.IO.File.ReadAllBytes(fileName);
        short[] rawImage = new short[rawBytes.Length / 2];

        //Create slice buffer
        bool[] slice = new bool[rawImage.Length];

        //Convert to bytes to short
        Buffer.BlockCopy(rawBytes, 0, rawImage, 0, rawBytes.Length);

        //Create Texture2D with estimated or specified width
        if (_width == 0 || _height == 0)
        {
            _width = (int)Math.Sqrt(rawImage.Length); //Estimated width/height
            _height = _width;
            topoMap = new Texture2D(_width, _height);
        }
        else
        {
            topoMap = new Texture2D(_width, _height);
        }

        topoMap.anisoLevel = 16;

        //Set background
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                topoMap.SetPixel(x, y, bkgColor);
            }
        }

        //Initial Min/Max values for signed 16bit value
        int minHeight = 32767;
        int maxHeight = -32767;

        //Find lowest and highest points
        for (int i = 0; i < rawImage.Length; i++)
        {
            if (rawImage[i] < minHeight)
            {
                minHeight = rawImage[i];
            }
            if (rawImage[i] > maxHeight)
            {
                maxHeight = rawImage[i];
            }
        }

        Debug.Log("Min: " + minHeight.ToString() + ", Max: " + maxHeight.ToString());

        //Create height band list
        int bandDistance = maxHeight / 12; //Number of height bands to create

        List<int> bands = new List<int>();

        //Get ranges
        int r = minHeight + bandDistance;
        while (r < maxHeight)
        {
            bands.Add(r);
            r += bandDistance;
        }

        //Draw bands
        for (int b = 0; b < bands.Count; b++)
        {

            //Get Slice
            for (int i = 0; i < rawImage.Length; i++)
            {
                if (rawImage[i] >= bands[b])
                {
                    slice[i] = true;
                }
                else
                {
                    slice[i] = false;
                }
            }

            //Detect edges on slice and write to output
            for (int y = 1; y < _height - 1; y++)
            {
                for (int x = 1; x < _width - 1; x++)
                {
                    if (slice[y * _width + x] == true)
                    {
                        if (slice[y * _width + (x - 1)] == false || slice[y * _width + (x + 1)] == false || slice[(y - 1) * _width + x] == false || slice[(y + 1) * _width + x] == false)
                        {
                            topoMap.SetPixel(x, y, bandColor);
                        }
                    }
                }
            }

        }

        topoMap.Apply();

        //Return result
        return topoMap;
    }
}
*/
