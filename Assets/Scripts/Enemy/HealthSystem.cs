using System;
using UnityEngine;
using MoreMountains.Tools;
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

        // Health bars
        public MMProgressBar MainPlayerHealthBar;
        public HealthBarController AllInformationHealthBar;

        // Event Handler
        public class OnDamageArgs : EventArgs
        {
            public float damage;
            public float newHealth;
            public GameObject attacker;
        }

        public event EventHandler<OnDamageArgs> OnDamageEvent;

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

            UpdateHealthBars();
        }
        private void UpdateHealthBars()
        {
            // Main player health bar
            if (MainPlayerHealthBar != null)
            {
                MainPlayerHealthBar.UpdateBar(CurrentHealth, 0, MaxHealth);
            }

            // All information health bar
            if (AllInformationHealthBar != null)
            {
                AllInformationHealthBar.ChangeValue(CurrentHealth / MaxHealth);
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
            if (UseShield && CurrentShield > 0)
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

            if (OnDamageEvent != null)
            {
                OnDamageEvent.Invoke(this, new OnDamageArgs { damage = damage, newHealth = CurrentHealth, attacker = attacker });
            }
        }
    }
}
