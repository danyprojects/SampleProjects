using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bacterio.Game
{
    public sealed class MapGrid
    {
        public Vector2 Dimensions { get; private set; }

        public MapGrid()
        {
            Dimensions = new Vector2(Constants.SMALL_MAP_DIMENSIONS, Constants.SMALL_MAP_DIMENSIONS);


        }
    }
}
