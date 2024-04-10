using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class MSManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject prefab;
    public GameObject flagPrefab;

    [Header("AudioSources")]
    public AudioSource clearSound;
    public AudioSource digSound;
    public AudioSource digMineSound;

    [Header("Canvas")]
    public GameObject settingCanvas;
    public GameObject endCanvas;
    public GameObject clearCanvas;

    public GameObject widthInputField;
    public GameObject heightInputField;
    public GameObject mineNumInputField;
    public GameObject errorText;
    public Transform forwardSorce;

    [Header("Controller")]
    public GameObject leftController;
    public GameObject rightController;
    public List<GameObject> flagsInController = new();
    public List<GameObject> shovelsInController = new();


    private Minesweeper minesweeper;

    private CellScript[,] cells;
    private ControllerMode controllerMode = ControllerMode.UI_SELECT;

    private GameObject[,] flags;

    public enum ControllerMode
    {
        UI_SELECT,
        SHOVEL,
        FLAG,
    }

    void Start()
    {
        GameInit();

        SetControllerMode(ControllerMode.UI_SELECT);
    }

    public void GameInit()
    {
        endCanvas.SetActive(false);
        clearCanvas.SetActive(false);

        // オブジェクトの破棄
        StartCoroutine(DestroyCellCoroutine());

        cells = null;
        minesweeper = null;

        var canvasPos = forwardSorce.position + forwardSorce.forward * 2;
        canvasPos.y = 1;
        var canvasRot = forwardSorce.rotation;
        canvasRot.x = 0;
        canvasRot.z = 0;
        settingCanvas.transform.SetPositionAndRotation(canvasPos, canvasRot);
        errorText.SetActive(false);
        settingCanvas.SetActive(true);
    }

    // 大量生成時の負荷軽減のためコルーチン化
    IEnumerator DestroyCellCoroutine()
    {
        while (transform.childCount > 0)
        {
            int count = 0;
            foreach (Transform n in transform)
            {
                Destroy(n.gameObject);
                if (count == 50) break;
            }
            yield return null;
        }
    }

    public void GameStart()
    {
        try
        {
            var width = int.Parse(widthInputField.GetComponent<TMP_InputField>().text);
            var height = int.Parse(heightInputField.GetComponent<TMP_InputField>().text);
            var mineNum = int.Parse(mineNumInputField.GetComponent<TMP_InputField>().text);

            minesweeper = new Minesweeper(width, height, mineNum);
            this.cells = new CellScript[width, height];
            this.flags = new GameObject[width, height];

            var managerPos = forwardSorce.position + forwardSorce.forward * 2;
            managerPos.y = 0;
            var managerRot = forwardSorce.rotation;
            managerRot.x = 0;
            managerRot.z = 0;
            transform.SetPositionAndRotation(managerPos, managerRot);

            StartCoroutine(CreateCellCorutine());

            settingCanvas.SetActive(false);

            SetControllerMode(ControllerMode.SHOVEL);
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
            errorText.GetComponent<TextMeshProUGUI>().text = e.Message;
            errorText.SetActive(true);
        }
    }

    // 大量生成時の負荷軽減のためコルーチン化
    IEnumerator CreateCellCorutine()
    {
        int count = 0;

        int height = minesweeper.height;
        int width = minesweeper.width;
        float cellSize = prefab.transform.localScale.x * 1.1f;

        for (int y = 0; y < height; ++y)
        {
            for (int x = 0; x < width; ++x)
            {
                var gameObject = Instantiate(prefab, transform);
                gameObject.transform.SetLocalPositionAndRotation(new Vector3((x - (width / 2f)) * cellSize, 0, y * cellSize), Quaternion.identity);
                gameObject.SetActive(true);

                var cellScript = gameObject.GetComponent<CellScript>();
                cellScript.Init(this, new Vector2Int(x, y));

                cells[x, y] = cellScript;

                count++;
                if (count == 30)
                {
                    count = 0;
                    yield return null;
                }
            }
        }
        UpdateBoard();
    }

    public void UpdateBoard()
    {

        var whiteColor = Color.white; whiteColor.a = 0.5f;
        var openColor = Color.gray; openColor.a = 0.1f;

        for (int y = 0; y < minesweeper.height; ++y)
        {
            for (int x = 0; x < minesweeper.width; ++x)
            {
                var cellScript = cells[x, y];
                var color = whiteColor;
                if (minesweeper.openBoard[x, y])
                {
                    color = openColor;
                    if (minesweeper.mineBoard[x, y] == -1)
                    {
                        color = Color.red;
                    }
                    else if (minesweeper.mineBoard[x, y] > 0)
                    {
                        cellScript.SetMineNum(minesweeper.mineBoard[x, y]);
                    }
                }
                cellScript.ChangeColor(color);

                if (minesweeper.flagBoard[x, y])
                {
                    if (flags[x, y] == null)
                    {
                        var flag = Instantiate(flagPrefab);
                        flag.transform.SetParent(cells[x, y].transform);
                        flag.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                        flag.SetActive(true);
                        flags[x, y] = flag;
                    }
                }
                else
                {
                    if (flags[x, y] != null)
                    {
                        Destroy(flags[x, y]);
                        flags[x, y] = null;
                    }
                }

                // 爆弾があるセルならば爆弾を設置（SetMine関数内にて複数回呼び出しは対処済み）
                if (minesweeper.mineBoard[x, y] == -1)
                {
                    cellScript.SetMine();
                }

            }
        }
    }

    public void OpenCell(Vector2Int pos)
    {
        if (minesweeper == null) return;
        if (minesweeper.IsEnd()) return;
        if (controllerMode == ControllerMode.FLAG)
        {
            minesweeper.ToggleFlag(pos);
            foreach (var flag in flagsInController)
            {
                flag.GetComponentInChildren<TextMeshPro>().text = minesweeper.RemainingFlag().ToString();
            }
            UpdateBoard();
            return;
        }
        else
        {
            Debug.Log("OpenCell: " + pos.x + ", " + pos.y);
            var result = minesweeper.Open(pos);
            UpdateBoard();
            switch (result)
            {
                case Minesweeper.Result.Safe:
                    digSound.Play();
                    break;
                case Minesweeper.Result.Clear:
                    {
                        Debug.Log("Game Clear");

                        clearSound.Play();

                        var canvasPos = forwardSorce.position + forwardSorce.forward * 2;
                        canvasPos.y = 1;
                        var canvasRot = forwardSorce.rotation;
                        canvasRot.x = 0;
                        canvasRot.z = 0;
                        clearCanvas.transform.SetPositionAndRotation(canvasPos, canvasRot);
                        clearCanvas.SetActive(true);
                        SetControllerMode(ControllerMode.UI_SELECT);
                        break;
                    }
                case Minesweeper.Result.Bomb:
                    {
                        digMineSound.Play();
                        // 爆弾を表示
                        for (int y = 0; y < minesweeper.height; ++y)
                        {
                            for (int x = 0; x < minesweeper.width; ++x)
                            {
                                cells[x, y].ShowMine();
                            }
                        }

                        var cellScript = cells[pos.x, pos.y];
                        cellScript.OnDig(ShowEndCanvas);
                        break;
                    }
            }
        }
    }

    // シャベルモードに切り替え
    public void OnPrimaryButtonDown()
    {
        Debug.Log(minesweeper.status);
        if (minesweeper.status == Minesweeper.Status.Playing)
        {
            SetControllerMode(ControllerMode.SHOVEL);
        }
    }

    // フラグモードに切り替え
    public void OnSecondaryButtonDown()
    {
        if (minesweeper.status == Minesweeper.Status.Playing)
        {
            SetControllerMode(ControllerMode.FLAG);
        }
    }

    public void SetControllerMode(ControllerMode mode)
    {
        controllerMode = mode;

        var leftXRRayInteractor = leftController.GetComponentInChildren<XRRayInteractor>();
        var rightXRRayInteractor = rightController.GetComponentInChildren<XRRayInteractor>();
        if (controllerMode == ControllerMode.UI_SELECT)
        {
            // UI選択モード
            // レイキャストの最大距離: 10m
            // シャベル：非表示、フラグ：非表示

            leftXRRayInteractor.maxRaycastDistance = 10;
            rightXRRayInteractor.maxRaycastDistance = 10;

            foreach (var shovel in shovelsInController)
            {
                shovel.SetActive(false);
            }
            foreach (var flag in flagsInController)
            {
                flag.SetActive(false);
            }
        }
        else if (controllerMode == ControllerMode.SHOVEL)
        {
            // シャベルモード
            // レイキャストの最大距離: 0.3m
            // シャベル：表示、フラグ：非表示

            leftXRRayInteractor.maxRaycastDistance = 0.3f;
            rightXRRayInteractor.maxRaycastDistance = 0.3f;

            foreach (var shovel in shovelsInController)
            {
                shovel.SetActive(true);
            }
            foreach (var flag in flagsInController)
            {
                flag.SetActive(false);
            }
        }
        else if (controllerMode == ControllerMode.FLAG)
        {
            // フラグモード
            // レイキャストの最大距離: 0.3m
            // シャベル：非表示、フラグ：表示

            leftXRRayInteractor.maxRaycastDistance = 0.3f;
            rightXRRayInteractor.maxRaycastDistance = 0.3f;

            foreach (var shovel in shovelsInController)
            {
                shovel.SetActive(false);
            }
            foreach (var flag in flagsInController)
            {
                flag.GetComponentInChildren<TextMeshPro>().text = minesweeper.RemainingFlag().ToString();
                flag.SetActive(true);
            }
        }
    }

    void ShowEndCanvas()
    {
        var canvasPos = forwardSorce.position + forwardSorce.forward * 2;
        canvasPos.y = 1;
        var canvasRot = forwardSorce.rotation;
        canvasRot.x = 0;
        canvasRot.z = 0;
        endCanvas.transform.SetPositionAndRotation(canvasPos, canvasRot);
        endCanvas.SetActive(true);
        SetControllerMode(ControllerMode.UI_SELECT);
    }
}