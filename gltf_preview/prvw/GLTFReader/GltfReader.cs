using SharpGLTF.Schema2;
using System;
using System.Collections.Generic;
using System.Text;

namespace prvw.GLTFReader
{
    internal class GltfReader
    {
        public GltfReader()
        {

        }

        public static ModelRoot LoadGltfFromFile(string path)
        {
            return ModelRoot.Load(path);
            
        }
    }
}
