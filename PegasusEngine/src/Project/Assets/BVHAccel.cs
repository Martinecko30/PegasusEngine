using System.Numerics;
using System.Runtime.InteropServices;

namespace PegasusEngine.Project.Assets;

public class BVHAccel
{
    [StructLayout(LayoutKind.Sequential, Pack = 16)]
    public struct Node
    {
        public Vector3 Min;
        public uint LeftChildOrFirstTri;
        public Vector3 Max;
        public uint TriCount;
    }
    
    public struct Aabb
    {
        public Vector3 BoxMin;
        public Vector3 BoxMax;
        
        public Aabb(Vector3 min, Vector3 max)
        {
            BoxMin = min;
            BoxMax = max;
        }
        
        public static Aabb Empty => new Aabb(new Vector3(float.MaxValue), new Vector3(float.MinValue));

        public void Grow(Vector3 p)
        {
            BoxMin = Vector3.Min(BoxMin, p);
            BoxMax = Vector3.Max(BoxMax, p);
        }

        public void Grow(Aabb aabb)
        {
            BoxMin = Vector3.Min(BoxMin, aabb.BoxMin);
            BoxMax = Vector3.Max(BoxMax, aabb.BoxMax);
        }

        public float Area()
        {
            Vector3 s = BoxMax - BoxMin;
            return (s.X * s.Y) + (s.Y * s.Z) + (s.Z * s.X);
        }
    }

    private struct Bin
    {
        public Aabb Aabb;
        public int TriCount;
    }

    private readonly Triangle[] _triBuff;
    private readonly uint _firstTriIdx;
    private readonly uint _triCount;
    private readonly Vector3[] _centroids;

    private uint _nodesUsed;
    private Node[] _nodeBuff;
    private uint[] _idxBuff;

    public BVHAccel(Triangle[] meshBuffer, uint firstTriIdx, uint triCount)
    {
        _triBuff = meshBuffer;
        _firstTriIdx = firstTriIdx;
        _triCount = triCount;
        _centroids = PrecomputeCentroids();
    }
    
    public BVHAccel(List<Triangle> meshBuffer, uint firstTriIdx, uint triCount) : this(meshBuffer.ToArray(), firstTriIdx, triCount) {}

    public void Build(
        List<Node> nodeBuffer,
        List<uint> indexBuffer,
        out uint firstNodeIdx,
        out uint nodeCount)
    {
        int n = (int)_triCount;
        int oldNodeSize = nodeBuffer.Count;
        firstNodeIdx = (uint)oldNodeSize;
        
        for (int i = 0; i < 2 * n - 1; i++)
            nodeBuffer.Add(new Node());
        
        for (int i = 0; i < n; i++)
            indexBuffer.Add(0);
        
        _nodeBuff = nodeBuffer.ToArray();
        _idxBuff = indexBuffer.ToArray();

        for (uint i = 0; i < n; i++) 
            _idxBuff[_firstTriIdx + i] = i;

        _nodesUsed = 0;
        ref Node root = ref _nodeBuff[firstNodeIdx + _nodesUsed++];
        root.LeftChildOrFirstTri = _firstTriIdx;
        root.TriCount = (uint)n;

        UpdateAABB(ref root);
        SubDivide(firstNodeIdx, 0);
        
        nodeCount = _nodesUsed;
        
        nodeBuffer.Clear();
        nodeBuffer.AddRange(_nodeBuff.Take((int)(firstNodeIdx + _nodesUsed)));
        indexBuffer.Clear();
        indexBuffer.AddRange(_idxBuff);
    }

    private void UpdateAABB(ref Node node)
    {
        Aabb aabb = Aabb.Empty;
        for (uint i = 0; i < node.TriCount; i++)
        {
            uint triIdx = _idxBuff[node.LeftChildOrFirstTri + i];
            Triangle t = _triBuff[_firstTriIdx + triIdx];
            aabb.Grow(t.V0.AsVector3());
            aabb.Grow(t.V1.AsVector3());
            aabb.Grow(t.V2.AsVector3());
        }
        node.Min = aabb.BoxMin;
        node.Max = aabb.BoxMax;
    }

    private float FindBestSplitPlane(ref Node node, out int splitAxis, out float splitPos)
    {
        const int BINS = 8;
        splitAxis = -1;
        splitPos = 0;
        float bestCost = float.MaxValue;

        for (int axis = 0; axis < 3; axis++)
        {
            float aabbMin = float.MaxValue;
            float aabbMax = float.MinValue;

            for (int i = 0; i < node.TriCount; i++)
            {
                float val = GetAxis(_centroids[_idxBuff[node.LeftChildOrFirstTri + i]], axis);
                aabbMin = Math.Min(aabbMin, val);
                aabbMax = Math.Max(aabbMax, val);
            }
            
            if (aabbMin == aabbMax) continue;
            
            Bin[] bins = new Bin[BINS];
            for (int i = 0; i < BINS; i++)
                bins[i].Aabb = Aabb.Empty;

            float sacle = BINS / (aabbMax - aabbMin);
            for (uint i = 0; i < node.TriCount; i++)
            {
                uint triIdx = _idxBuff[node.LeftChildOrFirstTri + i];
                int binIdx = Math.Min(BINS - 1, (int)((GetAxis(_centroids[triIdx], axis) - aabbMin) * sacle));
                bins[binIdx].TriCount++;
                var tri = _triBuff[_firstTriIdx + triIdx];
                bins[binIdx].Aabb.Grow(tri.V0.AsVector3());
                bins[binIdx].Aabb.Grow(tri.V1.AsVector3());
                bins[binIdx].Aabb.Grow(tri.V2.AsVector3());
            }

            float[] leftArea = new float[BINS - 1];
            float[] rightArea = new float[BINS - 1];
            int[] leftCount = new int[BINS - 1];
            int[] rightCount = new int[BINS - 1];
            
            int currLeftCount = 0, currRightCount = 0;
            Aabb currLeftAabb = Aabb.Empty, currRightAabb = Aabb.Empty;

            for (int i = 0; i < BINS - 1; i++)
            {
                currLeftCount += bins[i].TriCount;
                currLeftAabb.Grow(bins[i].Aabb);
                leftArea[i] = currLeftAabb.Area();
                leftCount[i] = currLeftCount;
                
                currRightCount += bins[BINS - 1 - i].TriCount;
                currRightAabb.Grow(bins[BINS - 1 - i].Aabb);
                rightArea[BINS - 2 - i] = currRightAabb.Area();
                rightCount[BINS - 2 - i] = currRightCount;
            }

            float binWidth = (aabbMax - aabbMin) / BINS;
            for (int i = 0; i < BINS - 1; i++)
            {
                float cost = leftCount[i] * leftArea[i] + rightCount[i] * rightArea[i];
                if (cost < bestCost)
                {
                    splitAxis = axis;
                    splitPos = aabbMin + binWidth * (i + 1);
                    bestCost = cost;
                }
            }
        }
        return bestCost;
    }

    private void SubDivide(uint firstNodeIdx, uint nodeOffset)
    {
        uint currentNodeIdx = firstNodeIdx + nodeOffset;
        int bestAxis;
        float bestPos;
        float bestCost = FindBestSplitPlane(ref _nodeBuff[currentNodeIdx], out bestAxis, out bestPos);

        Aabb parentAabb = new Aabb(_nodeBuff[currentNodeIdx].Min, _nodeBuff[currentNodeIdx].Max);
        float parentCost = _nodeBuff[currentNodeIdx].TriCount * parentAabb.Area();

        if (bestCost >= parentCost)
            return;

        uint leftPtr = _nodeBuff[currentNodeIdx].LeftChildOrFirstTri;
        uint rightPtr = _nodeBuff[currentNodeIdx].LeftChildOrFirstTri + _nodeBuff[currentNodeIdx].TriCount - 1;

        while (leftPtr < rightPtr)
        {
            if (GetAxis(_centroids[_idxBuff[leftPtr]], bestAxis) < bestPos)
                leftPtr++;
            else
                Swap(leftPtr, rightPtr--);
        }
        uint leftTriCount = leftPtr - _nodeBuff[currentNodeIdx].LeftChildOrFirstTri;
        if (leftTriCount == 0 || leftTriCount == _nodeBuff[currentNodeIdx].TriCount)
            return;
        
        uint leftChildIdx = _nodesUsed++;
        uint rightChildIdx = _nodesUsed++;

        _nodeBuff[firstNodeIdx + leftChildIdx].LeftChildOrFirstTri = _nodeBuff[currentNodeIdx].LeftChildOrFirstTri;
        _nodeBuff[firstNodeIdx + leftChildIdx].TriCount = leftTriCount;

        _nodeBuff[firstNodeIdx + rightChildIdx].LeftChildOrFirstTri = _nodeBuff[currentNodeIdx].LeftChildOrFirstTri + leftTriCount;
        _nodeBuff[firstNodeIdx + rightChildIdx].TriCount = _nodeBuff[currentNodeIdx].TriCount - leftTriCount;

        _nodeBuff[currentNodeIdx].TriCount = 0;
        _nodeBuff[currentNodeIdx].LeftChildOrFirstTri = leftChildIdx;

        UpdateAABB(ref _nodeBuff[firstNodeIdx + leftChildIdx]);
        UpdateAABB(ref _nodeBuff[firstNodeIdx + rightChildIdx]);

        SubDivide(firstNodeIdx, leftChildIdx);
        SubDivide(firstNodeIdx, rightChildIdx);
    }
    
    private Vector3[] PrecomputeCentroids()
    {
        Vector3[] centroids = new Vector3[_triCount];
        for (int i = 0; i < _triCount; i++)
        {
            Triangle t = _triBuff[_firstTriIdx + i];
            centroids[i] = (t.V0.AsVector3() + t.V1.AsVector3() + t.V2.AsVector3()) * 0.33333333f;
        }
        return centroids;
    }

    private void Swap(uint idx1, uint idx2)
    {
        uint tmp = _idxBuff[idx1];
        _idxBuff[idx1] = _idxBuff[idx2];
        _idxBuff[idx2] = tmp;
    }

    private float GetAxis(Vector3 v, int axis)
    {
        return axis switch { 0 => v.X, 1 => v.Y, 2 => v.Z, _ => 0 };
    }
}