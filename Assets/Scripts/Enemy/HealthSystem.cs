using ParrelSync.NonCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Shared
{
	public class HealthSystem : MonoBehaviour
	{
		[Header("Health Settings")]
		public float maxHealth = 100.0f;
		public float currentHealth;

		[Header("Damage Settings")]
		public LayerMask enemyLayers;
		public float damageOnCollision = 10.0f;

		public class OnDamageArgs : EventArgs
		{
			public float damage;
			public float newHealth;
			public GameObject attacker;
		}
		public event EventHandler<OnDamageArgs> OnDamageEvent;
		// Start is called before the first frame update
		void Start()
		{
			currentHealth = maxHealth;
		}

		/// <summary>
		/// Used to apply damage to this ped
		/// </summary>
		/// <param name="attacker"> The attacker doing the damage. Probably the object that the script calling this function is attached to. </param>
		/// <param name="damage"> Amount of damage to apply to target. </param>
		public void TakeDamage(GameObject attacker, float damage)
		{
			currentHealth -= damage;
			OnDamageEvent.Invoke(this, new OnDamageArgs { damage = damage, newHealth = currentHealth, attacker = attacker });
			//if (currentHealth <= 0)
			//{
			//	Die();
			//}
		}

		//private void OnTriggerEnter(Collider other)
		//{
		//	// Check if the collided object is in the enemyLayers
		//	if (((1 << other.gameObject.layer) & enemyLayers) != 0)
		//	{
		//		TakeDamage(damageOnCollision);
		//	}
		//}

		//private void Die()
		//{
		//	// Implement any logic needed when the object dies, e.g., destroying the object
		//	if (gameObject.CompareTag("TopDownCharacter"))
		//	{
		//		// Handle player death logic, e.g., showing Game Over screen, restarting the level, etc.
		//		Debug.Log("Player died!");
		//	}
		//	else
		//	{
		//		// Handle enemy death logic
		//		Destroy(gameObject);
		//	}
		//}
	}
}
