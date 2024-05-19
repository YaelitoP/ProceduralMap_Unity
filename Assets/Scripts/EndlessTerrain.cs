using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.AssetImporters;
using UnityEngine;
using UnityEngine.UIElements;

public class EndlessTerrain : MonoBehaviour
{
    Dictionary<Vector2, terrainChunk> terrainChunkDictionary = new Dictionary<Vector2, terrainChunk>();
    List<terrainChunk> lastVisibleChunk = new List<terrainChunk>();

    public Material mapMaterial;
    public LODinfo[] detailLevel;
    public Transform viewer;
    public static Vector2 viewerPos;
    static mapGenerator mapGenerator;
    Vector2 viewerPosOld;

    const float updateThreshold = 25f;
    const float sqrUpdateThreshold = updateThreshold * updateThreshold;

    int chunkSize;
    int chunkVisibleViewDist;
    public static float maxViewDist;

    void Start (){
        mapGenerator = FindObjectOfType<mapGenerator>();
        maxViewDist = detailLevel[detailLevel.Length - 1].distThreshold;
        chunkSize = mapGenerator.mapChunkSize -1;
        chunkVisibleViewDist = Mathf.RoundToInt(maxViewDist/chunkSize);
        UpdateVisibleChunk();
    }

    void Update (){
        viewerPos = new Vector2 (viewer.position.x, viewer.position.z);
        if ((viewerPosOld-viewerPos).sqrMagnitude > sqrUpdateThreshold){
            viewerPosOld = viewerPos;  
            UpdateVisibleChunk();
        }
    }   




    void UpdateVisibleChunk() {
        for (int i = 0; i < lastVisibleChunk.Count; i++){
            lastVisibleChunk[i].SetVisible(false);} 
        lastVisibleChunk.Clear();

        int currentChunkX = Mathf.RoundToInt(viewerPos.x / chunkSize);
        int currentChunkY = Mathf.RoundToInt(viewerPos.y / chunkSize);

        for (int yOffset = -chunkVisibleViewDist; yOffset <= chunkVisibleViewDist; yOffset ++){
            for (int xOffset = -chunkVisibleViewDist; xOffset <= chunkVisibleViewDist; xOffset ++){
                
                Vector2 viewChunkCoord = new Vector2(currentChunkX + xOffset, currentChunkY + yOffset);
                if (terrainChunkDictionary.ContainsKey (viewChunkCoord)){
                    terrainChunkDictionary[viewChunkCoord].UpdateChunk();
                    if (terrainChunkDictionary[viewChunkCoord].isVisible()){
                        lastVisibleChunk.Add (terrainChunkDictionary[viewChunkCoord]);
                    }
                } else{
                    terrainChunkDictionary.Add(viewChunkCoord, new terrainChunk (viewChunkCoord, chunkSize, detailLevel, transform, mapMaterial));
                }
            }
        }
    }




    public class terrainChunk {
        
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;
        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        LODinfo[] detailLevel;
        LODmesh[] LODmeshes;
        mapData mapData;

        bool hasMapData;
        int prevLODIndex = -1;

        public terrainChunk(Vector2 coord, int size, LODinfo[] detailLevel, Transform parent, Material material){
            this.detailLevel = detailLevel;
            position = coord * size;

            bounds = new Bounds(position, Vector2.one*size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject("terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();

            meshRenderer.material = material;
            meshObject.transform.position = positionV3;
            meshObject.transform.parent = parent;
            SetVisible(false);

            LODmeshes = new LODmesh[detailLevel.Length];

            for (int i = 0; i < detailLevel.Length; i++){
                LODmeshes[i] = new LODmesh(detailLevel[i].lod, UpdateChunk);
            }
            mapGenerator.requestMapData(position, onMapDataReceived);
        }

        void onMapDataReceived(mapData mapData){
           this.mapData = mapData;
           hasMapData = true;
           Texture2D texture = textureCreation.TextureFromColourMap(mapData.colourMap, mapGenerator.mapChunkSize, mapGenerator.mapChunkSize);
           meshRenderer.material.mainTexture = texture;
           UpdateChunk();
        }

        public void UpdateChunk(){
            if (hasMapData){
                float distFromEdge = MathF.Sqrt(bounds.SqrDistance(viewerPos));
                bool visible = distFromEdge <= maxViewDist;
                if (visible){
                    int lodIndex = 0;
                    for (int i = 0; i < detailLevel.Length -1; i++){
                        if (distFromEdge > detailLevel[i].distThreshold){
                            lodIndex = i + 1;
                        }else{
                            break;
                        }
                    }
                    if (lodIndex != prevLODIndex){
                        LODmesh lodMesh = LODmeshes[lodIndex];
                        if (lodMesh.hasMesh){
                            prevLODIndex = lodIndex;
                            meshFilter.mesh = lodMesh.mesh;
                        }else if (!lodMesh.hasRequestedMesh){
                            lodMesh.requestMesh(mapData);
                        }   
                    }
                }
            
             SetVisible(visible);    
            }
        }

        public void SetVisible(bool visible){
            meshObject.SetActive(visible);
        }

        public bool isVisible(){
            return meshObject.activeSelf;
        }
    }



    class LODmesh{
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        int lod;
        System.Action updateCallback;

        public LODmesh(int lod, System.Action updateCallback){
            this.lod = lod;
            this.updateCallback = updateCallback;
        }

        void onMeshReceived(meshData meshData){
            mesh = meshData.CreateMesh();
            hasMesh = true;
            updateCallback();
        }

        public void requestMesh(mapData mapData){
            hasRequestedMesh = true;
            mapGenerator.requestMeshData(mapData, lod, onMeshReceived);
        }
       
    }



    [System.Serializable]
    public struct LODinfo{
        public int lod;
        public float distThreshold;
    }

}   
