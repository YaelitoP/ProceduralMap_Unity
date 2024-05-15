using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class EndlessTerrain : MonoBehaviour
{
    Dictionary<Vector2, terrainChunk> terrainChunkDictionary = new Dictionary<Vector2, terrainChunk>();
    List<terrainChunk> lastVisibleChunk = new List<terrainChunk>();
   public const float maxViewDist = 450;
   public Transform viewer;
   public static Vector2 viewerPos;
   int chunkSize;
   int chunkVisibleViewDist;

   void Start (){
    chunkSize = mapGenerator.mapChunkSize -1;
    chunkVisibleViewDist = Mathf.RoundToInt(maxViewDist/chunkSize);

   }
   void Update (){
    viewerPos = new Vector2 (viewer.position.x, viewer.position.z);
    UpdateVisibleChunk();
   }
    void UpdateVisibleChunk() {
        for (int i = 0; i < lastVisibleChunk.Count; i++){
            lastVisibleChunk[i].SetVisible(false);
        } 
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
                    terrainChunkDictionary.Add(viewChunkCoord, new terrainChunk (viewChunkCoord, chunkSize, transform));
                }
            }

        }
    }
    public class terrainChunk {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;
        public terrainChunk(Vector2 coord, int size, Transform parent){

            position = coord * size;

            bounds = new Bounds(position, Vector2.one*size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);


            meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            meshObject.transform.position = positionV3;
            meshObject.transform.localScale = Vector3.one * size / 10f;
            meshObject.transform.parent = parent;
            
            SetVisible(false);
        }
        public void UpdateChunk(){
           float distFromEdge = MathF.Sqrt(bounds.SqrDistance(viewerPos));
           bool visible = distFromEdge <= maxViewDist;

           SetVisible(visible);
        }
        public void SetVisible(bool visible){
            meshObject.SetActive(visible);
        }
        public bool isVisible(){
            return meshObject.activeSelf;
        }

    }
}   
