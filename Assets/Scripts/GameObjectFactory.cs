using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameObjectFactory : MonoBehaviour
{
    static Dictionary<string, GameObject> Objects = new Dictionary<string, GameObject>();
    static float MidPointWidth;
    static float MidPointDepth;

    public static void InitializeObjects(
        LevelDescription Desc,
        GameObject BasicEnemy,
        GameObject SwarmEnemy
        )
    {
        MidPointWidth = Desc.FieldWidth / 2.0f;
        MidPointDepth = Desc.FieldDepth / 2.0f;

        Objects.Add("BasicEnemy", BasicEnemy);
        Objects.Add("SwarmEnemy", SwarmEnemy);
    }


    public static void CreateMapObjects(LevelDescription Desc, GameObject RoadTemplate, GameObject TerrainTemplate, GameObject TurretLocationTemplate)
    {
        for (int i = 0; i < Desc.Map.Count; i++)
        {
            int x = i % Desc.FieldWidth;
            int z = i / Desc.FieldWidth;
            GameObject template = null;
            switch (Desc.Map[i])
            {
                case 'E':
                    template = RoadTemplate;
                    break;
                case 'X':
                    template = RoadTemplate;
                    break;
                case 'R':
                    template = RoadTemplate;
                    break;
                case 'T':
                    template = TurretLocationTemplate;
                    break;
                case 'D':
                    template = TerrainTemplate;
                    break;
            }
            Debug.Assert(null != template);

            GameObject ro = Instantiate(template, new Vector3(MidPointWidth - x, 0.0f, MidPointDepth - z), Quaternion.identity);
        }
    }

    public static GameObject InstantiateObject(string AssetName)
    {
        GameObject template = Objects[AssetName];

        return Instantiate(template);
    }

    public static void DestroyObject(GameObject go)
    {
        Destroy(go);
    }

    public static void SetPosMapRelative(GameObject go, MapPos pos)
    {
        Vector3 posAsVec3 = new Vector3(MidPointWidth - pos.x, 0.0f, MidPointDepth - pos.y);
        go.transform.SetPositionAndRotation(posAsVec3, Quaternion.identity);
    }
}