using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Slime: patrouilliert, läuft zu Fleisch am Boden, frisst es (ca. 3 s) und geht zurück.
// Berührung = Neustart (wie Obstacle), außer der Slime frisst gerade.
// Frisst er, kann der Spieler ihn mit F fressen (PlayerInteraction).
//
// Prefab: SpriteRenderer, Rigidbody2D (Body Type Kinematic), Circle Collider 2D (KEIN Trigger), dieses Script.
// Keine Wegsuche: Der Slime sieht Fleisch nur mit freier Sichtlinie (keine Wand dazwischen).
// Er reagiert nur auf Fleisch, das der Spieler mit E abgelegt hat (nicht auf Fleisch aus dem Layout oder zufällig gespawntes).
[RequireComponent(typeof(Rigidbody2D))]
public class Slime : MonoBehaviour
{
    private enum State { Patrol, GoToMeat, Eating, Return }

    [Header("Aussehen")]
    [SerializeField] private Sprite[] walkFrames;   // slime_0, slime_1
    [SerializeField] private Sprite[] eatFrames;    // slime_eat_0, slime_eat_1
    [SerializeField] private float frameTime = 0.3f;

    [Header("Verhalten")]
    [SerializeField] private float patrolSpeed = 1f;    // Spieler hat 2.5, Slimes sind langsamer
    [SerializeField] private float chaseSpeed = 1.5f;
    [SerializeField] private float patrolRange = 3f;    // so weit läuft er von seinem Startpunkt weg (je Seite)
    [SerializeField] private float sightRange = 4f;     // so weit sieht er Fleisch
    [SerializeField] private float eatSeconds = 3f;
    [SerializeField] private bool vertical;             // false = links/rechts, true = hoch/runter (setzt der LevelBuilder: M / N)

    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer sr;
    private ContactFilter2D filter;
    private readonly RaycastHit2D[] hits = new RaycastHit2D[8];

    private State state = State.Patrol;
    private Vector2 home;
    private Vector2 axis = Vector2.right;
    private float sign = 1f;
    private float radius = 0.3f;

    private ItemPickup target;
    private readonly List<Vector2> trail = new List<Vector2>();   // Rückweg, damit er nie quer durch Wände läuft
    private float eatTimer;
    private float sightTimer;
    private bool triggered;
    private string key;

    public bool CanBeEaten => state == State.Eating;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();

        // Schlüssel aus Szene + Startposition: so bleibt ein gefressener Slime auch nach einem Tod weg
        Vector3 p = transform.position;
        key = SceneManager.GetActiveScene().name + "_" + Mathf.RoundToInt(p.x * 10f) + "_" + Mathf.RoundToInt(p.y * 10f);

        if (GameManager.Instance.IsSlimeEaten(key))
        {
            enabled = false;
            col.enabled = false;
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (!enabled) return;

        // Trigger-Collider (Fleisch, Hinweise, Platten) ignorieren, nur feste Collider zählen
        filter = new ContactFilter2D();
        filter.NoFilter();
        filter.useTriggers = false;

        radius = col.bounds.extents.x;
        home = rb.position;

        axis = vertical ? Vector2.up : Vector2.right;

        sightTimer = Random.value * 0.25f;
    }

    void FixedUpdate()
    {
        // Cutscene, Menü usw.: Slimes bleiben stehen
        if (GameManager.Instance.State != GameState.Playing) return;

        switch (state)
        {
            case State.Patrol:
                Patrol();
                LookForMeat();
                break;
            case State.GoToMeat:
                GoToMeat();
                break;
            case State.Eating:
                eatTimer -= Time.fixedDeltaTime;
                if (eatTimer <= 0f) FinishEating();
                break;
            case State.Return:
                ReturnHome();
                LookForMeat();
                break;
        }
    }

    void Update()
    {
        Sprite[] frames = state == State.Eating ? eatFrames : walkFrames;
        if (frames == null || frames.Length == 0) return;
        sr.sprite = frames[(int)(Time.time / frameTime) % frames.Length];
    }

    // ---------- Verhalten ----------
    private void Patrol()
    {
        float offset = Vector2.Dot(rb.position - home, axis);
        if (sign > 0f && offset >= patrolRange) sign = -1f;
        else if (sign < 0f && offset <= -patrolRange) sign = 1f;

        Vector2 dir = axis * sign;
        if (Blocked(dir, radius + 0.1f))
        {
            sign = -sign;
            return;
        }

        MoveTowards(rb.position + dir, patrolSpeed);
    }

    private void LookForMeat()
    {
        sightTimer -= Time.fixedDeltaTime;
        if (sightTimer > 0f) return;
        sightTimer = 0.25f;

        ItemPickup best = null;
        float bestDistance = float.MaxValue;

        foreach (Collider2D c in Physics2D.OverlapCircleAll(rb.position, sightRange))
        {
            ItemPickup pickup = c.GetComponent<ItemPickup>();
            if (pickup == null || pickup.item != Item.Fleisch) continue;
            if (pickup.record == null || !pickup.record.byPlayer) continue;   // nur Fleisch, das der Spieler abgelegt hat

            Vector2 pos = pickup.transform.position;
            float distance = Vector2.Distance(rb.position, pos);
            if (distance >= bestDistance) continue;
            if (LineBlocked(rb.position, pos)) continue;

            best = pickup;
            bestDistance = distance;
        }

        if (best == null) return;

        target = best;
        trail.Add(rb.position);   // hier war er, als er das Fleisch gesehen hat: der Weg zurück ist frei
        state = State.GoToMeat;
    }

    private void GoToMeat()
    {
        // Spieler hat das Fleisch schon weggenommen
        if (target == null)
        {
            state = State.Return;
            return;
        }

        Vector2 pos = target.transform.position;
        if (Vector2.Distance(rb.position, pos) <= 0.15f)
        {
            StartEating();
            return;
        }

        MoveTowards(pos, chaseSpeed);
    }

    private void StartEating()
    {
        // Das Fleisch ist ab jetzt verbraucht
        if (WorldItemManager.Instance != null) WorldItemManager.Instance.RemoveItem(target);
        target = null;

        state = State.Eating;
        eatTimer = eatSeconds;
        col.isTrigger = true;   // beim Fressen ungefährlich: der Spieler kann hingehen und F drücken
    }

    private void FinishEating()
    {
        col.isTrigger = false;
        state = State.Return;
    }

    private void ReturnHome()
    {
        Vector2 goal = trail.Count > 0 ? trail[trail.Count - 1] : home;

        if (Vector2.Distance(rb.position, goal) <= 0.1f)
        {
            if (trail.Count > 0) trail.RemoveAt(trail.Count - 1);
            else state = State.Patrol;
            return;
        }

        MoveTowards(goal, chaseSpeed);
    }

    private void MoveTowards(Vector2 goal, float speed)
    {
        Vector2 next = Vector2.MoveTowards(rb.position, goal, speed * Time.fixedDeltaTime);
        if (Mathf.Abs(next.x - rb.position.x) > 0.0001f) sr.flipX = next.x < rb.position.x;
        rb.MovePosition(next);
    }

    // ---------- Wände prüfen ----------
    private float FreeDistance(Vector2 dir, float max)
    {
        Vector2 from = rb.position;
        int count = Physics2D.Linecast(from, from + dir * max, filter, hits);
        float best = max;
        for (int i = 0; i < count; i++)
        {
            Collider2D c = hits[i].collider;
            if (c == col || c.CompareTag("Player")) continue;
            best = Mathf.Min(best, hits[i].distance);
        }
        return best;
    }

    private bool Blocked(Vector2 dir, float distance)
    {
        return FreeDistance(dir, distance) < distance - 0.001f;
    }

    private bool LineBlocked(Vector2 from, Vector2 to)
    {
        int count = Physics2D.Linecast(from, to, filter, hits);
        for (int i = 0; i < count; i++)
        {
            Collider2D c = hits[i].collider;
            if (c == col || c.CompareTag("Player")) continue;
            return true;
        }
        return false;
    }

    // ---------- Berührung = Neustart ----------
    void OnCollisionEnter2D(Collision2D collision)
    {
        Check(collision);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        Check(collision);
    }

    private void Check(Collision2D collision)
    {
        if (triggered || state == State.Eating) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        triggered = true;
        GameManager.Instance.OnPlayerDied(collision.transform.position);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ---------- Vom Spieler gefressen (F) ----------
    public void GetEaten()
    {
        GameManager.Instance.EatSlime(key);
        InteractPrompt.Hide();
        Destroy(gameObject);
    }
}