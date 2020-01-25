using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using VRTD.Gameplay;

public class GameObjectFactory : MonoBehaviour
{
    static float MidPointWidth;
    static float MidPointDepth;
    static float TranslationMultiplier = 2.0F;
    static List<GameObject> MapObjects = null;


    public static void Initialize(LevelDescription Desc)
    {
        MidPointWidth = Desc.FieldWidth / 2.0f;
        MidPointDepth = Desc.FieldDepth / 2.0f;
        MapObjects = new List<GameObject>();
    }

    public static void Cleanup()
    {
        if (null != MapObjects)
        {
            for (int i = 0; i < MapObjects.Count; i++)
            {
                Destroy(MapObjects[i]);
            }
            MapObjects = null;
        }
    }

    public static void CreateMapObjects(LevelDescription Desc)
    {

        MapPos pos = new MapPos(0,0);
        GameObject go = null;
        for (int i = 0; i < Desc.Map.Count; i++)
        {
            pos.x = i % Desc.FieldWidth;
            pos.z = i / Desc.FieldWidth;

            switch (Desc.Map[i])
            {
                case 'E':
                    // Entrance
                    go = InstantiateObject("RoadSegment");
                    break;
                case 'X':
                    // Exit
                    go = InstantiateObject("RoadSegment");
                    break;
                case 'R':
                    // Road
                    go = InstantiateObject("RoadSegment");
                    break;
                case 'T':
                    // Turret Space
                    go = InstantiateObject("TurretSpaceSegment");
                    go.tag = "TurretSpace";
                    break;
                case 'D':
                    // Don't instantiate terrain tiles now that there's a backdrop
                    continue;
            }
            if (null == go)
            {
                throw new Exception("Couldn't find asset");
            }
            SetMapPos(go, pos);
            // Rotate
            int rotationMult = (int)UnityEngine.Random.Range(0.0F, 3.99F);
            Quaternion rotation = Quaternion.AngleAxis(90.0F * rotationMult, Vector3.up);
            go.transform.rotation = rotation;
            MapObjects.Add(go);
        }
    }

    public static GameObject InstantiateObject(string AssetName)
    {
        GameObject template = Resources.Load(AssetName) as GameObject;

        return Instantiate(template);
    }

    public static void DestroyObject(GameObject go)
    {
        go.SetActive(false);
        Destroy(go);
    }

    public static void SetMapPos(GameObject go, MapPos pos)
    {
        SetWorldPos(go, MapPosToWorldVec3(pos.Pos));
    }

    public static void SetMapPos(GameObject go, Vector3 pos)
    {
        SetWorldPos(go, MapPosToWorldVec3(pos));
    }

    public static void SetWorldPos(GameObject go, Vector3 pos)
    {
        go.transform.SetPositionAndRotation(pos, Quaternion.identity);
    }


    public static Vector3 MapPosToWorldVec3(Vector3 pos)
    {
        Vector3 translatedPos =  new Vector3(pos.x - MidPointWidth, pos.y, MidPointDepth - pos.z);

        return translatedPos * TranslationMultiplier;
    }

    public static MapPos WorldVec3ToMapPos(Vector3 vec)
    {
        return new MapPos((int)((vec.x / TranslationMultiplier) + MidPointWidth), (int)(MidPointDepth - (vec.z / TranslationMultiplier)));
    }
}