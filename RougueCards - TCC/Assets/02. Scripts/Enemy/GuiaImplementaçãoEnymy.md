Este guia detalha a arquitetura do sistema modular de **Inimigos**, que utiliza **ScriptableObjects** para permitir a criação de diversos tipos de oponentes sem a necessidade de novos scripts, integrando-se aos sistemas de saúde, dano e knockback.

---

# 📑 Manual Técnico: Sistema de Inimigos Modulares

## 1. Anatomia dos Arquivos (O que é e para que serve)

### 1.1. A Camada de Dados (O "DNA")
*   **`EnemyData.cs` (ScriptableObject):** 
    *   **O que é:** Um arquivo de configuração que armazena as estatísticas base.
    *   **Para que serve:** Define o nome, o modelo 3D (`visualPrefab`), o controlador de animação, a vida máxima, a velocidade, o dano ao jogador e quanto de XP ele concede. Isso permite criar inimigos únicos (ex: Zumbi Lento vs. Morcego Rápido) apenas criando novos arquivos na pasta do projeto.

### 1.2. A Camada de Lógica (O "Cérebro")
*   **`Enemy.cs` (MonoBehaviour):**
    *   **O que é:** O script principal que controla o comportamento do inimigo.
    *   **Para que serve:** 
        1.  **Inicialização:** No momento em que nasce, ele "lê" o `EnemyData` e configura o visual e os atributos.
        2.  **IA de Perseguição:** Busca o jogador mais próximo e se move em sua direção.
        3.  **Física:** Processa a força de **Knockback** recebida pelos tiros do jogador.

### 1.3. A Camada de Produção (A "Fábrica")
*   **`EnemySpawner.cs` (MonoBehaviour):**
    *   **O que é:** O gerenciador de população na cena.
    *   **Para que serve:** Controla quantos inimigos existem simultaneamente, o intervalo entre os surgimentos e sorteia qual tipo de inimigo (baseado na lista de `EnemyData`) será instanciado. Ele aguarda a existência de jogadores antes de iniciar o ciclo.

---

## 2. Guia de Implementação: Criando um Novo Inimigo

Siga este fluxo para adicionar um novo tipo de oponente ao seu jogo:

### Passo 1: Definir os Dados (`EnemyData`)
1.  No Unity, vá em sua pasta de inimigos, clique com o botão direito e escolha: **Create > Gameplay > Enemy Data**.
2.  Dê um nome (ex: `Slime_Verde`).
3.  No Inspetor, arraste o modelo 3D para `Visual Prefab` e o seu Animator para `Animator Controller`.
4.  Defina os atributos: `Speed: 2`, `Max Health: 30`, `Damage: 5`, `XP Value: 10`.

### Passo 2: O Prefab Base
Você só precisa de **um** Prefab base de inimigo que contenha os seguintes scripts:
*   `Enemy.cs`
*   `Health.cs`
*   `DamageDealer.cs` (com a tag "Player" configurada para o dano)
*   `EnemyXP.cs`
*   `CharacterController` (para movimentação física)

### Passo 3: Configurar o Spawner
1.  Selecione o objeto **EnemySpawner** na sua cena.
2.  Arraste o seu **Prefab Base** para a lista `Enemy Base Prefabs`.
3.  Arraste o seu arquivo `Slime_Verde` (o `EnemyData`) para a lista `Available Enemy Data`.

---

## 3. Exemplo Prático: Inimigo "Tanque" (Lento e Resistente)

Para criar um inimigo que aguente muitos tiros mas seja fácil de desviar:

1.  **EnemyData**: Crie um novo arquivo chamado `Enemy_Tank`.
2.  **Configuração**:
    *   `Speed`: 1.5 (Metade da velocidade normal).
    *   `Max Health`: 300 (Muita vida).
    *   `Damage To Player`: 30 (Dano massivo se encostar).
    *   `Model Scale`: 1.5 (Deixe-o visualmente maior para intimidar).
3.  **Resultado**: Ao ser instanciado, o script `Enemy.cs` aplicará a escala 1.5, configurará o `Health` para 300 e o inimigo perseguirá o jogador lentamente.

---

## 4. Notas de Debug e Verificação

*   **Identificação de Erros**: Se o inimigo não estiver causando dano, verifique se o script `DamageDealer.cs` está configurado para detectar a Tag **"Player"** e se o seu jogador possui essa tag.
*   **Fogo Amigo**: Se os inimigos estiverem se matando, verifique se você desmarcou a colisão entre a layer "Enemy" e "Enemy" nas configurações de física do projeto.
*   **Atraso no Início**: O Spawner possui uma trava de segurança: ele **não criará nada** até que o `PlayerInputManager` instancie pelo menos um jogador com a tag "Player".
*   **Console**: O sistema avisará quando um inimigo for inicializado:
    *   `"[Enemy] Inicializado: Nome_Do_Inimigo com HP: 100"`

---

## 5. Resumo de Código (Summary)

*   **`EnemyData.cs`**: Atua como a "ficha de RPG" do inimigo. É totalmente independente de cena.
*   **`Enemy.cs`**: Centraliza a IA e a física de impacto. É o responsável por transformar o Prefab genérico no inimigo específico definido pelo `EnemyData`.
*   **`EnemySpawner.cs`**: Otimizado para evitar "burst spawn" (múltiplos nascimentos no mesmo frame) e respeitar o limite de população da fase.
*   **`DamageDealer.cs`**: Agora filtrado para ignorar outros inimigos, garantindo que o desafio seja apenas entre o jogador e as criaturas.