using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

public static class MeshGenerator
{
    public static meshData generateMesh(float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve, int levelOfDetail){
        AnimationCurve meshCurve = new AnimationCurve(heightCurve.keys);
        int simplification = (levelOfDetail == 0)?1:levelOfDetail * 2;

        int borderSize = heightMap.GetLength (0);
        int meshSize = borderSize - 2 * simplification;
        int meshSizeUnsimplify = borderSize - 2;
        float topLeftX = (meshSizeUnsimplify -1) / -2f;
        float topLeftZ = (meshSizeUnsimplify -1) / 2f;

        
        
        int verticesPerLine = (meshSize-1)/simplification+1;
        int[,] vertexIndexMap = new int[borderSize,borderSize];
        int meshVertexIndex = 0;
        int borderVertexIndex = -1;
        for (int y = 0; y < borderSize; y += simplification){
            for (int x = 0; x < borderSize; x += simplification){
                bool isBorderVertex = y ==0 || y ==borderSize-1 || x ==0 || x == borderSize -1;
                if (isBorderVertex){
                    vertexIndexMap[x,y] = borderVertexIndex;
                    borderVertexIndex--;
                }else{
                    vertexIndexMap[x,y] = meshVertexIndex;
                    meshVertexIndex++;
                }
            }
        }
        meshData meshData = new(verticesPerLine);
        
        for (int y = 0; y < borderSize; y += simplification){
            for (int x = 0; x < borderSize; x += simplification){
                int vertexIndex = vertexIndexMap[x,y];
               
                Vector2 percent = new Vector2((x-simplification)/(float)meshSize, (y-simplification)/(float)meshSize);  
                float height = meshCurve.Evaluate(heightMap[x,y]) * heightMultiplier;
                Vector3 vertexPos = new Vector3(topLeftX + percent.x * meshSizeUnsimplify, height, topLeftZ - percent.y * meshSizeUnsimplify);
                meshData.addVertex(vertexPos, percent, vertexIndex);           
                
                if (x < borderSize-1 && y < borderSize-1){
                    int a = vertexIndexMap[x,y];
                    int b = vertexIndexMap[x + simplification,y];
                    int c = vertexIndexMap[x,y + simplification];
                    int d = vertexIndexMap[x + simplification,y + simplification];
                    meshData.AddTriangle (a,d,c);
                    meshData.AddTriangle (d,a,b); 
                } 
                vertexIndex++;
            }
        }
        return meshData;

    }
}

public class meshData{
    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;
    Vector3[] borderVertices;
    int[] borderTriangles;

    int triangleIndex;
    int borderTriangleIndex;

    public meshData(int verticesPerLine){
        vertices = new Vector3[verticesPerLine * verticesPerLine];
        uvs = new Vector2[verticesPerLine * verticesPerLine];
        triangles = new int[(verticesPerLine -1) * (verticesPerLine -1)*6];
        borderVertices = new Vector3[(verticesPerLine * 4 + 4)];
        borderTriangles = new int[(24 * verticesPerLine)];
    }
    public void addVertex(Vector3 vertexPosition, Vector2 uv, int vertexIndex){
        if (vertexIndex < 0){
            borderVertices[-vertexIndex-1] = vertexPosition;
        }else{
            vertices[vertexIndex] = vertexPosition;
            uvs [vertexIndex] = uv;
        }
    }

    public void AddTriangle(int a, int b, int c){
        if (a < 0 || b < 0 || c < 0){
            borderTriangles [borderTriangleIndex] = a;
            borderTriangles [borderTriangleIndex+1] = b;
            borderTriangles [borderTriangleIndex+2] = c;
            borderTriangleIndex += 3;
        }else{
            triangles [triangleIndex] = a;
            triangles [triangleIndex+1] = b;
            triangles [triangleIndex+2] = c;
            triangleIndex += 3;
        }
    }

    Vector3[] calculateNormals(){
        Vector3[] vertexNormal = new Vector3[vertices.Length];
        int triangleCount = triangles.Length / 3;
        for (int i = 0; i < triangleCount; i++){
            int normalTriangleIndex = i * 3;
            int vertexIndexA = triangles [normalTriangleIndex];
            int vertexIndexB = triangles [normalTriangleIndex + 1];
            int vertexIndexC = triangles [normalTriangleIndex + 2];
            Vector3 triangleNormal = surfaceNormals(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNormal [vertexIndexA] += triangleNormal;
            vertexNormal [vertexIndexB] += triangleNormal;
            vertexNormal [vertexIndexC] += triangleNormal;
        }
        int borderTriangleCount = borderTriangles.Length / 3;
        for (int i = 0; i < borderTriangleCount; i++){
            int normalTriangleIndex = i * 3;
            int vertexIndexA = borderTriangles [normalTriangleIndex];
            int vertexIndexB = borderTriangles [normalTriangleIndex + 1];
            int vertexIndexC = borderTriangles [normalTriangleIndex + 2];
            Vector3 triangleNormal = surfaceNormals(vertexIndexA, vertexIndexB, vertexIndexC);
            if (vertexIndexA >= 0){
                vertexNormal [vertexIndexA] += triangleNormal;
            }
            if (vertexIndexB >= 0){
                vertexNormal [vertexIndexB] += triangleNormal;
            }
            if (vertexIndexC >= 0){
                vertexNormal [vertexIndexC] += triangleNormal;
            }
        }
        for (int i = 0; i < vertexNormal.Length; i++){
            vertexNormal[i].Normalize();
        }
        return vertexNormal;
    }

    Vector3 surfaceNormals(int indexA, int indexB, int indexC){
        Vector3 pointA = (indexA<0)?borderVertices[-indexA-1] : vertices [indexA];
        Vector3 pointB = (indexB<0)?borderVertices[-indexB-1] : vertices [indexB];
        Vector3 pointC = (indexC<0)?borderVertices[-indexC-1] : vertices [indexC];

        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;

        return Vector3.Cross (sideAB, sideAC).normalized;
    }
    

    public Mesh CreateMesh(){
        Mesh mesh = new()
        {
            vertices = vertices,
            triangles = triangles,
            uv = uvs
        };
        mesh.normals  = calculateNormals();
        return mesh;
    }
}   