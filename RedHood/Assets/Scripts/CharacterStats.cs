using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{

    public int _healthLevel = 10;
    public int _maxHealth;
    public int _currentHealth;

    public int _staminaLevel = 10;
    public int _maxStamina;
    public int _currentStamina;


    Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }
    // Start is called before the first frame update
    void Start()
    {
        _maxHealth = SetMaxHealthFromHealthLevel();
        _currentHealth = _maxHealth;
    }

    private int SetMaxHealthFromHealthLevel()
    {
        _maxHealth = _healthLevel * 10;
        return _maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
