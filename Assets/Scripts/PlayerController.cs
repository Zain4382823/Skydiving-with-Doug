using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rigidBody;
    Animator animator;
    public float speed = 5.0f;
    public float jumpForce = 8.0f;
    public float airControlForce = 10.0f;
    public float airControlMax = 1.5f;
    Vector2 boxExtents;
    public AudioSource coinSound;
    public AudioSource DeathSound;
    public AudioSource LevelEndSound;
    public TextMeshProUGUI uiText;
    int totalCoins;
    int coinsCollected;
    string curLevel;
    string nextLevel;


    // Use this for initialization
    void Start()
    {

        curLevel = SceneManager.GetActiveScene().name;
        if (curLevel == "Intro")
            nextLevel = "Level 1";
        else if (curLevel == "Level 1")
            nextLevel = "Level 2";
        else if (curLevel == "Level 2")
            nextLevel = "Level 3";
        else if (curLevel == "Level 3")
            nextLevel = "Finale";
        else if (curLevel == "Finale")
            nextLevel = "Win";
        else if (curLevel == "Win")
            nextLevel = "Credits";
        else if (curLevel == "Credits")
            nextLevel = "Finished";

        rigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // get the extent of the collision box
        boxExtents = GetComponent<BoxCollider2D>().bounds.extents;

        // find out how many coins in the level
        coinsCollected = 0;
        totalCoins = GameObject.FindGameObjectsWithTag("Coin").Length;


    }
    // Update is called once per frame
    void Update()
    {
        if (rigidBody.velocity.x * transform.localScale.x < 0.0f)
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);

        float xSpeed = Mathf.Abs(rigidBody.velocity.x);
        animator.SetFloat("xspeed", xSpeed);
        float ySpeed = rigidBody.velocity.y;
        animator.SetFloat("yspeed", ySpeed);

        string uiString = "x " + coinsCollected + "/" + totalCoins;
        uiText.text = uiString;

    }
    void FixedUpdate()
    {
        float h = Input.GetAxis("Horizontal");
        // check if we are on the ground
        Vector2 bottom = new Vector2(transform.position.x, transform.position.y - boxExtents.y);
        Vector2 hitBoxSize = new Vector2(boxExtents.x * 2.0f, 0.05f);
        RaycastHit2D result = Physics2D.BoxCast(bottom, hitBoxSize, 0.0f, new Vector3(0.0f, -1.0f), 0.0f, 1 << LayerMask.NameToLayer("Ground"));
        bool grounded = result.collider != null && result.normal.y > 0.9f;
        if (grounded)
        {
            if (Input.GetAxis("Jump") > 0.0f)
                rigidBody.AddForce(new Vector2(0.0f, jumpForce), ForceMode2D.Impulse);
            else
                rigidBody.velocity = new Vector2(speed * h, rigidBody.velocity.y);
        }
        else
        {
            // allow a small amount of movement in the air
            float vx = rigidBody.velocity.x;
            if (h * vx < airControlMax)
                rigidBody.AddForce(new Vector2(h * airControlForce, 0));
        }
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.tag == "Coin")
        {
            Destroy(coll.gameObject);
            coinSound.Play();
            coinsCollected++;
        }

        if (coll.gameObject.tag == "LevelEnd")
        {
            // hide the level end object
            coll.gameObject.SetActive(false);
            StartCoroutine(LoadNextLevel());
        }
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.gameObject.tag == "Death")
        {
            StartCoroutine(DoDeath());
        }
    }

    IEnumerator DoDeath()
    {
        // freeze the rigidbody
        rigidBody.constraints = RigidbodyConstraints2D.FreezeAll;
        // hide the player
        GetComponent<Renderer>().enabled = false;
        DeathSound.Play();
        // reload the level in 2 seconds
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(curLevel);
    }

    IEnumerator LoadNextLevel()
    {
        if (nextLevel != "Finished")
        {
            // hide the player
            GetComponent<Renderer>().enabled = false;
            LevelEndSound.Play();
            yield return new WaitForSeconds(2);
            SceneManager.LoadScene(nextLevel);
        }
    }

}
