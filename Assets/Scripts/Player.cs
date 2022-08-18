﻿using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using UnityEngine;

public class Player : MonoBehaviour
{
    /* 20220812 작성자 : 김두현
     * Player 스크립트에는 주인공 앨리스의 자동공격, 좌우 이동, 아이템 획득을 포함되어 있습니다.
     */

    public static Player instance = null;
    const int lockPositionX = 280;
    [SerializeField] GameObject attackPrefab; // 앨리스가 던지는 시계 투사체
    [SerializeField] float attackSpeed = 6f; // 앨리스의 이동속도, 공격속도(1초당 n회 공격)
    const float itemDuration = 4f;
    float speedUpDuration = 0;
    float damageUpDuration = 0;
    public bool shieldEnable = false;
    public GameObject itemShield;

    public bool isAlive { get; private set; }
    public bool isAttacked { get; private set; }
    public bool isEnhenced { get; private set; }
    public int playerHp { get; private set; }
    [SerializeField] int playerDamage = 1;

    [SerializeField]
    Vector2 enterStart, enterEnd; // 플레이어가 입장하는 출발점, 도착점
    RigidBody2DMove rbMove;
    Animator animator;
    SpriteRenderer spriteRenderer;
    BattleRoundUI battleRoundUI;

    Color colorOrigin;
    Color colorAttacked;

    Coroutine _attack = null; // 공격 코루틴
    Coroutine _move = null; // 움직임 코루틴
    Coroutine _checkItemDuration = null; // 아이템 먹은 상태 체크 코루틴

    public void Activate() // 플레이어 부활
    {
        if (gameObject.activeSelf) // 대신 이미 활성화 되어있다면 이 함수 무시
            return;
        gameObject.SetActive(true);
        battleRoundUI = null;
    }
    public void Deactivate() // 플레이어 비활성화
    {
        if (!gameObject.activeSelf)
            return;
        gameObject.SetActive(false);
        battleRoundUI = null;
    }
    public void SetScript(BattleRoundUI battleRoundUI) // battleRoundUI만 배틀 중일 때 따로 저장
    {
        if (gameObject.activeSelf)
            this.battleRoundUI = battleRoundUI;
    }

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        gameObject.SetActive(false);
    }
    void OnEnable()
    {
        rbMove = GetComponent<RigidBody2DMove>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        isAlive = true;
        isAttacked = false;
        isEnhenced = false;
        playerHp = 3;
        attackSpeed = 6f;
        rbMove.InitializeSpeed();
        playerDamage = 1;
        itemShield.SetActive(false);

        spriteRenderer.color = new Color(1, 1, 1, 1);
        colorOrigin = spriteRenderer.color;
        colorAttacked = new Color(1f, 0.5f, 0.5f);
        animator.speed = 1f;

        StartCoroutine(Start());
    }
    IEnumerator Start()
    {
        rbMove.SetPosition(enterStart);
        yield return rbMove.StartCoroutine(rbMove.MoveStart(enterStart, enterEnd)); // 플레이어 입장
        ActStart();
    }
    void ActStart()
    {
        MoveStart();
        AttackStart();
        CheckItemDurationStart();
    }
    public void ActStop()
    {
        MoveStop();
        AttackStop();
        CheckItemDurationStop();
    }

    public void MoveStart()
    {
        if (_move == null)
            _move = StartCoroutine(_Move());
    }
    public void MoveStop()
    {
        if (_move != null)
        {
            StopCoroutine(_move);
            _move = null;
        }
    }
    IEnumerator _Move()
    {
        while (true)
        {
            // 좌우방향키를 입력하여 앨리스가 좌우로 이동
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                gameObject.transform.Translate(new Vector2(-1 * rbMove.GetSpeed() * Time.deltaTime, 0));
            }
            if (Input.GetKey(KeyCode.RightArrow))
            {
                gameObject.transform.Translate(new Vector2(rbMove.GetSpeed() * Time.deltaTime, 0));
            }

            // x좌표가 일정 범위를 벗어나면 다시 되돌아오게 함
            if (rbMove.GetPosition().x > lockPositionX)
            {
                gameObject.transform.Translate(new Vector2(-1 * rbMove.GetSpeed() * Time.deltaTime, 0));
            }
            else if (rbMove.GetPosition().x < -1 * lockPositionX)
            {
                gameObject.transform.Translate(new Vector2(rbMove.GetSpeed() * Time.deltaTime, 0));
            }
            yield return null;
        }
    }

    public void AttackStart()
    {
        if (_attack == null)
            _attack = StartCoroutine(_Attack());
    }
    public void AttackStop()
    {
        if (_attack != null)
        {
            StopCoroutine(_attack);
            _attack = null;
        }
    }
    IEnumerator _Attack()
    {
        while (true)
        {
            // 시계가 플레이어 정수리에서 발사되도록 하였음
            GameObject obj = Instantiate(attackPrefab, rbMove.GetPosition() + new Vector2(18, 32), Quaternion.identity);
            PlayerAttackPrefab script = obj.GetComponent<PlayerAttackPrefab>();
            script.SetDamage(playerDamage);
            if (isEnhenced)
                script.SetHardAttack();
            else
                script.SetNormalAttack();
            yield return new WaitForSeconds(1f / attackSpeed);
        }
    }

    public void TakeItem(Item.ItemType_ _itemType)
    {
        switch(_itemType)
        {
            case Item.ItemType_.MoveSpeedUp: // 이동속도 증가 아이템 획득
                speedUpDuration = itemDuration;
                break;
            case Item.ItemType_.DamageUp: // 공격력 증가 아이템 획득
                damageUpDuration = itemDuration;
                break;
            case Item.ItemType_.Shield: // 방어막 아이템 획득
                shieldEnable = true;
                break;
        }
    }

    void CheckItemDurationStart()
    {
        if (_checkItemDuration == null)
            _checkItemDuration = StartCoroutine(_CheckItemDuration());
    }
    void CheckItemDurationStop()
    {
        if (_checkItemDuration != null)
        {
            StopCoroutine(_checkItemDuration);
            _checkItemDuration = null;
        }
    }
    IEnumerator _CheckItemDuration()
    {
        while (true)
        {
            if (speedUpDuration > 0)
            {
                speedUpDuration -= Time.deltaTime;
                rbMove.SetSpeed(300f);
            }
            else
                rbMove.InitializeSpeed();

            if (damageUpDuration > 0)
            {
                isEnhenced = true;
                damageUpDuration -= Time.deltaTime;
                playerDamage = 2;
            }
            else
            {
                isEnhenced = false;
                playerDamage = 1;
            }

            if (shieldEnable == true)
                itemShield.SetActive(true);
            else
                itemShield.SetActive(false);

            yield return null;
        }
    }

    public IEnumerator GetDamage(int damage) // 여왕이랑 같은 매커니즘
    {
        if (isAttacked) yield break; // 공격 받는 동안은 무적
        isAttacked = true;
        playerHp -= damage;
        if (playerHp <= 0) // 플레이어 소멸
        {
            isAlive = false;
            animator.speed = 0f; // 애니메이션 중지
            ActStop(); // 플레이어 모든 행동 중지
            for (int i = 0; i < 50; i++) // 플레이어 점차 희미하게 사라짐
            {
                spriteRenderer.color = new Color(colorOrigin.r, colorOrigin.g, colorOrigin.b, 1f - 0.02f * i);
                yield return null;
            }
            battleRoundUI.SetActiveOnPanelGameover(); // 게임오버 패널 창 띄움
            gameObject.SetActive(false); // 플레이어가 죽으면 비활성화
            yield break;
        }
        spriteRenderer.color = colorAttacked;
        yield return new WaitForSeconds(0.2f); // 0.2초간 피격 효과
        spriteRenderer.color = colorOrigin;
        isAttacked = false;
    }

    public void PlayerGameOver()
    {
        animator.speed = 0f;
        ActStop();
        battleRoundUI.SetActiveOnPanelGameover();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag.Equals("Item")) // 아이템 충돌
        {
            SoundManager.instance.PlaySfx(SoundManager.SFX_Name_.GetItem);
            TakeItem(other.GetComponent<Item>().itemType);
            Destroy(other.gameObject);
        }
        else if (other.tag.Equals("SpadeBullet")) // 1라운드 보스 총알 충돌
        {
            if (GameManager.instance.isClear) // 클리어 됐다면 플레이어는 무적
                return;
            StartCoroutine(GetDamage(1));
            Destroy(other.gameObject);
        }
        else if (other.tag.Equals("QueenBullet")) // 2라운드 보스 총알 충돌
        {
            if (GameManager.instance.isClear) // 클리어 됐다면 플레이어는 무적
                return;
            StartCoroutine(GetDamage(3)); // 즉사
        }
        else if (other.tag.Equals("Soldier")) // 병정들 맞았을 때
        {
            if (GameManager.instance.isClear) // 클리어 됐다면 플레이어는 무적
                return;
            if (other.GetComponent<CardSoldier>() != null && !other.GetComponent<CardSoldier>().isDead) // 1라 병정이 죽지 않았다면
                StartCoroutine(GetDamage(1));
            else if (other.GetComponent<SoldierInfo>() != null && other.GetComponent<SoldierInfo>().isAlive) // 2라 병정이 죽지 않았다면
                StartCoroutine(GetDamage(1));
        }
    }
}