using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class noise
{
    public static float[,] generateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offSets){
        
        float[,] noiseMap = new float[mapWidth, mapHeight];
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOfSets =  new Vector2[octaves];

        for (int i = 0; i < octaves; i ++){
            float offSetsX = prng.Next(-10000,10000) + offSets.x;
            float offSetsY = prng.Next(-10000,10000) + offSets.y; 
            octaveOfSets[i] = new Vector2(offSetsX,offSetsY);
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

                float amplitud = 1;
                float frequency = 1;
                float noiseHeight = 1;

                for (int i = 0; i < octaves; i ++){
                    float sampleX = (x-xHalf) / scale * frequency + octaveOfSets[i].x;
                    float sampleY = (y-yHalf) / scale * frequency + octaveOfSets[i].y;
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
                noiseMap[x,y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x,y]);
                
            }
        }

        return noiseMap;
    }

}
