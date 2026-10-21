using UnityEngine;
using System.Collections.Generic;
using System;
public class TesteDeLinha : MonoBehaviour
{
    public LineRenderer line;
    private List<Vector3> vector3array;
    void Start()
    {
        line = GetComponent<LineRenderer>();
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var players = GameObject.FindGameObjectsWithTag("Player");
        vector3array.Clear();
        foreach(var pl in players)
        {
           vector3array.Add(pl.transform.position);
        }
        line.SetPositions(vector3array.ToArray());
    }
}
