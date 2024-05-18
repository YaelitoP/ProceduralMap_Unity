using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

public static class MeshGenerator
{
    public static meshData generateMesh(float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve, int levelOfDetail){
        AnimationCurve meshCurve = new AnimationCurve(heightCurve.keys);
        int width = heightMap.GetLength (0);
        int height = heightMap.GetLength (1);
        float topLeftX = (width -1) / -2f;
        float topLeftZ = (height -1) / 2f;

        
        int simplification = (levelOfDetail == 0)?1:levelOfDetail * 2;
        int verticesPerLine = (width-1)/simplification+1;
        int vertexIndex = 0;
        meshData meshData = new(verticesPerLine, verticesPerLine);
        
        for (int y = 0; y < height; y += simplification){
            for (int x = 0; x < width; x += simplification){
                meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, meshCurve.Evaluate(heightMap[x,y]) * heightMultiplier, topLeftZ - y);
                meshData.uvs[vertexIndex] = new Vector2(x/(float)width, y/(float)height);               
                
                if (x < width-1 && y < height-1){
                    meshData.AddTriangle (vertexIndex, vertexIndex+verticesPerLine+1, vertexIndex+verticesPerLine);
                    meshData.AddTriangle (vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1); 
                } 
                vertexIndex++;
            }
        }
        return meshData;

    }
}

public class meshData{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;
    int triangleIndex;
    public meshData(int meshWidth, int meshHeight){
        vertices = new Vector3[meshWidth * meshHeight];
        uvs = new Vector2[meshWidth * meshHeight];
        triangles = new int[(meshWidth -1) * (meshHeight -1)*6];
    }


    public void AddTriangle(int a, int b, int c){
        triangles [triangleIndex] = a;
        triangles [triangleIndex+1] = b;
        triangles [triangleIndex+2] = c;
        triangleIndex += 3;
    }
    public Mesh CreateMesh(){
        Mesh mesh = new()
        {
            vertices = vertices,
            triangles = triangles,
            uv = uvs
        };
        mesh.RecalculateNormals();
        return mesh;
    }
}   