﻿using System;
using UnityEngine;

namespace Shared
{
	public class HealthSystem : MonoBehaviour
	{
		[Header("Health Settings")]
		public float MaxHealth = 100.0f;
		public float CurrentHealth;

		[Header("Shield Settings")]
		public bool UseShield = false;
		public float MaxShield = 100.0f;
		public float CurrentShield;
		public float RechargeDelay = 5.0f;
		public float RechargeRate = 1.0f;
		private float _rechargeTimer = 0.0f;
		private bool _rechargeTimerReset = false;

		// Event Handler
		// ---------------------------------------------------------------------
		public class OnDamageArgs : EventArgs
		{
			public float damage;
			public float newHealth;
			public GameObject attacker;
		}

		public event EventHandler<OnDamageArgs> OnDamageEvent;

		// ---------------------------------------------------------------------

		public void BuffHealth(float multipler)
		{
			float buffAmount = MaxHealth - MaxHealth * multipler;
			MaxHealth += buffAmount;
			CurrentHealth += buffAmount;
		}

		public void BuffShield(float multipler)
		{
			float buffAmount = MaxShield - MaxShield * multipler;
			MaxShield += buffAmount;
		}

		private void Start()
		{
			CurrentHealth = MaxHealth;
		}

		private void Update()
		{
			Shield();
		}

		private void Shield()
		{
			if (_rechargeTimerReset)
			{
				_rechargeTimer = 0.0f;
				_rechargeTimerReset = false;
			}

			if (_rechargeTimer < RechargeDelay)
			{
				return;
			}

			CurrentShield += Time.deltaTime * RechargeRate;
			if (CurrentShield >= MaxShield)
			{
				CurrentShield = MaxShield;
			}
		}

		/// <summary>
		/// Used to apply damage to an HealthSystem instance.
		/// </summary>
		/// <param name="attacker"> Source of Damage. </param>
		/// <param name="damage"> Amount of damage to apply to target. </param>
		public void TakeDamage(GameObject attacker, float damage)
		{
			_rechargeTimerReset = true;

			CurrentHealth -= damage;
			if (CurrentHealth <= 0)
			{
				Die();
			}

			OnDamageEvent.Invoke(this, new OnDamageArgs { damage = damage, newHealth = CurrentHealth, attacker = attacker });
		}

		private void Die()
		{

		}
	}
}
