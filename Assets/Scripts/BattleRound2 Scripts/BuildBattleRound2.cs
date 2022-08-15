﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildBattleRound2 : MonoBehaviour
{
    /// <summary>
    /// writer: 김희재
    /// update: 20220813
    /// explanation: Define and render all of what round 2's will be happening.
    /// </summary>
    
    [SerializeField]
    GameObject queen_prefab;
    GameObject _queen;
    [SerializeField]
    QueenSoilderSpawner leftSpawner;
    [SerializeField]
    QueenSoilderSpawner rightSpawner;

    [SerializeField]
    float queenEmergeTimeAfterWarning; // 경고 창 생성 후 해당 시간이 지나면 여왕 등장
    [SerializeField]
    float soilderEmergeTimeAfterQueen; // 여왕 등장 후 해당 시간이 지나면 병정 등장

    UI UI;

    void Awake()
    {
        UI = Camera.main.GetComponent<UI>();
    }
    IEnumerator Start()
    {
        UI.Warning();
        yield return new WaitForSeconds(queenEmergeTimeAfterWarning);
        _queen = Instantiate(queen_prefab);
        yield return new WaitForSeconds(soilderEmergeTimeAfterQueen);
        leftSpawner.DeployStart();
        rightSpawner.DeployStart();
    }
}
