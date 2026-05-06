Este guia detalha a integração final do sistema de atributos e fornece um passo a passo técnico de como a mecânica de **Knockback (Repulsão)** foi implementada utilizando a arquitetura de upgrades.

---

# 📑 Manual de Atributos: Implementação de Knockback

## 1. O que foi feito?
Conectamos as lógicas de gameplay que já existiam (movimento, dano, colisão) ao sistema de **StatSheet**. Agora, variáveis que antes eram fixas (como a força de empurrão de uma bala ou o ganho de XP) são lidas em tempo real da ficha de atributos do jogador.

### Atributos Totalmente Conectados:
*   **Sobrevivência:** `MaxHP`, `Armor` (Redução), `Evasion` (Esquiva).
*   **Ataque:** `Damage`, `AttackSpeed`, `ProjectileQty` (Multishot), `CritChance`, `CritDamage`, `Knockback`.
*   **Movimentação:** `MoveSpeed`, `JumpHeight`, `Size` (Escala física e visual).
*   **Utilidade:** `CollectionRange` (Ímã de XP), `XPBoost` (Multiplicador de ganho), `Duration` (Tempo de vida da bala).

---

## 2. Exemplo Prático: Implementando o Knockback

O Knockback é o exemplo perfeito de "Atributo de Fluxo", pois ele nasce no **Jogador**, passa pela **Bala** e afeta o **Inimigo**.

### Passo 1: O Atirador (AutoShooter.cs)
O atirador consulta a ficha do jogador para saber quanta força de repulsão ele possui e "injeta" esse valor na bala no momento da criação.

```csharp
// Dentro de AutoShooter.Shoot()
float finalKnockback = pStats != null ? pStats.stats.Knockback.Value : 0f;

// O Init da bala agora recebe este 5º parâmetro
b.Init(dir, (int)finalDamage, finalProjSpeed, finalLifeTime, finalKnockback);
```

### Passo 2: O Projétil (Bullet.cs)
A bala atua como o mensageiro. Ela carrega a força e, ao detectar um inimigo, entrega a "mensagem" física.

```csharp
// Dentro de Bullet.OnTriggerEnter()
var enemy = other.GetComponent<Enemy>();
if (enemy != null && knockbackForce > 0)
{
    // Passa a direção em que a bala viajava e a força do atributo
    enemy.ApplyKnockback(direction, knockbackForce);
}
```

### Passo 3: O Receptor (Enemy.cs)
O inimigo recebe a força e a processa em seu próprio `Update`. Criamos um sistema de "atrito" para que ele não deslize infinitamente.

```csharp
private Vector3 knockbackVelocity;
[SerializeField] private float knockbackResistance = 5f;

public void ApplyKnockback(Vector3 direction, float force) {
    knockbackVelocity = direction * force;
}

void Update() {
    if (knockbackVelocity.magnitude > 0.01f) {
        transform.position += knockbackVelocity * Time.deltaTime;
        // Faz a força diminuir gradualmente até parar
        knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, Time.deltaTime * knockbackResistance);
    }
    // ... resto da lógica de perseguição ...
}
```

---

## 3. Como Implementar Novos Upgrades (Fluxo de Trabalho)

Se você quiser adicionar um atributo que ainda não existe (ex: "Chance de Congelar"), siga este padrão:

1.  **StatType.cs**: Adicione `FreezeChance` ao Enum.
2.  **StatSheet.cs**: Adicione `public PlayerAttributes FreezeChance = new() { BaseValue = 0 };`.
3.  **PlayerStats.cs**: Adicione o `case StatType.FreezeChance => stats.FreezeChance;` no método `GetStat`.
4.  **Gameplay**: No script de dano, faça:
    ```csharp
    float chance = pStats.stats.FreezeChance.Value;
    if(Random.Range(0, 100) < chance) { /* Lógica de congelar */ }
    ```

---

## 4. Configuração da Carta no Unity (Exemplo Knockback)

Para que o upgrade funcione no jogo sem programar mais nada:

1.  **Criar o Arquivo**: Botão direito nos Assets > `Create > Cards > CardData`.
2.  **Nome**: "Impacto Pesado".
3.  **Stat To Upgrade**: Selecione `Knockback` no menu suspenso.
4.  **Upgrade Value**: Coloque `10` (para um empurrão forte) ou `2` (para algo sutil).
5.  **Is Percentage**: 
    *   **Desmarcado**: Soma +10 à força base.
    *   **Marcado**: Multiplica a força atual (ex: 1.5 aumenta em 50%).
6.  **Database**: Arraste esta carta para o seu arquivo `CardDatabase` na lista `All Cards`.

---

## 5. Resumo de Código (Summary)

*   **`AutoShooter.cs`**: Agora é o provedor de dados para os projéteis, unificando arma + atributos.
*   **`Bullet.cs`**: Tornou-se um objeto dinâmico que não possui valores fixos, apenas executa as ordens recebidas no `Init`.
*   **`Enemy.cs`**: Recebeu uma camada física simples para reagir aos impactos do sistema de combate.
*   **`PlayerController.cs`**: Agora gerencia a escala do `CharacterController` para garantir que o tamanho do colisor físico seja sempre idêntico ao tamanho visual do modelo.