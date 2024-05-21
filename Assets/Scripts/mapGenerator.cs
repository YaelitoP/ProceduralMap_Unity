using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;

public class mapGenerator : MonoBehaviour
{   public const int mapChunkSize = 241;
    public enum DrawMode {NOISE, COLOUR, MESH};
    public DrawMode drawMode;
    public noise.NormalizeMode normalizeMode;
    public int octaves;
    [Range(0,1)]
    public float persistance;
    public float lacunarity;
   
    public bool autoUpdate;
    [Range(0,6)]
    public int editorLOD;
    public float noiseScale;
    public float heightMultiplier;
    public AnimationCurve meshCurve;
    public int seed;
    public Vector2 offsets;
    public TerrainSet[] regions;
    Queue<mapThreadInfo<mapData>> mapDataThreadInfoQueue = new Queue<mapThreadInfo<mapData>>();
    Queue<mapThreadInfo<meshData>> meshDataThreadInfoQueue = new Queue<mapThreadInfo<meshData>>();

    public void drawInEditor(){

        mapData mapData = generateMapData(Vector2.zero);
        mapDisplay display = FindObjectOfType<mapDisplay>();

        if (drawMode == DrawMode.NOISE){
            display.drawTexture(textureCreation.TextureFromHeightMap(mapData.heightMap));
        }else if (drawMode == DrawMode.COLOUR){
            display.drawTexture(textureCreation.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));
        }else if (drawMode == DrawMode.MESH){
            display.drawMesh(MeshGenerator.generateMesh(mapData.heightMap, heightMultiplier, meshCurve, editorLOD), textureCreation.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));
        }
    }


    public void requestMapData (Vector2 center, Action<mapData> callback){
        ThreadStart threadStart = delegate{
            mapDataThread(center, callback);
        };
        new Thread(threadStart).Start();
    }

    public void requestMeshData(mapData mapData, int lod, Action<meshData> callback){
          ThreadStart threadStart = delegate{
            meshDataThread(mapData, lod, callback);
        };
        new Thread(threadStart).Start();
    }

    void meshDataThread(mapData mapData, int lod, Action<meshData> callback){
        meshData meshData = MeshGenerator.generateMesh(mapData.heightMap, heightMultiplier, meshCurve, lod);
        lock (meshDataThreadInfoQueue){
            meshDataThreadInfoQueue.Enqueue(new mapThreadInfo<meshData>(callback, meshData));
        }
    }

    void mapDataThread(Vector2 center, Action<mapData> callback){
        mapData mapData = generateMapData(center);
        lock (mapDataThreadInfoQueue){
            mapDataThreadInfoQueue.Enqueue (new mapThreadInfo<mapData>(callback, mapData));
        }
    }

    void Update(){

        if (mapDataThreadInfoQueue.Count > 0){
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++) {
                mapThreadInfo<mapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback (threadInfo.parameter);
            }
        }

        if (meshDataThreadInfoQueue.Count > 0){
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++) {
                mapThreadInfo<meshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback (threadInfo.parameter);
            }
        }
    }


    mapData generateMapData(Vector2 center){
        float[,] noiseMap = noise.generateNoiseMap (mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, center + offsets, normalizeMode);
        Color[] colourMap =  new Color [mapChunkSize * mapChunkSize];
        for (int x = 0; x < mapChunkSize; x++){
            for (int y = 0; y < mapChunkSize; y++){
                float currentHeight = noiseMap[x,y];
                for (int i = 0; i < regions.Length; i++){
                    if (currentHeight >= regions[i].height){
                        colourMap[y * mapChunkSize + x] = regions[i].colour;
                    }else{
                        break;
                    }
                }
            }
        }
        return new mapData(noiseMap, colourMap);
    }


    void OnValidate(){
    if (octaves < 0){
        octaves = 0;}

    if (lacunarity < 1){
        lacunarity = 1;}
    }

    struct mapThreadInfo<T> {
        public readonly Action<T> callback;
        public readonly T parameter;
        public mapThreadInfo(Action<T> callback, T parameter) {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}   

[System.Serializable]
public struct TerrainSet{
    public string names;
    public float height;
    public Color colour;
}

public struct mapData{
    public readonly float[,] heightMap;
    public readonly Color[] colourMap;
    public mapData (float[,] heightMap, Color[] colourMap){
        this.heightMap = heightMap;
        this.colourMap = colourMap;
    }
}