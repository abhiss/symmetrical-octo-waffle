using UnityEngine;
using MoreMountains.Tools;
using System;

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

        // ProgressBar
        public MMProgressBar healthBar;

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

        public void BuffHealth(float multiplier)
        {
            float buffAmount = MaxHealth - MaxHealth * multiplier;
            MaxHealth += buffAmount;
            CurrentHealth += buffAmount;

            // Update the health bar
            if (healthBar != null)
            {
                healthBar.UpdateBar(CurrentHealth, 0, MaxHealth);
            }
        }

        public void BuffShield(float multiplier)
        {
            float buffAmount = MaxShield - MaxShield * multiplier;
            MaxShield += buffAmount;
            CurrentShield += buffAmount;
        }

        private void Start()
        {
            CurrentHealth = MaxHealth;
            CurrentShield = MaxShield;

            // Initialize the health bar
            if (healthBar != null)
            {
                healthBar.UpdateBar(CurrentHealth, 0, MaxHealth);
            }
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

            if (CurrentShield < 0)
            {
                CurrentHealth -= CurrentShield;
                CurrentShield = 0;
            }

            if (CurrentHealth <= 0)
            {
                Die();
            }

            // Update the health bar
            if (healthBar != null)
            {
                healthBar.UpdateBar(CurrentHealth, 0, MaxHealth);
            }

            if (OnDamageEvent != null)
            {
                OnDamageEvent.Invoke(this, new OnDamageArgs { damage = damage, newHealth = CurrentHealth, attacker = attacker });
            }
        }

        private void Die()
        {
            // Implement dying behavior here
        }
    }
}
