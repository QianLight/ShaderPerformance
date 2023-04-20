using UnityEngine;


public class ParticleController : MonoBehaviour {

    [Tooltip("Force particles destroy speed")]
    public float speed = 1.5f;

    private Material particalMaterial;
    private float alpha = 1.0f;

	void Start ()
    {
        particalMaterial = GetComponent<Renderer>().material;
	}

    void Update()
    {
        // Set force shader alpha property
        particalMaterial.SetFloat("_Alpha", alpha);
        alpha = Mathf.Lerp(alpha, 0.0f, Time.fixedDeltaTime * speed);

        if (alpha <= 0.005f)
        {
            Destroy(this.gameObject);
        }
    }
}
