# 📑 Manual Técnico: Sistema de Sinergias e Combos

## 1. Anatomia dos Arquivos (O que é e para que serve)

### 1.1. A Camada de Definição (A "Receita")
*   **`ComboData.cs` (ScriptableObject):** 
    *   **O que é:** O DNA de uma sinergia específica.
    *   **Para que serve:** Define os "ingredientes" necessários (quais cartas o jogador precisa ter) e o "prêmio" (qual atributo será melhorado e por quanto tempo). É aqui que você decide se um combo é uma melhoria permanente ou um *buff* temporário de curto prazo.

### 1.2. A Camada de Armazenamento (O "Livro de Receitas")
*   **`ComboDatabase.cs` (ScriptableObject):** 
    *   **O que é:** Um repositório centralizado.
    *   **Para que serve:** O Maestro não sai procurando combos soltos nas pastas do projeto; ele consulta este banco de dados. Se um combo não estiver registrado aqui, ele nunca será ativado, mesmo que o jogador tenha as cartas necessárias.

### 1.3. A Camada de Inventário (A "Mochila")
*   **`PlayerStats.cs` (Extensão):** 
    *   **O que é:** Agora atua como o histórico do jogador.
    *   **Para que serve:** Armazena a lista `inventoryCards`. Toda vez que uma carta é escolhida no `CardManager`, uma cópia dela vai para esta lista. Sem esse rastro, o sistema não saberia quais cartas a dupla já possui para calcular a sinergia.

### 1.4. A Camada de Execução (O "Juiz")
*   **`AttributeMaestro.cs` (Lógica de Sinergia):** 
    *   **O que é:** O motor de busca de combinações.
    *   **Para que serve:** 
        1.  **Unificação:** Ele soma as cartas do Jogador 1 e Jogador 2 para permitir combos cooperativos.
        2.  **Verificação:** Varre o `ComboDatabase` para ver se os requisitos foram batidos.
        3.  **Temporizador:** Gerencia as Corrotinas que removem os bônus após o tempo expirar (Combo Temporário).

---

## 2. Guia Detalhado de Configuração e Expansão

### 2.1. Como Criar um Novo Combo (Ex: "Sinergia de Gelo e Fogo")
Para criar uma nova sinergia sem tocar no código, siga estes passos:

1.  **Crie o arquivo:** No Unity, clique com o botão direito e escolha: **Create > Cards > Combo Synergy**.
2.  **Defina os Requisitos:** Na lista `Required Cards`, arraste as cartas de `CardData` que formam o combo (Ex: Carta "Gelo" e Carta "Fogo").
3.  **Configure o Bônus:** 
    *   Em `Stat To Upgrade`, escolha o atributo (Ex: `Damage`).
    *   Em `Upgrade Value`, defina o valor (Ex: `20`).
4.  **Defina a Duração:** 
    *   Se for `0`, o bônus é permanente.
    *   Se for maior que `0` (Ex: `10`), o bônus sumirá após 10 segundos.
5.  **Registre no Banco:** Arraste este novo arquivo para a lista `All Possible Combos` dentro do seu objeto **`ComboDatabase`**.

### 2.2. Como o Combo é Ativado (Fluxo de Dados)
O sistema segue um gatilho em cadeia toda vez que o painel de cartas fecha:
1.  **`CardManager`** detecta a escolha -> Adiciona a carta ao `inventoryCards` do jogador.
2.  **`CardManager`** chama `AttributeMaestro.CheckForCardCombos()`.
3.  **`Maestro`** junta as listas de cartas do P1 e P2.
4.  **`Maestro`** pergunta para cada combo no banco: *"Os jogadores têm todos os seus requisitos?"*.
5.  Se **SIM**, o bônus é aplicado instantaneamente.

### 2.3. Regras de Sinergia Cooperativa
*   **Compartilhamento:** Se o Combo exige "Espada" e "Escudo", e o Jogador 1 tem a "Espada" enquanto o Jogador 2 tem o "Escudo", o combo **será ativado**.
*   **Efeito Global:** Assim como as cartas normais, o bônus do combo é aplicado a **ambos** os jogadores simultaneamente, reforçando o trabalho em equipe.

---

## 3. Notas de Debug e Verificação

*   **Verificando o Inventário:** Durante o jogo, selecione o Prefab do Jogador na Hierarchy. No script `PlayerStats`, observe a lista `Inventory Cards`. Ela deve crescer a cada carta coletada.
*   **Logs de Ativação:** O Console exibirá uma mensagem em **Amarelo** quando um combo for formado: 
    *   `"COMBO ATIVADO: [Nome do Combo]!"`
*   **Rastreio de Expiração:** Se o combo for temporário, o Console avisará quando o bônus for removido:
    *   `"Combo expirado: [Nome do Combo]"`
*   **Dica de Sinergia Cooperativa:** Se um combo não estiver ativando, verifique se o script `AttributeMaestro` possui as referências corretas de `Player 1` e `Player 2`. Se um dos jogadores estiver nulo, o Maestro só conseguirá checar as cartas de um deles.