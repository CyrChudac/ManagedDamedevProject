using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] CharacterController2D controller;
    [SerializeField] float speed = 1f;
    [SerializeField] HidingController hidingController;
    [SerializeField] ClimbingController climbingController;
    [SerializeField] ExtinguishController extinguishController;

    private bool jump;
    private float velocity_x;
    private float velocity_y;
    private bool taken;
    public bool Hiding => hidingController.IsHiding;

    void Update()
    {
        bool jump2;
        velocity_x = Input.GetAxisRaw("Horizontal");
        velocity_y = Input.GetAxisRaw("Vertical");
        jump2 = Input.GetButtonDown("Jump");
        if(taken) {
            jump = jump2; 
            taken = false;
        } else {
            jump = jump || jump2;
        }
        if(Input.GetButtonDown("Extinguish")) {
            extinguishController.TryExtinguish(gameObject);
        }
        if(Input.GetButtonDown("Hide")) {
            hidingController.TryHide();
        }
        if(jump || velocity_y > 0 || (!hidingController.IsHiding && velocity_x != 0)){
            hidingController.StopHide();
        }
    }

	private void FixedUpdate() {
        taken = true;
        bool climbing = false;
        if(!jump && velocity_x == 0 && (velocity_y != 0 || climbingController.IsClimbing))
            climbing = climbingController.TryClimb(velocity_y);
        if(!climbing){
            bool forceJump = false;
            if(climbingController.IsClimbing) {
                climbingController.StopClimbing();
                forceJump = true;
            }
            controller.Move(Hiding ? 0 : velocity_x * speed, velocity_x.Sign(), jump, forceJump: forceJump);
        }
        jump = false;
	}
}
