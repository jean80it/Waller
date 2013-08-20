using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TiledMapLoader;
using System;



public class MapManagerScript : MonoBehaviour {

    const string floorPropertyName = "floor";
    const string ignorePropertyName = "ignore";

    public Vector2 TileMultiplier = new Vector2(1, 1);
    public Vector2 ObjectMultiplier = new Vector2(0.03125f, 0.03125f); // 1/32 (if 32 is your base tile size in pixels and you want 1 to be the base prefab size in Unity)

    public List<GameObject> TilesPrefabs = new List<GameObject>();
    
    public string MapPath = "";
	
	public float FloorHeight = 2.0f;
	
    void Start () {
        var map = TileMap.FromFile(MapPath);
        
        Dictionary<int, GameObject> tileCodeToPrefab = new Dictionary<int, GameObject>();

        for (int i = 0, l = TilesPrefabs.Count; i < l; i++)
        {
            tileCodeToPrefab.Add(i, TilesPrefabs[i]);
        }

        foreach (MapLayer l in map.Layers)
		{
           float floor = 0;

           if (l.Properties.Contains(floorPropertyName))
			{
                if (!float.TryParse(l.Properties[floorPropertyName].Value, out floor))
				{
					floor = 0;
				}
			}

			bool ignore = false;

            if (l.Properties.Contains(ignorePropertyName))
			{
                ignore = !string.IsNullOrEmpty(l.Properties[ignorePropertyName].Value);
			}
			
			if (!ignore)
			{
                TiledMapUnityUtils.InstantiateMap(map, l.Name, tileCodeToPrefab, TileMultiplier.x, TileMultiplier.y, floor * FloorHeight);
			}
        }

        foreach (ObjectLayer l in map.ObjectGroups)
        {
            string tempString = l.Name;

            float floor = 0;

            if (l.Properties.Contains(floorPropertyName))
            {
                if (!float.TryParse(l.Properties[floorPropertyName].Value, out floor))
                {
                    floor = 0;
                }
            }

            bool ignore = false;

            if (l.Properties.Contains(ignorePropertyName))
            {
                ignore = !string.IsNullOrEmpty(l.Properties[ignorePropertyName].Value);
            }

            if (!ignore)
            {
                TiledMapUnityUtils.InstantiateObjects(map, l.Name, tileCodeToPrefab, ObjectMultiplier.x, ObjectMultiplier.y, floor * FloorHeight);
            }
        }
	}
}
