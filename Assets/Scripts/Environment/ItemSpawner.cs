using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ItemSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject _item;
    [SerializeField] private float _spawnTime;
    private float _spawnTimer = 0f; 
    private bool _hasItem;
    private Vector3 spawnLocation;

    // Start is called before the first frame update
    void Start()
    {
        float yCord = transform.position.y + 0.2f;
        spawnLocation = new Vector3(transform.position.x, yCord, transform.position.z);
        Instantiate(_item, spawnLocation, Quaternion.identity); 
    }

    // Update is called once per frame
    void Update()
    {
        if(noItem() && checkTimer())
        {
            Instantiate(_item, spawnLocation, Quaternion.identity);
        }
        
    }

    private bool noItem()
    {
        Collider[] items = Physics.OverlapSphere(transform.position, 0.5f, LayerMask.GetMask("Item"));
        if( items.Length > 0)
        {
            _spawnTimer = 0f;
            return false;
            
        }
        _spawnTimer += Time.deltaTime;
        return true;
    }

    private bool checkTimer()
    {
        if( _spawnTimer < _spawnTime)
        {
            return false;
        }
        return true;
    }

}
