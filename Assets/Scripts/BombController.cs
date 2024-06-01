using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using Photon.Pun;

public class BombController : MonoBehaviour
{
    PhotonView view;
    [Header("Bomb")]
    public KeyCode inputKey = KeyCode.Space;
    public GameObject bombPrefab;
    public float bombFuseTime = 3f;
    public int bombAmount = 1;
    private int bombsRemaining;

    [Header("Explosion")]
    public Explosion explosionPrefab;
    public LayerMask explosionLayerMask;
    public float explosionDuration = 1f;
    public int explosionRadius = 1;

    [Header("Destructible")]
    public Tilemap destructibleTiles;
    public Destructible destructiblePrefab;

    private void OnEnable()
    {
        bombsRemaining = bombAmount;
        view = GetComponent<PhotonView>();
        
    }

    private void Update()
    {
        
        if (bombsRemaining > 0 && Input.GetKeyDown(inputKey)) {
            StartCoroutine(PlaceBomb());
        }
    }

    private IEnumerator PlaceBomb()
    {
        if (view.IsMine)
        {
            Vector2 position = transform.position;
            position.x = Mathf.Round(position.x);
            position.y = Mathf.Round(position.y);

            GameObject bomb = Instantiate(bombPrefab, position, Quaternion.identity);
            bombsRemaining--;

            yield return new WaitForSeconds(bombFuseTime);

            position = bomb.transform.position;
            position.x = Mathf.Round(position.x);
            position.y = Mathf.Round(position.y);

            Explosion explosion = Instantiate(explosionPrefab, position, Quaternion.identity);
            explosion.SetActiveRenderer(explosion.start);
            explosion.DestroyAfter(explosionDuration);

            Explode(position, Vector2.up, explosionRadius);
            Explode(position, Vector2.down, explosionRadius);
            Explode(position, Vector2.left, explosionRadius);
            Explode(position, Vector2.right, explosionRadius);

            Destroy(bomb);
            bombsRemaining++;
            
            view.RPC("RPC_PlaceBomb", RpcTarget.All, position, bombFuseTime, explosionRadius, explosionDuration);
        }
    }

    private void Explode(Vector2 position, Vector2 direction, int length)
    {
        if (length <= 0) {
            return;
        }

        position += direction;

        if (Physics2D.OverlapBox(position, Vector2.one / 2f, 0f, explosionLayerMask))
        {
            ClearDestructible(position);
            return;
        }

        Explosion explosion = Instantiate(explosionPrefab, position, Quaternion.identity);
        explosion.SetActiveRenderer(length > 1 ? explosion.middle : explosion.end);
        explosion.SetDirection(direction);
        explosion.DestroyAfter(explosionDuration);

        Explode(position, direction, length - 1);
    }

    private void ClearDestructible(Vector2 position)
    {
        Vector3Int cell = destructibleTiles.WorldToCell(position);

        Debug.Log("World Position: " + position);
        Debug.Log("Cell Position: " + cell);

        TileBase tile = destructibleTiles.GetTile(cell);
        Debug.Log("Tile: " + tile + "world position" + position + "cell position" + cell);

        if (tile != null)
        {
            Instantiate(destructiblePrefab, position, Quaternion.identity);
            destructibleTiles.SetTile(cell, null);
        }
    }

    [PunRPC]
    void RPC_PlaceBomb(Vector2 position, float bombFuseTime, int explosionRadius, float explosionDuration)
    {
        GameObject bomb = Instantiate(bombPrefab, position, Quaternion.identity);
        bombsRemaining--;

        StartCoroutine(ExplodeRPC(bomb, position, Vector2.up, explosionRadius, explosionDuration));
        StartCoroutine(ExplodeRPC(bomb, position, Vector2.down, explosionRadius, explosionDuration));
        StartCoroutine(ExplodeRPC(bomb, position, Vector2.left, explosionRadius, explosionDuration));
        StartCoroutine(ExplodeRPC(bomb, position, Vector2.right, explosionRadius, explosionDuration));

        Destroy(bomb);
        bombsRemaining++;
        
        view.RPC("RPC_DestroyBomb", RpcTarget.All, bomb);
    }
    
    [PunRPC]
    private IEnumerator ExplodeRPC(GameObject bomb, Vector2 position, Vector2 direction, int length, float duration)
    {
        if (length <= 0) {
            yield break;
        }

        position += direction;

        if (Physics2D.OverlapBox(position, Vector2.one / 2f, 0f, explosionLayerMask))
        {
            ClearDestructible(position);
            yield break;
        }

        Explosion explosion = Instantiate(explosionPrefab, position, Quaternion.identity);
        explosion.SetActiveRenderer(length > 1 ? explosion.middle : explosion.end);
        explosion.SetDirection(direction);
        explosion.DestroyAfter(duration);

        yield return new WaitForSeconds(duration);

        Explode(position, direction, length - 1);
    }
    public void AddBomb()
    {
        bombAmount++;
        bombsRemaining++;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Bomb")) {
            other.isTrigger = false;
        }
    }

}
