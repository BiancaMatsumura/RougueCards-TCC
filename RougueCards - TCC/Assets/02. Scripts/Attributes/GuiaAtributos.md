#  Manual Técnico: Ecossistema de Atributos e Upgrades

## 1. Anatomia dos Arquivos (O que é e para que serve)

### 1.1. A Camada de Definição (Os "Nomes")
*   **`StatType.cs` (Enum):** 
    *   **O que é:** Uma lista fixa de nomes (IDs).
    *   **Para que serve:** Serve como a "Língua Comum" do projeto. Quando uma Carta diz que aumenta o "Dano", ela usa esse Enum. Isso evita erros de digitação e permite que o sistema localize o atributo correto instantaneamente.

### 1.2. A Camada de Lógica Matemática (O "Motor")
*   **`PlayerAttributes.cs` (Classe C#):**
    *   **O que é:** A lógica matemática pura de cada atributo individual.
    *   **Para que serve:** Ela separa o **Valor Base** (que nunca muda) dos **Bônus**. 
    *   **Por que é assim:** Em vez de simplesmente somar valores, esta classe guarda: `Base + Aditivo * Multiplicador`. Isso permite criar bônus temporários ou permanentes sem quebrar a matemática original do personagem.

*   **`StatSheet.cs` (Classe de Organização):**
    *   **O que é:** Uma grande "tabela" que agrupa todos os `PlayerAttributes`.
    *   **Para que serve:** Organização visual e estrutural. Agrupa os 30+ atributos em categorias: *Sobrevivência, Ataque, Movimentação e Economia*, facilitando a inspeção no Unity.

### 1.3. A Camada de Execução (Os "Agentes")
*   **`PlayerStats.cs` (MonoBehaviour):**
    *   **O que é:** O representante da ficha de atributos dentro do boneco do jogador.
    *   **Para que serve:** 
        1.  **Armazenamento:** Carrega a `StatSheet` individual.
        2.  **Combo:** Monitora o contador de mortes e o tempo de expiração.
        3.  **Ponte:** Possui a função `GetStat()`, que traduz um `StatType` no atributo real para ser modificado.

*   **`AttributeMaestro.cs` (Singleton / Maestro):**
    *   **O que é:** O regente supremo da cena.
    *   **Para que serve:** 
        1.  **Conexão:** Guarda a referência do Player 1 e Player 2.
        2.  **Efeito Compartilhado:** Aplica upgrades em todos os jogadores simultaneamente.
        3.  **Sinergia de Combo:** Dá bônus ao parceiro quando o jogador entra em combo.
        4.  **Arbitragem:** Decide quem tem prioridade de escolha na UI (maior combo).

### 1.4. A Camada de Conteúdo e UI (As "Cartas")
*   **`CardData.cs` (ScriptableObject):** O "DNA" de uma carta (ícone, custo de XP, atributo e valor).
*   **`CardDatabase.cs` (ScriptableObject):** Repositório que filtra cartas baseadas no XP.
*   **`CardManager.cs` (MonoBehaviour):** Gerente da UI que pausa o jogo e processa a escolha do jogador.

---

## 2. Guia Detalhado de Configuração e Expansão

Este guia explica o passo a passo para adicionar novos conteúdos ao sistema sem quebrar a lógica existente.

### 2.1. Como Adicionar um Novo Atributo (Ex: "Tamanho da Explosão")
Para que um novo atributo exista e possa ser melhorado por cartas, você deve seguir esta trilha de 3 arquivos:

1.  **No arquivo `StatType.cs`:** Adicione o nome do novo atributo ao final da lista `enum`. 
    *   *Ex: `ExplosionSize,`*
2.  **No arquivo `StatSheet.cs`:** Crie a variável que guardará os dados desse atributo.
    *   *Ex: `public PlayerAttributes ExplosionSize = new() { BaseValue = 1.0f };`* (O 1.0f é o valor inicial padrão).
3.  **No arquivo `PlayerStats.cs`:** No método `GetStat(StatType type)`, adicione uma nova linha ao `switch` para que o sistema saiba onde encontrar esse dado.
    *   *Ex: `case StatType.ExplosionSize: return stats.ExplosionSize;`*

**Resultado:** O atributo agora aparecerá no Inspector do Player e poderá ser selecionado em qualquer arquivo de Carta (`CardData`).

### 2.2. Como Criar e Configurar uma Nova Carta de Upgrade
As cartas são arquivos de dados que não exigem programação para serem criadas:
1.  No Unity, vá na pasta de sua preferência, clique com o botão direito e escolha: **Create > Cards > CardData**.
2.  **Configuração de Imagem:** Coloque um Sprite em `Front Image` (o que o jogador vê ao virar) e em `Back Image` (o verso da carta).
3.  **Configuração de XP:** Em `Xp Required`, defina quanto XP o grupo precisa ter para que essa carta comece a aparecer no sorteio.
4.  **Configuração do Efeito:** 
    *   Em `Stat To Upgrade`, selecione o atributo (Ex: `Damage`).
    *   Em `Upgrade Value`, coloque o valor (Ex: `0.5`).
    *   Marque `Is Percentage` se quiser que o valor seja multiplicado (Ex: +50% de dano). Deixe desmarcado para valores fixos (Ex: +10 de Vida).
5.  **Registro:** Arraste esse novo arquivo criado para a lista **All Cards** no seu objeto **`CardDatabase`**. Se não arrastar, ela nunca será sorteada.

### 2.3. Como Configurar o Sistema em uma Nova Cena
Se você criar uma fase nova, siga esta ordem de conexão:
1.  **Game Controller:** Crie um objeto vazio e anexe o `AttributeMaestro`, o `PlayerInputManager` e o `PlayerProgress`.
2.  **UI:** Certifique-se de que o objeto com o `UIDocument` tenha o script `CardManager`. No `CardManager`, arraste o seu `CardDatabase` e o seu `PlayerProgress` nos campos vazios.
3.  **Prefabs:** Verifique se os prefabs dos seus jogadores têm o script `PlayerStats`.
4.  **Spawn:** No `PlayerInputManager`, arraste os Prefabs dos jogadores e defina os pontos de nascimento (Transforms). O código fará o resto da conexão automaticamente ao dar Play.

### 2.4. Ajustando o Equilíbrio do Combo
Você pode ajustar quão forte é a sinergia entre os jogadores:
*   **Tempo de Combo:** No `PlayerStats`, mude o `Combo Duration` para aumentar ou diminuir a janela de tempo que o jogador tem entre uma morte e outra.
*   **Valor do Boost:** No `AttributeMaestro`, dentro de `OnPlayerComboStarted`, você pode alterar o valor do bônus que o amigo recebe (atualmente definido como 0.1f / 10%).

---

## 3. Notas de Debug e Verificação
*   **Inspector em Tempo Real:** Para confirmar se um upgrade funcionou, selecione o jogador na Hierarchy enquanto o jogo roda. Abra o script `PlayerStats` > `Stats` > [Atributo Escolhido]. Observe o campo `additiveBonus`. Se ele mudou de 0 para o valor da carta, o sistema processou corretamente.
*   **Console:** O sistema está configurado para avisar no Console:
    *   *"O jogador [ID] decide qual carta pegar!"* (Informa quem teve o maior combo).
    *   *"Upgrade aplicado a ambos!"* (Confirma o sucesso da operação cooperativa).
    *   *"XP compartilhado: [Valor]"* (Confirma que a coleta de XP está funcionando).