using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.Distributions;

public class Test : MonoBehaviour
{
    public double GaussianRandom(float m, float sd)
    {
        var a = new Normal(m, sd);
        double b = a.Sample();
        Debug.Log(b);
        return b;
    }

    public void test()
    {
        Debug.Log(Random.value);
    }
}
