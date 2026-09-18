using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class CaoticEvents : MonoBehaviour
{
    public float SpawnRate = 10.0f;

    public GameObject PrefabToSpawn;

    public Transform SpawnCenter;

    public float RAIO;

    private bool IsEnable;
    
    public float Heigth;

   

    void Start()
    {
        StartCoroutine(SpawnPrefab(PrefabToSpawn));
    }

    IEnumerator SpawnPrefab(GameObject Pref) 
    { 
        IsEnable = true;

        while(IsEnable) 
        {

            float angle = Random.Range(0f, Mathf.PI * 2f);
            float distance = Random.Range(0f, RAIO);

            float PosX = Mathf.Cos(angle) * distance;
            float PosY = Mathf.Sin(angle) * distance;


              
            Vector3 Location = SpawnCenter.position + new Vector3(PosX, Heigth, PosY);

            Debug.DrawLine(Location, Location, Color.red);

            Instantiate(Pref, Location, Quaternion.identity);
            Debug.Log("SpawnPRefab");

            yield return new WaitForSeconds(SpawnRate);
        
        }
    }

    public void StopSpawn() 
    {    
       IsEnable = false;
    }

    void Awake()
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float distance = Random.Range(0f, RAIO);

        float PosX = Mathf.Cos(angle) * distance;
        float PosY = Mathf.Sin(angle) * distance;

        Vector3 A = new Vector3(PosX, PosY, 0);
        Debug.DrawLine(Vector3.zero, A);
    }
}
