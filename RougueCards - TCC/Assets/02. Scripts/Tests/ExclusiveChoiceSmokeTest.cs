#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using RougueCards.Attributes;
using RougueCards.GameOver;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

/// <summary>
/// Teste de fumaça (roda em Play Mode, somente no Editor) da escolha exclusiva de carta e da contagem de abates da tabela.
/// Cria dois jogadores com gamepads virtuais, inimigos comuns e especiais, e confere se o decisor muda,
/// se o jogador sem prioridade perde o dispositivo (e com ele o Submit da UI) e se os abates chegam à tabela.
/// Não usa cena do jogo: tudo é criado em código. Disparado por <c>ExclusiveChoiceTestRunner</c>.
/// </summary>
public class ExclusiveChoiceSmokeTest : MonoBehaviour
{
    /// <summary> Chaves de SessionState usadas para entregar o resultado ao ExclusiveChoiceTestRunner (que roda no Editor e mostra a janela). </summary>
    public const string PendingKey = "ExclusiveChoiceTest.Pending";
    public const string ScenesKey = "ExclusiveChoiceTest.Scenes";
    public const string ActiveSceneKey = "ExclusiveChoiceTest.ActiveScene";
    public const string TotalKey = "ExclusiveChoiceTest.Total";
    public const string FailuresKey = "ExclusiveChoiceTest.Failures";
    public const string FailedKey = "ExclusiveChoiceTest.Failed";

    private const string UiControlsPath = "Assets/UI_Controls.inputactions";
    private const float TimeoutSeconds = 60f;

    private readonly List<string> report = new List<string>();
    private int failures;
    private bool finished;

    private AttributeMaestro maestro;
    private PlayerStats player1;
    private PlayerStats player2;
    private Gamepad pad1;
    private Gamepad pad2;

    private Keyboard keyboard;

    private InputActionAsset uiActions;
    private int submitCount;
    private int navigateCount;
    private int lastPressCount;
    private int lastNavigateCount;
    private int player2Kills;

    private InputSettings.BackgroundBehavior previousBackground;
    private InputSettings.EditorInputBehaviorInPlayMode previousEditorBehavior;

    /// <summary> Executa a bateria de checagens e encerra o Play Mode ao final. </summary>
    private IEnumerator Start()
    {
        StartCoroutine(Watchdog());

        yield return RunAll();

        Finish();
    }

    /// <summary> Garante que o teste nunca fique travado: se estourar o tempo, marca falha e encerra. </summary>
    private IEnumerator Watchdog()
    {
        yield return new WaitForSecondsRealtime(TimeoutSeconds);

        if (!finished)
        {
            Check("O teste terminou dentro do tempo limite", false, $"{TimeoutSeconds}s");
            Finish();
        }
    }

    /// <summary> Sequência completa de checagens. </summary>
    private IEnumerator RunAll()
    {
        Setup();
        yield return null;

        // --- Controle: sem bloqueio, os dois jogadores conseguem enviar Submit (valida o próprio teste) ---
        yield return PressSouth(pad1);
        Check("Controle: sem bloqueio, o gamepad do P1 dispara Submit na UI", lastPressCount > 0);
        yield return PressSouth(pad2);
        Check("Controle: sem bloqueio, o gamepad do P2 dispara Submit na UI", lastPressCount > 0);

        // --- Regra de decisão ---
        player1.currentCombo = 5;
        player2.currentCombo = 0;
        Check("Sem inimigo especial morto, decide quem tem mais combo (P1)", Decider() == player1);

        KillBy(player2, special: false);
        yield return null;
        Check("Inimigo comum morto pelo P2 NÃO troca o decisor", Decider() == player1);

        KillBy(player2, special: true);
        yield return null;
        Check("Inimigo especial morto pelo P2 faz o P2 decidir, mesmo com combo menor", Decider() == player2);

        KillBy(player1, special: true);
        yield return null;
        Check("Outro especial (spawn repetido) morto pelo P1 devolve a decisão ao P1", Decider() == player1);

        KillBy(player2, special: true);
        yield return null;
        Check("Especial seguinte morto pelo P2 passa a decisão ao P2", Decider() == player2);

        // --- Bloqueio de dispositivo e Submit da UI (decisor = P2, bloqueado = P1) ---
        maestro.LockOutNonDecidingPlayer();
        Check("Com P2 decidindo, o gamepad do P1 (bloqueado) está desativado", !pad1.enabled);
        Check("Com P2 decidindo, o gamepad do P2 (decisor) continua ativo", pad2.enabled);

        yield return PressSouth(pad1);
        Check("Jogador bloqueado (P1) NÃO consegue disparar Submit na UI", lastPressCount == 0, $"disparos: {lastPressCount}");
        yield return PressSouth(pad2);
        Check("Jogador decisor (P2) consegue disparar Submit na UI", lastPressCount > 0);

        maestro.RestoreLockedPlayerInput();
        yield return null;
        Check("Ao fechar o painel, o gamepad do P1 é reativado", pad1.enabled);

        yield return PressSouth(pad1);
        Check("Depois de restaurado, o P1 volta a disparar Submit na UI", lastPressCount > 0);

        // --- Interruptor do controle: desligado, os dois escolhem ---
        maestro.exclusiveChoiceGamepad = false;
        maestro.LockOutNonDecidingPlayer();
        Check("Exclusividade do controle desligada: ninguém é bloqueado", maestro.LockedOutPlayer == null);
        Check("Exclusividade do controle desligada: o gamepad do P1 continua ativo", pad1.enabled);
        yield return PressSouth(pad1);
        Check("Exclusividade do controle desligada: o P1 também dispara Submit na UI", lastPressCount > 0);
        maestro.RestoreLockedPlayerInput();
        maestro.exclusiveChoiceGamepad = true;

        // --- Teclado: os dois jogadores passam a usar o mesmo teclado virtual (P1 = WASD, P2 = setas) ---
        maestro.RegisterPlayerDevice(1, null);
        maestro.RegisterPlayerDevice(2, null);
        maestro.RegisterPlayerKeyboard(1, KeyboardLayout.Wasd);
        maestro.RegisterPlayerKeyboard(2, KeyboardLayout.Arrows);
        keyboard = InputSystem.AddDevice<Keyboard>("TesteTeclado");

        yield return PressKey(Key.Space);
        Check("Controle: sem bloqueio, Espaço dispara Submit na UI", lastPressCount > 0);
        yield return PressKey(Key.Enter);
        Check("Controle: sem bloqueio, Enter dispara Submit na UI", lastPressCount > 0);
        yield return PressKey(Key.W);
        Check("Controle: sem bloqueio, W navega na UI", lastNavigateCount > 0);
        yield return PressKey(Key.UpArrow);
        Check("Controle: sem bloqueio, seta para cima navega na UI", lastNavigateCount > 0);

        KillBy(player2, special: true);
        yield return null;
        maestro.LockOutNonDecidingPlayer();
        Check("Teclado: P2 decide, então o P1 (WASD) fica bloqueado", maestro.LockedOutPlayer == player1);

        yield return PressKey(Key.Space);
        Check("Teclado: P1 bloqueado NÃO dispara Submit com Espaço", lastPressCount == 0, $"disparos: {lastPressCount}");
        yield return PressKey(Key.W);
        Check("Teclado: P1 bloqueado NÃO navega com W", lastNavigateCount == 0, $"navegações: {lastNavigateCount}");
        yield return PressKey(Key.Enter);
        Check("Teclado: P2 (decisor) dispara Submit com Enter", lastPressCount > 0);
        yield return PressKey(Key.UpArrow);
        Check("Teclado: P2 (decisor) navega com a seta para cima", lastNavigateCount > 0);

        maestro.RestoreLockedPlayerInput();
        yield return PressKey(Key.Space);
        Check("Teclado: depois de restaurado, Espaço volta a disparar Submit", lastPressCount > 0);
        Check("Teclado: nenhuma tecla de UI ficou alterada depois de restaurar", CountBindingOverrides() == 0, $"alteradas: {CountBindingOverrides()}");

        KillBy(player1, special: true);
        yield return null;
        maestro.LockOutNonDecidingPlayer();
        Check("Teclado: P1 decide, então o P2 (setas) fica bloqueado", maestro.LockedOutPlayer == player2);

        yield return PressKey(Key.Enter);
        Check("Teclado: P2 bloqueado NÃO dispara Submit com Enter", lastPressCount == 0, $"disparos: {lastPressCount}");
        yield return PressKey(Key.RightArrow);
        Check("Teclado: P2 bloqueado NÃO navega com as setas", lastNavigateCount == 0, $"navegações: {lastNavigateCount}");
        yield return PressKey(Key.Space);
        Check("Teclado: P1 (decisor) dispara Submit com Espaço", lastPressCount > 0);
        maestro.RestoreLockedPlayerInput();

        // --- Interruptor do teclado: desligado, os dois escolhem ---
        maestro.exclusiveChoiceKeyboard = false;
        maestro.LockOutNonDecidingPlayer();
        Check("Exclusividade do teclado desligada: ninguém é bloqueado", maestro.LockedOutPlayer == null);
        yield return PressKey(Key.Enter);
        Check("Exclusividade do teclado desligada: o P2 também dispara Submit", lastPressCount > 0);
        maestro.RestoreLockedPlayerInput();
        maestro.exclusiveChoiceKeyboard = true;

        // --- Tabela: os abates contados chegam ao dado que alimenta a tabela ---
        GameOverPlayerStats table2 = GameOverStatsBuilder.Build(player2);
        Check("Tabela do P2 tem a linha 'Abates'", table2.Stats.Count > 0 && table2.Stats[0].Label == "Abates");
        Check(
            "Tabela do P2 mostra a mesma quantidade de abates que o jogo contou",
            table2.Stats.Count > 0 && table2.Stats[0].Value == player2Kills.ToString(),
            $"esperado {player2Kills}, tabela {(table2.Stats.Count > 0 ? table2.Stats[0].Value : "vazia")}");
    }

    /// <summary> Cria o Maestro, os dois jogadores com gamepads virtuais e a cópia das ações de UI do projeto. </summary>
    private void Setup()
    {
        InputSettings settings = InputSystem.settings;
        previousBackground = settings.backgroundBehavior;
        previousEditorBehavior = settings.editorInputBehaviorInPlayMode;
        settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;

        pad1 = InputSystem.AddDevice<Gamepad>("TestePad1");
        pad2 = InputSystem.AddDevice<Gamepad>("TestePad2");

        AttributeMaestro.Instance = null;
        maestro = new GameObject("Maestro").AddComponent<AttributeMaestro>();

        player1 = CreatePlayer(1, pad1);
        player2 = CreatePlayer(2, pad2);

        InputActionAsset original = AssetDatabase.LoadAssetAtPath<InputActionAsset>(UiControlsPath);
        if (original == null)
        {
            Check("Asset de ações de UI encontrado", false, UiControlsPath);
            return;
        }

        uiActions = InputActionAsset.FromJson(original.ToJson());
        InputAction submit = uiActions.FindAction("UI/Submit", true);
        submit.performed += _ => submitCount++;
        InputAction navigate = uiActions.FindAction("UI/Navigate", true);
        navigate.performed += context =>
        {
            if (context.ReadValue<Vector2>() != Vector2.zero)
            {
                navigateCount++;
            }
        };
        uiActions.Enable();

        maestro.uiActions = uiActions;
    }

    /// <summary> Cria um jogador mínimo (apenas PlayerStats) e o registra no Maestro. </summary>
    private PlayerStats CreatePlayer(int id, Gamepad pad)
    {
        PlayerStats stats = new GameObject($"Jogador{id}").AddComponent<PlayerStats>();
        stats.playerID = id;

        if (id == 1)
        {
            maestro.player1 = stats;
        }
        else
        {
            maestro.player2 = stats;
        }

        maestro.RegisterPlayerDevice(id, pad);
        return stats;
    }

    /// <summary>
    /// Cria um inimigo (comum ou especial) e o mata atribuindo o abate ao jogador informado,
    /// pelo mesmo caminho usado no jogo: Health.lastAttacker + Die().
    /// </summary>
    private void KillBy(PlayerStats killer, bool special)
    {
        EnemyData data = ScriptableObject.CreateInstance<EnemyData>();
        data.isSpecialEnemy = special;
        data.maxHealth = 10;

        GameObject enemyObject = new GameObject(special ? "InimigoEspecial" : "InimigoComum");
        Enemy enemy = enemyObject.AddComponent<Enemy>();
        enemy.Initialize(data);

        Health health = enemyObject.GetComponent<Health>();
        health.lastAttacker = killer;
        health.Kill();

        if (killer == player2)
        {
            player2Kills++;
        }
    }

    /// <summary> Aperta e solta o botão Sul do gamepad e guarda em <see cref="lastPressCount"/> quantos Submit a UI recebeu. </summary>
    private IEnumerator PressSouth(Gamepad pad)
    {
        int before = submitCount;

        InputSystem.QueueStateEvent(pad, new GamepadState().WithButton(GamepadButton.South));
        yield return null;
        yield return null;

        InputSystem.QueueStateEvent(pad, new GamepadState());
        yield return null;

        lastPressCount = submitCount - before;
    }

    /// <summary>
    /// Aperta e solta uma tecla do teclado virtual e guarda em <see cref="lastPressCount"/> e
    /// <see cref="lastNavigateCount"/> quantos Submit e quantas navegações a UI recebeu.
    /// </summary>
    private IEnumerator PressKey(Key key)
    {
        int submitBefore = submitCount;
        int navigateBefore = navigateCount;

        InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
        yield return null;
        yield return null;

        InputSystem.QueueStateEvent(keyboard, new KeyboardState());
        yield return null;

        lastPressCount = submitCount - submitBefore;
        lastNavigateCount = navigateCount - navigateBefore;
    }

    /// <summary> Conta quantos bindings das ações de UI estão com alguma alteração (override) aplicada. </summary>
    private int CountBindingOverrides()
    {
        int count = 0;

        foreach (InputAction action in uiActions)
        {
            foreach (InputBinding binding in action.bindings)
            {
                if (binding.overridePath != null)
                {
                    count++;
                }
            }
        }

        return count;
    }

    private PlayerStats Decider()
    {
        return maestro.GetDecidingPlayer();
    }

    /// <summary> Registra o resultado de uma checagem. </summary>
    private void Check(string name, bool condition, string detail = "")
    {
        if (!condition)
        {
            failures++;
        }

        string suffix = string.IsNullOrEmpty(detail) ? string.Empty : $" ({detail})";
        string line = $"{(condition ? "PASSOU" : "FALHOU")} - {name}{suffix}";
        report.Add(line);
        Debug.Log($"[TESTE] {line}");
    }

    /// <summary> Escreve o relatório, limpa o que foi criado e encerra o Play Mode (ou o Editor, em modo batch). </summary>
    private void Finish()
    {
        if (finished)
        {
            return;
        }

        finished = true;

        int total = report.Count;
        string summary = failures == 0
            ? $"TODOS OS TESTES PASSARAM ({total})"
            : $"{failures} DE {total} CHECAGENS FALHARAM";
        StoreResultForEditor(total);
        report.Add(summary);
        Debug.Log($"[TESTE] {summary}");

        WriteReportFile();
        Cleanup();

        if (Application.isBatchMode)
        {
            EditorApplication.Exit(failures == 0 ? 0 : 1);
        }
        else
        {
            EditorApplication.isPlaying = false;
        }
    }

    /// <summary> Guarda o resultado em SessionState para o ExclusiveChoiceTestRunner mostrar a janela de "passou/falhou" quando o Play Mode terminar. </summary>
    /// <param name="total">Quantas checagens foram feitas.</param>
    private void StoreResultForEditor(int total)
    {
        List<string> failedLines = new List<string>();

        foreach (string line in report)
        {
            if (line.StartsWith("FALHOU"))
            {
                failedLines.Add(line);
            }
        }

        SessionState.SetInt(TotalKey, total);
        SessionState.SetInt(FailuresKey, failures);
        SessionState.SetString(FailedKey, string.Join("\n", failedLines));
    }

    /// <summary> Se o argumento -resultFile foi passado na linha de comando, grava o relatório nesse arquivo. </summary>
    private void WriteReportFile()
    {
        string[] args = System.Environment.GetCommandLineArgs();

        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "-resultFile")
            {
                File.WriteAllLines(args[i + 1], report);
                return;
            }
        }
    }

    /// <summary> Remove gamepads virtuais e restaura as configurações de input alteradas pelo teste. </summary>
    private void Cleanup()
    {
        if (uiActions != null)
        {
            uiActions.Disable();
        }

        if (pad1 != null)
        {
            InputSystem.RemoveDevice(pad1);
        }

        if (pad2 != null)
        {
            InputSystem.RemoveDevice(pad2);
        }

        if (keyboard != null)
        {
            InputSystem.RemoveDevice(keyboard);
        }

        InputSettings settings = InputSystem.settings;
        settings.backgroundBehavior = previousBackground;
        settings.editorInputBehaviorInPlayMode = previousEditorBehavior;
    }
}
#endif
