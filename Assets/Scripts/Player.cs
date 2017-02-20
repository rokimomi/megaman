using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Controller2D))]
[RequireComponent(typeof(Animator))]

public class Player : MonoBehaviour {

    public float maxJumpHeight = 4;
    public float minJumpHeight = 1;
    public float timeToJumpApex = .4f;
    public float accelerationTimeAirborne = .2f;
    public float accelerationTimeGrounded = .1f;
    public float moveSpeed = 6;

    public Vector2 wallJumpClimb;
    public Vector2 wallJumpOff;
    public Vector2 wallLeap;

    public float wallSlideSpeedMax = 3;
    public float wallStickTime = .25f;
    float timeToWallUnstick;

    float gravity;
    float maxJumpVelocity;
    float minJumpVelocity;
    Vector3 velocity;
    float velocityXSmoothing;

    Controller2D controller;
    Animator animator;
    SpriteRenderer sprite;

    Vector2 directionalInput;
    bool wallSliding;
    int wallDirX;

    public BoxCollider2D megabusterShot;
    public float megaBusterAnimationEndBuffer = 0.25F;
    private float megaBusterAnimationEnd = 0.0F;
    private int maxActiveBusterShots = 3;
    public int activeBusterShots = 0;

    void Start() {
        controller = GetComponent<Controller2D>();
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();

        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
    }

    void Update() {
        CalculateVelocity();
        HandleWallSliding();

        controller.Move(velocity * Time.deltaTime, directionalInput);

        if (controller.collisions.above || controller.collisions.below) {
            if (controller.collisions.slidingDownMaxSlope) {
                velocity.y += controller.collisions.slopeNormal.y * -gravity * Time.deltaTime;
            } else {
                velocity.y = 0;
            }
        }

        HandleAnimationTransitions();
    }

    public void SetDirectionalInput(Vector2 input) {
        directionalInput = input;
    }

    public void OnJumpInputDown() {
        if (wallSliding) {
            if (wallDirX == directionalInput.x) {
                velocity.x = -wallDirX * wallJumpClimb.x;
                velocity.y = wallJumpClimb.y;
            }
            else if (directionalInput.x == 0) {
                velocity.x = -wallDirX * wallJumpOff.x;
                velocity.y = wallJumpOff.y;
            }
            else {
                velocity.x = -wallDirX * wallLeap.x;
                velocity.y = wallLeap.y;
            }
        }
        if (controller.collisions.below) {
            if (controller.collisions.slidingDownMaxSlope) {
                if (directionalInput.x != -Mathf.Sign(controller.collisions.slopeNormal.x)) { // not jumping against max slope
                    velocity.y = maxJumpVelocity * controller.collisions.slopeNormal.y;
                    velocity.x = maxJumpVelocity * controller.collisions.slopeNormal.x;
                }
            } else {
                velocity.y = maxJumpVelocity;
            }
        }

    }

    public void OnJumpInputUp() {
        if (velocity.y > minJumpVelocity) {
            velocity.y = minJumpVelocity;
        }
    }

    public void OnShootInputDown() {
        if (activeBusterShots < maxActiveBusterShots) {
            activeBusterShots++;
            megaBusterAnimationEnd = Time.time + megaBusterAnimationEndBuffer;

            Vector3 shotSpawnPos = transform.position + new Vector3(controller.collisions.faceDir * 1.5f, 0, 0);
            BoxCollider2D megabusterShotClone = (BoxCollider2D)Instantiate(megabusterShot, shotSpawnPos, transform.rotation);
            MegabusterBlastController scriptController = megabusterShotClone.GetComponent<MegabusterBlastController> ();

            // TODO: does modifying the active shots this way really work the way I intend? especially for multiplayer?
            scriptController.direction = new Vector2(controller.collisions.faceDir, 0);
            scriptController.player = this;
        }
    }	

	void HandleWallSliding() {
		wallDirX = (controller.collisions.left) ? -1 : 1;
		wallSliding = false;
		if ((controller.collisions.left || controller.collisions.right) && !controller.collisions.below && velocity.y < 0) {
			wallSliding = true;

			if (velocity.y < -wallSlideSpeedMax) {
				velocity.y = -wallSlideSpeedMax;
			}

			if (timeToWallUnstick > 0) {
				velocityXSmoothing = 0;
				velocity.x = 0;

				if (directionalInput.x != wallDirX && directionalInput.x != 0) {
					timeToWallUnstick -= Time.deltaTime;
				}
				else {
					timeToWallUnstick = wallStickTime;
				}
			}
			else {
				timeToWallUnstick = wallStickTime;
			}

		}

	}

	void CalculateVelocity() {
		float targetVelocityX = directionalInput.x * moveSpeed;
		velocity.x = Mathf.SmoothDamp (velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below)?accelerationTimeGrounded:accelerationTimeAirborne);
		velocity.y += gravity * Time.deltaTime;
	}

    void HandleAnimationTransitions() {
        if (!controller.collisions.below) {
            animator.SetBool("isAirborne", true);
        }
        else {
            animator.SetBool("isAirborne", false);
        }

        if (directionalInput.x != 0) {

            if (directionalInput.x < 0) {
                sprite.flipX = true;
            }
            else {
                sprite.flipX = false;
            }

            animator.SetBool("isRunning", true);
        }
        else {
            animator.SetBool("isRunning", false);
        }

        // not currently shooting, but animation buffer has been triggered
        if (animator.GetBool("isShooting") == false && Time.time < megaBusterAnimationEnd) {
            animator.SetBool("isShooting", true);
        }
        if (animator.GetBool("isShooting") == true && Time.time >= megaBusterAnimationEnd) {
            animator.SetBool("isShooting", false);
        }

    }
}
