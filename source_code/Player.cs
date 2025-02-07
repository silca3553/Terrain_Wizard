using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;
    public float jumpPow;
    private int jumpCount = 0;

    public Transform cup;

    public float gameOverHeight;
    private bool isGameOver = false;
    private Vector3 originPos;

    public Camera playerCamera;

    public AudioSource mySfx;
    public AudioClip jumpSfx;

    Rigidbody body;
    // Start is called before the first frame update
    void Start()
    {
        body = GetComponent<Rigidbody>();
        originPos = transform.position;
    }



    // Update is called once per frame
    private void Update()
    {
        if (!isGameOver && playerCamera.enabled)
        {
            Move();
            Jump();
        }
        if (transform.position.y < gameOverHeight)
        {
            GameOver();
        }
    }

    void Move()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(h, 0, v).normalized;

        h = h * speed * Time.deltaTime; // 1/ frame per second
        v = v * speed * Time.deltaTime;

        if (!(h == 0 && v == 0))
        {
            transform.position += movement * speed * Time.deltaTime;
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(movement), Time.deltaTime * speed);
        }
        //Vector3 moveDirection = transform.forward * v + transform.right * h;
        //Quaternion targetRotation = Quaternion.LookRotation(movement);
        //transform.Translate(moveDirection, Space.World);
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < 2)
        {
            body.AddForce(Vector3.up * Mathf.Sqrt(jumpPow * -Physics.gravity.y), ForceMode.Impulse);
            mySfx.PlayOneShot(jumpSfx);
            jumpCount += 1;
        }
    }

    void GameOver()
    {
        Debug.Log("Game Over!");
        isGameOver = true;
        if (Input.GetKeyDown(KeyCode.R)) // restart
        {
            Restart();
        }
    }

    void Restart()
    {
        isGameOver = false;
        transform.position = originPos;

    }

    //callbacks
    private void OnCollisionEnter(Collision collision)
    {
        jumpCount = 0;
    }
}
