﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleRound1_SpawnManager : MonoBehaviour
{
    const int screenHalfX = 240;
    const float patternTime = 3.5f;
    const float spawnTime = 2.4f;
    const float spawnTimeHalf = 1.2f;
    int patternNum = 0;
    float roundTime = 60f;
    float roundTime_ = 0f;
    bool canSpawn = true;
    public int[] spawnPattern;
    public GameObject[] cardSoldier;
    public int patternRushSoldierNum, patternFollowSoldierNum, patternZigzagSoldierNum, patternNormalSoldierNum;

    void Update()
    {
        if (canSpawn)
        {
            canSpawn = false;
            if (roundTime_ >= roundTime)
            {
                return;
            }
            StartCoroutine(PatternTimeCheck());
        }
        roundTime_ += Time.deltaTime;
    }
    IEnumerator PatternTimeCheck()
    {
        yield return new WaitForSeconds(patternTime);
        if (patternNum < spawnPattern.Length)
        {
            SpawnPattern(spawnPattern[patternNum]);
            patternNum++;
            canSpawn = true;
        }
    }

    IEnumerator PatternRush(float _time)
    {
        yield return new WaitForSeconds(_time);
        Instantiate(cardSoldier[(int)CardSoldier.SoldierType_.Spade], new Vector2(Random.Range(-1 * screenHalfX, screenHalfX), transform.position.y), Quaternion.identity);
    }

    IEnumerator PatternFollow(float _time)
    {
        yield return new WaitForSeconds(_time);
        Instantiate(cardSoldier[(int)CardSoldier.SoldierType_.Heart], new Vector2(Random.Range(-1 * screenHalfX, screenHalfX), transform.position.y), Quaternion.identity);
    }

    IEnumerator PatternZigzag(float _time, int num)
    {
        yield return new WaitForSeconds(_time);
        for (int i = 0; i < patternZigzagSoldierNum; i++)
        {
            Instantiate(cardSoldier[(int)CardSoldier.SoldierType_.Diamond], new Vector2(-150 + 60 * num + i * (300 / patternZigzagSoldierNum), transform.position.y), Quaternion.identity);
        }
    }

    void PatternNormal()
    {
        for (int i = 0; i < patternNormalSoldierNum; i++)
        {
            Instantiate(cardSoldier[(int)CardSoldier.SoldierType_.Clover], new Vector2(-120 + i * 80, transform.position.y), Quaternion.identity);
        }
    }


    void SpawnPattern(int num)
    {
        switch (num)
        {
            case 1: // 돌진 - 스페이드 랜덤으로 3마리
                for (int i = 0; i < patternRushSoldierNum; i++)
                {
                    StartCoroutine(PatternRush(spawnTime - i * spawnTime / patternRushSoldierNum));
                }
                break;
            case 2: // 유도 - 하트 랜덤으로 3마리
                for (int i = 0; i < patternFollowSoldierNum; i++)
                {
                    StartCoroutine(PatternFollow(spawnTime - i * spawnTime / patternFollowSoldierNum));
                }
                break;
            case 3: // 격자 - 다이아몬드 어긋나게 3마리씩 어긋나게 6마리
                for (int i = 0; i < 2; i++)
                {
                    StartCoroutine(PatternZigzag(spawnTimeHalf - i * spawnTimeHalf / 2, i));
                }
                break;
            case 4: // 일반 - 클로버 일자 4마리
                PatternNormal();
                break;
        }
    }
}