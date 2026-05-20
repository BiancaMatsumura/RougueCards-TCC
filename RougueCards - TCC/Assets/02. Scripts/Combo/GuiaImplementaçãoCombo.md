Este guia detalha a implementação do sistema de **Sinergia de Cartas (Combos)**, que permite criar efeitos especiais baseados na combinação de cartas específicas coletadas pelos jogadores, com suporte a bônus temporários e configuração via ScriptableObjects.

---

# 📑 Manual de Sinergias: Sistema de Combos de Cartas

## 1. O que foi feito?
Criamos uma camada de inteligência acima do sistema de cartas. Agora, o jogo não apenas aplica bônus individuais, mas monitora o "Inventário" da dupla de jogadores. Quando um conjunto específico de cartas é reunido, um **Combo** é disparado automaticamente.

### Características Principais:
*   **Configuração Externa:** Combos são criados como arquivos (ScriptableObjects), sem precisar programar cada nova sinergia.
*   **Duração Configurável:** Combos podem ser permanentes ou temporários (expiram após X segundos).
*   **Sinergia Cooperativa:** O sistema soma as cartas do Jogador 1 e do Jogador 2 para verificar se o requisito foi atingido.

---

## 2. Exemplo Prático: Implementando o Combo "Super Velocidade"

Este combo exige que os jogadores possuam as cartas "Tênis de Corrida" e "Energético" para ganhar um bônus massivo de velocidade por 10 segundos.

### Passo 1: O Inventário (PlayerStats.cs)
Cada jogador agora possui uma lista que armazena as cartas coletadas.
```csharp
// Dentro de PlayerStats.cs
public List<CardData> inventoryCards = new List<CardData>();

public void AddCardToInventory(CardData card) {
    if (!inventoryCards.Contains(card)) inventoryCards.Add(card);
}
```

### Passo 2: A Verificação (AttributeMaestro.cs)
O Maestro une as cartas de ambos os jogadores e compara com a lista de requisitos do Combo.
```csharp
// Dentro de AttributeMaestro.cs
public void CheckForCardCombos() {
    List<CardData> allCards = new List<CardData>();
    allCards.AddRange(player1.inventoryCards);
    allCards.AddRange(player2.inventoryCards);

    foreach (var combo in comboDatabase.allPossibleCombos) {
        if (combo.IsSatisifed(allCards)) {
            ActivateCombo(combo);
        }
    }
}
```

### Passo 3: O Tempo Limite (ComboData.cs)
O combo define se é temporário. Se for, o Maestro inicia uma contagem regressiva para remover o bônus.
```csharp
// Lógica de expiração no Maestro
private IEnumerator RemoveComboAfterTime(ComboData combo) {
    yield return new WaitForSecondsRealtime(combo.comboDuration);
    // Remove o bônus aplicando o valor negativo
    ApplySharedUpgrade(combo.statToUpgrade, -combo.upgradeValue, combo.isPercentage);
}
```

---

## 3. Como Criar um Novo Combo (Fluxo de Trabalho)

Para adicionar uma nova sinergia ao seu jogo, siga este padrão:

1.  **Criar as Cartas**: Certifique-se de que as cartas individuais (ex: "Bala de Prata" e "Mira Laser") já existem como `CardData`.
2.  **Criar o Combo**: Botão direito nos Assets > `Create > Cards > Combo Synergy`.
3.  **Configurar Requisitos**: Arraste as cartas necessárias para a lista `Required Cards`.
4.  **Definir o Prêmio**: Escolha qual atributo será melhorado (ex: `CritChance`) e o valor.
5.  **Definir o Tempo**: Se quiser que dure apenas uma fase ou momento, coloque o tempo em `Combo Duration`. Use `0` para bônus permanente.

---

## 4. Configuração no Unity (Exemplo Prático)

Para configurar o combo "Vampirismo Supremo" (Exige as cartas *Dano* e *Vida*):

1.  **Arquivo Combo**: Crie o `ComboData` chamado "SinergiaVampirica".
2.  **Required Cards**: Adicione sua carta de `Dano` e sua carta de `Vida`.
3.  **Stat to Upgrade**: Selecione `LifeSteal`.
4.  **Upgrade Value**: Coloque `10`.
5.  **Combo Duration**: Coloque `15` (segundos).
6.  **Database**: **IMPORTANTE!** Arraste este novo arquivo de Combo para a lista `All Possible Combos` no seu objeto `ComboDatabase`.

---

## 5. Resumo de Código (Summary)

*   **`ComboData.cs`**: ScriptableObject que define as "regras" da sinergia (quem precisa de quem e o que ganha).
*   **`ComboDatabase.cs`**: Centralizador que o Maestro consulta para saber quais combos existem no jogo.
*   **`PlayerStats.cs`**: Agora atua como um inventário, guardando a lista de `CardData` coletadas pelo jogador.
*   **`AttributeMaestro.cs`**: Monitora a união das cartas da dupla, ativa os efeitos e gerencia o tempo de expiração via Corrotinas.
*   **`CardManager.cs`**: Ao coletar uma carta, ele agora executa três ações: Adiciona ao inventário, aplica o efeito base e pede ao Maestro para checar combos.