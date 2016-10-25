using UnityEngine;
using System.Collections;

public class DriveMap : MonoBehaviour {

    public RectTransform mapIndicator;
    public bool mirrorMap = false; //because PCH is weird
    public Vector3 tlMap;
    public Vector3 brMap;
	
	void Update () {
        //update map
        if (gameObject.activeSelf)
        {
            /*
            Vector3 tlMap = Vector3.zero;
            Vector3 brMap = Vector3.one;
            var environment = AppController.Instance.currentSessionSettings.selectedEnvironment;
            switch (environment)
            {
                case Environment.COASTAL:
                    tlMap = new Vector3(1481f, 0f, 4559f);
                    brMap = new Vector3(-2354f, 0f, -3331f);
                    break;
                case Environment.SCENIC:
                    tlMap = new Vector3(-6915f, 0f, 2589f);
                    brMap = new Vector3(5120f, 0f, -3249f);
                    break;
                case Environment.URBAN:
                    tlMap = new Vector3(90.5f, 0f, 2330f);
                    brMap = new Vector3(6327.5f, 0f, -700f);
                    break;

            }
            */

            Vector3 currentPosition = TrackController.Instance.car.transform.position;
            Vector3 currentRotation = TrackController.Instance.car.transform.rotation.eulerAngles - Vector3.up * 90f;

            float xPc = (currentPosition.x - tlMap.x) / (brMap.x - tlMap.x);
            float zPc = (currentPosition.z - brMap.z) / (tlMap.z - brMap.z);

            //PCH is weird and flipped
            if (mirrorMap)
            {
                zPc = (currentPosition.x - brMap.x) / (tlMap.x - brMap.x);
                xPc = 1 - (currentPosition.z - brMap.z) / (tlMap.z - brMap.z);
                currentRotation.y -= 90f;
            }

            mapIndicator.anchoredPosition = new Vector2((xPc * 1440), zPc * 700);
            mapIndicator.localRotation = Quaternion.Euler(0f, 0f, -currentRotation.y);

            //store for remote!
            TrackController.Instance.mapXPos = xPc;
            TrackController.Instance.mapZPos = zPc;

        }
    }
}
