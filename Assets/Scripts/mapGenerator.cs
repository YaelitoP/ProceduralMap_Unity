using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mapGenerator : MonoBehaviour
{
    public bool autoUpdate;
    public int mapWidth;
    public int mapHeight;
    public float noiseScale;
    public void generateMap(){
    float[,] noiseMap = noise.generateNoiseMap(mapHeight, mapWidth, noiseScale);

    mapDisplay display = FindObjectOfType<mapDisplay>();
    display.drawNoiseMap(noiseMap);
   }
}
