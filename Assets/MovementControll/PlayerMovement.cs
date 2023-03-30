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
    [SerializeField] MyInput inputManager;
    [SerializeField] AnimationController animationController;
    [SerializeField] float climbingCayote;


    private bool jump;
    private float velocity_x;
    private float velocity_y;
    private bool taken;
    private float lastClimb = float.MinValue;
    public bool Hiding => hidingController.IsHiding;

    void Update()
    {
        bool jump2;
        velocity_x = inputManager.HorizontalAxis;
        velocity_y = inputManager.VerticalAxis;
        jump2 = inputManager.Jump;
        if(taken) {
            jump = jump2; 
            taken = false;
        } else {
            jump = jump || jump2;
        }
        if(inputManager.Extinguish) {
            extinguishController.TryExtinguish();
        }
        if(inputManager.Hide) {
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
            bool forceJump = (lastClimb + climbingCayote > Time.timeSinceLevelLoad);
            if(climbingController.IsClimbing) {
                climbingController.StopClimbing();
            }
            controller.Move(Hiding ? 0 : velocity_x * speed, velocity_x.Sign(), jump, forceJump: forceJump);
        }else {
            lastClimb = Time.timeSinceLevelLoad;
        }
        animationController.IsClimbing = climbing;
        jump = false;
	}
}
