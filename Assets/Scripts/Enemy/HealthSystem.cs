using System;
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
            CurrentShield += buffAmount;
        }

        private void Start()
        {
            CurrentHealth = MaxHealth;
            CurrentShield = MaxShield;
        }

        private void Update()
        {
            if (UseShield)
            {
                Shield();
            }
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
            if (CurrentShield > 0)
            {
                CurrentShield -= damage;
            }
            else
            {
                CurrentHealth -= damage;
            }

            // Left over damage carries to health
            if (CurrentShield < 0)
            {
                CurrentHealth -= CurrentShield;
                CurrentHealth = 0;
            }

            if (CurrentHealth <= 0)
            {
                Die();
            }

            // This null check needs to happen, otherwise it will break anything that uses this function.
            if (OnDamageEvent != null)
            {
                OnDamageEvent.Invoke(this, new OnDamageArgs { damage = damage, newHealth = CurrentHealth, attacker = attacker });
            }
        }

        private void Die()
        {

        }
    }
}
