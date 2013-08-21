using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TiledMapLoader;
using System;



public class MapManagerScript : MonoBehaviour {

    public string FloorPropertyName = "floor";
    public string IgnorePropertyName = "ignore";

    public Vector2 TilesPositionMultiplier = new Vector2(1.0f, 1.0f);
    public Vector2 ObjectsPositionMultiplier = new Vector2(1.0f, 1.0f); // 1/32 (if 32 is your base tile size in pixels and you want 1 to be the base prefab size in Unity)

    public List<GameObject> TilesPrefabs = new List<GameObject>();
    
    public string MapPath = "";
	
	public float FloorHeight = 2.0f;

    public Vector3 XDirection = new Vector3(1.0f, 0.0f, 0.0f);
    public Vector3 YDirection = new Vector3(0.0f, 0.0f, 1.0f);
    public Vector3 ZDirection = new Vector3(0.0f, 1.0f, 0.0f);

    void Start () {
        var map = TileMap.FromFile(MapPath);
        
        Dictionary<int, GameObject> tileCodeToPrefab = new Dictionary<int, GameObject>();

        for (int i = 0, l = TilesPrefabs.Count; i < l; i++)
        {
            tileCodeToPrefab.Add(i, TilesPrefabs[i]);
        }

        // TODO: use InstantiateWholeMap instead

        foreach (TileLayer l in map.Layers)
		{
           float floor = 0;

           if (l.Properties.Contains(FloorPropertyName))
			{
                if (!float.TryParse(l.Properties[FloorPropertyName].Value, out floor))
				{
					floor = 0;
				}
			}

			bool ignore = false;

            if (l.Properties.Contains(IgnorePropertyName))
			{
                ignore = !string.IsNullOrEmpty(l.Properties[IgnorePropertyName].Value);
			}
			
			if (!ignore)
			{
                TiledMapUnityUtils.InstantiateTilesLayer(map, l.Name, tileCodeToPrefab, TilesPositionMultiplier.x, TilesPositionMultiplier.y, floor * FloorHeight);
			}
        }

        foreach (ObjectGroup l in map.ObjectGroups)
        {
            string tempString = l.Name;

            float floor = 0;

            if (l.Properties.Contains(FloorPropertyName))
            {
                if (!float.TryParse(l.Properties[FloorPropertyName].Value, out floor))
                {
                    floor = 0;
                }
            }

            bool ignore = false;

            if (l.Properties.Contains(IgnorePropertyName))
            {
                ignore = !string.IsNullOrEmpty(l.Properties[IgnorePropertyName].Value);
            }

            if (!ignore)
            {
                TiledMapUnityUtils.InstantiateObjectsGroup(map, l.Name, tileCodeToPrefab, ObjectsPositionMultiplier.x, ObjectsPositionMultiplier.y, floor * FloorHeight);
            }
        }
	}
}
