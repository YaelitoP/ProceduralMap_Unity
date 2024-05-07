using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class InputManager : MonoBehaviour
{   
    public static Vector2 movement;
    private PlayerInput _playerInput;
    private InputAction _moveAction;
    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();
        _moveAction = _playerInput.actions["move"];
    }

    // Update is called once per frame
    private void Update()
    {
        movement = _moveAction.ReadValue<Vector2>();
    }
}
