### 1. Sistema de Atributos Dinâmicos (O Motor)
Saímos de variáveis simples para um sistema matemático robusto:
*   **PlayerAttributes**: Classe que calcula o valor final usando a fórmula `(Base + Aditivo) * Multiplicador`. Isso permite bônus permanentes e temporários.
*   **StatSheet**: Uma ficha completa com mais de 30 atributos (Dano, Vida, Velocidade, Sorte, Coleta, etc.).
*   **AttributeMaestro**: O "regente" que garante que, quando um jogador pega uma carta, o bônus seja aplicado a **ambos** os jogadores (Efeito Compartilhado). Ele também gerencia quem decide a carta baseado no combo de mortes.

### 2. Sistema de Cartas e Upgrades
*   **CardData (ScriptableObjects)**: Criamos "DNAs" de cartas que podem aumentar atributos ou desbloquear armas novas.
*   **CardManager**: Gerencia a interface de seleção, pausa o jogo, sorteia cartas do banco de dados e aplica os efeitos.
*   **CardController**: Cuida da parte visual e da animação de "flip" (virar) das cartas na UI.

### 3. Sistema de Combate Escalável
*   **AutoShooter**: O sistema de tiro agora lê os atributos do jogador. O dano, a velocidade do tiro, a cadência (Attack Speed) e a quantidade de balas (Multishot) mudam em tempo real conforme as cartas coletadas.
*   **RangedWeaponData**: Permite criar armas diferentes (Pistola, Escopeta, Metralhadora) apenas mudando um arquivo de dados.
*   **Bullet**: O projétil é dinâmico; ele recebe sua força, dano e tempo de vida do atirador no momento do disparo.
*   **Knockback (Repulsão)**: Implementamos o impacto físico onde os tiros empurram os inimigos baseados no atributo de força do jogador.

### 4. Sistema de Inimigos Modulares
*   **EnemyData**: Criamos um sistema onde você define a vida, velocidade, dano e visual do inimigo em um ScriptableObject.
*   **Enemy (IA)**: Um script único e autônomo que pode se transformar em qualquer monstro (Zumbi, Boss, Slime) ao ler o `EnemyData`.
*   **EnemySpawner**: Controla a população de inimigos, evita "nascimentos" antes dos jogadores entrarem e sorteia diferentes tipos de monstros para a cena.

### 5. Mecânicas Cooperativas e Sobrevivência
*   **Downed State & Revive**: Se um jogador perde toda a vida, ele entra em estado "abatido". O parceiro tem um tempo para chegar perto e revivê-lo (segurando o botão), caso contrário, é Game Over.
*   **Split Screen Dinâmico**: A câmera se divide automaticamente se os jogadores se afastarem e se une quando estão próximos.
*   **Seta de Indicação**: Adicionamos setas na UI que apontam para a direção do parceiro quando eles estão em telas separadas.

### 6. Sistema de Sinergia (Combos de Cartas)
*   **ComboData**: Sistema que verifica se a dupla possui cartas específicas (ex: "Fogo" + "Gelo").
*   **Combo Temporário**: Se a combinação for feita, um bônus especial (ex: +50% de Dano) é ativado por um tempo configurável e depois removido automaticamente.

### 7. Core Loop e UI
*   **PlayerProgress**: Gerencia o ganho de XP compartilhado e a subida de nível.
*   **UI Controllers**: Telas de Pause, Game Over e Menu Principal totalmente funcionais e integradas ao sistema de Input (Teclado e Gamepad).
*   **Health & XP Bars**: Barras visuais que refletem exatamente os valores da `StatSheet` e do `PlayerProgress`.