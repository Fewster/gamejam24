using Game.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class AgentService : GameService<AgentService>
{
    public int MaxAgents = 65536;
    public ComputeShader Compute = null;

    int PathingComputeKernel;
    int AppendAgentsComputeKernel;
    int ConsumeAgentsComputeKernel;
    ComputeBuffer AgentBuffer;
    ComputeBuffer PathBuffer;
    GraphicsBuffer IndirectArgs;
    ComputeBuffer ConsumeArgsBuffer;
    GraphicsBuffer.IndirectDrawIndexedArgs[] IndirectArgsData;
    const int IndirectArgsCount = 1;

    public Material mat;
    public Mesh mesh;

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
                AgentBuffer.SetCounterValue(0);
                AgentBuffer.Release();
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

        try
        {
            if (ConsumeArgsBuffer != null)
            {
                ConsumeArgsBuffer.Dispose();
            }
        }
        catch { }
    }



    public void InitAgentShader()
    {
        PathingComputeKernel = Compute.FindKernel("CSMain");
        AppendAgentsComputeKernel = Compute.FindKernel("AppendRunner");
        ConsumeAgentsComputeKernel = Compute.FindKernel("ConsumeRunner");

        //Setup buffers
        AgentBuffer = new ComputeBuffer(MaxAgents, AGENT_LAYOUT_SIZE, ComputeBufferType.Append);
        AgentBuffer.SetCounterValue(0);

        ConsumeArgsBuffer = new ComputeBuffer(1, 4);
        ConsumeArgsBuffer.SetData(new int[] { 0 });

        //Bind relevant buffers to programs
        Compute.SetBuffer(PathingComputeKernel, "AgentBuffer", AgentBuffer);
        Compute.SetBuffer(PathingComputeKernel, "AgentConsumeCount", ConsumeArgsBuffer);
        Compute.SetBuffer(AppendAgentsComputeKernel, "AppendBuffer", AgentBuffer);
        Compute.SetBuffer(ConsumeAgentsComputeKernel, "ConsumeBuffer", AgentBuffer);
        Compute.SetBuffer(ConsumeAgentsComputeKernel, "AgentConsumeCount", ConsumeArgsBuffer);

        Compute.SetInt("AgentCount", 0);

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
        Compute.SetBuffer(PathingComputeKernel, "Paths", PathBuffer);
        Compute.SetBuffer(AppendAgentsComputeKernel, "Paths", PathBuffer);
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


    public void SpawnAgents(int count)
    {
        Compute.SetInt("AgentsToAdd", count);

        Compute.GetKernelThreadGroupSizes(AppendAgentsComputeKernel, out uint threadGroupSizeX, out _, out _);
        int threadGroupSize = Mathf.CeilToInt((float)count / threadGroupSizeX);

        Compute.Dispatch(AppendAgentsComputeKernel, threadGroupSize, 1, 1);
    }


    public void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            SpawnAgents(8);
        }

        UpdateAgents();
    }

    public int GetActiveAgentCount()
    {
        ComputeBuffer countBuffer = new ComputeBuffer(1, 4, ComputeBufferType.IndirectArguments);
        ComputeBuffer.CopyCount(AgentBuffer, countBuffer, 0);

        int[] counterResult = new int[1];
        countBuffer.GetData(counterResult, 0, 0, 1);
        countBuffer.Dispose();

        AgentBuffer.SetCounterValue((uint)counterResult[0]);

        return counterResult[0];
    }

    public void UpdateAgents()
    {
        int activeAgents = GetActiveAgentCount();

        if(activeAgents <= 0) 
        {
            return;
        }
        
        Compute.GetKernelThreadGroupSizes(PathingComputeKernel, out uint threadGroupSizeX, out _, out _);
        int threadGroupSize = Mathf.CeilToInt((float)activeAgents / threadGroupSizeX);

        Compute.SetInt("AgentCount", activeAgents);
        Compute.SetFloat("_Time", Time.time);

        //Run pathing logic
        Compute.Dispatch(PathingComputeKernel, threadGroupSize, 1, 1);

        //Dispose agents at end of track
        Compute.Dispatch(ConsumeAgentsComputeKernel, threadGroupSize, 1, 1);
        //Reset consumption args
        ConsumeArgsBuffer.SetData(new int[] { 0 });
        RenderAgents(GetActiveAgentCount());
    }

    public void RenderAgents(int count)
    {
        var renderParams = new RenderParams(mat);
        renderParams.worldBounds = new Bounds(Vector3.zero, 10000 * Vector3.one);
        IndirectArgsData[0].indexCountPerInstance = mesh.GetIndexCount(0);
        IndirectArgsData[0].instanceCount = (uint)count;
        IndirectArgs.SetData(IndirectArgsData);
        Graphics.RenderMeshIndirect(renderParams, mesh, IndirectArgs, IndirectArgsCount);
    }
}
