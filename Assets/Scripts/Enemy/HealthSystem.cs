﻿using UnityEngine;
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

        // ProgressBars
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

        private void Start()
        {
            CurrentHealth = MaxHealth;
            CurrentShield = MaxShield;

            // Initialize the health bars
            UpdateHealthBars();
        }

        private void Update()
        {
            if (UseShield)
            {
                Shield();
            }
            UpdateHealthBars();
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

            UpdateHealthBars();

            OnDamageEvent?.Invoke(this, new OnDamageArgs { damage = damage, newHealth = CurrentHealth, attacker = attacker });
        }

        private void Die()
        {
            // Implement dying behavior here
        }
    }
}
