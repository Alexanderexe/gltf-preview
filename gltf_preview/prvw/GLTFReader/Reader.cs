using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using SharpGLTF.Schema2;
using SharpGLTF.Transforms;
using System.Drawing;

namespace prvw.GLTFReader
{
    public class Renderer
    {
        public float[] Vertices;
        public uint[] Indices;

        public void Render(string path)
        {
            var model = GltfReader.LoadGltfFromFile(path);
            var nodes = new List<Node>();
            nodes.AddRange(model.LogicalNodes);
            var nodesWithGeometry = nodes.Where(n => n.Mesh is object).ToList();
            var NodeTransforms = CollectNodesTransforms(nodesWithGeometry, nodes);
            var ElementMeshPrimitives = GetNodePrimitives(nodesWithGeometry, 1, NodeTransforms);

            var Ys = new List<float>();
            var Zs = new List<float>();
            var Xs = new List<float>();
            var TexCordXs = new List<float>();
            var TexCordYs = new List<float>();
            var indices = new List<uint>();

            foreach (var item in ElementMeshPrimitives)
            {
                Ys.AddRange(item.Ys);
                Zs.AddRange(item.Zs);
                Xs.AddRange(item.Xs);
                TexCordXs.AddRange(item.TexCoordXs);
                TexCordYs.AddRange(item.TexCoordYs);
                indices.AddRange(item.Indices);
            }

            var listVertices = new List<float>();
            for (int i = 0; i < Xs.Count; i++)
            {
                listVertices.Add(Xs[i]);
                listVertices.Add(Ys[i]);
                listVertices.Add(Zs[i]);
                listVertices.Add(TexCordXs[i]);
                listVertices.Add(TexCordYs[i]);
            }
            Vertices = listVertices.ToArray();
            Indices = indices.ToArray();


        }

        public static Dictionary<int, List<AffineTransform>> CollectNodesTransforms(List<Node> nodesWithGeometry, List<Node> nodes)
        {
            var NodeTransformsData = new Dictionary<int, List<AffineTransform>>();

            foreach (var node in nodesWithGeometry)
            {
                var transformations = new List<AffineTransform>();
                var currentNode = node;
                transformations.Add(currentNode.LocalTransform);
                while (currentNode.VisualParent is object)
                {
                    transformations.Add(currentNode.VisualParent.LocalTransform);
                    currentNode = nodes[currentNode.VisualParent.LogicalIndex];
                }
                NodeTransformsData.Add(node.LogicalIndex, transformations);
            }
            return NodeTransformsData;
        }

        static List<Element> GetNodePrimitives(List<Node> nodes, int index, Dictionary<int, List<AffineTransform>> NodeTransforms)
        {
            var resultList = new List<Element>();
            foreach (var node in nodes)
            {
                var nodePrimitives = new List<MeshPrimitive>();
                foreach (var primitive in node.Mesh.Primitives)
                {
                    nodePrimitives.Add(primitive);
                }
                var nodeIndex = node.LogicalIndex;
                resultList.Add(new Element(nodeIndex, index, nodePrimitives, node.Name, NodeTransforms[nodeIndex]));
            }
            return resultList;
        }
    }

    internal class Element
    {
        public int NodeIndex;
        public string NodeName;
        public int ModelIndex;
        public List<MeshPrimitive> Primitives;
        public List<Vector3> PositionVectors;
        public List<float> Xs = new List<float>();
        public List<float> Ys = new List<float>();
        public List<float> Zs = new List<float>();
        public List<float> TexCoordXs = new List<float>();
        public List<float> TexCoordYs = new List<float>();
        public new List<uint> Indices = new List<uint>();

        public Element(int nodeIndex, int modelIndex, List<MeshPrimitive> primitives, string nodeName, List<AffineTransform> transforms)
        {
            Primitives = primitives;
            NodeIndex = nodeIndex;
            NodeName = nodeName;
            ModelIndex = modelIndex;

            PositionVectors = new List<Vector3>();
            foreach (var primitive in Primitives)
            {
                var positionAccessor = primitive.VertexAccessors["POSITION"];
                var texCordAccessor = primitive.VertexAccessors["TEXCOORD_0"];
                foreach (var vector in texCordAccessor.AsVector2Array())
                {

                    TexCoordXs.Add(vector.X);
                    TexCoordYs.Add(vector.Y);

                }

                var accessorVector = Transformation.TransformAccessor(positionAccessor, transforms);
                var indices = primitive.IndexAccessor;
                Indices = indices.AsIndicesArray().ToList();
                PositionVectors.AddRange(accessorVector);
                foreach (var vector in PositionVectors)
                {
                    Xs.Add(vector.X);
                    Ys.Add(vector.Y);
                    Zs.Add(vector.Z);
                }
            }



        }
    }
}
