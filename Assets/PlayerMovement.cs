using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] CharacterController2D controller;
    [SerializeField] float speed = 1f;

    private bool jump;
    private bool sneak;
    private float velocity_x;
    private bool taken;

    void Update()
    {
        bool jump2;
        bool sneak2 = sneak;
        velocity_x = Input.GetAxisRaw("Horizontal");
        jump2 = Input.GetButtonDown("Jump");
        if(Input.GetButtonDown("Sneak"))
            sneak2 = true;
        else if(Input.GetButtonUp("Sneak"))
            sneak2 = false;
        if(taken) {
            sneak = sneak2;
            jump = jump2;
            taken = false;
        } else {
            jump = jump || jump2;
            sneak = sneak || sneak2;
        }

    }

	private void FixedUpdate() {
        taken = true;
        controller.Move(velocity_x * speed, sneak, jump);
        jump = false;
	}
}
