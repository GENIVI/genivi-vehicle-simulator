using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SurroundTest : MonoBehaviour {

    public DirectPlayer player;
    public AudioClip[] monoClips;

    List<float[]> clipData;
 
    void Awake ()
    {
             BufferClipData ();
    }
 
    public void PlayClipInChannel (int clip, int channel )
    {
          player.PlayData (clipData[clip], channel );
    }
    void BufferClipData ()
    {
        clipData = new List<float[]>();
        for (int i = 0; i < monoClips.Length; i++ )
        {
            AudioClip monoClip = monoClips[i];
            var data = new float[monoClip.samples];
            monoClip.GetData(data, 0);
            clipData.Add(data);
        }
    }

    void OnGUI()
    {
        for (int i = 0; i < clipData.Count; i++)
        {
            if (GUI.Button(new Rect(0, i * 50, 100, 45), (i+1).ToString()))
            {
                PlayClipInChannel(i, i);
            }
        }

    }

}
