using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]

public class RTCameraController : MonoBehaviour
{

    public Material grassMaterial;  
    public GameObject player;

    // public float speed = 1.5f;
    // public Material particalMaterial;
    // private float alpha = 1.0f;
    public GameObject forceParticle;
    public int particleMaxNum = 20;
    private Vector3 oldPos;
    private Queue<GameObject> particleQueue = new Queue<GameObject>();

    private void Start()
    {
        grassMaterial.SetFloat("_CameraSize", GetComponent<Camera>().orthographicSize);
        // particalMaterial = GetComponent<Renderer>().material;
        oldPos = transform.position;
    }

    private void Update()
    {
        transform.position = new Vector3(player.transform.position.x, player.transform.position.y , player.transform.position.z);
        // Generate force particles
        if ((transform.position - oldPos).magnitude > 0.01f)
        {
            var go = (GameObject)Instantiate(forceParticle);
            particleQueue.Enqueue(go);
            go.transform.position = transform.position;
            oldPos = transform.position;
        }
        while(particleQueue.Count > particleMaxNum)
        {
            var go = particleQueue.Dequeue();
             Destroy(go);
        }

        // particalMaterial.SetFloat("_Alpha", alpha);
        // alpha = Mathf.Lerp(alpha, 0.0f, Time.fixedDeltaTime * speed);

        // if (alpha <= 0.005f)
        // {
        //     Destroy(this.gameObject);
        // }

        //Move with character
        transform.position = new Vector3(player.transform.position.x, player.transform.position.y + 5.0f, player.transform.position.z);
        grassMaterial.SetVector("_CameraCenterPos", new Vector4(transform.position.x, 0.0f, transform.position.z, 1.0f));
    }

    //   private void OnRenderImage(RenderTexture src, RenderTexture dest) 
    // {
    //     if(RT==null)
    //     {
    //         RT=new RenderTexture(256,256,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.Linear);
    //         RT.Create();
    //     }
        
    //     if(GroundMaterial!=null)
    //     {
    //          Graphics.Blit(null,RT,GroundMaterial);            
    //     }  
    //      Graphics.Blit(src,dest);    
    // }

    // private void OnGUI() 
    // {
    //     if(RT!=null)
    //     {
    //         GUILayout.BeginArea(new Rect(0, 0, 256, 256));
    //         GUILayout.Box(RT);
    //         //GUILayout.
    //         GUILayout.EndArea();
    //     }
    // }


}
