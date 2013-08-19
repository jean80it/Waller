#define DotNetZip

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace TiledMapLoader
{
    public interface INamed
    { 
        string Name {get; set;}
    }

    public class TileSetNamedList : KeyedCollection<string, Tileset>
    {
        protected override string GetKeyForItem(Tileset item)
        {
            return item.Name;
        }
    }

    public class MapLayerNamedList : KeyedCollection<string, MapLayer>
    {
        protected override string GetKeyForItem(MapLayer item)
        {
            return item.Name;
        }
    }

    public class ObjectLayerNamedList : KeyedCollection<string, ObjectLayer>
    {
        protected override string GetKeyForItem(ObjectLayer item)
        {
            return item.Name;
        }
    }

    //e.g.: <map version="1.0" orientation="isometric" width="25" height="25" tilewidth="64" tileheight="32">

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
            return (TileMap)MapXmlSerializer.Deserialize(stream);
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

        [XmlElement("tileset")]
        public TileSetNamedList TileSets { get; set; }

        [XmlElement("layer")]
        public MapLayerNamedList Layers { get; set; }

        [XmlElement("objectgroup")]
        public ObjectLayerNamedList ObjectGroups { get; set; }

    }


    //e.g.: <tileset firstgid="1" name="isometric_grass_and_water" tilewidth="64" tileheight="64" spacing="2" margin="1">

    //[XmlRoot("tileset")]
    public class Tileset : INamed
    {
        [XmlAttribute("firstgid")]
        public uint FirstGid { get; set; }

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


    //e.g.: <tileoffset x="0" y="16"/>

    //[XmlRoot("tileoffset")]
    public class TileOffset
    {
        [XmlAttribute("x")]
        public int X { get; set; }

        [XmlAttribute("y")]
        public int Y { get; set; }
    }


    //e.g.: <image source="isometric_grass_and_water.png" width="256" height="384"/>

    //[XmlRoot("image")]
    public class TileImage
    {
        [XmlAttribute("source")]
        public string Source { get; set; }

        [XmlAttribute("width")]
        public int Width { get; set; }

        [XmlAttribute("height")]
        public int Height { get; set; }
    }

    //e.g.: <layer name="Tile Layer 1" width="25" height="25">

    //[XmlRoot("layer")]
    public class MapLayer : INamed
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("width")]
        public int WidthCells { get; set; }

        [XmlAttribute("height")]
        public int HeightCells { get; set; }

        [XmlAttribute("opacity")]
        public float Opacity { get; set; }

        [XmlElement("data")]
        public MapData Data { get; set; }

        public MapLayer()
        { 
            // default values
            Opacity = 100;
        }


        // <data encoding="base64" compression="gzip">
        
		//[XmlRoot("data")]
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
    }

    //e.g.: <objectgroup name="ItemLayer1" width="10" height="10">
    public class ObjectLayer : INamed
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("width")]
        public int WidthCells { get; set; }

        [XmlAttribute("height")]
        public int HeightCells { get; set; }

        [XmlElement("object")]
        public List<TiledObject> TiledObjects { get; set; }
    }

    //e.g.: <object gid="1" x="34" y="128"/>
    public class TiledObject
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
    }

    public class PolyLineObjectTag
    {
        [XmlAttribute("points")]
        public string PointsString { get; set; }

        // TODO: structured points

        // TODO: transformed points
    }

    public class PolygonObjectTag
    {
        [XmlAttribute("points")]
        public string PointsString { get; set; }

        // TODO: structured points

        // TODO: transformed points
    }

    public class EllipseObjectTag
    {
    }

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