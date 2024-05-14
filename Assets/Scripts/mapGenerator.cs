using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class mapGenerator : MonoBehaviour
{
    public enum DrawMode {NOISE, COLOUR, MESH};
    public DrawMode drawMode;
    public int octaves;
    [Range(0,1)]
    public float persistance;
    public float lacunarity;
   
    public bool autoUpdate;
    public int mapWidth;
    public int mapHeight;
    public float noiseScale;
    public float heightMultiplier;
    public AnimationCurve meshCurve;
    public int seed;
    public Vector2 offsets;

    public TerrainSet[] regions;

    public void generateMap(){
        float[,] noiseMap = noise.generateNoiseMap(mapHeight, mapWidth, seed, noiseScale, octaves, persistance, lacunarity, offsets);

        mapDisplay display = FindObjectOfType<mapDisplay>();
        
        Color[] colourMap =  new Color[mapWidth * mapHeight];

        for (int x = 0; x < mapWidth; x++){
            for (int y = 0; y < mapHeight; y++){
                float currentHeight = noiseMap[x,y];

                for (int i = 0; i < regions.Length; i++){

                    if (currentHeight < regions[i].height){

                        colourMap[y * mapWidth + x] = regions[i].colour;
                        break;
                    }
                }
            }
        }
        
        if (drawMode == DrawMode.NOISE){
            display.drawTexture(textureCreation.TextureFromHeightMap(noiseMap));

        }else if (drawMode == DrawMode.COLOUR){
            display.drawTexture(textureCreation.TextureFromColourMap(colourMap, mapWidth, mapHeight));

        }else if (drawMode == DrawMode.MESH){
            display.drawMesh(MeshGenerator.generateMesh(noiseMap, heightMultiplier, meshCurve), textureCreation.TextureFromColourMap(colourMap, mapWidth, mapHeight));

        }
        
    }


    void OnValidate(){
    if (octaves < 0){
        octaves = 0;}

    if (lacunarity < 1){
        lacunarity = 1;}

    if (mapWidth < 1){
        mapWidth = 1;}

    if (mapHeight < 1){
        mapHeight = 1;}
}
}

[System.Serializable]
public struct TerrainSet{
    public string names;
    public float height;
    public Color colour;


}
