using UnityEngine;

public class Parallax : MonoBehaviour
{
    private float length;
    private float StartPos;

    // Arrastar a main camera
    public Transform cam; 

    public float ParallaxEffect;

    void Start()
    {
       StartPos = transform.position.x;
       //length = GetComponent<SpriteRenderer>().bounds.size.x;
       
       // Se  não arrastou nada, ele tenta buscar a Main Camera
       if (cam == null)
       {
           cam = Camera.main.transform;
       }
    }

    void Update()
    {
       // O check abaixo evita o erro caso a câmera ainda não tenha sido encontrada
       if (cam == null) return;

       float Distance = cam.position.x * ParallaxEffect; 
       transform.position = new Vector3(StartPos + Distance, transform.position.y, transform.position.z);
    }
}