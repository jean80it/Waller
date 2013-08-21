using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TiledMapLoader;
using System;

public class TiledPropertiesUnityComponent : MonoBehaviour
{
    public PropertiesNamedList Properties { get; set; }
    public TileLayer MyLayer { get; set; }
    public ObjectGroup MyGroup { get; set; }
    public TileMap MyMap { get; set; }
}