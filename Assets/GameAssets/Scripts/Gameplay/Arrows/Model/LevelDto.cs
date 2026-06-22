using System;
using System.Collections.Generic;

namespace HDG.Gameplay.Arrows.Model
{
    [Serializable]
    public class LevelDto
    {
        public int XSize;
        public int YSize;
        public List<ArrowDto> Arrows;
    }
}
