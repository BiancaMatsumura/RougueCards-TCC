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

        /// <summary> Verdadeiro para o jogador com mais abates (só quando há um único líder), usado para destacá-lo na tabela. </summary>
        public bool IsTopPlayer;

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
                PlayerName = $"JOGADOR {stats.playerID}"
            };

            data.Stats.Add(new GameOverStatEntry("Abates", stats.kills.ToString()));
            // Novas estatísticas (ex: Mortes, Dano Causado) entram aqui como novas linhas.

            return data;
        }

        /// <summary>
        /// Marca como <see cref="GameOverPlayerStats.IsTopPlayer"/> o jogador com mais abates.
        /// Não marca ninguém se houver empate ou se ninguém abateu nada.
        /// </summary>
        /// <param name="players">As estatísticas já montadas de cada jogador.</param>
        /// <param name="kills">Os abates de cada jogador, na mesma ordem de <paramref name="players"/>.</param>
        public static void MarkTopPlayer(List<GameOverPlayerStats> players, List<int> kills)
        {
            if (players.Count < 2)
            {
                return;
            }

            int max = 0;
            int topIndex = -1;
            bool tie = false;

            for (int i = 0; i < kills.Count; i++)
            {
                if (kills[i] > max)
                {
                    max = kills[i];
                    topIndex = i;
                    tie = false;
                }
                else if (kills[i] == max && max > 0)
                {
                    tie = true;
                }
            }

            if (topIndex >= 0 && !tie)
            {
                players[topIndex].IsTopPlayer = true;
            }
        }
    }
}
