using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public int defaultAmount = 100;

    private Dictionary<GameObject, List<GameObject>> objects = new Dictionary<GameObject, List<GameObject>>();

    public void Setup(GameObject prefab)
    {
        Setup(prefab, defaultAmount);
    }

    public void Setup(GameObject prefab, int initialAmount)
    {
        if (!objects.ContainsKey(prefab))
        {
            List<GameObject> list = new List<GameObject>();
            objects.Add(prefab, list);

            for (int i = 0; i < initialAmount; i++)
            {
                GameObject obj = Instantiate(prefab, transform);
                obj.SetActive(false);
                list.Add(obj);
            }
        }
    }

    public GameObject GetObject(GameObject prefab)
    {
        if (objects.ContainsKey(prefab))
        {
            foreach (GameObject obj in objects[prefab])
                if (!obj.activeSelf)
                {
                    obj.SetActive(true);
                    return obj;
                }

            GameObject newObj = Instantiate(prefab, transform);
            objects[prefab].Add(newObj);
            return newObj;
        }

        return null;
    }
}
