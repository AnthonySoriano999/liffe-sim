using UnityEngine;

public class Food : MonoBehaviour
{
    [SerializeField] private float nutritionValue = 25f;

    public float Consume()
    {
        Destroy(gameObject);
        return nutritionValue;
    }
}
