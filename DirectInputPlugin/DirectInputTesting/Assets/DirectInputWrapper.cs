using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;

[StructLayout(LayoutKind.Sequential, Pack = 2)]
public struct DeviceState
{
    public int lX;              /* x-axis position              */
    public int lY;              /* y-axis position              */
    public int lZ;              /* z-axis position              */
    public int lRx;             /* x-axis rotation              */
    public int lRy;             /* y-axis rotation              */
    public int lRz;             /* z-axis rotation              */
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public int[] rglSlider;     /* extra axes positions         */
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public uint[] rgdwPOV;      /* POV directions               */
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
    public byte[] rgbButtons;   /* 128 buttons                  */         
  
};

public class DirectInputWrapper {

    [DllImport("DirectInputPlugin", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static extern long Init();

    [DllImport("DirectInputPlugin", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static extern int DevicesCount();

    [DllImport("DirectInputPlugin", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr GetProductName(int device);

    [DllImport("DirectInputPlugin", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool HasForceFeedback(int device);

    [DllImport("DirectInputPlugin", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static extern void Close();

    [DllImport("DirectInputPlugin", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static extern void Update();

    [DllImport("DirectInputPlugin", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr GetState(int device);

    [DllImport("DirectInputPlugin", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static extern int GetNumEffects(int device);

    [DllImport("DirectInputPlugin", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr GetEffectName(int device, int index);

    //params in range 0 - 1000
    [DllImport("DirectInputPlugin", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static extern long PlaySpringForce(int device, int offset, int saturation, int coefficient);

    [DllImport("DirectInputPlugin", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool StopSpringForce(int device);

    [DllImport("DirectInputPlugin", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static extern long PlayDamperForce(int device, int damperAmount);

    [DllImport("DirectInputPlugin", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool StopDamperForce(int device);

    [DllImport("DirectInputPlugin", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static extern long PlayConstantForce(int device, int force);

    [DllImport("DirectInputPlugin", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static extern long UpdateConstantForce(int device, int force);

    [DllImport("DirectInputPlugin", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool StopConstantForce(int device);



    public static DeviceState GetStateManaged(int device)
    {
        DeviceState ret = new DeviceState();
        ret.rglSlider = new int[2];
        ret.rgdwPOV = new uint[4];
        ret.rgbButtons = new byte[128];

        var ptr = GetState(device);
        if(ptr != IntPtr.Zero)
        {
            try
            {
                ret = (DeviceState)Marshal.PtrToStructure(ptr, typeof(DeviceState));
            } catch(Exception e)
            {
                //TODO: do something better here
               // Debug.Log(e.ToString());
            }
        }

        return ret;
    }

    public static string GetProductNameManaged(int device)
    {
        var pName = DirectInputWrapper.GetProductName(device);
        return Marshal.PtrToStringUni(pName);
    }

    public static string GetEffectNameManaged(int device, int index)
    {
        var pName = DirectInputWrapper.GetEffectName(device, index);
        return Marshal.PtrToStringUni(pName);
    }

}
