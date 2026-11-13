using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TestData", menuName = "Scriptable Objects/TestData")]
public class TestData : ScriptableObject
{
    public int num_a;
    public int num_b;
    public string str_a;
}
