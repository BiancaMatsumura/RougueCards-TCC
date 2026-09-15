# Manual Técnico: Tabela de Estatísticas no Game Over

## 1. Anatomia dos Arquivos (O que é e para que serve)

### 1.1. A Camada de Dados (O "Molde")
*   **`GameOverStats.cs` (Classes puras C#):**
    *   **O que é:** O ponto único que define quais estatísticas existem e como elas são montadas a partir de um jogador.
    *   **Para que serve:** Separa "o que mostrar" de "como desenhar". Contém três peças:
        1.  **`GameOverStatEntry` (struct):** Uma linha da tabela — um rótulo (`"Abates"`) e um valor já formatado em texto (`"12"`).
        2.  **`GameOverPlayerStats` (classe):** O pacote de um jogador inteiro — `PlayerID`, `PlayerName` e a lista de `GameOverStatEntry` dele.
        3.  **`GameOverStatsBuilder` (classe estática):** A fábrica. O método `Build(PlayerStats stats)` recebe o `PlayerStats` de um jogador e devolve o `GameOverPlayerStats` pronto, já com a lista de estatísticas preenchida.
    *   **Por que é assim:** Para adicionar uma nova estatística (mortes, dano causado, etc.) você mexe em **um único lugar** — o método `Build()` — sem tocar em UI, UXML ou no watcher que dispara o Game Over.

### 1.2. A Camada de Coleta (Quem "Repara" nas Mortes)
*   **`GameOverWatcher.cs` (MonoBehaviour / Singleton):**
    *   **O que é:** O regente que decide quando a partida acabou.
    *   **Para que serve na tabela:**
        1.  **Registro:** Cada `Health` de jogador se registra nele via `RegisterPlayer(Health health)` ao nascer, e o Watcher guarda essa referência numa lista interna (`registeredHealths`).
        2.  **Coleta:** Quando o Game Over é confirmado (solo, ou os dois jogadores mortos em co-op), o método privado `BuildPlayersStats()` percorre essa lista, pega o `PlayerStats` de cada `Health` (`health.pStats`) e chama `GameOverStatsBuilder.Build()` para cada um.
        3.  **Entrega:** O resultado (`List<GameOverPlayerStats>`) é passado para `uiController.ShowGameOver(...)`.

### 1.3. A Camada de Execução (Quem Manda na Tela)
*   **`UI_Controller.cs` (MonoBehaviour):**
    *   **O que é:** O maestro de todas as telas de UI (Pause, Game Over, Opções, etc.).
    *   **Para que serve na tabela:** O método `ShowGameOver(List<GameOverPlayerStats> playersStats = null)` recebe a lista pronta do `GameOverWatcher` e repassa para a tela (`gameOverScreen.SetPlayerStats(...)`) antes de exibi-la. O parâmetro é opcional para não quebrar quem chamar `ShowGameOver()` sem estatísticas.

*   **`GameOverScreen.cs` (Classe de UI / `BaseScreen`):**
    *   **O que é:** A ponte entre os dados (`GameOverPlayerStats`) e os elementos visuais (`VisualElement`) da tela de Game Over.
    *   **Para que serve:** O método `SetPlayerStats(...)` limpa o container `StatsTable` e monta, **em código**, uma coluna por jogador:
        *   Um `Label` com o nome do jogador (`gameover-stats-title`).
        *   Uma linha (`gameover-stats-row`) por `GameOverStatEntry`, com um `Label` de rótulo (`gameover-stats-label`) e um `Label` de valor (`gameover-stats-value`).
    *   **Por que é assim:** Como as linhas são criadas dinamicamente a partir da lista de estatísticas, **nenhuma estatística nova precisa de alteração no UXML** — o container `StatsTable` já está vazio e pronto para receber qualquer quantidade de colunas/linhas.

### 1.4. A Camada Visual (UXML e USS)
*   **`UI_HUD.uxml`:**
    *   **O que é:** O documento de UI que contém, entre outras telas, a `GameOverMenu`.
    *   **Para que serve:** Dentro de `GameOverMenu`, existe um `VisualElement` vazio chamado **`StatsTable`** (antes do `ButtonContainer`), que é o alvo que `GameOverScreen.SetPlayerStats()` preenche em tempo de execução.
*   **`mainMenu02.uss`:**
    *   **O que é:** A folha de estilos usada pela `UI_HUD.uxml`.
    *   **Para que serve:** Define as classes usadas pelas linhas montadas em código: `.gameover-stats-column`, `.gameover-stats-title`, `.gameover-stats-row`, `.gameover-stats-label` e `.gameover-stats-value`.

---

## 2. Guia Detalhado de Configuração e Expansão

### 2.1. Como Adicionar uma Nova Estatística (Ex: "Mortes")
Para que uma nova estatística apareça na tabela do Game Over, você só precisa mexer em **`GameOverStats.cs`**:

1.  Garanta que o dado já existe em algum lugar acessível a partir de `PlayerStats` (ex: um campo `public int deaths` em `PlayerStats.cs`, do mesmo jeito que `kills` já existe).
2.  No método `GameOverStatsBuilder.Build(PlayerStats stats)`, adicione uma nova linha:
    ```csharp
    data.Stats.Add(new GameOverStatEntry("Mortes", stats.deaths.ToString()));
    ```
3.  Pronto. Não é preciso mexer no UXML, na USS, no `GameOverScreen` ou no `GameOverWatcher` — a nova linha aparece automaticamente embaixo de "Abates" na coluna de cada jogador.

### 2.2. Como Mudar o Texto ou a Formatação de uma Estatística
*   **Texto do rótulo:** Troque a primeira string do `GameOverStatEntry` (ex: `"Abates"` → `"Kills"`).
*   **Formatação do valor:** Troque o `.ToString()` por um formato específico, por exemplo `stats.kills.ToString("00")` para manter dois dígitos, igual ao HUD (`KillCounterUI.cs`).

### 2.3. Como Mudar o Visual da Tabela
Toda a aparência (cores, tamanho de fonte, espaçamento entre colunas) fica nas classes USS em **`mainMenu02.uss`**:
*   `.gameover-stats-column`: espaçamento entre as colunas dos jogadores.
*   `.gameover-stats-title`: nome do jogador (ex: "P1").
*   `.gameover-stats-row` / `.gameover-stats-label` / `.gameover-stats-value`: layout e cor de cada linha de estatística.

Se quiser reposicionar a tabela inteira na tela, mexa no estilo inline do `VisualElement` **`StatsTable`** dentro de `GameOverMenu`, em `UI_HUD.uxml`.

### 2.4. Como Configurar o Sistema em uma Nova Cena
Se você criar uma fase nova, a tabela funciona automaticamente desde que:
1.  **Game Controller:** O `GameOverWatcher` esteja no objeto de gerenciadores da cena (junto com `AttributeMaestro`, `SplitScreenManager`, etc.), com o campo **UI Controller** arrastado no Inspector.
2.  **UI:** O `UI_Controller` esteja no objeto com o `UIDocument` principal, usando a mesma `UI_HUD.uxml` (que já contém `GameOverMenu` > `StatsTable`).
3.  **Prefabs:** Os prefabs de jogador tenham `Health` (que se registra sozinho no `GameOverWatcher.Instance` ao nascer) e `PlayerStats` (de onde vêm os dados, como `kills`).

---

## 3. Notas de Debug e Verificação
*   **Inspector em Tempo Real:** Para confirmar que os kills estão sendo contados, selecione o jogador na Hierarchy enquanto o jogo roda e observe o campo `Kills` dentro do componente `PlayerStats`.
*   **Console:** Toda vez que um jogador mata algo, aparece `"[Player {ID}] Kills: {total}"` no Console (vem de `PlayerStats.AddKill()`).
*   **Tabela vazia ou sem colunas:** Se a tabela aparecer vazia no Game Over, confira:
    1.  Se o `VisualElement` `StatsTable` existe dentro de `GameOverMenu` na `UI_HUD.uxml` (o nome precisa ser exatamente `StatsTable`).
    2.  Se `GameOverWatcher` está mesmo recebendo os jogadores — cada `Health` chama `GameOverWatcher.Instance?.RegisterPlayer(this)` no `Awake()`; se o Watcher não existir na cena antes dos jogadores nascerem, o registro é silenciosamente ignorado (`?.`).
    3.  Se `health.pStats` não é `null` — ele vem de `GetComponentInParent<PlayerStats>()` em `Health.Awake()`, então o `PlayerStats` precisa estar num pai (ou no próprio objeto) do `Health`.
