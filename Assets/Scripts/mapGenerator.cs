using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class mapGenerator : MonoBehaviour
{   public const int mapChunkSize = 241;
    public enum DrawMode {NOISE, COLOUR, MESH};
    public DrawMode drawMode;
    public int octaves;
    [Range(0,1)]
    public float persistance;
    public float lacunarity;
   
    public bool autoUpdate;
    [Range(0,6)]
    public int levelOfDetail;
    public float noiseScale;
    public float heightMultiplier;
    public AnimationCurve meshCurve;
    public int seed;
    public Vector2 offsets;

    public TerrainSet[] regions;

    public void generateMap(){
        float[,] noiseMap = noise.generateNoiseMap (mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offsets);

        mapDisplay display = FindObjectOfType<mapDisplay>();
        
        Color[] colourMap =  new Color [mapChunkSize * mapChunkSize];

        for (int x = 0; x < mapChunkSize; x++){
            for (int y = 0; y < mapChunkSize; y++){
                float currentHeight = noiseMap[x,y];

                for (int i = 0; i < regions.Length; i++){

                    if (currentHeight < regions[i].height){

                        colourMap[y * mapChunkSize + x] = regions[i].colour;
                        break;
                    }
                }
            }
        }
        
        if (drawMode == DrawMode.NOISE){
            display.drawTexture(textureCreation.TextureFromHeightMap(noiseMap));

        }else if (drawMode == DrawMode.COLOUR){
            display.drawTexture(textureCreation.TextureFromColourMap(colourMap, mapChunkSize, mapChunkSize));

        }else if (drawMode == DrawMode.MESH){
            display.drawMesh(MeshGenerator.generateMesh(noiseMap, heightMultiplier, meshCurve, levelOfDetail), textureCreation.TextureFromColourMap(colourMap, mapChunkSize, mapChunkSize));

        }
        
    }


    void OnValidate(){
    if (octaves < 0){
        octaves = 0;}

    if (lacunarity < 1){
        lacunarity = 1;}
    }
}   

[System.Serializable]
public struct TerrainSet{
    public string names;
    public float height;
    public Color colour;


}
