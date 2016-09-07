using UnityEngine;
using System.Collections;

public class TestSS : MonoBehaviour {

    // Use this for initialization
    IEnumerator Start () {
        yield return new WaitForEndOfFrame();

        var rt = new RenderTexture(1248, 226, 0,RenderTextureFormat.ARGB32);
        rt.Create();
        Camera.main.targetTexture = rt;
        Camera.main.Render();
        Camera.main.targetTexture = null;

        yield return null;
        yield return new WaitForEndOfFrame();

        Texture2D screenshot = new Texture2D(1203, 226, TextureFormat.RGB24, false);
        RenderTexture.active = rt;
        screenshot.ReadPixels(new Rect(0, 0, 1203, 226), 0, 0, false);
        RenderTexture.active = null;

        yield return null;

        // screenshot.ReadPixels(new Rect(Mathf.RoundToInt((Screen.width - width) / 2), 0, width, Screen.height), 0, 0, false);
        screenshot.Apply();
        // Graphics.Blit(Camera.main.targetTexture, screenshotTarget);

        var bytes = screenshot.EncodeToJPG();
        System.IO.File.WriteAllBytes(Application.dataPath + "test.jpg", bytes);

        rt.Release();
    }
    


    // Update is called once per frame
    void Update () {
    
    }
}
