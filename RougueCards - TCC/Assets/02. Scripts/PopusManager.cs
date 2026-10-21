
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;


public class PopusManager : MonoBehaviour
{   

    public PopupsClass[] allpopups;
    public Camera cameall;
    public Camera came1;
    public Camera came2;

   
     public void POPUP(Vector3 transform, int value, int prefabNumber)
    {
        var classepop = allpopups[prefabNumber];
        if(classepop.existentpopups.Count >= classepop.Limit)
        {
            if(classepop.existentpopups.Contains(classepop.existentpopups[Random.Range(0,classepop.Limit+1)])){
                
                classepop.existentpopups[Random.Range(0,classepop.Limit+1)].transform.position = transform;
                classepop.existentpopups[Random.Range(0,classepop.Limit+1)].GetComponent<Animation>().Play();
                // Set value define o valor do popup em ROTATE CAM
                classepop.existentpopups[Random.Range(0,classepop.Limit+1)].GetComponent<RotateToCam>().SetValue(value,classepop.BeforSimble);
                }
        }
        else
        {
            var objeto = Instantiate(classepop.popupObject, transform + Vector3.up * classepop.GroundDistance, Quaternion.identity);
            classepop.existentpopups.Add(objeto);
            objeto.GetComponent<RotateToCam>().SetValue(value,classepop.BeforSimble);
            Debug.LogWarning($"{classepop.existentpopups.Count}");
            
        }


}}

    

