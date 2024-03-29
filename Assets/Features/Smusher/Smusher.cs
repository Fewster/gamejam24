using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Smusher : MonoBehaviour
{
    public float SmushRate = 1.0f;
    public float SmushRange = 1.0f;

    public Transform Block;
    public AnimationCurve SmushCurve;

    public void Smush()
    {

    }

    private void Update()
    {
        var t = (Time.time * SmushRate) % 1.0f;
        var h = SmushCurve.Evaluate(t) * SmushRange;

        Block.transform.localPosition = new Vector3(0, -h, 0);
    }

    private IEnumerator SmushRoutine()
    {
        var start = Time.time;


        var end = start + SmushRate;

        while (true)
        {
            var now = Time.time;
            var norm = now - start;



            yield return null;
        }
    }
}
