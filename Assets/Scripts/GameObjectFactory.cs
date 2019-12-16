using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameObjectFactory : MonoBehaviour
{

    public static void CreateMapObjects(LevelDescription Desc, GameObject RoadTemplate, GameObject TerrainTemplate, GameObject TurretLocationTemplate)
    {
        float midPointWidth = Desc.FieldWidth / 2.0f;
        float midPointDepth = Desc.FieldDepth / 2.0f;
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

            GameObject ro = Instantiate(template, new Vector3(midPointWidth - x, 0.0f, midPointDepth - z), Quaternion.identity);
        }
    }

}