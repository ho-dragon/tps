const debug = require('debug')('Room');
var room = {
	players : []
}

module.exports.addPlayer = addPlayer;
module.exports.getOtherPlayers = getOtherPlayers;
module.exports.updateLastPosition = updateLastPosition;

function addPlayer(playerName) {
	var playerNum = room.players.length;
	var player = new Player(playerName, playerNum, getTeamCode(playerNum), 100, 100, null);
	room.players.push(player);
	debug("added player :: name = " + player.name + " / number = " + player.number);
	return player;
}

function getTeamCode(playerNum) {
	if(playerNum % 2 == 0) {
		return 1;
	}
	return 2;
}

function getOtherPlayers(playerNum) {
	let otherPlayers = [];
	for (let key in room.players) {
		if (room.players[key].number != playerNum) {
			otherPlayers.push(room.players[key]);
		}
	}
	return otherPlayers;
}

function updateLastPosition(playerNum, lastPosition) {
	for (let key in room.players) {
		if (room.players[key].number == playerNum) {
			room.players[key].lastPosition = lastPosition;
		}
	}
}

function attackPlayer(attackPlayer, damagedPlayer, attackPosition) {
	var player;
	for(var i in room.players) {
		if(i.playerNum == damagedPlayer) {
			i.currentHP -= 10;
			player = i;
		}
	}
	return player;
}

function Player(name, number, teamCode, currentHP, maxHP, lastPosition) {
	this.name = name;
	this.number = number;
	this.teamCode = teamCode;
	this.currentHP = currentHP;
	this.maxHP = maxHP;
	this.lastPosition = lastPosition;
}