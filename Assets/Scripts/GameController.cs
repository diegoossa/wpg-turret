using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [SerializeField]
    private GameObject trollPrefab = null;

    [SerializeField]
    private Cannon cannon = null;

    [SerializeField]
    private GameObject topRight = null;

    [SerializeField]
    private GameObject bottomLeft = null;

    [SerializeField]
    private Text scoreText = null;

    [SerializeField]
    private Button restartButton = null;

    [SerializeField]
    private float trollSpawnTimeMin = 0;

    [SerializeField]
    private float trollSpawnTimeMax = 0;

    [SerializeField]
    private float trollSpawnTimeChangeFactor = 0;

    [SerializeField]
    private float trollSpeedMin = 0;

    [SerializeField]
    private float trollSpeedMax = 0;

    [SerializeField]
    private float trollSpeedChangeFactor = 0;

    // Bullet list.
    private List<GameObject> bullets = new List<GameObject>(100);

    // Distance between bullet and troll to count as as hit.
    private const float bulletHitToleranceSq = 2;

    // Next troll id.
    private int nextTrollId = 0;

    // Dead troll count.
    private int trollsKilled = 0;

    // Next troll timer.
    private float nextTrollTimer = 0.0f;

    // Game running flag.
    private bool gameRunning = false;

    // Troll list property.
    public List<Troll> Trolls { get; private set; } = new List<Troll>(10);

    // Game board dimensions property.
    private Rect GameBoard
    {
        get
        {
            return new Rect(bottomLeft.transform.position.x, bottomLeft.transform.position.z, topRight.transform.position.x - bottomLeft.transform.position.x, topRight.transform.position.z - bottomLeft.transform.position.z);
        }
    }

    // Reset game.
    public void Reset()
    {
        // Destroy all bullets.
        foreach (var bullet in bullets)
        {
            Destroy(bullet);
        }
        bullets.Clear();

        // Destroy all trolls.
        foreach (var troll in Trolls)
        {
            Destroy(troll.gameObject);
        }
        Trolls.Clear();

        // Reset cannon.
        cannon.Reset();

        // Reset game variables.
        nextTrollId = 0;
        nextTrollTimer = trollSpawnTimeMin;
        trollsKilled = 0;
        gameRunning = true;

        // Reset UI.
        scoreText.text = trollsKilled.ToString();
        restartButton.gameObject.SetActive(false);
    }

    // Register a new bullet with the game controller.
    public void RegisterBullet(GameObject bullet)
    {
        bullets.Add(bullet);
    }

    // Start is called before the first frame update.
    private void Start()
    {
        // Reset (which initialises game variables).
        Reset();
    }

    // Update is called once per frame.
    private void Update()
    {
        UpdateBullets();

        // Early out if game not running.
        if (!gameRunning)
        {
            return;
        }

        UpdateTrolls();
    }

    // Helper function.
    private void UpdateTrolls()
    {
        // Update next troll timer and check for spawning new troll.
        nextTrollTimer -= Time.deltaTime;
        if (nextTrollTimer <= 0)
        {
            // Spawn new troll.
            var trollPos = new Vector3(Random.Range(GameBoard.xMin, GameBoard.xMax), 0, GameBoard.yMax);
            var targetPos = new Vector3(Random.Range(GameBoard.xMin, GameBoard.xMax), 0, GameBoard.yMin);
            var trollOrientation = Quaternion.LookRotation(targetPos - trollPos, Vector3.up);
            var newTroll = Instantiate(trollPrefab, trollPos, trollOrientation);
            var newTrollComponent = newTroll.GetComponent<Troll>();
            var trollSpeed = Random.Range(trollSpeedMin, trollSpeedMax);
            trollSpeed *= Mathf.Pow(trollSpeedChangeFactor, nextTrollId);
            newTrollComponent.Speed = trollSpeed;
            newTrollComponent.TrollId = nextTrollId++;
            Trolls.Add(newTrollComponent);

            // Reset next troll timer.
            nextTrollTimer = Random.Range(trollSpawnTimeMin, trollSpawnTimeMax);
            nextTrollTimer *= Mathf.Pow(trollSpawnTimeChangeFactor, nextTrollId);
        }

        // Check for troll win.
        foreach (var troll in Trolls)
        {
            // Test for troll reaching the line of the cannon.
            if (troll.transform.position.z <= cannon.transform.position.z)
            {
                var trollComponent = troll.GetComponent<Troll>();
                trollComponent.SetState(Troll.State.Winning);

                OnDefeat();

                break;
            }
        }
    }

    // Helper function.
    private void UpdateBullets()
    {
        // Find out of bounds bullets.
        var bulletsToRemove = new List<GameObject>();
        foreach (var bullet in bullets)
        {
            if (bullet.transform.position.x < GameBoard.xMin ||
                bullet.transform.position.x > GameBoard.xMax ||
                bullet.transform.position.z < GameBoard.yMin ||
                bullet.transform.position.z > GameBoard.yMax)
            {
                bulletsToRemove.Add(bullet);
            }
        }

        // Rough and ready collision check for bullets against trolls.
        foreach (var bullet in bullets)
        {
            var trollsToKill = new List<Troll>();
            foreach (var troll in Trolls)
            {
                if (Vector3.SqrMagnitude(bullet.transform.position - troll.transform.position) <= bulletHitToleranceSq)
                {
                    bulletsToRemove.Add(bullet);
                    trollsToKill.Add(troll);
                    break;
                }
            }
            foreach (var troll in trollsToKill)
            {
                KillTroll(troll);
            }
        }

        // Destroy bullets to delete.
        foreach (var bullet in bulletsToRemove)
        {
            Destroy(bullet);
            bullets.Remove(bullet);
        }
        bulletsToRemove.Clear();
    }

    // Kill troll.
    private void KillTroll(Troll troll)
    {
        // Kill troll.
        Trolls.Remove(troll);
        Destroy(troll.gameObject);

        // Update score (if game is running).
        if (gameRunning)
        {
            trollsKilled++;
            scoreText.text = trollsKilled.ToString();
        }
    }

    // On defeat.
    private void OnDefeat()
    {
        gameRunning = false;
        restartButton.gameObject.SetActive(true);
    }
}
