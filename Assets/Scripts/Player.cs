 
using System.Numerics;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;
public class Player : MonoBehaviour
{   [SerializeField] public float speed = 5f;

    private Rigidbody rb;
    private Animator _animator;

    
    private UnityEngine.Vector3 direction;

    private const string _horizontal = "Horizontal";
    private const string _vertical = "Vertical";
    private const string _LastVertical = "LastVertical";
    private const string _LastHorizontal = "LastHorizontal";
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    private void Update()
    {
        direction.Set(InputManager.movement.x, 0, InputManager.movement.y);
        rb.velocity = direction * speed;

        _animator.SetFloat(_horizontal, direction.x);
        _animator.SetFloat(_vertical, direction.z);

        if (direction != UnityEngine.Vector3.zero)
        {
            _animator.SetFloat(_LastVertical, direction.z);
            _animator.SetFloat(_LastHorizontal, direction.x);
        }
    }
}
