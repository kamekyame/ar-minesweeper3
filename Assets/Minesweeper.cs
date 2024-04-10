using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;


public class Minesweeper
{
    public int width;
    public int height;
    public int mines;

    /// <summary>
    /// -1: mine, 0-8: number of mines around
    /// </summary>
    public int[,] mineBoard;
    public bool[,] openBoard;
    public bool[,] flagBoard;

    public bool isStart;
    // public bool isEnd;

    public enum Result
    {
        Safe, Bomb, Clear, Flaged, Other
    }

    public enum Status
    {
        Wait, Playing, Clear, Bomb
    }

    [NonSerialized]
    public Status status = Status.Wait;


    public Minesweeper(int width, int height, int mineNum)
    {
        if ((width * height - 1) < mineNum)
        {
            throw new System.Exception("Number of mines is too large");
        }

        this.width = width;
        this.height = height;
        this.mines = mineNum;
        this.isStart = false;

        this.mineBoard = new int[width, height];
        this.openBoard = new bool[width, height];
        this.flagBoard = new bool[width, height];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                mineBoard[i, j] = 0;
                openBoard[i, j] = false;
                flagBoard[i, j] = false;
            }
        }
    }

    public void Start(Vector2Int pos)
    {
        var rand = new System.Random();

        // Place mines
        for (int i = 0; i < mines; i++)
        {
            var minePos = new Vector2Int(rand.Next(width), rand.Next(height));
            if (pos == minePos || IsMine(minePos))
            {
                i--;
            }
            else
            {
                mineBoard[minePos.x, minePos.y] = -1;
            }
        }

        // Calculate numbers
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (mineBoard[i, j] == -1)
                {
                    continue;
                }
                int count = 0;
                for (int dx = -1; dx <= 1; dx++)
                {
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0)
                        {
                            continue;
                        }
                        var n = new Vector2Int(i + dx, j + dy);
                        if (!CheckOnBoard(n))
                        {
                            continue;
                        }
                        if (IsMine(n))
                        {
                            count++;
                        }
                    }
                }
                mineBoard[i, j] = count;
            }
        }

        status = Status.Playing;
    }

    public bool IsClear()
    {

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (mineBoard[i, j] != -1 && openBoard[i, j] == false)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public bool IsEnd()
    {
        return status == Status.Clear || status == Status.Bomb;
    }

    public Result Open(Vector2Int pos)
    {
        // if (isEnd)
        // {
        //     return Result.Safe;
        // }

        // スタート前なら爆弾をランダムに配置してスタート
        if (status == Status.Wait)
        {
            Start(pos);
        }
        // 終了後なら何もしない
        else if (status == Status.Clear || status == Status.Bomb)
        {
            return Result.Other;
        }
        // フラグが立っていたら何もしない
        else if (flagBoard[pos.x, pos.y])
        {
            return Result.Flaged;
        }
        openBoard[pos.x, pos.y] = true;

        // 爆弾があったら終了
        if (mineBoard[pos.x, pos.y] == -1)
        {
            // isEnd = true;
            status = Status.Bomb;
            return Result.Bomb;
        }
        else if (mineBoard[pos.x, pos.y] == 0)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    var n = new Vector2Int(pos.x + dx, pos.y + dy);
                    if (!CheckOnBoard(n))
                    {
                        continue;
                    }
                    else if (openBoard[n.x, n.y] == true)
                    {
                        continue;
                    }
                    Open(n);
                }
            }
        }
        if (IsClear())
        {
            // isEnd = true;
            status = Status.Clear;
            return Result.Clear;
        }
        return Result.Safe;
    }

    public void ToggleFlag(Vector2Int pos)
    {
        if (openBoard[pos.x, pos.y] == false)
        {
            this.flagBoard[pos.x, pos.y] = !this.flagBoard[pos.x, pos.y];
        }
    }

    public bool IsMine(Vector2Int pos)
    {
        return mineBoard[pos.x, pos.y] == -1;
    }
    public bool IsMine(int x, int y)
    {
        return mineBoard[x, y] == -1;
    }

    public bool CheckOnBoard(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }

    public int RemainingFlag()
    {
        int count = 0;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (flagBoard[i, j])
                {
                    count++;
                }
            }
        }
        return mines - count;
    }
}
