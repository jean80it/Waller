using System;
using System.Collections.Generic;
using TiledMapLoader;
using UnityEngine;
using TilesTransforms = TiledMapLoader.MapLayer.MapData.TilesTransforms;

public class TiledMapUnityUtils
{
	static public IDictionary<int, GameObject> LoadTileBlocks(string name, int startCode, int endCode)
	{
		Dictionary<int, GameObject> blocks = new Dictionary<int, GameObject>();
		
		for (int i = startCode;i<=endCode;++i)
		{
	    	var prefab = (GameObject)Resources.Load(string.Format("{0}{1:00}", name, i), typeof(GameObject));
			blocks.Add(i, prefab);
			//Debug.Log(string.Format("prefab {0:00} is {1}",i, prefab==null?"null":"not null"));
		}
		
		return blocks;
	}

    static public void InstantiateMap(MapLayer l, IDictionary<int, GameObject> tilesBlocksMap, float xMultiplier, float yMultiplier, float yOffset)
    {
    }

    static public void InstantiateMap(TileMap map, string layerName, IDictionary<int, GameObject> tilesBlocksMap, float xMultiplier, float yMultiplier, float yOffset)
	{
		var l  = map.Layers[layerName];
		for (int y = 0; y<l.HeightCells; ++y)
		{	
			for (int x = 0; x<l.WidthCells; ++x)
			{
				var t = l.GetTile(x,y);
				var code = l.Data.GetCode(t);
				TilesTransforms rot = l.Data.GetTransform(t);
				
				//... instantiate gameobject accordingly
				GameObject block;
				if (!tilesBlocksMap.TryGetValue((int)code, out block))
				{
					continue;
				}
				
				if (block==null)
				{
					if (code != 0)
					{
						throw new Exception(string.Format("Tile code {0} missing.", code));
					}
				}
				else
				{
					instantiateBlock(block, new Vector3(x*xMultiplier, yOffset, -y*yMultiplier), rot);
				}
			}
		}
	}
	
	static private void instantiateBlock(UnityEngine.Object original, Vector3 pos, TilesTransforms rot)
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

    static public void InstantiateObjects(TileMap map, string layerName, IDictionary<int, GameObject> tilesBlocksMap, float xMultiplier, float yMultiplier, float yOffset)
    {
        var l = map.ObjectGroups[layerName];
        foreach (TiledObject to in l.TiledObjects)
        {
            //... instantiate gameobject accordingly
            GameObject block;
            if (!tilesBlocksMap.TryGetValue(to.Gid, out block))
            {
                continue;
            }

            instantiateBlock(block, new Vector3(to.X * xMultiplier, yOffset, -to.Y * yMultiplier), to.Rotation);
        }
    }

    static private void instantiateBlock(UnityEngine.Object original, Vector3 pos, float yrotation)
    {
        Quaternion a = Quaternion.Euler(0, yrotation, 0);
        
        GameObject obj = (GameObject)GameObject.Instantiate(original, pos, a);
    }

    
	
}

