﻿using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;

public class PlayerManager : MonoBehaviourInstance<PlayerManager> {
    public GameObject playerPrefab;
    public List<Player> remotePlayers;
    public Player localPlayer;
  
    protected override void _Awake() {
        Assert.IsNotNull(this.playerPrefab);
        this.remotePlayers = new List<Player>();
    }

    public int GetPlayerCount() {
        if(this.remotePlayers == null) {
            return 1;
        }           
        return this.remotePlayers.Count + 1;
    }

    private bool IsExsitRemotePlayer(int playerNum) {
        return this.remotePlayers.Exists(x => x.Number == playerNum);
    }

    public void JoinPlayer(PlayerModel player, bool isLocalPlayer) {
        if (player == null) {
            Logger.Error("[PlayerManager.JoinedPlayer] player is null");
            return;
        }

        if (isLocalPlayer == false && IsExsitRemotePlayer(player.number)) {//Todo. updateData
            Logger.Error("[PlayerManager.JoinedPlayer] player is already joined.");
            return;
        }

        GameObject clone = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity) as GameObject;

        if (player.lastPosition != null) {
            clone.transform.position = new Vector3(player.lastPosition[0], player.lastPosition[1], player.lastPosition[2]);
            clone.transform.localRotation = Quaternion.Euler(clone.transform.localRotation.eulerAngles.x, player.lastYaw, clone.transform.localRotation.eulerAngles.z);
        } else {
            clone.transform.position = new Vector3(UnityEngine.Random.Range(0, 10), UnityEngine.Random.Range(10, 20), UnityEngine.Random.Range(0, 10));
        }        

        Player newPlayer = clone.GetComponent<Player>();
        //newPlayer.hpBar.facing.SetCamara(PlayerCamera.inst.camera);//HP카메라 보는 부분 일단 주석

        newPlayer.Init(isLocalPlayer,
               (TeamCode)player.teamCode
             , player.number
             , player.name
             , player.currentHP
             , player.maxHP
             , player.isDead
             , player.killCount
             , player.deadCount
             , (number, movePos, roationY) => {
                 TcpSocket.inst.Request.MovePlayer(number, movePos, roationY);
             });

        if (isLocalPlayer) {
            this.localPlayer = newPlayer;
            this.localPlayer.AttachCamera();
            Logger.DebugHighlight("[PlayerManager.JoinedPlayer] added local player / name  = {0} / number = {1}", player.name, player.number);
        } else {
            if (this.remotePlayers.Exists(x => x.Number == player.number)) {
                Logger.Error("[Main.JoinPlayer] already exist player = " + player.number);
                return;
            }
            this.remotePlayers.Add(newPlayer);
            Logger.DebugHighlight("[PlayerManager.JoinedPlayer] added remote player / name  = {0} / number = {1}", player.name, player.number);
        }        
    }

    public void AssignTeam(Dictionary<int, int> playerTeamNumbers) {
        if (playerTeamNumbers == null || playerTeamNumbers.Count > 0 == false) {
            Logger.Error("[PlayerManger.AssignTeam] playerTeamNumber is empty");
            return;
        }
        foreach (KeyValuePair<int, int> i in playerTeamNumbers) {
            Logger.DebugHighlight("[PlayerManager.AssignTeam] key = " + i.Key + " / value =" + i.Value);
        }
        Logger.DebugHighlight("[PlayerManager.AssignTeam] localPlayer.Number = " + localPlayer.Number);
        this.localPlayer.AssignTeamCode((TeamCode)playerTeamNumbers[localPlayer.Number]);
        if (this.remotePlayers != null) {
            for(int i = 0; i < this.remotePlayers.Count; i++) {
                if (playerTeamNumbers.ContainsKey(this.remotePlayers[i].Number)) {
                    this.remotePlayers[i].AssignTeamCode((TeamCode)playerTeamNumbers[this.remotePlayers[i].Number]);
                } else {
                    Logger.Error("[PlayerManager.AssignTeam] key is not found.");
                }               
            }
        }
    }

    public Player GetPlayer(int playerNumber) {
        if(localPlayer.Number == playerNumber) {
            return this.localPlayer;
        }

        if (this.remotePlayers != null) {
            return this.remotePlayers.Find(x => x.Number == playerNumber);
        }
        Logger.Error("[PlayerManager.GetPlayer] not found player");
        return null;
    }

    public void OnMove(int playerNumb, Vector3 movePosition, float yaw) {
        Player player = this.remotePlayers.Find(x => x.Number == playerNumb);
        if (player != null) {
            if (player.IsLocalPlayer == false) {                
                player.ActionController.OnMove(movePosition, yaw);
            }
        }
    }

    public void OnACtion(int playerNumb, PlayerActionType actionType) {
        Player player = this.remotePlayers.Find(x => x.Number == playerNumb);
        if (player != null) {
            if (player.IsLocalPlayer == false) {
                player.ActionController.OnAction(actionType);
            }
        }
    }

    public void OnDamaged(DamageModel result) {
        Logger.DebugHighlight("[PlayerManager.DamagedPlayer]--------result / damagedPlayerNumb = " + result.damagedPlayer);
        if (this.localPlayer.Number == result.damagedPlayer) {
            Logger.DebugHighlight("[PlayerManager.DamagedPlayer]--------SetLocalHP / damagedPlayerNumb = " + result.damagedPlayer);
            this.localPlayer.SetHealth(result.currentHP, result.maxHP);
            return;
        }

        Player player = this.remotePlayers.Find(x => x.Number == result.damagedPlayer);
        if (player == null) {
            Logger.Error("[PlayerManager.SetDamage] player is null");
            return;
        }
        Logger.DebugHighlight("[PlayerManager.DamagedPlayer]--------SetRemoteHP / damagedPlayerNumb = " + result.damagedPlayer);
        player.SetHealth(result.currentHP, result.maxHP);
    }
}