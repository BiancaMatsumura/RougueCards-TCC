using System;
using System.Collections.Generic;
using RougueCards.Attributes;

namespace RougueCards.GameOver
{
    /// <summary> Uma linha da tabela de estatísticas: um rótulo e o valor já formatado. </summary>
    public struct GameOverStatEntry
    {
        public string Label;
        public string Value;

        public GameOverStatEntry(string label, string value)
        {
            Label = label;
            Value = value;
        }
    }

    /// <summary> Conjunto de estatísticas de um jogador para exibir na tela de Game Over. </summary>
    public class GameOverPlayerStats
    {
        public int PlayerID;
        public string PlayerName;
        public List<GameOverStatEntry> Stats = new();
    }

    /// <summary>
    /// Ponto único de montagem das estatísticas mostradas no Game Over.
    /// Para adicionar uma nova estatística (ex: Mortes), acrescente uma linha em Build().
    /// </summary>
    public static class GameOverStatsBuilder
    {
        public static GameOverPlayerStats Build(PlayerStats stats)
        {
            var data = new GameOverPlayerStats
            {
                PlayerID = stats.playerID,
                PlayerName = $"P{stats.playerID}"
            };

            data.Stats.Add(new GameOverStatEntry("Abates", stats.kills.ToString()));
            // Novas estatísticas (ex: Mortes, Dano Causado) entram aqui como novas linhas.

            return data;
        }
    }
}
