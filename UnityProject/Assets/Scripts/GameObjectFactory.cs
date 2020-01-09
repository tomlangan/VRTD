using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using VRTD.Gameplay;

public class GameObjectFactory : MonoBehaviour
{
    static Dictionary<string, GameObject> Objects = new Dictionary<string, GameObject>();
    static float MidPointWidth;
    static float MidPointDepth;
    static float TranslationMultiplier = 2.0F;
    static float ScalingMultiplier = 2.0F;


    public static void CreateMapObjects(LevelDescription Desc)
    {
        MidPointWidth = Desc.FieldWidth / 2.0f;
        MidPointDepth = Desc.FieldDepth / 2.0f;

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
                    go = InstantiateObject("SM_Env_Path_Stone_02");
                    break;
                case 'X':
                    // Exit
                    go = InstantiateObject("SM_Env_Path_Stone_02");
                    break;
                case 'R':
                    // Road
                    go = InstantiateObject("SM_Env_Path_Stone_02");
                    break;
                case 'T':
                    // Turret Space
                    go = InstantiateObject("SM_Env_Path_Cobble_Stone_01");
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
            go.transform.localScale = (Vector3.one * ScalingMultiplier);
            SetMapPos(go, pos);
        }
    }

    public static GameObject InstantiateObject(string AssetName)
    {
        GameObject template = Resources.Load(AssetName) as GameObject;

        return Instantiate(template);
    }

    public static void DestroyObject(GameObject go)
    {
        Destroy(go);
    }

    public static void SetMapPos(GameObject go, MapPos pos)
    {
        SetWorldPos(go, MapPosToVec3(pos.Pos));
    }

    public static void SetMapPos(GameObject go, Vector3 pos)
    {
        SetWorldPos(go, MapPosToVec3(pos));
    }

    public static void SetWorldPos(GameObject go, Vector3 pos)
    {
        go.transform.SetPositionAndRotation(pos, Quaternion.identity);
    }


    public static Vector3 MapPosToVec3(Vector3 pos)
    {
        Vector3 translatedPos =  new Vector3(pos.x - MidPointWidth, pos.y, MidPointDepth - pos.z);

        return translatedPos * TranslationMultiplier;
    }

    public static MapPos Vec3ToMapPos(Vector3 vec)
    {
        return new MapPos((int)((vec.x + MidPointWidth) / TranslationMultiplier), (int)((MidPointDepth + -vec.z) / TranslationMultiplier));
    }
}