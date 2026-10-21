using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Splines.Interpolators;

public class RotateToCam : MonoBehaviour
{
    public PopusManager Manager ;
    private RectTransform Recta ;

    private Camera cameratofollow; 

    private TextMeshPro textval;

    public int popvalue;

    private string  beforstringapply; 
    void Start()
    {
        Manager = FindFirstObjectByType<PopusManager>();
        Recta = GetComponent<RectTransform>();
        textval = GetComponent<TextMeshPro>();
    }
    void FixedUpdate()
    {
        
        if(Recta != null){
        SelectCamFollower();
        var direction = cameratofollow.transform.position - Recta.position;
        Recta.rotation =  Quaternion.LookRotation(-direction,-direction);
        textval.SetText($"{beforstringapply}{popvalue}");
        }
    } 

    void SelectCamFollower()
    {
       if(Manager.came1.enabled || Manager.came2.enabled)
        {
            var proxvector = Manager.came1.transform.position - Recta.position;
            var proxvector2 = Manager.came2.transform.position - Recta.position;

            if(proxvector.sqrMagnitude < proxvector2.sqrMagnitude)
            {
                cameratofollow = Manager.came1;
            }
            else
            {
                cameratofollow = Manager.came2;
            }

        }
        else
        {
            cameratofollow = Manager.cameall;
        }



    }

    public void SetValue(int value, string beforstring)
    {
        popvalue = value;
        beforstringapply = beforstring;
        


    }
}
