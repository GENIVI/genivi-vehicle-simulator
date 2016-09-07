using UnityEngine;
using System.Collections;

public class DITesy : MonoBehaviour {
    // Use this for initialization

    public int sat;
    public int coeff;
    public int constant;
    public int damper;

    void Start()
    {
        Debug.Log("init: " + DirectInputWrapper.Init());
        Debug.Log("Count: " + DirectInputWrapper.DevicesCount());

        for (int i = 0; i < DirectInputWrapper.DevicesCount(); i++)
        {
            Debug.Log("Name: " + DirectInputWrapper.GetProductNameManaged(i));
            Debug.Log("FFB: " + DirectInputWrapper.HasForceFeedback(i));
            
            for(int j = 0; j < DirectInputWrapper.GetNumEffects(i); j++)
            {
                Debug.Log("EFFECT: " + DirectInputWrapper.GetEffectNameManaged(i, j));
            }
        }
    }

    void OnDestroy()
    {
        DirectInputWrapper.Close();
    }


    bool playing = false;
    // Update is called once per frame
    void Update()
    {
        DirectInputWrapper.Update();
        DeviceState state = DirectInputWrapper.GetStateManaged(0);

        if (Input.GetKeyDown(KeyCode.P)) {
            //Debug.Log("PLAY: " + DirectInputWrapper.PlaySpringForce(0, 0, sat, coeff));
            //Debug.Log("PLAY: " + DirectInputWrapper.PlayConstantForce(0, constant));
            Debug.Log("PLAY: " + DirectInputWrapper.PlayDamperForce(0, damper));
            playing = true;
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            //Debug.Log("STOP:" + DirectInputWrapper.StopSpringForce(0));
            //Debug.Log("STOP:" + DirectInputWrapper.StopConstantForce(0));
            Debug.Log("STOP:" + DirectInputWrapper.StopDamperForce(0));
            playing = false;
        }
        
        if(playing)
        {
         //   DirectInputWrapper.UpdateConstantForce(0, constant);
        }

    }
}
