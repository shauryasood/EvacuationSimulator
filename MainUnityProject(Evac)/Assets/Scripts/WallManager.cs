using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallManager : MonoBehaviour
{
public float wallProb = 0.3f;
public float wallSpawnRadius = 20;
public GameObject wallPrefab;

private List<Agent> walls = new List<Agent>();
private GameObject wallParent;
private static HashSet<GameObject> wallObjs = new HashSet<GameObject>();

void Start()
{
Random.InitState(0);

wallParent = GameObject.Find("Walls");
for (int i = -Mathf.RoundToInt(wallSpawnRadius / 2); i < wallSpawnRadius / 2; i++)
for (int j = -Mathf.RoundToInt(wallSpawnRadius / 2); j < wallSpawnRadius / 2; j++)
{
if (Random.value < wallProb)
{
GameObject wall = null;
wall = Instantiate(wallPrefab, new Vector3(i + 0.5f, 0.5f, j + 0.5f), Quaternion.identity);
wall.name = "Wall " + i;
wall.transform.parent = wallParent.transform;
var wallScript = wall.GetComponent<Agent>();

walls.Add(wallScript);
wallObjs.Add(wall);
}
}
for (int i = -Mathf.RoundToInt(wallSpawnRadius / 2); i < wallSpawnRadius / 2; i++)
{
foreach (var j in new[] { -Mathf.RoundToInt(wallSpawnRadius / 2) - 1, wallSpawnRadius / 2 })
{
GameObject wall = null;
wall = Instantiate(wallPrefab, new Vector3(i + 0.5f, 0.5f, j + 0.5f), Quaternion.identity);
wall.name = "Wall " + i;
wall.transform.parent = wallParent.transform;
var wallScript = wall.GetComponent<Agent>();

walls.Add(wallScript);
wallObjs.Add(wall);
}
}
for (int j = -Mathf.RoundToInt(wallSpawnRadius / 2); j < wallSpawnRadius / 2; j++)
{
foreach (var i in new[] { -Mathf.RoundToInt(wallSpawnRadius / 2) - 1, wallSpawnRadius / 2 })
{
GameObject wall = null;
wall = Instantiate(wallPrefab, new Vector3(i + 0.5f, 0.5f, j + 0.5f), Quaternion.identity);
wall.name = "Wall " + i;
wall.transform.parent = wallParent.transform;
var wallScript = wall.GetComponent<Agent>();

walls.Add(wallScript);
wallObjs.Add(wall);
}
}
}

void Update()
{
}

#region Public Functions

public static bool IsWall(GameObject obj)
{
return wallObjs.Contains(obj);
}

#endregion
}
