#define DotNetZip

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace TiledMapLoader
{
    // e.g.: <map version="1.0" orientation="isometric" width="25" height="25" tilewidth="64" tileheight="32">
    [XmlRoot("map")]
    public class TileMap
    {
        #region load/save stuff

        private static XmlSerializer _mapXmlSerializer = null;

        protected static XmlSerializer MapXmlSerializer
        {
            get 
            {
                if (_mapXmlSerializer == null)
                {
                    _mapXmlSerializer = new XmlSerializer(typeof(TileMap));
                }

                return _mapXmlSerializer;
            }
        }

        public static TileMap FromStream(Stream stream)
        {
            TileMap map = (TileMap)MapXmlSerializer.Deserialize(stream);
            fixLayersParent(map);
            return map;
        }

        private static void fixLayersParent(TileMap map)
        {
            foreach (var l in map.Layers)
            {
                l.ParentMap = map;
            }
            
            foreach (var l in map.ObjectGroups)
            {
                l.ParentMap = map;
            }
        }

        public static TileMap FromFile(string fileName)
        {
            using (var fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open))
            {
                return TileMap.FromStream(fs);
            }
        }

        public void ToStream(Stream stream)
        {
            MapXmlSerializer.Serialize(stream, this);
        }

        public void ToFile(string fileName)
        {
            using (var fs = new System.IO.FileStream(fileName, System.IO.FileMode.Create))
            {
                ToStream(fs);
            }
        }

        #endregion // load/save stuff

        public enum Orientations
        {
            orthogonal,
            isometric,
        }

        [XmlAttribute("version")]
        public string Version { get; set; }

        [XmlAttribute("orientation")]
        public Orientations Orientation { get; set; }

        [XmlAttribute("width")]
        public int Width { get; set; }

        [XmlAttribute("height")]
        public int Height { get; set; }

        [XmlAttribute("tilewidth")]
        public int TileWidth { get; set; }

        [XmlAttribute("tileheight")]
        public int TileHeight { get; set; }

        // TODO: backgroundcolor

        [XmlElement("tileset")]
        public TileSetNamedList TileSets { get; set; }

        [XmlElement("layer")]
        public MapLayerNamedList Layers { get; set; }

        [XmlElement("objectgroup")]
        public ObjectLayerNamedList ObjectGroups { get; set; }
	
        [XmlArray("properties")]
        [XmlArrayItem("property")] 
        public PropertiesNamedList Properties { get; set; }
    }

    // e.g.: <tileset firstgid="1" name="isometric_grass_and_water" tilewidth="64" tileheight="64" spacing="2" margin="1">
    public class Tileset : INamed
    {
        [XmlAttribute("firstgid")]
        public uint FirstGid { get; set; }

        // source:??? (external TSX)

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("tilewidth")]
        public int TileWidth { get; set; }

        [XmlAttribute("tileheight")]
        public int TileHeight { get; set; }

        [XmlAttribute("spacing")]
        public int Spacing { get; set; }

        [XmlAttribute("margin")]
        public int Margin { get; set; }

        [XmlElement("tileoffset")]
        public TileOffset Offset { get; set; }

        [XmlElement("image")]
        public TileImage Image { get; set; }
    }

    // e.g.: <tileoffset x="0" y="16"/>
    public class TileOffset
    {
        [XmlAttribute("x")]
        public int X { get; set; }

        [XmlAttribute("y")]
        public int Y { get; set; } // positive is down
    }

    // e.g.: <image source="isometric_grass_and_water.png" width="256" height="384"/>
    public class TileImage
    {
        //format: Used for embedded images, in combination with a data child element

        [XmlAttribute("source")]
        public string Source { get; set; }

        // trans: Defines a specific color that is treated as transparent (example value: "FF00FF" for magenta)

        [XmlAttribute("width")]
        public int Width { get; set; }

        [XmlAttribute("height")]
        public int Height { get; set; }
    }

    // TODO: terraintypes, terrain

    // e.g.: <layer name="Tile Layer 1">
    public class TileLayer : INamed
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("opacity")]
        public float Opacity { get; set; }

        // visible: Whether the layer is shown (1) or hidden (0). Defaults to 1

        [XmlElement("data")]
        public MapData Data { get; set; }

        public TileLayer()
        { 
            // default values
            Opacity = 1.0f;
        }


        // e.g.:<data encoding="base64" compression="gzip">
        public class MapData : IXmlSerializable
        {
            public MapData()
            {
                DefaultTileCode = 0;
            }

            public enum Compressions
            {
                none,
                gzip,
                zlib,
                //... 
            }

            public enum Encodings
            {
                XML,
                base64
                //...
            }

            public uint DefaultTileCode { get; set; }

            [XmlAttribute("compression")]
            public Compressions Compression { get; set; }

            [XmlAttribute("encoding")]
            public Encodings Encoding { get; set; }

            public uint[] Data { get; set; }

            [Flags]
            public enum TilesTransforms : uint
            {
                None = 0,
                HFlip = 4,
                VFlip = 2,
                DFlip = 1,

                // ---
                Rotate90 = DFlip | HFlip,
                Rotate180 = HFlip | VFlip,
                Rotate270 = VFlip | DFlip,

                VFlipRot90 = HFlip | VFlip | DFlip,
                HFlipRot270 = VFlipRot90,
            }

            public TilesTransforms GetTransform(uint i)
            {
                return (TilesTransforms)(i >> 29); 
            }

            public uint GetCode(uint i)
            {
                return i & 536870911;
            }

            public uint this[long idx]
            {
                get
                {
                    try
                    {
                        return Data[idx];
                    }
                    catch
                    {
                        return DefaultTileCode;
                    }

                }

                set
                {
                    Data[idx] = value;
                }
            }

            public System.Xml.Schema.XmlSchema GetSchema()
            {
                throw new NotImplementedException();
            }

            public void ReadXml(System.Xml.XmlReader reader)
            {
                if (reader.GetAttribute("encoding") != null)
                {
                    string encodingString = reader.GetAttribute("encoding");
                    string compressionString = reader.GetAttribute("compression");
					
                    switch (encodingString)
                    {
                        case "": // ??
                            Encoding = Encodings.XML;
                            break;

                        case "base64":
                            Encoding = Encodings.base64;
                            break;

                        default:
                            throw new Exception("Unrecognized encoding.");
                    }

                    switch (compressionString)
                    {
                        case "gzip":
                            Compression = Compressions.gzip;
                            break;

                        case "zlib":
                            Compression = Compressions.zlib;
                            break;

                        default:
                            throw new Exception("Unrecognized compression.");
                    }
					
					switch (Encoding)
                    {
                        case Encodings.base64:
                            {
                                //Stream stream = new Base64ToXmlReaderWriterStream(reader);
								string d = reader.ReadElementContentAsString();
								Stream stream = new MemoryStream(Convert.FromBase64String(d));
						
								switch (Compression)
								{
									case Compressions.gzip:
#if DotNetZip
										stream = new Ionic.Zlib.GZipStream(stream, Ionic.Zlib.CompressionMode.Decompress, true);
#else
                                    	stream = new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress, false);
#endif
									break;
								
									case Compressions.zlib:
#if DotNetZip
										stream = new Ionic.Zlib.ZlibStream(stream, Ionic.Zlib.CompressionMode.Decompress, true);
#else
                                    	throw new Exception("zlib not available without external libs");
#endif

									break;
						
								}
						
                                List<uint> data = new List<uint>();

                                using (stream)
                                {
                                    using (var br = new BinaryReader(stream))
                                    {
                                        while (true)
                                        {
                                            try
                                            {
                                                data.Add(br.ReadUInt32());
                                            }
                                            catch (System.IO.EndOfStreamException)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }

                                Data = data.ToArray();

                            };
                            break;

                        default:
                            throw new Exception("Unimplemented encoding reader.");
                    }
                }
                else
                {
                    Encoding = Encodings.XML;

                    using (var st = reader.ReadSubtree())
                    {
                        List<uint> data = new List<uint>();
                        while (!st.EOF)
                        {
                            switch (st.NodeType)
                            {
                                case XmlNodeType.Element:
                                    if (st.Name == "tile")
                                    {
                                        //if (i < ntiles)
                                        {
                                            data.Add(uint.Parse(st.GetAttribute("gid")));
                                        }
                                    }

                                    break;
                                case XmlNodeType.EndElement:
                                    break;
                            }

                            st.Read();
                        }

                        Data = data.ToArray();
                    }
                }
            }

            public void WriteXml(System.Xml.XmlWriter writer)
            {
                switch (Encoding)
                { 
                    case Encodings.base64:
                        writer.WriteAttributeString("encoding", "base64");
                        break;

                    default:
                        throw new Exception("Unimplemented encoding writer.");
                }

                switch (Compression)
                { 
                    case Compressions.gzip:
                        writer.WriteAttributeString("compression", "gzip");
                        break;

                    default:
                        throw new Exception("Unimplemented encoding writer.");
                }

                Stream encodedStream = null;
                Stream compressedStream = null;

                if ((Encoding == Encodings.base64))
                {
                    encodedStream = new Base64ToXmlReaderWriterStream(writer);
                }
                else
                {
                    throw new Exception("Unimplemented encoding handler.");
                }
				
				switch (Compression)
				{
					case Compressions.gzip:
#if DotNetZip
						compressedStream = new Ionic.Zlib.GZipStream(encodedStream, Ionic.Zlib.CompressionMode.Compress);
#else
                    	compressedStream = new GZipStream(encodedStream, CompressionMode.Compress, false);
#endif
					break;
				
					case Compressions.zlib:
#if DotNetZip
						compressedStream = new Ionic.Zlib.ZlibStream(encodedStream, Ionic.Zlib.CompressionMode.Compress);
#else
                    	throw new Exception("zlib not available without external libs");
#endif

					break;
		
				}
				
                BufferedStream bufferedStream = new BufferedStream(compressedStream, 4096); // GZipStream seems to be doing compression every time a write is performed (!)

                BinaryWriter binaryWriter = new BinaryWriter(bufferedStream);

                using(encodedStream)
                using (compressedStream)
                using(bufferedStream)
                using (binaryWriter)
                {
                    int l = Data.Length;
                    for (int i = 0; i < l; ++i)
                    {
                        binaryWriter.Write(Data[i]);
                    }
                }               
            }
        }

        public uint GetTile(int x, int y)
        {
            if ((x >= 0) && (y >= 0) && (x < this.WidthCells) && (y < this.HeightCells))
            {
                return Data[x + y * this.WidthCells];
            }
            else
            {
                return Data.DefaultTileCode;
            }
        }
	
        [XmlArray("properties")]
        [XmlArrayItem("property")] 
        public PropertiesNamedList Properties { get; set; }

        #region parent map stuff

        private TileMap _parentMap = null;

        public TileMap ParentMap
        {
            get 
            {
                // TODO: some sort of automatic parent retrieval
                return _parentMap;
            }

            set 
            {
                if (_parentMap != null)
                {
                    if (_parentMap.Layers.Contains(this))
                    {
                        _parentMap.Layers.Remove(this);
                    }
                }
                
                _parentMap = value;
                
                if (!_parentMap.Layers.Contains(this))
                {
                    _parentMap.Layers.Add(this);
                }
            }
        }

        [XmlIgnore]
        public int WidthCells
        {
            get { return ParentMap.Width; }
        }

        [XmlIgnore]
        public int HeightCells
        {
            get { return ParentMap.Height; }
        }

        #endregion // parent map stuff
    }

    // e.g.: <objectgroup name="ItemLayer1">
    public class ObjectGroup : INamed
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        // color: The color used to display the objects in this group
       
        [XmlAttribute("opacity")]
        public float Opacity { get; set; }

        [XmlElement("object")]
        public List<TiledObject> TiledObjects { get; set; }

	
        [XmlArray("properties")]
        [XmlArrayItem("property")] 
        public PropertiesNamedList Properties { get; set; }

        public ObjectGroup()
        {
            Opacity = 1.0f;
        }

        #region parent map stuff

        private TileMap _parentMap = null;

        public TileMap ParentMap
        {
            get
            {
                // TODO: some sort of automatic parent retrieval
                return _parentMap;
            }

            set
            {
                if (_parentMap != null)
                {
                    if (_parentMap.ObjectGroups.Contains(this))
                    {
                        _parentMap.ObjectGroups.Remove(this);
                    }
                }

                _parentMap = value;

                if (!_parentMap.ObjectGroups.Contains(this))
                {
                    _parentMap.ObjectGroups.Add(this);
                }
            }
        }

        [XmlIgnore]
        public int WidthCells
        {
            get { return ParentMap.Width; }
        }

        [XmlIgnore]
        public int HeightCells
        {
            get { return ParentMap.Height; }
        }

        #endregion // parent map stuff
    }

    // e.g.: <object name="thatStuff" type="someStuff" x="992" y="96" width="384" height="320" rotation="36.3129">
    public class TiledObject : INamed
    {
        public enum ObjectShapes : int
        { 
            Unset = 0,
            Tile = 1,
            Rectangle = 2,
            Ellipse = 3,
            Polygon = 4,
            PolyLine = 5
        }

        public TiledObject()
        {
            Gid = -1;
        }

        private int strToInt(string s)
        {
            int i;
            if (int.TryParse(s, out i))
            {
                return i;
            }
            else
            {
                return -1;
            }
        }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("type")]
        public string TypeString { get; set; }

        [XmlAttribute("gid")]
        public string GidString 
        {
            get
            {
                return Gid + "";
            }

            set
            {
                Gid = strToInt(value);
            }
        }

        [XmlIgnore]
        public int Gid { get; set; }

        [XmlAttribute("x")]
        public int X { get; set; }

        [XmlAttribute("y")]
        public int Y { get; set; }


        [XmlIgnore]
        public int Width { get; set; }

        [XmlIgnore]
        public int Height { get; set; }

        [XmlAttribute("width")]
        public string WidthString
        {
            get
            {
                return Width + "";
            }

            set
            {
                Width = strToInt(value);
            }
        }

        [XmlAttribute("height")]
        public string HeightString
        {
            get
            {
                return Height + "";
            }

            set
            {
                Height = strToInt(value);
            }
        }

        [XmlAttribute("rotation")]
        public float Rotation { get; set; }

        // visible: Whether the object is shown (1) or hidden (0). Defaults to 1. (since 0.9.0)

        // Object Type handling

        [XmlElement("polyline")]
        public PolyLineObjectTag PolyLineParams { get; set; }

        [XmlElement("polygon")]
        public PolygonObjectTag PolygonObjectParams { get; set; }

        [XmlElement("ellipse")]
        public EllipseObjectTag EllipseObjectParams { get; set; }

        public ObjectShapes ObjectShape
        {
            get
            {
                if (Gid != -1)
                    return ObjectShapes.Tile;

                if ((PolyLineParams == null) && (PolygonObjectParams == null) && (EllipseObjectParams == null))
                {
                    return ObjectShapes.Rectangle;
                }

                if (PolyLineParams != null)
                {
                    return ObjectShapes.PolyLine;
                }

                if (PolygonObjectParams != null)
                {
                    return ObjectShapes.Polygon;
                }

                if (EllipseObjectParams != null)
                {
                    return ObjectShapes.Ellipse;
                }

                return ObjectShapes.Unset;
            }

            set 
            {
                switch (value)
                {
                    case ObjectShapes.Tile:
                        PolygonObjectParams = null;
                        PolyLineParams = null;
                        EllipseObjectParams = null;
                        break;

                    case ObjectShapes.Ellipse:
                        Gid = -1;
                        PolygonObjectParams = null;
                        PolyLineParams = null;
                        EllipseObjectParams = new EllipseObjectTag();
                        break;

                    case ObjectShapes.Polygon:
                        Gid = -1;
                        PolygonObjectParams = new PolygonObjectTag();
                        PolyLineParams = null;
                        EllipseObjectParams = null;
                        break;

                    case ObjectShapes.PolyLine:
                        Gid = -1;
                        PolygonObjectParams = null;
                        PolyLineParams = new PolyLineObjectTag();
                        EllipseObjectParams = null;
                        break;

                    //case ObjectShapes.Unset:
                    //case ObjectShapes.Rectangle:
                    default:
                        Gid = -1;
                        PolygonObjectParams = null;
                        PolyLineParams = null;
                        EllipseObjectParams = null;
                        break;
                }
            }
        }

	[XmlArray("properties")]
    	[XmlArrayItem("property")] 
	public PropertiesNamedList Properties { get; set; }
    }

    // e.g.: <polyline points="0,0 256,64 0,256 -192,192 -320,96"/>
    public class PolyLineObjectTag
    {
        [XmlAttribute("points")]
        public string PointsString { get; set; }

        // TODO: structured points

        // TODO: transformed points
    }

    // e.g.: <polygon points="0,0 0,288 352,96"/>
    public class PolygonObjectTag
    {
        [XmlAttribute("points")]
        public string PointsString { get; set; }

        // TODO: structured points

        // TODO: transformed points
    }

    // e.g.: <ellipse/>
    public class EllipseObjectTag
    { }

    // e.g.: <property name="Strength" value="100"/>
    public class KeyValueProperty
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("value")]
        public string Value { get; set; }
    }

    // TODO: imagelayer

    #region Named lists

    public interface INamed
    {
        string Name { get; set; }
    }

    public class TileSetNamedList : KeyedCollection<string, Tileset>
    {
        protected override string GetKeyForItem(Tileset item)
        {
            return item.Name;
        }
    }

    public class MapLayerNamedList : KeyedCollection<string, TileLayer>
    {
        protected override string GetKeyForItem(TileLayer item)
        {
            return item.Name;
        }
    }

    public class ObjectLayerNamedList : KeyedCollection<string, ObjectGroup>
    {
        protected override string GetKeyForItem(ObjectGroup item)
        {
            return item.Name;
        }
    }

    public class PropertiesNamedList : KeyedCollection<string, KeyValueProperty>
    {
        protected override string GetKeyForItem(KeyValueProperty item)
        {
            return item.Name;
        }

        public float GetAsFloat(string propertyName)
        {
            return GetAsFloat(propertyName, 0.0f);
        }

        public float GetAsFloat(string propertyName, float defaultValue)
        {
            if (!this.Contains(propertyName))
            {
                return defaultValue;
            }

            float v;

            if (!float.TryParse(this[propertyName].Value, out v))
            {
                return v;
            }

            return defaultValue;
        }

        public bool GetAsBool(string propertyName)
        {
            return GetAsBool(propertyName, false);
        }

        public bool GetAsBool(string propertyName, bool defaultValue)
        {
            if (!this.Contains(propertyName))
            {
                return defaultValue;
            }

            string v = this[propertyName].Value.Trim().ToLowerInvariant();

            return !(string.IsNullOrEmpty(v) || v.Equals("0") || v.Equals("false"));
        }
    }

    #endregion // Named lists

    public class Base64ToXmlReaderWriterStream : Stream
    {
        System.Xml.XmlReader _reader;
        System.Xml.XmlWriter _writer;

        int _position = 0;

        bool _readMode;

        public Base64ToXmlReaderWriterStream(System.Xml.XmlReader reader)
        {
            _reader = reader;
            _readMode = true;
        }

        public Base64ToXmlReaderWriterStream(System.Xml.XmlWriter writer)
        {
            _writer = writer;
            _readMode = false;
        }

        public override bool CanRead
        {
            get { return _readMode; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return !_readMode; }
        }

        public override void Flush()
        {
            _writer.Flush();
        }

        public override long Length
        {
            get { throw new NotSupportedException(); }
        }

        public override long Position
        {
            get
            {
                return _position;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int red = _reader.ReadElementContentAsBase64(buffer, 0, count);
            _position += red;
            return red;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _writer.WriteBase64(buffer, offset, count);
            _position += count;
        }
    }
}