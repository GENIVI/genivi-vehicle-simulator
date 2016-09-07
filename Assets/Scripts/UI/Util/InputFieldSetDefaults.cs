/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;
using UnityEngine.Events;


public class InputFieldSetDefaults : MonoBehaviour {

    public string eventName;
    public NewAdminScreen target;
    public bool everyFrame = false;

    private System.Reflection.MethodInfo minfo;

    public void OnResetDefaults()
    {
        if (GetComponent<UnityEngine.UI.InputField>() != null)
        {
            var onCheckDefaults = GetComponent<UnityEngine.UI.InputField>().onValueChanged;
            onCheckDefaults.GetPersistentMethodName(0);
            var mName = onCheckDefaults.GetPersistentMethodName(0).Replace("Set", "Get");
            minfo = UnityEventBase.GetValidMethodInfo(onCheckDefaults.GetPersistentTarget(0), mName, new System.Type[0]);
            target = (NewAdminScreen)onCheckDefaults.GetPersistentTarget(0);
            if (minfo == null)
                Debug.LogError("No Matching Field Getter for object " + gameObject.transform.parent.name);
            string val = (string)minfo.Invoke(target, null);
            GetComponent<UnityEngine.UI.InputField>().text = val;
        }
        else if(GetComponent<UnityEngine.UI.Toggle>() != null)
        {
            var onCheckDefaults = GetComponent<UnityEngine.UI.Toggle>().onValueChanged;
            onCheckDefaults.GetPersistentMethodName(0);
            var mName = onCheckDefaults.GetPersistentMethodName(0).Replace("Set", "Get").Replace("Toggle", "Get");
            target = (NewAdminScreen)onCheckDefaults.GetPersistentTarget(0);
            minfo = UnityEventBase.GetValidMethodInfo(onCheckDefaults.GetPersistentTarget(0), mName, new System.Type[0]);
            bool val = (bool)minfo.Invoke(onCheckDefaults.GetPersistentTarget(0), null);
            GetComponent<UnityEngine.UI.Toggle>().isOn = val;

        }
        else if(!string.IsNullOrEmpty(eventName))
        {
            minfo = UnityEventBase.GetValidMethodInfo(target, eventName, new System.Type[0]);
            if (minfo == null)
                Debug.LogError("No Matching Field Getter for object " + gameObject.transform.parent.name);
            string val = (string)minfo.Invoke(target, null);
            GetComponent<UnityEngine.UI.Text>().text = val;
        } 
    }

    void Update()
    {
        if(everyFrame)
        {
            if (GetComponent<UnityEngine.UI.InputField>() != null)
            {
                string val = (string)minfo.Invoke(target, null);
                GetComponent<UnityEngine.UI.InputField>().text = val;
            }
            else if (GetComponent<UnityEngine.UI.Toggle>() != null)
            {
                bool val = (bool)minfo.Invoke(target, null);
                GetComponent<UnityEngine.UI.Toggle>().isOn = val;
            }
            else if (!string.IsNullOrEmpty(eventName))
            {
                string val = (string)minfo.Invoke(target, null);
                GetComponent<UnityEngine.UI.Text>().text = val;
            }
        }
    }
}
