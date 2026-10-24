
using UnityEngine;

public class CoinEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem effectPrefab;

    public void PlayEffect(Vector3 position)
    {
        if (effectPrefab == null)
            return;

        ParticleSystem effect = Instantiate(
            effectPrefab,
            position,
            Quaternion.identity
        );

        effect.Play();

        Destroy(
            effect.gameObject,
            effect.main.duration + effect.main.startLifetime.constantMax
        );
    }
}
