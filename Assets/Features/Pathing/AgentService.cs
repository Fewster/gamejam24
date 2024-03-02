using Game.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class AgentService : GameService<AgentService>
{
    public int MaxAgents = 1024;
    public ComputeShader Compute = null;

    int Kernel;
    ComputeBuffer AgentBuffer;
    ComputeBuffer PathBuffer;
    GraphicsBuffer IndirectArgs;
    GraphicsBuffer.IndirectDrawIndexedArgs[] IndirectArgsData;
    const int IndirectArgsCount = 1;

    public Material mat;
    public Mesh mesh;

    [StructLayout(LayoutKind.Sequential)]
    public struct AgentLayout
    {
        public Vector3 Position;
        public float Hash;
        public int Track;
        public int Goal;
    }

    const int AGENT_LAYOUT_SIZE = sizeof(float) * (3 + 1) + sizeof(int) * (2);

    [StructLayout(LayoutKind.Sequential)]
    public struct PathLayout
    {
        public int PointCount;
        public Vector3 Points0;
        public Vector3 Points1;
        public Vector3 Points2;
        public Vector3 Points3;
        public Vector3 Points4;
        public Vector3 Points5;
        public Vector3 Points6;
        public Vector3 Points7;
    }

    const int PATH_LAYOUT_SIZE = sizeof(float) * (3 * 8) + sizeof(int) * (1);

    List<PathLayout> Paths = new List<PathLayout>(8) 
    { 
        new PathLayout(),
        new PathLayout(),
        new PathLayout(),
        new PathLayout(),
        new PathLayout(),
        new PathLayout(),
        new PathLayout(),
        new PathLayout()
    };

    protected override void OnSetup()
    {
        base.OnSetup();

        InitAgentShader();
    }

    protected override void OnCleanup()
    {
        base.OnCleanup();

        try
        {
            if (AgentBuffer != null)
            {
                AgentBuffer.Dispose();
            }
        }
        catch { }

        try
        {
            if (PathBuffer != null)
            {
                PathBuffer.Dispose();
            }
        }
        catch { }

        try
        {
            if (IndirectArgs != null)
            {
                IndirectArgs.Dispose();
            }
        }
        catch { }
    }



    public void InitAgentShader()
    {
        Kernel = Compute.FindKernel("CSMain");

        List<AgentLayout> agents = new List<AgentLayout>(MaxAgents);
        for(int i = 0; i < MaxAgents; i++)
        {
            var randomisedPosition = new Vector3(Random.Range(-1f,1f), Random.Range(-1f,1f), Random.Range(-1f,1f)) * 1;
            agents.Add(new AgentLayout { Position = randomisedPosition, Goal = 0, Hash = Random.Range(0f, 1f), Track = Random.Range(0,2)});
        }

        AgentBuffer = new ComputeBuffer(agents.Count, AGENT_LAYOUT_SIZE);
        AgentBuffer.SetData(agents);

        Compute.SetBuffer(Kernel, "AgentBuffer", AgentBuffer);
        Compute.SetInt("AgentCount", MaxAgents);

        //Path setup
        UpdatePaths();

        //Args for drawing
        IndirectArgs = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, IndirectArgsCount, GraphicsBuffer.IndirectDrawIndexedArgs.size);
        IndirectArgsData = new GraphicsBuffer.IndirectDrawIndexedArgs[IndirectArgsCount];

        mat.SetBuffer("AgentBuffer", AgentBuffer);
    }

    private void UpdatePaths()
    {
        if(PathBuffer == null)
        {
            PathBuffer = new ComputeBuffer(8, PATH_LAYOUT_SIZE);
        }

        PathBuffer.SetData(Paths);
        Compute.SetBuffer(Kernel, "Paths", PathBuffer);
    }


    public void RegisterPath(int pathIndex, Path path)
    {
        var pointCount = path.transform.childCount;

        if (pathIndex >= Paths.Count)
        {
            Debug.LogError("Cannot add path outside of path limit");
            return;
        }

        Paths[pathIndex] = new PathLayout()
        {
            PointCount = pointCount,
            Points0 = 0 < pointCount ? path.transform.GetChild(0).position : Vector3.zero,
            Points1 = 1 < pointCount ? path.transform.GetChild(1).position : Vector3.zero,
            Points2 = 2 < pointCount ? path.transform.GetChild(2).position : Vector3.zero,
            Points3 = 3 < pointCount ? path.transform.GetChild(3).position : Vector3.zero,
            Points4 = 4 < pointCount ? path.transform.GetChild(4).position : Vector3.zero,
            Points5 = 5 < pointCount ? path.transform.GetChild(5).position : Vector3.zero,
            Points6 = 6 < pointCount ? path.transform.GetChild(6).position : Vector3.zero,
            Points7 = 6 < pointCount ? path.transform.GetChild(7).position : Vector3.zero,
        };

        UpdatePaths();
    }

    

    public void Update()
    {
        UpdateAgents();
    }

    public void UpdateAgents()
    {
        Compute.SetFloat("_Time", Time.time);
        Compute.GetKernelThreadGroupSizes(Kernel, out uint threadGroupSizeX, out _, out _);
        int threadGroupSize = Mathf.CeilToInt((float)MaxAgents / threadGroupSizeX);

        Compute.Dispatch(Kernel, threadGroupSize, 1, 1);

        var renderParams = new RenderParams(mat);
        renderParams.worldBounds = new Bounds(Vector3.zero, 10000 * Vector3.one);
        IndirectArgsData[0].indexCountPerInstance = mesh.GetIndexCount(0);
        IndirectArgsData[0].instanceCount = (uint)MaxAgents;
        
        IndirectArgs.SetData(IndirectArgsData);
        Graphics.RenderMeshIndirect(renderParams, mesh, IndirectArgs, IndirectArgsCount);
    }
}
