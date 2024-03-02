using Game.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleBehaviour : GameBehaviour
{
    public ExampleService exampleService;

    protected override void OnSetup()
    {
        exampleService = Resolver.Resolve<ExampleService>();
    }
}
