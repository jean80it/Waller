using System;
using System.Collections.Generic;
using TiledMapLoader;
using UnityEngine;
using TilesTransforms = TiledMapLoader.TileLayer.MapData.TilesTransforms;

public class TiledMapUnityUtils
{
    const string DefaultFloorPropertyName = "floor";
    const string DefaultIgnorePropertyName = "ignore";

	static public IDictionary<int, GameObject> LoadTilesGameObjects(string name, int startCode, int endCode)
	{
		Dictionary<int, GameObject> blocks = new Dictionary<int, GameObject>();
		
		for (int i = startCode;i<=endCode;++i)
		{
	    	var prefab = (GameObject)Resources.Load(string.Format("{0}{1:00}", name, i), typeof(GameObject));
			blocks.Add(i, prefab);
		}
		
		return blocks;
	}

    static public void InstantiateTilesLayer(TileMap map, string layerName, IDictionary<int, GameObject> tilesPrefabsMap, float xMultiplier, float yMultiplier, float heightOffset)
    {
        TileLayer l = map.Layers[layerName];
        InstantiateTilesLayer(l, tilesPrefabsMap, xMultiplier, yMultiplier, heightOffset);
    }

    static public void InstantiateTilesLayer(TileLayer l, IDictionary<int, GameObject> tilesPrefabsMap, float xMultiplier, float yMultiplier, float heightOffset)
    {
		for (int y = 0; y<l.HeightCells; ++y)
		{	
			for (int x = 0; x<l.WidthCells; ++x)
			{
				var t = l.GetTile(x,y);
				var code = l.Data.GetCode(t);
				TilesTransforms rot = l.Data.GetTransform(t);
				
				//... instantiate gameobject accordingly
				GameObject gameObject;
				if (!tilesPrefabsMap.TryGetValue((int)code, out gameObject))
				{
					continue;
				}
				
				if (gameObject==null)
				{
					if (code != 0)
					{
						throw new Exception(string.Format("Tile code {0} missing.", code));
					}
				}
				else
				{
                    instantiateGameObject(gameObject, new Vector3(x * xMultiplier, heightOffset, -y * yMultiplier), rot);
				}
			}
		}
	}

    static public void InstantiateTilesLayer(TileMap map, string layerName, IList<GameObject> tilesPrefabsMap, float xMultiplier, float yMultiplier, float heightOffset)
    {
        TileLayer l = map.Layers[layerName];
        InstantiateTilesLayer(l, tilesPrefabsMap, xMultiplier, yMultiplier, heightOffset);
    }

    static public void InstantiateTilesLayer(TileLayer l, IList<GameObject> tilesPrefabsMap, float xMultiplier, float yMultiplier, float heightOffset)
    {
        for (int y = 0; y < l.HeightCells; ++y)
        {
            for (int x = 0; x < l.WidthCells; ++x)
            {
                var t = l.GetTile(x, y);
                var code = l.Data.GetCode(t);
                TilesTransforms rot = l.Data.GetTransform(t);

                //... instantiate gameobject accordingly
                GameObject gameObject;
                if (!tilesPrefabsMap.Count<=code)
                {
                    continue;
                }

                gameObject = tilesPrefabsMap[(int)code];

                if (gameObject == null)
                {
                    if (code != 0)
                    {
                        throw new Exception(string.Format("Tile code {0} missing.", code));
                    }
                }
                else
                {
                    instantiateGameObject(gameObject, new Vector3(x * xMultiplier, heightOffset, -y * yMultiplier), rot);
                }
            }
        }
    }

	static private void instantiateGameObject(UnityEngine.Object original, Vector3 pos, TilesTransforms rot)
	{
		Vector3 s = new Vector3(1,1,1);
        float angle = 0;

        if ((rot && TilesTransforms.Rotate90) == TilesTransforms.Rotate90)
        {
            angle = 90;
            rot &= !TilesTransforms.Rotate90;
        }
        else
        {
            if ((rot && TilesTransforms.Rotate180) == TilesTransforms.Rotate180)
            {
                angle = 180;
                rot &= !TilesTransforms.Rotate180;
            }
            else
            {
                if ((rot && TilesTransforms.Rotate270) == TilesTransforms.Rotate270)
                {
                    angle = 180;
                    rot &= !TilesTransforms.Rotate270;
                }
            }
        }

		Quaternion a = Quaternion.Euler(0,0,0);
//		
//		bool hflipped = false;
//		bool vflipped = false;
//		
//		if ((rot & TilesTransforms.HFlip) == TilesTransforms.HFlip)
//		{
//			s.x = -s.x;
//			hflipped = true;
//		}
//		
//		if ((rot & TilesTransforms.VFlip) == TilesTransforms.VFlip)
//		{
//			s.z = -s.z;
//			vflipped = true;
//		}
//		
//		if ((rot & TilesTransforms.DFlip) == TilesTransforms.DFlip)
//		{
//			a = Quaternion.Euler(0,hflipped^!vflipped?-90: 90,0); 
//			s.x = -s.x;
//		}

		// ======================================
		
		switch (rot) {
		case TilesTransforms.None:
			a = Quaternion.Euler(0,180,0); 
			break;
			
		case TilesTransforms.Rotate90:
			a = Quaternion.Euler(0,270,0); 
			break;
			
		case TilesTransforms.Rotate180:
			a = Quaternion.Euler(0,0,0); 
			break;
			
		case TilesTransforms.Rotate270:
			a = Quaternion.Euler(0,90,0); 
			break;

//		case TilesTransforms.VFlipRot90:
//			a = Quaternion.Euler(0,-90,0);
//			s.z = -s.z;
//			break;
			
//		case TilesTransforms.HFlipRot270:
//			a = Quaternion.Euler(0,270,0); 
//			s.x = -s.x;
//			break;
			
//		case TilesTransforms.HFlip: 
//			s.x = -s.x;
//			break;
//            
//		case TilesTransforms.VFlip:
//			s.z = -s.z;
//			break;
//			
//		case TilesTransforms.DFlip: 
//			a = Quaternion.Euler(0,90,0); 
//			s.x = -s.x;
//			break;
//			
			// TODO: take a careful look to this.... :(
		
		//default:
			
			
		}

		GameObject obj = (GameObject)GameObject.Instantiate(original, pos, a);
		obj.transform.localScale = s;

	}

    static public void InstantiateObjectsGroup(TileMap map, string layerName, IDictionary<int, GameObject> tilesPrefabsMap, float xMultiplier, float yMultiplier, float yOffset)
    {
        ObjectGroup l = map.ObjectGroups[layerName];
        InstantiateObjectsGroup(l, tilesPrefabsMap, xMultiplier, yMultiplier, yOffset);
    }

    static public void InstantiateObjectsGroup(ObjectGroup l, IDictionary<int, GameObject> tilesPrefabsMap, float xMultiplier, float yMultiplier, float yOffset)
    {
        xMultiplier = xMultiplier / map.TileWidth;
        yMultiplier = yMultiplier / map.TileHeight;

        foreach (TiledObject to in l.TiledObjects)
        {
            //... instantiate gameobject accordingly
            GameObject gameObject;
            if (!tilesPrefabsMap.TryGetValue(to.Gid, out gameObject))
            {
                continue;
            }

            instantiateGameObject(gameObject, new Vector3(to.X * xMultiplier, yOffset, -to.Y * yMultiplier), to.Rotation);
        }
    }

    static public void InstantiateObjectsGroup(TileMap map, string layerName, IList<GameObject> tilesPrefabsMap, float xMultiplier, float yMultiplier, float yOffset)
    {
        ObjectGroup l = map.ObjectGroups[layerName];
        InstantiateObjectsGroup(l, tilesPrefabsMap, xMultiplier, yMultiplier, yOffset);
    }

    static public void InstantiateObjectsGroup(ObjectGroup l, IList<GameObject> tilesPrefabsMap, float xMultiplier, float yMultiplier, float yOffset)
    {
        xMultiplier = xMultiplier / map.TileWidth;
        yMultiplier = yMultiplier / map.TileHeight;

        var l = map.ObjectGroups[layerName];
        foreach (TiledObject to in l.TiledObjects)
        {
            //... instantiate gameobject accordingly
            if (!tilesPrefabsMap.Count <= code)
            {
                continue;
            }

            gameObject = tilesPrefabsMap[(int)code];

            if (gameObject == null)
            {
                if (code != 0)
                {
                    throw new Exception(string.Format("Tile code {0} missing (while instantiating object).", code));
                }
            }
            else
            {
                instantiateGameObject(gameObject, new Vector3(to.X * xMultiplier, yOffset, -to.Y * yMultiplier), to.Rotation);
            }
        }
    }

    static private void instantiateGameObject(UnityEngine.Object original, Vector3 pos, float yrotation)
    {
        instantiateGameObject(original, pos, yrotation, null);
    }

    static private void instantiateGameObject(UnityEngine.Object original, Vector3 pos, float yrotation, PropertiesNamedList properties)
    {
        Quaternion a = Quaternion.Euler(0, yrotation, 0);
        
        GameObject obj = (GameObject)GameObject.Instantiate(original, pos, a);

        TiledPropertiesUnityComponent propertiesComponent = (TiledPropertiesUnityComponent)obj.AddComponent("TiledPropertiesUnityComponent");
        propertiesComponent.Properties = properties;
        //propertiesComponent.MyMap = ...
    }

    // TODO: add directions, scale and orientation correction; consider making this a "factory"

    static public void InstantiateWholeMap(TileMap map, IList<GameObject> tilesPrefabsMap)
    {
        InstantiateWholeMap(map, tilesPrefabsMap, DefaultFloorPropertyName, DefaultIgnorePropertyName);
    }

    static public void InstantiateWholeMap(TileMap map, IDictionary<int, GameObject> tilesPrefabsMap)
    {
        InstantiateWholeMap(map, tilesPrefabsMap, DefaultFloorPropertyName, DefaultIgnorePropertyName);
    }

    static public void InstantiateWholeMap(TileMap map, IList<GameObject> tilesPrefabsMap, string floorPropertyName, string ignorePropertyName)
    {
        InstantiateWholeMap(map, tilesPrefabsMap, floorPropertyName, ignorePropertyName, new Vector2(1.0f, 1.0f), new Vector2(1.0f, 1.0f));
    }

    static public void InstantiateWholeMap(TileMap map, IDictionary<int, GameObject> tilesPrefabsMap, string floorPropertyName, string ignorePropertyName)
    {
        InstantiateWholeMap(map, tilesPrefabsMap, floorPropertyName, ignorePropertyName, new Vector2(1.0f, 1.0f), new Vector2(1.0f, 1.0f));
    }

    static public void InstantiateWholeMap(TileMap map, IList<GameObject> tilesPrefabsMap, string floorPropertyName, string ignorePropertyName, Vector2 TilesPositionMultiplier, Vector2 ObjectsPositionMultiplier)
    {
        foreach (TileLayer l in map.Layers)
        {
            if (!l.Properties.GetAsBool(ignorePropertyName))
            {
                InstantiateTilesLayer(map, l.Name, tilesPrefabsMap, TilesPositionMultiplier.x, TilesPositionMultiplier.y, l.Properties.GetAsFloat(floorPropertyName) * FloorHeight);
            }
        }

        foreach (ObjectGroup l in map.ObjectGroups)
        {
            if (!l.Properties.GetAsBool(ignorePropertyName))
            {
                InstantiateObjectsGroup(map, l.Name, tilesPrefabsMap, ObjectsPositionMultiplier.x, ObjectsPositionMultiplier.y, l.Properties.GetAsFloat(floorPropertyName) * FloorHeight);
            }
        }
    }

    static public void InstantiateWholeMap(TileMap map, IDictionary<int, GameObject> tilesPrefabsMap, string floorPropertyName, string ignorePropertyName, Vector2 TilesPositionMultiplier, Vector2 ObjectsPositionMultiplier)
    {
        foreach (TileLayer l in map.Layers)
        {
            if (!l.Properties.GetAsBool(ignorePropertyName))
            {
                InstantiateTilesLayer(l, tilesPrefabsMap, TilesPositionMultiplier.x, TilesPositionMultiplier.y, l.Properties.GetAsFloat(floorPropertyName) * FloorHeight);
            }
        }

        foreach (ObjectGroup l in map.ObjectGroups)
        {
            if (!l.Properties.GetAsBool(ignorePropertyName))
            {
                InstantiateObjectsGroup(l, tilesPrefabsMap, ObjectsPositionMultiplier.x, ObjectsPositionMultiplier.y, l.Properties.GetAsFloat(floorPropertyName) * FloorHeight);
            }
        }
    }
	
}

