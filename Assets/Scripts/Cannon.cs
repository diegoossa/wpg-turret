using UnityEngine;

public class Cannon : MonoBehaviour
{
    [SerializeField]
    private GameController gameController = null;

    [SerializeField]
    private GameObject bulletPrefab = null;

    // The barrel of the cannon. Rotate this to aim.
    [SerializeField]
    private GameObject barrel = null;

    // The mouth of the barrel (where the projectiles are spawned).
    [SerializeField]
    private GameObject mouth = null;

    // Starting ammunition.
    [SerializeField]
    private int startingAmmo = 0;

    // Projectile speed.
    [SerializeField]
    private float bulletSpeed = 0;

    // Maximum speed you are allowed to rotate the barrel, in degrees per second.
    [SerializeField]
    private float maxRotationSpeed = 0;

    // Current ammunition remaining.
    private float ammo = 0;

    // Reset is called on start of game and on restart after losing.
    public void Reset()
    {
        // Reset parameters.
        ammo = startingAmmo;

        // Reset barrel angle.
        barrel.transform.rotation = Quaternion.identity;
    }

    // Start is called before the first frame update.
    private void Start()
    {
    }

    // Update is called once per frame.
    private void Update()
    {
        // Get the current list of trolls from the GameController.
        var trolls = gameController.Trolls;

        // An example of getting useful values from an individual troll:
        // Vctor3 trollPosition = troll.transform.position;
        // Quaternion trollOrientation = troll.transform.rotation;
        // float trollSpeed = troll.Speed;
        // int trollId = troll.TrollId;

        // TODO: Better aiming logic needed! (Remember not to exceed maxRotationSpeed).
        barrel.transform.Rotate(0, Time.deltaTime * maxRotationSpeed, 0);

        // TODO: Better firing logic needed!
        Fire();
    }

    // Fire the cannon.
    private void Fire()
    {
        // Fire (if there is ammo remaining).
        if (ammo > 0)
        {
            ammo--;

            // Spawn a bullet and register with game controller.
            var bullet = Instantiate(bulletPrefab, mouth.transform.position, mouth.transform.rotation);
            var bulletBody = bullet.GetComponent<Rigidbody>();
            bulletBody.velocity = bullet.transform.forward * bulletSpeed;
            gameController.RegisterBullet(bullet);
        }
    }
}
