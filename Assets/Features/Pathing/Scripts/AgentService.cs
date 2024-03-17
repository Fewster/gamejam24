using Game.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;

public class RouteCompleteArgs
{
    public int RouteId;
    public int AgentAmount;
}

public class AgentService : GameService<AgentService>
{
    public int MaxAgents = 65536;
    public ComputeShader Compute = null;

    //Invoked whenever the compute shader has detected that a route has been completed by agent(s)
    public UnityEvent<RouteCompleteArgs> OnRouteComplete;

    int PathingComputeKernel;
    int ForceComputeKernel;
    int AppendAgentsComputeKernel;
    int PrepCullComputeKernel;
    int CullComputeKernel;

    ComputeBuffer AgentBuffer;
    ComputeBuffer AgentCopyBuffer;
    ComputeBuffer PathBuffer;
    ComputeBuffer ComputeInfoIndirectArgs;
    ComputeBuffer CullIndexerIndirectArgs;

    //Rendering
    GraphicsBuffer IndirectDrawingArgs;
    GraphicsBuffer.IndirectDrawIndexedArgs[] DrawingArgsData;
    public Material mat;
    public Mesh mesh;

    //Path management
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


    [StructLayout(LayoutKind.Sequential)]
    public struct AgentLayout
    {
        public Vector2 Position;
        public Vector2 velocity;
        public float Hash;
        public int Track;
        public int Goal;
    }

    const int AGENT_LAYOUT_SIZE = sizeof(float) * (4 + 1) + sizeof(int) * (2);

    [StructLayout(LayoutKind.Sequential)]
    public struct PathLayout
    {
        public int PointCount;
        public Vector2 Points0;
        public Vector2 Points1;
        public Vector2 Points2;
        public Vector2 Points3;
        public Vector2 Points4;
        public Vector2 Points5;
        public Vector2 Points6;
        public Vector2 Points7;
    }

    const int PATH_LAYOUT_SIZE = sizeof(float) * (2 * 8) + sizeof(int) * (1);


    protected override void OnSetup()
    {
        base.OnSetup();

        InitShaders();
    }

    protected override void OnCleanup()
    {

        base.OnCleanup();

        try
        {
            if (AgentBuffer != null)
            {
                AgentBuffer.Release();
                AgentBuffer.Dispose();
            }
        }
        catch { }
        
        try
        {
            if (AgentCopyBuffer != null)
            {
                AgentCopyBuffer.Release();
                AgentCopyBuffer.Dispose();
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
            if (IndirectDrawingArgs != null)
            {
                IndirectDrawingArgs.Dispose();
            }
        }
        catch { }

        try
        {
            if (ComputeInfoIndirectArgs != null)
            {
                ComputeInfoIndirectArgs.Dispose();
            }
        }
        catch { }
        
        try
        {
            if (CullIndexerIndirectArgs != null)
            {
                CullIndexerIndirectArgs.Dispose();
            }
        }
        catch { }
    }



    private void InitShaders()
    {
        //Setup Kernel hooks
        PathingComputeKernel = Compute.FindKernel("CSMain");
        ForceComputeKernel = Compute.FindKernel("ForceCalculator");
        AppendAgentsComputeKernel = Compute.FindKernel("SpawnAgents");
        PrepCullComputeKernel = Compute.FindKernel("PrepareCull");
        CullComputeKernel = Compute.FindKernel("CullAgents");

        //Setup buffers
        AgentBuffer = new ComputeBuffer(MaxAgents, AGENT_LAYOUT_SIZE);
        AgentCopyBuffer = new ComputeBuffer(MaxAgents, AGENT_LAYOUT_SIZE);
        ComputeInfoIndirectArgs = new ComputeBuffer(1, 4 * 10, ComputeBufferType.IndirectArguments);
        ComputeInfoIndirectArgs.SetData(new int[] { 0, 0,
                                                    0, 0, 0, 0, 0, 0, 0, 0});//Hardcoded to 8 paths
        CullIndexerIndirectArgs = new ComputeBuffer(1, 4, ComputeBufferType.IndirectArguments);
        CullIndexerIndirectArgs.SetData(new int[] { 0 });


        //Bind relevant buffers to programs
        // - CSMain
        Compute.SetBuffer(PathingComputeKernel, "ComputeInfo", ComputeInfoIndirectArgs);
        Compute.SetBuffer(PathingComputeKernel, "AgentBuffer", AgentBuffer);

        // - Force
        Compute.SetBuffer(ForceComputeKernel, "ComputeInfo", ComputeInfoIndirectArgs);
        Compute.SetBuffer(ForceComputeKernel, "AgentBuffer", AgentBuffer);

        // - Prepare Cull
        Compute.SetBuffer(PrepCullComputeKernel, "ComputeInfo", ComputeInfoIndirectArgs);
        Compute.SetBuffer(PrepCullComputeKernel, "CullIndexer", CullIndexerIndirectArgs);
        Compute.SetBuffer(PrepCullComputeKernel, "AgentBuffer", AgentBuffer);
        Compute.SetBuffer(PrepCullComputeKernel, "CleanAgentBuffer", AgentCopyBuffer);

        // - Cull Agents
        Compute.SetBuffer(CullComputeKernel, "ComputeInfo", ComputeInfoIndirectArgs);
        Compute.SetBuffer(CullComputeKernel, "AgentBuffer", AgentBuffer);
        Compute.SetBuffer(CullComputeKernel, "CleanAgentBuffer", AgentCopyBuffer);

        // - Append agents
        Compute.SetBuffer(AppendAgentsComputeKernel, "ComputeInfo", ComputeInfoIndirectArgs);
        Compute.SetBuffer(AppendAgentsComputeKernel, "AgentBuffer", AgentBuffer);

        UpdatePaths();

        Compute.SetInt("AgentsToAdd", 0);

        //Args for drawing
        IndirectDrawingArgs = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 1, GraphicsBuffer.IndirectDrawIndexedArgs.size);
        DrawingArgsData = new GraphicsBuffer.IndirectDrawIndexedArgs[1];
        mat.SetBuffer("AgentBuffer", AgentBuffer);
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

    private void UpdatePaths()
    {
        if(PathBuffer == null)
        {
            PathBuffer = new ComputeBuffer(8, PATH_LAYOUT_SIZE);
        }

        PathBuffer.SetData(Paths);
        Compute.SetBuffer(PathingComputeKernel, "Paths", PathBuffer);
        Compute.SetBuffer(ForceComputeKernel, "Paths", PathBuffer);
        Compute.SetBuffer(AppendAgentsComputeKernel, "Paths", PathBuffer);
    }


    public void SpawnAgents(int count)
    {
        Compute.SetInt("AgentsToAdd", count);

        Compute.GetKernelThreadGroupSizes(AppendAgentsComputeKernel, out uint threadGroupSizeX, out _, out _);
        int threadGroupSize = Mathf.CeilToInt((float)count / threadGroupSizeX);

        Compute.Dispatch(AppendAgentsComputeKernel, threadGroupSize, 1, 1);
    }


    public void Update()
    {
        UpdateAgents();
    }

    public int GetActiveAgentCount()
    {
        int[] counterResult = new int[10];
        ComputeInfoIndirectArgs.GetData(counterResult, 0, 0, 10);
        return counterResult[0];
    }

    public int GetDestructionQueueSize()
    {
        int[] counterResult = new int[10];
        ComputeInfoIndirectArgs.GetData(counterResult, 0, 0, 10);
        return counterResult[1];
    }

    private void UpdateAgents()
    {
        Compute.SetFloat("_DeltaTime", Time.deltaTime);
        Compute.SetFloat("_Time", Time.time);
        int activeAgents = GetActiveAgentCount();

        if(activeAgents <= 0) 
        {
            return;
        }
        
        Compute.GetKernelThreadGroupSizes(PathingComputeKernel, out uint threadGroupSizeX, out _, out _);
        int threadGroupSize = Mathf.CeilToInt((float)activeAgents / threadGroupSizeX);

        //Run pathing logic
        Compute.Dispatch(ForceComputeKernel, threadGroupSize, 1, 1);
        Compute.Dispatch(PathingComputeKernel, threadGroupSize, 1, 1);

        var destrutionQueueSize = GetDestructionQueueSize();
        if (destrutionQueueSize > 0)
        {
            CullAgents(activeAgents, destrutionQueueSize); 
        }

        RenderAgents(GetActiveAgentCount());
    }

    private void CullAgents(int activeAgentCount, int markedForCullCount)
    {
        Compute.GetKernelThreadGroupSizes(CullComputeKernel, out uint threadGroupSizeX, out _, out _);
        int threadGroupSize = Mathf.CeilToInt((float)activeAgentCount / threadGroupSizeX);

        Compute.Dispatch(PrepCullComputeKernel, threadGroupSize, 1, 1);
        Compute.Dispatch(CullComputeKernel, threadGroupSize, 1, 1);

        int[] counterResult = new int[10];
        ComputeInfoIndirectArgs.GetData(counterResult, 0, 0, 10);

        for(int i = 0; i < 8; i++)
        {
            if (counterResult[2 + i] != 0)
            {
                OnRouteComplete.Invoke(new RouteCompleteArgs
                {
                    AgentAmount = counterResult[2 + i],
                    RouteId = i
                });
            }
        }



        ComputeInfoIndirectArgs.SetData(new int[] { activeAgentCount - markedForCullCount, 0, 
                                                   0,0,0,0,0,0,0,0 });// Hardcoded to 8 routes
        CullIndexerIndirectArgs.SetData(new int[] { 0 });
    }

    private void RenderAgents(int count)
    {
        var renderParams = new RenderParams(mat);
        renderParams.worldBounds = new Bounds(Vector3.zero, 10000 * Vector3.one);
        DrawingArgsData[0].indexCountPerInstance = mesh.GetIndexCount(0);
        DrawingArgsData[0].instanceCount = (uint)count;
        IndirectDrawingArgs.SetData(DrawingArgsData);
        Graphics.RenderMeshIndirect(renderParams, mesh, IndirectDrawingArgs, 1);
    }
}
