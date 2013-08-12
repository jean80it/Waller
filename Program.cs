using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var map = TiledMapLoader.TileMap.FromFile("prova2.tmx");
        }
    }
}
