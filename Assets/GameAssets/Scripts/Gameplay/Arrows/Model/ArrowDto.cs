using System;
using System.Collections.Generic;

namespace HDG.Gameplay.Arrows.Model
{
    /// <summary>
    /// Mirror of the JSON arrow data baked into level prefabs.
    /// Indices is row-major: index = y * XSize + x.
    /// </summary>
    [Serializable]
    public class ArrowDto
    {
        public List<int> Indices;
        public string color;       // hex RGBA, e.g. "EBDE3DFF"
        public int colorType;      // 0=Random, 1=Custom, 2=Any
        public int colorIndex;
        public bool isCollected;
        public bool isClicked;
    }
}
