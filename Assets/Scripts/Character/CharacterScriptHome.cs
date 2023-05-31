using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shared;
using System;

[RequireComponent(typeof(Shared.HealthSystem))]
public class CharacterScriptHome : MonoBehaviour
{
    private HealthSystem _healthSystem;
    // Start is called before the first frame update
    void Start()
    {
        _healthSystem = GetComponent<HealthSystem>();
        _healthSystem.OnDamageEvent += new EventHandler<HealthSystem.OnDamageArgs>((_, args) => {
            if(args.newHealth <= 0)
            {
                // to do, do what ever you do when player is dead here
                Debug.Log("player dead.");
                // right now destry character object will result in character camera return error from null checking
                Destroy(gameObject); 
            }
            //taking damage
            Debug.Log($"Player taking damage. Remaining health: {args.newHealth}");
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
