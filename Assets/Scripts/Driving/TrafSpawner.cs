/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;

public interface ITrafficSpawner
{
    void SetTraffic(bool state);
    bool GetState();
}

public class TrafSpawner : MonoBehaviour, ITrafficSpawner {

    public TrafSystem system;

    public GameObject[] prefabs;
    public GameObject[] fixedPrefabs;

    public int numberToSpawn = 50;
    public int numberFixedToSpawn = 5;

    public int lowDensity = 120;
    public int mediumDensity = 250;
    public int heavyDensity = 500;
    public int maxIdent = 20;
    public int maxSub = 4;
    public float checkRadius = 8f;

    private int[] bridgeIds = new int[] { 168, 168, 170 };
    private const int numberToSpawnOnBridge = 30;

    public void SpawnHeaps()
    {
        system.ResetIntersections();

        for (int i = 0; i < numberFixedToSpawn; i++)
        {
            SpawnFixed();
        }

        for (int i = 0; i < numberToSpawn; i++)
        {
            Spawn();
        }

        for (int i = 0; i < numberToSpawnOnBridge; i++)
        {
            Spawn(bridgeIds[Random.Range(0, bridgeIds.Length)], Random.Range(0, 3));
        }
    }

    public void SpawnFixed()
    {
        GameObject prefab = fixedPrefabs[Random.Range(0, fixedPrefabs.Length)];
        var pMotor = prefab.GetComponent<TrafAIMotor>();
        int index = Random.Range(0, pMotor.fixedPath.Count);
        int id = pMotor.fixedPath[index].id;
        int subId = pMotor.fixedPath[index].subId;
        float distance = Random.value * 0.8f + 0.1f;
        TrafEntry entry = system.GetEntry(id, subId);
        if (entry == null)
            return;
        InterpolatedPosition pos = entry.GetInterpolatedPosition(distance);

        if (!Physics.CheckSphere(pos.position, checkRadius * 3, 1 << LayerMask.NameToLayer("Traffic")))
        {
            GameObject go = GameObject.Instantiate(prefab, pos.position, Quaternion.identity) as GameObject;
            TrafAIMotor motor = go.GetComponent<TrafAIMotor>();

            motor.currentIndex = pos.targetIndex;
            motor.currentEntry = entry;
            go.transform.LookAt(entry.waypoints[pos.targetIndex]);
            motor.system = system;
            motor.Init();

            motor.currentFixedNode = index;

        }
    }


    public void Spawn()
    {
        int id = Random.Range(0, maxIdent);
        int subId = Random.Range(0, maxSub);
        Spawn(id, subId);
    }

    public void Spawn(int id, int subId)
    {
        float distance = Random.value * 0.8f + 0.1f;
        TrafEntry entry = system.GetEntry(id, subId);
        if (entry == null)
            return;
        InterpolatedPosition pos = entry.GetInterpolatedPosition(distance);

        if (!Physics.CheckSphere(pos.position, checkRadius, 1 << LayerMask.NameToLayer("Traffic")))
        {
            GameObject go = GameObject.Instantiate(prefabs[Random.Range(0, prefabs.Length)], pos.position, Quaternion.identity) as GameObject;
            TrafAIMotor motor = go.GetComponent<TrafAIMotor>();

            motor.currentIndex = pos.targetIndex;
            motor.currentEntry = entry;
            go.transform.LookAt(entry.waypoints[pos.targetIndex]);
            motor.system = system;
            motor.Init();
        }
    }

    public void Kill()
    {
        var allTraffic = Object.FindObjectsOfType(typeof(TrafAIMotor)) as TrafAIMotor[];
        foreach(var t in allTraffic)
        {
            GameObject.Destroy(t.gameObject);
        }
        
    }

    bool spawned = false;

    public bool GetState()
    {
        return spawned;
    }

    public void SetTraffic(bool state)
    {
        if(spawned && !state)
        {
            Kill();
            spawned = false;
        }
        else if(!spawned && state)
        {
            SpawnHeaps();
            spawned = true;
        }
    }

    void OnGUI()
    {
        if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.U)
        {
            numberToSpawn = mediumDensity;
            if(spawned)
                Kill();
            else
                SpawnHeaps();

            spawned = !spawned;
        }

        else if(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.H) {
            if (spawned)
                Kill();

            numberToSpawn = heavyDensity;
            SpawnHeaps();
            spawned = true;                
        }

        else if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.L)
        {
            if (spawned)
                Kill();

            numberToSpawn = lowDensity;
            SpawnHeaps();
            spawned = true;
        }
    }
    
}
