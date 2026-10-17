using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class PopusManager : MonoBehaviour
{   


    public GameObject popupObject;
    
    public List<GameObject> existentpopups;

    public int Limit;

    public Camera cameall;
    public Camera came1;
    public Camera came2;

    //[SerializeField]
    //private UnityEvent<Vector3, int> popupevent;


    public float GroundDistance = 1.5f; 

    
    void Start()
    {
        //popupevent.AddListener(POPUP);
    }
    public void POPUP(Vector3 transform, int value)
    {
        
        if(existentpopups.Count >= Limit)
        {
            if(existentpopups.Contains(existentpopups[0])){
              
                existentpopups[0].transform.position = transform;
                existentpopups[0].GetComponent<Animation>().Play();
                existentpopups[0].GetComponent<RotateToCam>().SetValue(value);
                existentpopups.RemoveAt(0);
                }
        }
        else
        {
            var objeto = Instantiate(popupObject, transform + Vector3.up * GroundDistance, Quaternion.identity);
            existentpopups.Add(objeto);
            objeto.GetComponent<RotateToCam>().SetValue(value);
            Debug.LogWarning($"{existentpopups.Count}");
            
        }

    }

    
}
