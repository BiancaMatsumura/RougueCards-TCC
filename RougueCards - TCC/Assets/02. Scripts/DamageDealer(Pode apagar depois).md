Este guia detalha as alterações realizadas no script `DamageDealer` e as configurações obrigatórias nos **Prefabs** para garantir que o sistema de dano e física funcione corretamente, evitando que os inimigos atravessem o jogador ou parem de causar dano.

---

# 📑 Manual de Integração: Sistema de Dano e Colisão

## 1. Alterações no Código (`DamageDealer.cs`)

O script foi otimizado para resolver o problema de "dano intermitente" e garantir que o primeiro impacto seja sempre registrado.

### O que mudou:
*   **Dano Instantâneo:** Substituímos o contador acumulativo por uma lógica de `nextDamageTime`. Isso garante que, ao encostar no player, o dano ocorra no **primeiro frame** (milissegundo 0), e só depois comece a contar o intervalo.
*   **Prevenção de "Physics Sleep":** Adicionamos `rb.WakeUp()`. Na Unity, se dois objetos ficam parados um contra o outro, a física "dorme" para poupar CPU. Isso acordará o motor de física para que o `OnTriggerStay` continue disparando.
*   **Reset no Egresso:** O `OnTriggerExit` agora zera o tempo, permitindo que se o inimigo sair e voltar rapidamente, o dano seja aplicado instantaneamente de novo.

---

## 2. Configuração do Prefab: PLAYER

Para evitar conflitos físicos (como o player ser "empurrado" ou tremer), siga esta configuração:

1.  **Tag:** Deve ser obrigatoriamente **"Player"**.
2.  **Colisores:** Use apenas o **Character Controller**. Se houver um *Capsule Collider* ou *Box Collider* extra no objeto pai, **remova-o**. Ter dois colisores no mesmo objeto causa bugs de detecção.
3.  **Rigidbody:** Se o seu player tiver um Rigidbody, marque a caixa **Is Kinematic**. Isso impede que forças externas (como o inimigo batendo nele) movam o player de forma não controlada pelo seu script de movimento.

---

## 3. Configuração do Prefab: INIMIGO

O inimigo agora funciona como um "Gatilho de Dano" (Trigger), o que permite que ele encoste no player sem causar erros de colisão sólida.

1.  **Rigidbody (Obrigatório):** Adicione um componente Rigidbody.
    *   **Is Kinematic:** Marcado (True).
    *   **Collision Detection:** Mude para **Continuous** (melhora a precisão em altas velocidades).
2.  **Box Collider:**
    *   **Is Trigger:** Marcado (True). 
    *   *Nota: O script `Enemy` ajusta o tamanho deste colisor automaticamente baseando-se no modelo visual.*
3.  **Camadas (Layers):** Recomendado colocar o Inimigo na Layer "Enemy" e o Player na Layer "Player". Verifique em *Project Settings > Physics* se essas duas camadas estão autorizadas a colidir.

---

## 4. Resumo Técnico (Summary)

*   **`DamageDealer.cs`**: Atua como o motor de ataque. Ele não precisa saber quem é o inimigo, apenas que "algo com a tag Player" entrou em seu raio de ação.
*   **Physics Pair**: Para que um Trigger funcione, um dos dois objetos **precisa** ter um Rigidbody. Como o Player usa Character Controller (que é um colisor especial), o Rigidbody **deve** estar no Inimigo.
*   **Estabilidade**: Ao manter o Inimigo como `IsTrigger` e o Player como `IsKinematic`, eliminamos o bug do inimigo "empurrar" o jogador para fora do mapa ou para dentro do chão.