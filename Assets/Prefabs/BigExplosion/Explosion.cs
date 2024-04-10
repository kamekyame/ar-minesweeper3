using System.Collections;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [Header("Audios")]
    public AudioSource explosionSound;
    public AudioSource timerSound;


    [Header("Flame Prefabs")]
    public GameObject largeFlames;
    public GameObject mediumFlames;
    public GameObject tinyFlames;

    private ParticleSystem thisParticleSystem;
    [System.NonSerialized]
    public bool exploded = false;


    public void Start()
    {
        thisParticleSystem = GetComponent<ParticleSystem>();
    }

    private void RandomInstantiate(GameObject prefab)
    {
        var offsetX = Random.Range(-5f, 5f);
        var offsetY = Random.Range(-5f, 5f);
        // LocalPositionで指定すると親オブジェクトの回転に影響されてしまうためグローバル座標等で指定
        var gameObject = Instantiate(prefab, transform.position + new Vector3(offsetX, 0, offsetY), prefab.transform.rotation, transform);
        gameObject.SetActive(true);
    }
    public void OnExplosion()
    {
        // 二重爆発を防ぐ
        if (exploded) return;
        exploded = true;
        timerSound.Stop();
        thisParticleSystem.Play();
        explosionSound.Play();

        for (int i = 0; i < 5; ++i)
        {
            RandomInstantiate(tinyFlames);
        }
        for (int i = 0; i < 3; ++i)
        {
            RandomInstantiate(mediumFlames);
        }
        for (int i = 0; i < 0; ++i)
        {
            RandomInstantiate(largeFlames);
        }
    }

    // delayTime遅れて爆発するコルーチン
    public IEnumerator ExplosionByChainCorutine(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        OnExplosion();
    }

    // タイマーが作動してから爆発するまでのコルーチン
    public IEnumerator ExplosionByDigCoroutine(System.Action callback)
    {
        Debug.Log("Game Over");

        // 爆発までタイマー音を鳴らす
        timerSound.Play();

        // 爆発までの時間をランダムに設定
        float waitTime = Random.Range(1.0f, 3.0f);
        yield return new WaitForSeconds(waitTime);

        // 爆発
        OnExplosion();

        callback();
    }
}
