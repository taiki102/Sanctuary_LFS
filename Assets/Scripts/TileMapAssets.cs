using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMapAssets : MonoBehaviour
{
    private GameObject[] tilemapsAssets;

    private void Start()
    {
        tilemapsAssets = new GameObject[transform.childCount];
        for (int i = 0; i < tilemapsAssets.Length; i++)
        {
            tilemapsAssets[i] = transform.GetChild(i).GetChild(0).gameObject; 
        }
        //Debug.Log(transform.childCount);
    }

    public GameObject[] tilemapObj()
    {
        if (tilemapsAssets == null)
        {
            return null;
        }
        return tilemapsAssets;
    }
}
