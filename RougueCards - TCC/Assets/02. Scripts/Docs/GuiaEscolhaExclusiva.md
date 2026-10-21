#  Manual Técnico: Escolha Exclusiva de Carta (Inimigo Especial)

## 0. Resumo rápido (leia isto primeiro)

**O que a mecânica faz, em uma frase:** quando os jogadores sobem de nível e o painel de cartas abre, **só um deles** pode escolher a carta — o que deu o golpe final no último **inimigo especial**. O outro fica parado até a escolha acabar, e a tela avisa quem está escolhendo.

**Como configurar (3 passos, sem programar):**
1.  **Marque o inimigo especial.** Na pasta `Assets/02. Scripts/Enemy`, clique em um arquivo de inimigo (ex: `Enemy2`). No painel da direita (o *Inspector*), marque a caixinha **`Is Special Enemy`**. Marque em **só 1 tipo** de inimigo e desmarque nos outros.
2.  **Escolha quem pode ser bloqueado.** Na cena, clique no objeto **`-- GAME CONTROLLER --`** e procure o componente **`Attribute Maestro`**. Na parte de cima há duas caixinhas:
    *   **`Exclusive Choice Gamepad`**: ligada = quem joga de **controle** (Xbox, PS5, Switch) fica bloqueado quando não é a vez dele.
    *   **`Exclusive Choice Keyboard`**: ligada = quem joga de **teclado** fica bloqueado quando não é a vez dele.
    *   Desligada = esse tipo de jogador **também pode escolher**. Desligue as duas para "os dois escolhem".
3.  **Dê Play e confira o Console.** Se algo estiver estranho (ex: todos os inimigos marcados como especiais), aparece um aviso em português começando com `[Inimigo Especial]`.

**Como testar:** use o teste automático — menu **`Tools > Testes > Escolha Exclusiva e Tabela`** (explicado em `GuiaTestes.md`). Ele diz "PASSOU" ou "FALHOU" numa janela.

**Se der problema:** vá direto para a seção 3 (Notas de Debug). O resto deste guia explica como a mecânica funciona por dentro, para quem for mexer no código.

---

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
        3.  **Bloqueio de Input:** `LockOutNonDecidingPlayer()` desativa o `PlayerInput` e a entrada na UI do jogador que **não** tem prioridade — o dispositivo físico inteiro (`InputSystem.DisableDevice()`, para controle) ou as teclas de UI da metade do teclado dele (`BlockKeyboardUi()`) — para que só o jogador decisor consiga interagir com o painel de cartas (ver seção 2.3).
        4.  **Interruptores:** `exclusiveChoiceGamepad` e `exclusiveChoiceKeyboard` (Inspector) decidem se o bloqueio vale para um jogador de controle e/ou de teclado. `IsExclusiveFor()` consulta esses bools antes de bloquear.
        5.  **Restauração:** `RestoreLockedPlayerInput()` reativa o `PlayerInput`, o dispositivo e as teclas de UI do jogador bloqueado assim que a escolha termina.
        6.  **Consulta:** a propriedade `LockedOutPlayer` informa quem está bloqueado no momento (nulo se ninguém estiver, por exemplo com a exclusividade desligada). O aviso do painel usa isso.

*   **`PlayerInputManager.cs` (MonoBehaviour):**
    *   **O que é:** O script que instancia os jogadores ao entrarem na partida.
    *   **Para que serve:** Logo após criar o `PlayerInput` de cada jogador, registra essa referência no Maestro via `RegisterPlayerInput(playerID, input)`. Para jogadores de gamepad, também registra o `Gamepad` físico via `RegisterPlayerDevice(playerID, gamepad)`. Para jogadores de teclado, registra qual metade do teclado ele usa (`KeyboardLayout.Wasd` ou `Arrows`) via `RegisterPlayerKeyboard(playerID, layout)`. Sem esses registros o Maestro não sabe o que bloquear.

### 1.4. A Camada de UI (O "Gatilho")
*   **`CardManager.cs` (MonoBehaviour):**
    *   **O que é:** O gerente do painel de cartas (já descrito em `GuiaAtributos.md`).
    *   **Para que serve:** Chama `AttributeMaestro.LockOutNonDecidingPlayer()` dentro de `ShowPanel()` (ao abrir o painel) e `AttributeMaestro.RestoreLockedPlayerInput()` dentro de `HidePanel()` (ao fechar, seja por escolha de carta ou outro motivo).
    *   **Aviso na tela:** `UpdateChooserBanner()` mostra no topo do painel quem está escolhendo ("JOGADOR 2 ESCOLHE A CARTA / Jogador 1 aguarda"), para o jogador bloqueado entender por que está sem controle. Se ninguém estiver bloqueado (exclusividade desligada ou partida solo), mostra "ESCOLHA UMA CARTA". O estilo fica na classe `.card-chooser-banner` em `player01.uss`.

> **Atenção de configuração:** o inimigo especial só faz sentido se **poucos** tipos de inimigo tiverem `Is Special Enemy` marcado. Se todos os `EnemyData` forem especiais, o "último a matar um especial" muda a cada abate e a escolha parece aleatória para os jogadores. O `EnemySpawner.CheckSpecialEnemySetup()` confere isso ao dar Play e escreve um aviso no Console (todos especiais = aviso amarelo; nenhum especial = mensagem informando que vale a regra do combo).

*   **Organização do Inspector do `AttributeMaestro`:** o que se **configura** fica no topo (as duas caixinhas de exclusividade e, mais abaixo, o banco de combos). Os campos do grupo **"Preenchidos sozinhos ao jogar (não precisa mexer)"** — `Player 1/2`, `Player 1/2 Input` e `Player 1/2 Keyboard` — são preenchidos pelo `PlayerInputManager` quando os jogadores entram; ficam visíveis só para você conferir o que está acontecendo durante o Play.

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
O painel de cartas usa um único `InputSystemUIInputModule` **global**, compartilhado pelos dois jogadores (os bindings de `Submit`/`Navigate`/`Click` em `UI_Controls.inputactions` são curinga, ex: `*/{Submit}`, e respondem a qualquer dispositivo conectado). Por isso, só desativar o `PlayerInput` do jogador (que bloqueia apenas as ações de *gameplay* dele) **não é suficiente** — o dispositivo dele continuaria navegando/clicando na UI normalmente. Para resolver isso, o bloqueio acontece em duas camadas.

Ao abrir o painel de cartas (`CardManager.ShowPanel()`):
1.  O Maestro descobre o jogador decisor com `GetDecidingPlayer()`.
2.  Se o interruptor do tipo de controle do **outro** jogador estiver desligado, nada é bloqueado e os dois escolhem (veja "Interruptores" abaixo). Caso contrário, segue:
3.  O `PlayerInput` do outro jogador é desativado com `DeactivateInput()` — bloqueia as ações de gameplay dele (mover, atacar, pular, etc.).
4.  Se ele joga de **controle**: o dispositivo físico dele (o `Gamepad`, registrado via `RegisterPlayerDevice()`) é desativado com `InputSystem.DisableDevice()` — isso impede que ele continue disparando `Submit`/`Navigate`/`Click` no módulo de UI compartilhado, já que um dispositivo desativado não gera nenhum evento de input para ninguém. Xbox, PS5 e Switch aparecem todos como `Gamepad`.
5.  Se ele joga de **teclado**: as teclas de UI dele são removidas dos bindings (veja o próximo parágrafo).

Ao fechar o painel (`CardManager.HidePanel()`, chamado automaticamente após a escolha em `HandlePickUp()`), o `PlayerInput` e o dispositivo do jogador bloqueado são reativados com `ActivateInput()` e `InputSystem.EnableDevice()`.

**Jogadores de teclado:** os dois jogadores de teclado (`WASD_Player` e `SETAS_Player`) compartilham o **mesmo** `Keyboard` físico, então não dá para desativar o dispositivo. Em vez disso, o Maestro remove, **só enquanto o painel está aberto**, as teclas de UI da metade do teclado do jogador bloqueado (`BlockKeyboardUi()`), e as devolve ao fechar (`RestoreKeyboardUi()`):
*   Jogador **WASD**: `W`, `A`, `S`, `D` e `Espaço`.
*   Jogador das **setas**: setas e `Enter`. O `Enter` também entra pelo binding curinga `*/{Submit}`, por isso esse binding passa a valer só para controles (`<Gamepad>/{Submit}`) durante o bloqueio.

O **mouse** não pertence a nenhum jogador e continua clicando nas cartas — isso não é bloqueável por jogador.

**Interruptores (Inspector do `AttributeMaestro`, seção "Escolha Exclusiva de Carta (configure aqui)"):**
*   `Exclusive Choice Gamepad` (padrão: ligado): vale para o jogador **sem prioridade** que joga de controle (Xbox, PS5, Switch — todos aparecem como `Gamepad`).
*   `Exclusive Choice Keyboard` (padrão: ligado): vale para o jogador **sem prioridade** que joga de teclado.
*   Desligado para um tipo de controle = esse jogador **não** é bloqueado e também pode escolher. Os dois desligados = "os dois escolhem". O aviso "JOGADOR X ESCOLHE A CARTA" só aparece quando alguém está de fato bloqueado.

A carta continua sendo creditada ao jogador decisor (`CardManager.HandlePickUp()`), independentemente de quem clicou.

### 2.4. Como Configurar o Sistema em uma Nova Cena
Segue a mesma ordem de conexão do `GuiaAtributos.md` (seção 2.3). Nenhuma conexão extra no Inspector é necessária: o registro de `PlayerInput`, do `Gamepad` e do layout de teclado no Maestro acontece automaticamente pelo `PlayerInputManager` ao dar Play. Para bloquear teclado, a cena precisa ter um `InputSystemUIInputModule` com as ações de UI (`UI_Controls`) configuradas — é de lá que o Maestro pega os bindings.

---

## 3. Notas de Debug e Verificação
*   **Console:** O sistema avisa no Console:
    *   *"[Maestro] Inimigo especial eliminado por Player [ID]. Ele decidirá a próxima carta."* (Confirma o registro da eliminação especial).
    *   *"[Maestro] Input do Player [ID] bloqueado durante a escolha de carta."* (Confirma que o gameplay do jogador não-decisor foi bloqueado).
    *   *"[Maestro] Dispositivo do Player [ID] desativado durante a escolha de carta."* (Confirma que o dispositivo físico dele foi desativado — só aparece para jogadores de controle).
    *   *"[Maestro] Teclas de UI do teclado [Wasd/Arrows] desativadas durante a escolha de carta."* (Só aparece para jogadores de teclado).
    *   *"[Maestro] Exclusividade desligada para o tipo de controle do Player [ID]: os dois podem escolher."* (O interruptor correspondente está desligado).
    *   *"[Maestro] Ações de UI não encontradas..."* (Aviso: não há `InputSystemUIInputModule` na cena, então o teclado do jogador bloqueado **não** foi bloqueado).
    *   *"[Maestro] Input do Player [ID] restaurado."* / *"[Maestro] Dispositivo do Player [ID] reativado."* (Confirmam que tudo voltou ao normal após a escolha).
*   **Inspector em Tempo Real:** Durante o jogo, selecione o objeto do `AttributeMaestro` na Hierarchy e observe `Player1 Input`/`Player2 Input`, `Player1 Device`/`Player2 Device`, `Player1 Keyboard`/`Player2 Keyboard` e os dois interruptores. Se o dispositivo do jogador não-decisor estiver desativado enquanto o painel de cartas está aberto, o bloqueio está funcionando.
*   **Teste Rápido (manual):** Com dois jogadores, mate o inimigo especial com o Player 2 e libere um upgrade em seguida. O Player 1 não deve conseguir mais se mover, nem navegar/clicar no painel de cartas, enquanto ele estiver aberto.
*   **Teste Automático:** a mecânica tem um teste que roda sozinho, sem precisar jogar. Veja `GuiaTestes.md`.
