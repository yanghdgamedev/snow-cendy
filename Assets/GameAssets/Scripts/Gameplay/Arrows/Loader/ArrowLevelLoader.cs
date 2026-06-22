using System.Collections.Generic;
using HDG.Gameplay.Arrows.Controller;
using HDG.Gameplay.Arrows.Model;
using UnityEngine;

namespace HDG.Gameplay.Arrows.Loader
{
    /// <summary>
    /// Loads a level from a TextAsset (JSON) and populates the ArrowsController.
    /// Source format: {"XSize":7,"YSize":6,"Arrows":[{"Indices":[...],"color":"...","colorType":0,"colorIndex":0}]}.
    /// </summary>
    public class ArrowLevelLoader : MonoBehaviour
    {
        [SerializeField] private ArrowsController _arrowsController;
        [SerializeField] private string _resourcesFolder = "Levels";

        public LevelDto LoadFromResources(string levelName)
        {
            var ta = Resources.Load<TextAsset>($"{_resourcesFolder}/{levelName}");
            if (ta == null)
            {
                Debug.LogError($"[ArrowLevelLoader] Level '{levelName}' not found under Resources/{_resourcesFolder}/");
                return null;
            }
            return LoadFromJson(ta.text);
        }

        public LevelDto LoadFromJson(string json)
        {
            var dto = JsonLevelParser.Parse(json);
            if (dto == null || dto.Arrows == null)
            {
                Debug.LogError("[ArrowLevelLoader] Failed to parse level JSON");
                return null;
            }

            var fieldSize = new Vector2Int(dto.XSize, dto.YSize);
            _arrowsController.SetFieldSize(fieldSize);

            foreach (var arrowDto in dto.Arrows)
            {
                if (arrowDto.Indices == null || arrowDto.Indices.Count == 0) continue;

                var tiles = new List<Vector2Int>(arrowDto.Indices.Count);
                for (int i = 0; i < arrowDto.Indices.Count; i++)
                {
                    tiles.Add(JsonLevelParser.IndexToTile(arrowDto.Indices[i], dto.XSize));
                }

                var color = JsonLevelParser.ParseHexColor(arrowDto.color);
                var colorType = (ColorType)arrowDto.colorType;

                // If color is fully transparent (tutorial style), assign a random color
                if (color.a <= 0.01f) color = RandomColor();

                _arrowsController.AddArrow(tiles, color, colorType, arrowDto.colorIndex);
            }

            _arrowsController.NotifyInitialized();
            return dto;
        }

        private static readonly Color[] DefaultPalette =
        {
            new(0.92f, 0.87f, 0.24f), // yellow
            new(0.07f, 0.57f, 0.87f), // blue
            new(0.81f, 0.30f, 0.30f), // red
            new(0.31f, 0.78f, 0.41f), // green
            new(0.83f, 0.52f, 0.92f), // purple
            new(0.98f, 0.65f, 0.16f), // orange
        };

        private int _paletteIndex;
        private Color RandomColor()
        {
            var c = DefaultPalette[_paletteIndex % DefaultPalette.Length];
            _paletteIndex++;
            return c;
        }
    }
}
