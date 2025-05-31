using UnityEngine;

public class FadeOutShader : MonoBehaviour
{
    public float duration = 1f; // Tiempo para desvanecerse
    private float time;
    private Material mat;

    void Start()
    {
        // Instancia única del material para no afectar a otros objetos
        mat = GetComponent<Renderer>().material;
        time = 0f;
    }

    void Update()
    {
        time += Time.deltaTime;
        float alpha = Mathf.Clamp01(1 - (time / duration));
        mat.SetFloat("_Opacity", alpha);

        // Destruir al final opcionalmente
        if (time >= duration + 0.1f)
        {
            Destroy(gameObject);
        }
    }
}
