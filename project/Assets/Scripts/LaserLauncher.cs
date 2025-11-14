using UnityEngine;
using System.Collections;

public class LaserLauncher : MonoBehaviour
{
    [Header("Configuração do Launcher")]
    public Transform spawnPoint; // Arraste o Empty Object 'LaserSpawnPoint' aqui!
    public GameObject laserPrefab; // Arraste seu prefab do laser (vermelho ou azul) aqui!

    [Header("Configuração de Tiro")]
    public float fireDelay = 3.0f;    // Tempo que o laser fica desligado (ex: 3s)
    public float activeDuration = 1.0f; // Tempo que o laser fica ligado (ex: 1s)

    [Header("Delay Inicial (Para Alternar)")]
    [Tooltip("Delay máximo aleatório antes de começar a atirar. Faz os lasers não atirarem juntos.")]
    public float maxInitialDelay = 3.0f; // <-- BOTA O MESMO VALOR DO 'fireDelay' AQUI!

    void Start()
    {
        if (spawnPoint == null)
        {
            spawnPoint = this.transform;
        }

        // Em vez de chamar o FireLoop direto, a gente chama o "Esquenta"
        StartCoroutine(StartFiringSequence());
    }

    IEnumerator StartFiringSequence()
    {
        // 1. GERA UM TEMPO ALEATÓRIO
        // Sorteia um tempo entre 0 e o delay máximo que você definiu
        float initialWait = Random.Range(0.0f, maxInitialDelay);
        
        Debug.Log($"Launcher {gameObject.name} vai esperar {initialWait:F2}s para começar.");

        // 2. ESPERA ESSE TEMPO
        yield return new WaitForSeconds(initialWait);
        
        // 3. COMEÇA A ATIRAR NORMALMENTE
        StartCoroutine(FireLoop());
    }

    // O loop de tiro continua o mesmo de antes
    IEnumerator FireLoop()
    {
        while (true)
        {
            // Fica desligado
            yield return new WaitForSeconds(fireDelay);

            // Atira
            GameObject instantiatedLaser = Instantiate(laserPrefab, spawnPoint.position, spawnPoint.rotation);
            
            // Fica ligado
            yield return new WaitForSeconds(activeDuration);

            // Destrói o laser
            if (instantiatedLaser != null)
            {
                Destroy(instantiatedLaser);
            }
        }
    }
}