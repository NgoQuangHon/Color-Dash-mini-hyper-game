
using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 120f;

    private bool collected;

    private void Update()
    {
        transform.Rotate(
            0f,
            rotateSpeed * Time.deltaTime,
            0f,
            Space.World
        );
    }

    public void Collect()
    {
        if (collected)
            return;

        collected = true;

        if (GameManager.Instance != null)
            GameManager.Instance.AddCoin();

        Destroy(gameObject);
    }
}
