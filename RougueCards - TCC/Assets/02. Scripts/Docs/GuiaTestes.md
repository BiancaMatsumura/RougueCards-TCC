#  Manual Técnico: Teste Automático da Escolha Exclusiva e da Tabela

## 0. Resumo rápido (leia isto primeiro)

**Quero só rodar o teste:**
1.  Abra o projeto no Unity e espere o carregamento terminar.
2.  No menu de cima, clique em **`Tools > Testes > Escolha Exclusiva e Tabela`**.
3.  Espere alguns segundos (o Unity entra e sai do Play Mode sozinho).
4.  Aparece uma janela: **"PASSOU!"** (está tudo certo) ou **"FALHOU"** (ela lista o que quebrou). Sua cena é reaberta automaticamente.

**Quando rodar:** sempre que mexer na escolha de carta, nos inimigos, nos controles dos jogadores ou na tabela de kills — antes de commitar.

**Palavras que aparecem neste guia:**
*   **Inspector:** o painel da direita do Unity que mostra as propriedades do objeto selecionado.
*   **Console:** a janela do Unity (aba "Console") onde aparecem mensagens e erros do jogo.
*   **Play Mode:** quando o jogo está rodando dentro do Unity (botão de Play).
*   **Teste automático:** um código que joga "no seu lugar" e confere se o resultado é o esperado.

---

## 1. Para que serve

Conferir, sem precisar jogar, se a **escolha exclusiva de carta** e a **contagem de abates da tabela** funcionam. É a forma rápida de saber se uma mudança quebrou algo, e de mostrar o funcionamento a quem for avaliar o projeto (o teste gera um relatório de "passou/falhou" por checagem).

Ele responde perguntas como:
*   O jogador que mata um inimigo especial passa a decidir a carta?
*   Um inimigo comum **não** muda quem decide?
*   O jogador sem prioridade perde mesmo o controle da UI (controle **e** teclado)?
*   Tudo volta ao normal quando o painel fecha?
*   Os interruptores `Exclusive Choice Gamepad` / `Exclusive Choice Keyboard` funcionam?
*   Os abates contados chegam à linha "Abates" da tabela?

**O que ele NÃO cobre** (continua sendo conferido jogando):
*   O painel de cartas de verdade (animação, aviso "JOGADOR X ESCOLHE", cartas na tela) e a tela de Game Over desenhada.
*   Controles reais (DualSense, Pro Controller...): o teste usa um gamepad e um teclado **virtuais**.
*   O mouse, que não pertence a nenhum jogador e sempre consegue clicar.
*   O `PlayerInput` (movimento e ataque) dos jogadores.

---

## 2. Anatomia dos Arquivos

Ficam em `Assets/02. Scripts/Tests/`:

*   **`ExclusiveChoiceSmokeTest.cs` (MonoBehaviour, só no Editor):**
    *   **O que é:** O teste em si. Todo o arquivo está dentro de `#if UNITY_EDITOR`, então **nunca vai para a build do jogo**.
    *   **Para que serve:** Em Play Mode, cria tudo em código (não usa nenhuma cena do jogo): o `AttributeMaestro`, dois jogadores, um gamepad virtual para cada um, um teclado virtual e uma **cópia** das ações de UI (`UI_Controls.inputactions`). Depois mata inimigos comuns e especiais, aperta botões/teclas virtuais e confere o resultado com `Check(...)`.
    *   **Segurança:** trabalha numa cópia das ações de UI e, ao final, remove os dispositivos virtuais e restaura as configurações de input que alterou. Um relógio de segurança de 60 segundos marca falha se o teste travar.
*   **`Editor/ExclusiveChoiceTestRunner.cs` (classe estática de Editor):**
    *   **O que é:** O "botão de iniciar".
    *   **Para que serve:** Guarda quais cenas estavam abertas, abre uma cena vazia, coloca o teste nela e entra em Play Mode. Quando o Play Mode termina, **reabre as suas cenas** e mostra a janela **PASSOU/FALHOU**. Oferece o menu `Tools > Testes > Escolha Exclusiva e Tabela` (uso manual) e o método `RunBatch` (uso por linha de comando, sem janela e sem reabrir cenas).
    *   **Detalhe:** o teste (que roda dentro do jogo) entrega o resultado ao runner por `SessionState` (uma memória temporária do Editor), porque o código de jogo não pode chamar código de Editor.

**Por que não é um teste do "Unity Test Framework"?** Os scripts do jogo ficam no assembly padrão do Unity (`Assembly-CSharp`), e testes criados com assembly próprio (`.asmdef`) não conseguem referenciá-lo. Usar o Test Framework exigiria reorganizar o projeto inteiro em assemblies. Este formato dá o mesmo resultado sem mexer em nada do jogo.

---

## 3. Como executar

### 3.1. Pelo Unity (jeito mais simples)
1.  Abra o projeto no Unity e espere terminar de compilar (sem erros vermelhos no Console).
2.  Se a cena tiver alterações não salvas, o Unity pergunta se quer salvar (não perde nada sem avisar).
3.  Clique em **`Tools > Testes > Escolha Exclusiva e Tabela`**.
4.  O Unity entra em Play Mode sozinho, roda por alguns segundos e sai do Play Mode. **Não clique em nada nesse tempo.**
5.  Aparece uma janela:
    *   **PASSOU!** — todas as checagens deram certo.
    *   **FALHOU** — a janela lista as checagens que quebraram (até 8). Anote ou copie.
    *   **Interrompido** — você saiu do Play Mode antes de acabar, ou houve um erro. Veja o Console.
6.  Sua cena é reaberta sozinha. Se quiser ver o relatório completo, abra o **Console** e digite `[TESTE]` no campo de filtro: cada checagem aparece como `[TESTE] PASSOU - ...` ou `[TESTE] FALHOU - ...`, e a última linha é o resumo.

### 3.2. Por linha de comando (sem abrir a janela)
Serve para rodar o teste sem abrir o editor, ou para automatizar.

**Requisitos:**
*   O **projeto não pode estar aberto** no Unity (o Unity não abre o mesmo projeto duas vezes).
*   O **Unity Hub precisa estar logado** com licença ativa. Sem isso o Unity encerra com o código `198` ("No valid Unity Editor license found").
*   A versão do editor deve ser a do projeto (`ProjectSettings/ProjectVersion.txt`).

**Comando (PowerShell).** Ajuste os caminhos para a sua máquina:

```powershell
$unity = "C:\Program Files\Unity\Hub\Editor\6000.5.0f1\Editor\Unity.exe"
$projeto = "C:\Users\jonfp\Desktop\Atalhos\Jogos_Desenvolvimento\RougueCards-TCC\RougueCards - TCC"
$relatorio = "$env:TEMP\relatorio-escolha-exclusiva.txt"
$log = "$env:TEMP\unity-teste.log"

$argumentos = "-batchmode -nographics -projectPath `"$projeto`" -executeMethod ExclusiveChoiceTestRunner.RunBatch -resultFile `"$relatorio`" -logFile `"$log`""
$p = Start-Process -FilePath $unity -ArgumentList $argumentos -Wait -PassThru
"Código de saída: $($p.ExitCode)"
Get-Content $relatorio -Encoding UTF8
```

O `-Wait` é importante: sem ele o PowerShell volta na hora, sem esperar o Unity terminar.

**Códigos de saída:**

| Código | Significado |
|---|---|
| `0` | Todas as checagens passaram. |
| `1` | Alguma checagem falhou (veja o relatório). |
| `198` | Sem licença válida do Unity (faça login no Hub). |
| outro | O Unity não conseguiu abrir o projeto ou rodar o teste. Abra o arquivo de log e procure por `error CS` (erro de compilação) ou `Exception`. |

O **relatório** (`-resultFile`) é um arquivo de texto com uma linha `PASSOU`/`FALHOU` por checagem. O **log** (`-logFile`) é o log completo do Unity, onde as mesmas linhas aparecem com o prefixo `[TESTE]`.

---

## 4. Como ler e usar o resultado

*   **Tudo passou:** a mecânica se comporta como descrito nos guias `GuiaEscolhaExclusiva.md` e `GuiaTabelaKills.md`.
*   **Algo falhou:** a linha `FALHOU` diz exatamente qual regra quebrou (ex: *"P2 bloqueado NÃO dispara Submit com Enter (disparos: 1)"*). Foi assim que se descobriu que o `Enter` entrava pelo binding curinga `*/{Submit}`.
*   **Falhou "terminou dentro do tempo limite":** o teste travou. Veja o log e procure `Exception`.
*   **Mexeu em `AttributeMaestro`, `Enemy`, `PlayerInputManager`, `CardManager` ou nas ações de UI?** Rode o teste antes de commitar.

---

## 5. Como adicionar uma nova checagem

Tudo fica em `ExclusiveChoiceSmokeTest.cs`, dentro de `RunAll()`:

1.  Crie a situação: `KillBy(player2, special: true)` mata um inimigo especial atribuindo o abate ao jogador 2; `maestro.LockOutNonDecidingPlayer()` e `maestro.RestoreLockedPlayerInput()` simulam abrir e fechar o painel.
2.  Simule o input: `yield return PressSouth(pad1)` (botão do controle) ou `yield return PressKey(Key.Space)` (tecla). Depois, `lastPressCount` diz quantos "Submit" a UI recebeu e `lastNavigateCount` quantas navegações.
3.  Confira: `Check("descrição do que deveria acontecer", condição);`. Se a condição for falsa, a checagem é registrada como `FALHOU` e o resultado final muda.

Lembre das regras do projeto: todo `if` com `{}` e todo método com `<summary>`.

---

## 6. Observações
*   Ao abrir o projeto, o Unity pode regravar arquivos de configuração (por exemplo `ProjectSettings/ShaderGraphSettings.asset`). Isso acontece por causa da versão do editor e **não** faz parte do teste — não precisa entrar no commit.
*   O teste usa `Health.Kill()` com `lastAttacker` definido, que é o mesmo caminho de crédito de abate do jogo. Ele evita o `TakeDamage` de propósito, porque este chama o `PopusManager` (números de dano flutuantes), que exige uma cena configurada.
