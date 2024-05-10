using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mapDisplay : MonoBehaviour
{
   public Renderer textureRenderer;

   public void drawTexture(Texture2D texture){

    textureRenderer.sharedMaterial.mainTexture = texture;
    textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
   }
    
}
