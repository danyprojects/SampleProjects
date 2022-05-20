using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BacterioEditor
{
    public class StructureNodeGeneratorObject : ScriptableObject
    {
        public Texture _nodeTexture = null;

        public Bacterio.Databases.StructureDbId _structureDbId = Bacterio.Databases.StructureDbId.None;
        public bool _hasTerritory;
        public Texture2D _structureTexture = null;
    }
}
