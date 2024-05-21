using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class noise
{
    public enum NormalizeMode {local, global}
    public static float[,] generateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offSets, NormalizeMode mode){
        
        float[,] noiseMap = new float[mapWidth, mapHeight];
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOfSets =  new Vector2[octaves];
        float amplitud = 1;
        float frequency = 1;
        float maxPossibleHeight = 0;

        for (int i = 0; i < octaves; i ++){
            float offSetsX = prng.Next(-10000,10000) + offSets.x;
            float offSetsY = prng.Next(-10000,10000) - offSets.y; 
            octaveOfSets[i] = new Vector2(offSetsX,offSetsY);

            maxPossibleHeight += amplitud;
            amplitud *= persistance;
        }
        if (scale <= 0){
            scale = 0.0001f;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float xHalf = mapWidth/2;
        float yHalf = mapHeight/2;
        for (int y = 0; y < mapHeight; y ++){
            for (int x = 0; x < mapWidth; x ++){

                amplitud = 1;
                frequency = 1;
                float noiseHeight = 1;

                for (int i = 0; i < octaves; i ++){
                    float sampleX = (x-xHalf + octaveOfSets[i].x) / scale * frequency;
                    float sampleY = (y-yHalf + octaveOfSets[i].y) / scale * frequency;
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;

                    amplitud *= persistance;
                    frequency *= lacunarity;

                    noiseHeight += perlinValue * amplitud;
                }

                if (noiseHeight > maxNoiseHeight){
                    maxNoiseHeight = noiseHeight;

                }else if (noiseHeight < minNoiseHeight){
                    minNoiseHeight = noiseHeight;
                }

                noiseMap[x,y] = noiseHeight;

            }
        }

        for (int y = 0; y < mapHeight; y ++){
            for (int x = 0; x < mapWidth; x ++){
                if (mode == NormalizeMode.local){
                    noiseMap[x,y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x,y]);
                }else{
                    float normalizeHeight = (noiseMap[x,y]+1) / maxPossibleHeight;
                    noiseMap[x,y] = Mathf.Clamp(normalizeHeight, 0, int.MaxValue);
                }
            }
        }

        return noiseMap;
    }

}
