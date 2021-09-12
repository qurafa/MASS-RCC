
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class TopoLines : MonoBehaviour
{
    //public Terrain terrain;

    public List<Terrain> terrains = new List<Terrain>();

    public GameObject plane;

    public int numberOfBands = 12;

    public Color bandColor = Color.white;
    public Color bkgColor = Color.clear;

    //public Renderer outputPlain;

    public Texture2D topoMap;
    private float time = 0;

    void Start()
    {
        GenerateTopoLines();
    }

    void Update(){}

    void GenerateTopoLines()
    {
        for (int i = 0; i < terrains.Count; i++)
        {
            topoMap = ContourMap.FromTerrain(terrains[i]);
            //topoMap = ContourMap.FromTerrain( terrain, numberOfBands );
            //topoMap = ContourMap.FromTerrain(terrain, numberOfBands, bandColor, bkgColor);
            GameObject p = Instantiate(plane);
            p.SetActive(true);
            //Destroy(p, 3);
            p.name = "Plane" + i;
            Renderer rend = p.GetComponent<Renderer>();
            rend.material = new Material(Shader.Find("Standard"));
            rend.material.name = "Mat" + i;
            p.transform.position = (new Vector3(terrains[i].transform.position.x + (terrains[i].terrainData.size.x / 2), 0, terrains[i].transform.position.z + (terrains[i].terrainData.size.z / 2)));
            //p.transform.Rotate(0, 180, 0);
            p.transform.localScale = new Vector3(terrains[i].terrainData.size.x / 10, 1, terrains[i].terrainData.size.z / 10);
            p.transform.parent = terrains[i].transform.parent;
            if (rend)
            {
                rend.material.mainTexture = topoMap;
            }
        }
    }
}

/*
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TopoLines : MonoBehaviour
{

    public GameObject plane;

    public List<Terrain> terrains = new List<Terrain>();

    public Texture2D topoMap;

    private Material outputMaterial;
    private string heightmapPath;

    void Start()
    {
        //The terrains are sorted with the last added one first and the first added one last

        for (int i = 1; i <= terrains.Count; i++)
        {
            heightmapPath = @"Assets/HeightMaps/Terrain" + i + ".raw";

            GameObject p = Instantiate(plane);
            p.name = "Plane" + i;
            Renderer rend = p.GetComponent<Renderer>();
            rend.material = new Material(Shader.Find("Standard"));
            rend.material.name = "Mat" + i;
            outputMaterial = p.GetComponent<Renderer>().material;
            p.transform.position = (new Vector3(terrains[i - 1].transform.position.x + (terrains[i - 1].terrainData.size.x / 2), 0, terrains[i - 1].transform.position.z + (terrains[i - 1].terrainData.size.z / 2)));
            p.transform.Rotate(0, 180, 0);
            p.transform.localScale = new Vector3(terrains[i - 1].terrainData.size.x / 10, 1, terrains[i - 1].terrainData.size.z / 10);

            
            topoMap = ContourMap.FromRawHeightmap16bpp(heightmapPath);

            if (topoMap == null)
            {
                Debug.Log("Creation of topomap failed.");
            }
            else
            {
                Debug.Log("Creation of topomap was successful.");
            }

            if (outputMaterial != null)
            {
                Debug.Log(i + ", " + terrains[i - 1].name + ", " + outputMaterial.name + ", " + p.name + ", " + heightmapPath);
                outputMaterial.mainTexture = topoMap;
            }

        }
    }

}
*/
