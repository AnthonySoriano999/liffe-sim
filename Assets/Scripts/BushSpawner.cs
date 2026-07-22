using UnityEngine;
using System.Collections.Generic;

public class BushSpawner : MonoBehaviour
{
    [SerializeField] private GameObject foodPrefab;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private int maxFood = 5;
    [SerializeField] private float spawnRadius = 1.5f;

    private readonly List<GameObject> spawnedFood = new List<GameObject>();
    private float spawnTimer;

    private void Update()
    {
        spawnedFood.RemoveAll(food => food == null);

        if (spawnedFood.Count >= maxFood)
        {
            return;
        }

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnFood();
            spawnTimer = spawnInterval;
        }
    }

    private void SpawnFood()
    {
        Vector2 offset = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPosition = transform.position + new Vector3(offset.x, offset.y, 0f);
        GameObject food = Instantiate(foodPrefab, spawnPosition, Quaternion.identity);
        spawnedFood.Add(food);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.6f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
