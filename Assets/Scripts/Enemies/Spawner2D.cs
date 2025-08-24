using UnityEngine;

public class Spawner2D : MonoBehaviour
{
    public Transform player;
    public GameObject zombiePrefab;

    [Header("Границы мира (как у камеры/стен)")]
    public Vector2 min = new(-100, -60);
    public Vector2 max = new(100, 60);

    [Header("Параметры спавна")]
    public float interval = 2.0f;  // раз в N секунд
    float t;

    void Update()
    {
        t += Time.deltaTime;
        if (t >= interval) { t = 0f; SpawnAtEdge(); }
    }

    void SpawnAtEdge()
    {
        int edge = Random.Range(0, 4);
        float x = (edge == 2 ? min.x : edge == 3 ? max.x : Random.Range(min.x, max.x));
        float y = (edge == 0 ? min.y : edge == 1 ? max.y : Random.Range(min.y, max.y));

        var z = Instantiate(zombiePrefab, new Vector3(x, y, 0f), Quaternion.identity);
        var ai = z.GetComponent<ZombieSimpleAI2D>();
        if (ai) ai.target = player;
    }

    // Чтобы видеть рамку в редакторе
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 c = new((min.x + max.x) * 0.5f, (min.y + max.y) * 0.5f, 0);
        Vector3 s = new(max.x - min.x, max.y - min.y, 0);
        Gizmos.DrawWireCube(c, s);
    }
}
