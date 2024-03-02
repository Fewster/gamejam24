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
        public Vector3 Velocity;
        public int Track;
    }

    const int AGENT_LAYOUT_SIZE = sizeof(float) * (3 + 1 + 3) + sizeof(int) * (1);


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
            if (IndirectArgs != null)
            {
                IndirectArgs.Dispose();
            }
        }
        catch { }
    }



    public void InitAgentShader()
    {
        //Kernel = Compute.FindKernel("CSMain");

        List<AgentLayout> agents = new List<AgentLayout>(MaxAgents);

        for(int i = 0; i < MaxAgents; i++)
        {
            var randomisedPosition = new Vector3(Random.Range(-1f,1f), Random.Range(-1f,1f), Random.Range(-1f,1f)) * 100;
            agents.Add(new AgentLayout { Position = randomisedPosition, Velocity = Vector3.up, Hash = Random.Range(0f, 1f), Track = i});
        }

        AgentBuffer = new ComputeBuffer(agents.Count, AGENT_LAYOUT_SIZE);
        AgentBuffer.SetData(agents);


        //Compute.SetBuffer(Kernel, "AgentBuffer", AgentBuffer);
        //Compute.SetInt("AgentCount", MaxAgents);


        IndirectArgs = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, IndirectArgsCount, GraphicsBuffer.IndirectDrawIndexedArgs.size);
        IndirectArgsData = new GraphicsBuffer.IndirectDrawIndexedArgs[IndirectArgsCount];

        mat.SetBuffer("AgentBuffer", AgentBuffer);
    }

    public void Update()
    {
        UpdateAgents();
    }

    public void UpdateAgents()
    {
        //Compute.GetKernelThreadGroupSizes(Kernel, out uint threadGroupSizeX, out _, out _);
        //int threadGroupSize = Mathf.CeilToInt((float)MaxAgents / threadGroupSizeX);

        //Compute.Dispatch(Kernel, threadGroupSize, 1, 1);

        var renderParams = new RenderParams(mat);
        renderParams.worldBounds = new Bounds(Vector3.zero, 10000 * Vector3.one);
        IndirectArgsData[0].indexCountPerInstance = mesh.GetIndexCount(0);
        IndirectArgsData[0].instanceCount = (uint)MaxAgents;
        
        IndirectArgs.SetData(IndirectArgsData);
        Graphics.RenderMeshIndirect(renderParams, mesh, IndirectArgs, IndirectArgsCount);
    }
}
