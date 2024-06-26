using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class fallOff 
{
    public static float[,] generateFallOff(int size){
        float[,] map = new float[size,size];
        for (int i = 0; i < size; i++){
            for (int j = 0; j < size; j++){
                float x = i / (float)size * 2-1;
                float y = j / (float)size * 2-1;
                float value = Mathf.Max (MathF.Abs (x), Mathf.Abs (y));
                map[i,j] = evaluate(value);
            }
        }
        return map;
    }
    static float evaluate(float value){
        float a = 3;
        float b = 5f;
        return Mathf.Pow(value, a) / (Mathf.Pow(value,a) + Mathf.Pow(b-b*value,a));
    }
}
