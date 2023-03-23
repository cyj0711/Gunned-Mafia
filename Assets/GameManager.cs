using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

enum E_GAMESTATE  // 현재 게임 방의 진행 상태
{
    WAIT,       // 둘 이상의 유저를 기다리는 상태 (1명일때만 이 상태 유지)
    PREPARE,    // 충분한 수의 유저(둘 이상)가 모여, 게임 시작을 기다리는 상태(카운트다운)
    PLAY,       // 게임이 시작되고 진행중인 상태(카운트다운)
    OVERTIME,   // 게임의 진행시간이 끝났으나, 추가시간이 적용된 상태(카운트다운)
    COOLING     // 모든 게임이 끝나고 새 라운드를 기다리는 상태(카운트다운)
}

public class GameManager : SingletonPunCallbacks<GameManager>
{
    E_GAMESTATE gameState;

    void Start()
    {
        gameState = E_GAMESTATE.WAIT;
    }

    void Update()
    {
        
    }
}
