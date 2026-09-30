#  Manual Técnico: Escolha Exclusiva de Carta (Inimigo Especial)

## 1. Anatomia dos Arquivos (O que é e para que serve)

### 1.1. A Camada de Definição (O "Inimigo Especial")
*   **`EnemyData.cs` (ScriptableObject):**
    *   **O que é:** O "DNA" de um tipo de inimigo, com um novo campo booleano `isSpecialEnemy`.
    *   **Para que serve:** Marca um `EnemyData` como "especial". Como a mecânica lê apenas essa flag (e não uma instância única), o mesmo inimigo pode spawnar várias vezes durante a partida — cada morte dele atualiza quem tem prioridade de escolha, sem precisar de nenhum identificador extra por instância.

### 1.2. A Camada de Detecção (A "Morte")
*   **`Health.cs` (MonoBehaviour):**
    *   **O que é:** Já possuía o campo `lastAttacker`, preenchido em `TakeDamage(int amount, PlayerStats attacker)` sempre que um jogador causa dano.
    *   **Para que serve:** É a fonte da verdade de "quem deu o golpe final". Essa mecânica reutiliza esse campo em vez de criar um novo sistema de atribuição de dano.

*   **`Enemy.cs` (MonoBehaviour):**
    *   **O que é:** O controlador unificado do inimigo, que escuta `health.OnDeath`.
    *   **Para que serve:** Em `HandleDeath()`, se o `EnemyData` atual tiver `isSpecialEnemy == true`, ele envia `health.lastAttacker` para o `AttributeMaestro` através de `RegisterSpecialEnemyKill()`.

### 1.3. A Camada de Execução (O "Regente")
*   **`AttributeMaestro.cs` (Singleton / Maestro):**
    *   **O que é:** O mesmo regente da cena descrito em `GuiaAtributos.md`, agora também responsável pela prioridade de escolha ligada ao inimigo especial.
    *   **Para que serve:**
        1.  **Prioridade:** `RegisterSpecialEnemyKill(killer)` guarda o último jogador que eliminou o inimigo especial.
        2.  **Arbitragem Atualizada:** `GetDecidingPlayer()` passa a checar primeiro esse jogador; só cai na regra antiga (maior combo) se nenhum inimigo especial tiver sido eliminado ainda.
        3.  **Bloqueio de Input:** `LockOutNonDecidingPlayer()` desativa o `PlayerInput` do jogador que **não** tem prioridade, para que só o jogador decisor consiga interagir com o painel de cartas.
        4.  **Restauração:** `RestoreLockedPlayerInput()` reativa o `PlayerInput` do jogador bloqueado assim que a escolha termina.

*   **`PlayerInputManager.cs` (MonoBehaviour):**
    *   **O que é:** O script que instancia os jogadores ao entrarem na partida.
    *   **Para que serve:** Logo após criar o `PlayerInput` de cada jogador, registra essa referência no Maestro via `RegisterPlayerInput(playerID, input)`, para que o bloqueio/restauração funcione depois.

### 1.4. A Camada de UI (O "Gatilho")
*   **`CardManager.cs` (MonoBehaviour):**
    *   **O que é:** O gerente do painel de cartas (já descrito em `GuiaAtributos.md`).
    *   **Para que serve:** Chama `AttributeMaestro.LockOutNonDecidingPlayer()` dentro de `ShowPanel()` (ao abrir o painel) e `AttributeMaestro.RestoreLockedPlayerInput()` dentro de `HidePanel()` (ao fechar, seja por escolha de carta ou outro motivo).

---

## 2. Guia Detalhado de Configuração e Expansão

### 2.1. Como Marcar um Inimigo como "Especial"
Nenhum script novo é necessário, o processo é 100% feito por dados:
1.  No Unity, selecione (ou crie) um `EnemyData` em **Create > Gameplay > Enemy Data**.
2.  Marque a checkbox **`Is Special Enemy`** no Inspector.
3.  Adicione esse `EnemyData` ao pool de spawn normalmente (`EnemySpawner.dadosInimigosDisponiveis`) ou instancie-o manualmente (ex: um `BossFightManager` dedicado), como já é feito hoje com o boss.

**Resultado:** Toda vez que um inimigo instanciado com esse `EnemyData` morrer, o jogador que deu o golpe final passa a ter prioridade de escolha na próxima liberação de upgrade.

### 2.2. Como Funciona a Ordem de Prioridade
A prioridade de escolha segue esta ordem, verificada em `GetDecidingPlayer()`:
1.  **Última eliminação de inimigo especial:** se algum já morreu na partida, quem deu o golpe final decide.
2.  **Maior combo atual:** se nenhum inimigo especial morreu ainda, vale a regra antiga (quem tem o `currentCombo` mais alto).

O valor da prioridade especial **não é consumido** após uma escolha: ele permanece válido até que um novo inimigo especial morra (o que reflete o fato de que ele pode spawnar várias vezes ao longo da fase).

### 2.3. Como Funciona o Bloqueio de Input
Ao abrir o painel de cartas (`CardManager.ShowPanel()`):
1.  O Maestro descobre o jogador decisor com `GetDecidingPlayer()`.
2.  O `PlayerInput` do **outro** jogador é desativado com `DeactivateInput()` — isso bloqueia as ações de gameplay dele (mover, atacar, pular, etc.).
3.  Ao fechar o painel (`CardManager.HidePanel()`, chamado automaticamente após a escolha em `HandlePickUp()`), o `PlayerInput` desse jogador é reativado com `ActivateInput()`.

**Atenção:** a navegação do painel de cartas usa um único `InputSystemUIInputModule` global, compartilhado pelos dois jogadores. O bloqueio acima impede as ações de **gameplay** do jogador não-decisor, mas não impede fisicamente o dispositivo dele de navegar pela UI compartilhada — isso é uma limitação de arquitetura já existente antes desta mecânica.

### 2.4. Como Configurar o Sistema em uma Nova Cena
Segue a mesma ordem de conexão do `GuiaAtributos.md` (seção 2.3). Nenhuma conexão extra no Inspector é necessária: o registro de `PlayerInput` no Maestro acontece automaticamente pelo `PlayerInputManager` ao dar Play.

---

## 3. Notas de Debug e Verificação
*   **Console:** O sistema avisa no Console:
    *   *"[Maestro] Inimigo especial eliminado por Player [ID]. Ele decidirá a próxima carta."* (Confirma o registro da eliminação especial).
    *   *"[Maestro] Input do Player [ID] bloqueado durante a escolha de carta."* (Confirma que o jogador não-decisor foi bloqueado).
    *   *"[Maestro] Input do Player [ID] restaurado."* (Confirma que o input voltou ao normal após a escolha).
*   **Inspector em Tempo Real:** Durante o jogo, selecione o objeto do `AttributeMaestro` na Hierarchy e observe os campos `Player1 Input` / `Player2 Input`. Se um deles estiver com o comportamento pausado no painel de cartas, o bloqueio está funcionando.
*   **Teste Rápido:** Mate o inimigo especial com o Player 2 e libere um upgrade em seguida. Se o Player 1 não conseguir mais se mover enquanto o painel estiver aberto, a mecânica está correta.
