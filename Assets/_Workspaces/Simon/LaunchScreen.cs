using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LaunchScreen : MonoBehaviour {

    public Texture2D jlrLogo;
    public Texture2D elementsLogo;

    public float fadeTime = 2f;
    public float showTime = 4f;

    public GUIStyle largeTextStyle;
    public GUIStyle smallTextStyle;

    public Rect largeTextRect;
    public Rect smallTextRect;
    public Rect jlrRect;
    public Rect elementsRect;

    private float mainTextAlpha = 0f;
    private float createdByAlpha = 0f;
    private float presentedByAlpha = 0f;
    private float elementsAlpha = 0f;
    private float jlrAlpha = 0f;
    
    public IEnumerator StartSequence(System.Action OnFinished) {
        yield return StartCoroutine(Fade (f => mainTextAlpha = f, mainTextAlpha, 1f, fadeTime));
        yield return new WaitForSeconds (showTime);
        yield return StartCoroutine (Fade (f => presentedByAlpha = f, presentedByAlpha, 1f, fadeTime));
        yield return new WaitForSeconds (showTime/2f);

        yield return StartCoroutine (Fade (f =>  jlrAlpha = f, jlrAlpha, 1f, fadeTime));
        yield return new WaitForSeconds (showTime);

        StartCoroutine (Fade (f => jlrAlpha = f, jlrAlpha, 0f, fadeTime));
        yield return StartCoroutine (Fade (f => presentedByAlpha = f, presentedByAlpha, 0f, fadeTime));
        yield return StartCoroutine (Fade (f => createdByAlpha = f, createdByAlpha, 1f, fadeTime));
        yield return new WaitForSeconds (showTime/2f);
        yield return StartCoroutine (Fade (f => elementsAlpha = f, elementsAlpha, 1f, fadeTime));
        yield return new WaitForSeconds (showTime);

        StartCoroutine(Fade(f => elementsAlpha = f, elementsAlpha, 0f, fadeTime));
        StartCoroutine(Fade(f => createdByAlpha = f, createdByAlpha, 0f, fadeTime));
        yield return StartCoroutine(Fade(f => mainTextAlpha = f, mainTextAlpha, 0f, fadeTime));
        yield return new WaitForSeconds (showTime);

        //Application.LoadLevel("CarSelectLevel");
        OnFinished();
    }

    public void OnGUI() {

        //set up GUI matrix here please
        Matrix4x4 oldMatrix = GUI.matrix;

        if (AdminSettings.Instance.displayType == AdminScreen.DisplayType.PARABOLIC)
        {
            GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width / 4992f, Screen.height / 1080f, 1f));
            GUI.BeginGroup(new Rect(1536, 0, 1920, 1080));
        }
        else
        {
            GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width / 5760f, Screen.height / 1080f, 1f));
            GUI.BeginGroup(new Rect(1920, 0, 1920, 1080));
        }

        GUI.color = new Color (1f, 1f, 1f, mainTextAlpha);
        GUI.Label (largeTextRect, "OSTC DRIVING SIMULATOR", largeTextStyle);

        GUI.color = new Color (1f, 1f, 1f, presentedByAlpha);
        GUI.Label (smallTextRect, "presented by", smallTextStyle);

        GUI.color = new Color (1f, 1f, 1f, createdByAlpha);
        GUI.Label (smallTextRect, "created by", smallTextStyle);

        GUI.color = new Color (1f, 1f, 1f, elementsAlpha);
        GUI.DrawTexture (elementsRect, elementsLogo);

        GUI.color = new Color (1f, 1f, 1f, jlrAlpha);
        GUI.DrawTexture (jlrRect, jlrLogo);

        GUI.color = Color.white;

        GUI.EndGroup();

        GUI.matrix = oldMatrix;

    }


    private IEnumerator Fade(System.Action<float> onUpdate, float alpha, float target, float fadeTime) {
        float startVal = alpha;
        float startTime = Time.time;

        while (Time.time - startTime < fadeTime) {
            onUpdate(Mathf.Lerp(startVal, target, (Time.time - startTime)/fadeTime));
            yield return null;
        }

        onUpdate(target);
    }
}
