using System;
using System.Collections.Generic;

namespace SeaBattle.Models
{
    public class GameState
    {
        public string GameId { get; } = Guid.NewGuid().ToString();
        public Player Player1 { get; set; }
        public Player Player2 { get; set; }
        public bool GameStarted { get; set; }
        public bool IsPlayer1Turn { get; set; }
        public string Winner { get; set; }
    }

    public class Player
    {
        public string ConnectionId { get; set; }
        public string Name { get; set; }
        public int[,] Board { get; set; } = new int[10, 10];
        public bool IsReady { get; set; }
    }
} 