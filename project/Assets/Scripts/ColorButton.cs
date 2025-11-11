using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public enum ColorType { Blue, Yellow, Red, Green }

// Isso garante que o objeto tenha os componentes que a gente precisa
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class ColorButton : MonoBehaviour
{
    [Header("Identificação")]
    public ColorType colorId;
    public UnityEvent<ColorType> OnButtonPressed;

    [Header("Configurações")]
    [SerializeField] private float flashDuration = 0.3f; // Tempo que fica aceso
    public AudioClip soundClip; // <--- ARRASTE O SOM ESPECÍFICO DESSA COR AQUI

    private SpriteRenderer sr;
    private Color originalColor;
    private AudioSource audioSource;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        // Salva a cor original pra poder voltar depois do brilho
        if (sr != null)
        {
            originalColor = sr.color;
        }
    }

    // Chamado tanto pelo Player (clique) quanto pelo PC (sequência)
    public void Press()
    {
        // Começa a rotina visual e sonora
        StartCoroutine(AnimatePress());
        // Avisa o GameController que foi apertado
        OnButtonPressed?.Invoke(colorId);
    }

    IEnumerator AnimatePress()
    {
        // 1. Toca o som (se tiver um clipe configurado)
        if (audioSource != null && soundClip != null)
        {
            // PlayOneShot é melhor aqui porque permite sobrepor sons rápidos se precisar
            audioSource.PlayOneShot(soundClip);
        }

        // 2. Faz o efeito visual de brilho
        if (sr != null)
        {
            sr.color = Color.white; // Fica branco pra brilhar (ou use Color.yellow pro vermelho se preferir)
            
            yield return new WaitForSeconds(flashDuration);

            sr.color = originalColor; // Volta pra cor normal
        }
    }

    // Detecta o clique do mouse
    private void OnMouseDown()
    {
        Press();
    }
}