#  Manual Técnico: Sistema de Cheats

## 1. Anatomia dos Arquivos (O que é e para que serve)

### 1.1. A Camada de Execução (O "Painel")
*   **`CheatManager.cs` (MonoBehaviour):**
    *   **O que é:** Um componente único que fica sempre ativo na cena de gameplay, escutando o teclado.
    *   **Para que serve:** Centraliza todos os atalhos de teste (God Mode, matar inimigos, curar, dar XP, avançar estágio) em um só lugar, sem precisar mexer no código dos outros sistemas durante o teste.
    *   **Onde está na cena:** Anexado ao objeto **`-- GAME CONTROLLER --`**, junto com `AttributeMaestro`, `GameOverWatcher`, `SplitScreenManager` e `PlayerInputManager`.

### 1.2. A Camada de Suporte (Os "Ganchos")
*   **`Health.cs` (MonoBehaviour):**
    *   **O que é:** O script de vida de jogadores e inimigos.
    *   **Para que serve nos cheats:**
        1.  **`isInvincible` (bool):** Flag checada no início de `TakeDamage()`. Quando `true`, todo dano é ignorado. É isso que o God Mode liga/desliga.
        2.  **`Kill()`:** Zera a vida e chama `Die()` direto, ignorando armadura e esquiva (diferente de `TakeDamage`, que aplica essas reduções). Usado para matar inimigos instantaneamente.
        3.  **`FullHeal()`:** Restaura `currentHealth` para o `maxHealth` e dispara `OnHealthChanged` para a UI atualizar.

*   **`PlayerProgress.cs` (ScriptableObject):**
    *   **O que é:** O asset compartilhado que guarda XP e estágio atual (já documentado no ecossistema de atributos).
    *   **Para que serve nos cheats:** O `CheatManager` chama `AddXP()` e `CompleteStage()` diretamente nele, então os cheats de progresso disparam os mesmos eventos (`OnXPChanged`, `OnStageCompleted`) que a coleta normal de XP em jogo.

---

## 2. Guia Detalhado de Configuração e Expansão

### 2.1. Atalhos Disponíveis
Todos configuráveis pelo Inspector do `CheatManager` (campos em **Teclas**):

| Tecla padrão | Campo no Inspector    | Efeito                                                    |
|:------------:|------------------------|------------------------------------------------------------|
| **F1**       | `godModeKey`            | Liga/desliga invencibilidade dos dois jogadores            |
| **F2**       | `killAllEnemiesKey`     | Mata todos os objetos com a tag `Enemy` na cena             |
| **F3**       | `healPlayersKey`        | Cura totalmente os jogadores (não revive quem está morto)  |
| **F4**       | `addXPKey`              | Adiciona `xpPerCheat` (padrão 50) de XP ao `PlayerProgress` |
| **F5**       | `completeStageKey`      | Completa o estágio atual instantaneamente                  |

*   **Ligar/desligar tudo:** desmarque `Cheats Enabled` no Inspector. Os atalhos param de responder sem precisar remover o componente.
*   **Trocar uma tecla:** troque o valor do campo correspondente (tipo `Key`, do Input System) no Inspector.

### 2.2. Como Adicionar um Novo Cheat
Siga esta trilha de 3 passos dentro de `CheatManager.cs`:

1.  **Campo de tecla:** Adicione um novo `[SerializeField] private Key minhaNovaTeclaKey = Key.F6;` no bloco **Teclas**.
2.  **Leitura em `Update()`:** Adicione o bloco de leitura seguindo o padrão dos outros:
    ```csharp
    if (kb[minhaNovaTeclaKey].wasPressedThisFrame)
    {
        MeuNovoMetodo();
    }
    ```
3.  **Método do cheat:** Crie o método privado com a lógica (ex: `MeuNovoMetodo()`), terminando com um `Debug.Log("[Cheat] ...")` para aparecer no Console.

**Resultado:** O novo cheat já funciona em Play, sem precisar reconfigurar nada na cena.

### 2.3. Como Configurar o Sistema em uma Nova Cena
Se você criar uma fase nova e quiser levar os cheats junto:
1.  **Game Controller:** No objeto vazio de gerenciadores da cena, adicione o componente `CheatManager` (o mesmo objeto que já recebe `AttributeMaestro` e `PlayerInputManager`).
2.  **Player Progress:** Arraste o asset `PlayerProgress` (pasta `Assets/02. Scripts/XP_PlayerProgress`) para o campo **Player Progress**. Sem isso, os cheats de XP e estágio só avisam no Console e não fazem nada.
3.  **Tags:** Confirme que os prefabs de jogador estão com a tag `Player` e os de inimigo com a tag `Enemy` — é assim que `CheatManager` encontra quem afetar.

### 2.4. Ajustando o Valor de XP do Cheat
No Inspector do `CheatManager`, altere o campo `Xp Per Cheat` (padrão `50`) para aumentar ou diminuir quanto XP cada aperto de `F4` concede.

---

## 3. Notas de Debug e Verificação
*   **Console:** Todo cheat ativado registra uma mensagem com o prefixo `[Cheat]`, por exemplo:
    *   *"[Cheat] God mode: ATIVADO"*
    *   *"[Cheat] 6 inimigo(s) eliminado(s)."*
    *   *"[Cheat] Jogadores curados."*
    *   *"[Cheat] +50 XP concedido."*
    *   *"[Cheat] Estágio completado."*
*   **Aviso de configuração:** Se `Player Progress` não estiver atribuído no Inspector, `F4` e `F5` mostram `[Cheat] PlayerProgress não atribuído no CheatManager.` no Console em vez de travar o jogo.
*   **Inspector em Tempo Real:** Para confirmar que o God Mode está ativo, selecione o jogador na Hierarchy durante o Play e observe o campo `Is Invincible` dentro do componente `Health` — deve estar marcado.
