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
        GameObject BasicEnemy,
        GameObject SwarmEnemy
        )
    {
        Objects.Add("BasicEnemy", BasicEnemy);
        Objects.Add("SwarmEnemy", SwarmEnemy);
    }


    public static void CreateMapObjects(LevelDescription Desc, GameObject RoadTemplate, GameObject TerrainTemplate, GameObject TurretLocationTemplate)
    {
        MidPointWidth = Desc.FieldWidth / 2.0f;
        MidPointDepth = Desc.FieldDepth / 2.0f;

        MapPos pos = new MapPos(0,0);
        for (int i = 0; i < Desc.Map.Count; i++)
        {
            pos.Pos.x = i % Desc.FieldWidth;
            pos.Pos.z = i / Desc.FieldWidth;
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

            GameObject ro = Instantiate(template);
            SetPos(ro, pos);
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

    public static void SetPos(GameObject go, MapPos pos)
    {
        SetPos(go, pos.Pos);
    }
    public static void SetPos(GameObject go, Vector3 pos)
    {
        go.transform.SetPositionAndRotation(MapPosToVec3(pos), Quaternion.identity);
    }


    static Vector3 MapPosToVec3(Vector3 pos)
    {
        return new Vector3(pos.x - MidPointWidth, 0.0f, MidPointDepth - pos.z);
    }
}