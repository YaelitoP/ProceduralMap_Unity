using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mapDisplay : MonoBehaviour
{
   public Renderer textureRenderer;
   public MeshFilter meshFilter;
   public MeshRenderer meshRender;
   public void drawTexture(Texture2D texture){

    textureRenderer.sharedMaterial.mainTexture = texture;
    textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
   }
   public void drawMesh(MeshData meshData, Texture2D texture){
      meshRender.sharedMaterial.mainTexture = texture;
      meshFilter.sharedMesh = meshData.CreateMesh();
   }
}
