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
	
	static public void InstantiateMap(TileMap map, string layerName, IDictionary<int, GameObject> tilesBlocksMap, float xSpacing, float ySpacing)
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
				
				instantiateBlock(block, new Vector3(x*xSpacing,0,-y*ySpacing), rot);
			}
		}
	}
	
	static private void instantiateBlock(UnityEngine.Object original, Vector3 pos, TilesTransforms rot)
	{
		Vector3 s = new Vector3(-1,1,-1);
		Quaternion a = Quaternion.Euler(0,0,0);
		
		bool hflipped = false;
		bool vflipped = false;
		
		if ((rot & TilesTransforms.HFlip) == TilesTransforms.HFlip)
		{
			s.x = -s.x;
			hflipped = true;
		}
		
		if ((rot & TilesTransforms.VFlip) == TilesTransforms.VFlip)
		{
			s.z = -s.z;
			vflipped = true;
		}
		
		if ((rot & TilesTransforms.DFlip) == TilesTransforms.DFlip)
		{
			a = Quaternion.Euler(0,hflipped^!vflipped?-90: 90,0); 
			s.x = -s.x;
		}
		
		GameObject obj = (GameObject)GameObject.Instantiate(original, pos, a);
		obj.transform.localScale = s;
	}
}

