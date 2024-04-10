using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class CellScript : MonoBehaviour
{
    [Header("Cell")]
    public MeshRenderer quad;
    public TextMeshPro aroundMineNumText;

    [Header("Mine")]
    public GameObject minePrefab;
    public GameObject bigExplosionPrefab;


    private Vector2Int myPos;
    private MSManager manager;

    private Color nowColor;

    private bool settedMine = false;

    private GameObject mineObject;
    private Explosion explosionScript;

    void Start()
    {
        ChangeColor(Color.white);
        aroundMineNumText.text = "";
    }

    public void Init(MSManager manager, Vector2Int pos)
    {
        myPos = pos;
        transform.name = $"Cell_{pos.x}_{pos.y}";
        this.manager = manager;
    }

    public void ChangeColor(Color color)
    {
        nowColor = color;
        ChangeColorRaw(color);
    }

    public void ChangeColorRaw(Color color)
    {
        var material = quad.material;
        material.color = color;
        material.SetColor("_EmissionColor", color * 2f);
    }

    public void SetMineNum(int num)
    {
        aroundMineNumText.text = num.ToString();
    }



    // 選択されたときの処理
    public void OnSelectEntered()
    {
        manager.OpenCell(myPos);
    }

    // ホバーされたときの処理
    public void OnHoverEntered()
    {
        ChangeColorRaw(Color.green);
    }
    public void OnHoverExited()
    {
        ChangeColor(nowColor);
    }


    // 地雷をセット
    public void SetMine()
    {
        if (settedMine) return;
        mineObject = Instantiate(minePrefab, transform.position, minePrefab.transform.rotation);
        mineObject.transform.SetParent(transform);
        mineObject.SetActive(false);

        var explosionObject = Instantiate(bigExplosionPrefab, transform.position, bigExplosionPrefab.transform.rotation, transform);
        explosionObject.SetActive(true);
        explosionScript = explosionObject.GetComponent<Explosion>();

        settedMine = true;
    }

    // 地雷を表示するときの処理
    public void ShowMine()
    {
        if (settedMine == false) return;
        ChangeColorRaw(Color.red);
        mineObject.SetActive(true);
    }

    // 地雷を踏んだときの処理
    public void OnDig(Action callback)
    {
        if (settedMine == false) return;
        StartCoroutine(OnDigCoroutine(callback));
    }
    IEnumerator OnDigCoroutine(Action callback)
    {
        yield return explosionScript.ExplosionByDigCoroutine(callback);
        // 爆発後に地雷を非表示にする
        mineObject.SetActive(false);
        // 他の爆弾にも連鎖的に爆発を発生させる
        Debug.Log("OnDigCorutine" + transform.parent.gameObject.name);
        transform.parent.gameObject.BroadcastMessage("OnExplosionByChain", myPos, SendMessageOptions.DontRequireReceiver);
    }

    // 地雷が連鎖爆発したときの処理
    public void OnExplosionByChain(Vector2Int startPos)
    {
        if (settedMine == false) return;
        if (explosionScript.exploded) return;
        var dis = Vector2Int.Distance(startPos, myPos);

        // 10マス(マス目の単位、not実距離)以上離れていたら爆発しない
        if (dis > 15) return;
        StartCoroutine(OnExplosionByChainCoroutine(dis * 0.3f));
    }
    IEnumerator OnExplosionByChainCoroutine(float delay)
    {
        yield return explosionScript.ExplosionByChainCorutine(delay);
        // 爆発後に地雷を非表示にする
        mineObject.SetActive(false);
        // 他の爆弾にも連鎖的に爆発を発生させる
        transform.parent.gameObject.BroadcastMessage("OnExplosionByChain", myPos, SendMessageOptions.DontRequireReceiver);
    }
}
