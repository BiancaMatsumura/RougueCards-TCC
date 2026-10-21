using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Dispara o <see cref="ExclusiveChoiceSmokeTest"/>.
/// No Editor: menu Tools > Testes > Escolha Exclusiva e Tabela. Ao terminar, mostra uma janela "PASSOU/FALHOU" e reabre a sua cena.
/// Em linha de comando: Unity -batchmode -projectPath "..." -executeMethod ExclusiveChoiceTestRunner.RunBatch -resultFile relatorio.txt
/// </summary>
[InitializeOnLoad]
public static class ExclusiveChoiceTestRunner
{
    private const int MaxFailuresInDialog = 8;

    /// <summary> Registra o aviso de "Play Mode terminou", usado para reabrir a cena e mostrar o resultado. </summary>
    static ExclusiveChoiceTestRunner()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    /// <summary> Atalho de menu. Pergunta antes de descartar alterações não salvas da cena aberta. </summary>
    [MenuItem("Tools/Testes/Escolha Exclusiva e Tabela")]
    public static void RunFromMenu()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        Run(interactive: true);
    }

    /// <summary> Ponto de entrada para o modo batch (sem janela). O resultado sai no código de saída e em -resultFile. </summary>
    public static void RunBatch()
    {
        Run(interactive: false);
    }

    /// <summary> Guarda quais cenas estavam abertas (só no modo interativo), abre uma cena vazia, coloca o teste nela e entra em Play Mode. </summary>
    /// <param name="interactive">Verdadeiro quando disparado pelo menu: reabre as cenas e mostra a janela no final.</param>
    private static void Run(bool interactive)
    {
        SessionState.SetBool(ExclusiveChoiceSmokeTest.PendingKey, interactive);
        SessionState.SetInt(ExclusiveChoiceSmokeTest.TotalKey, 0);
        SessionState.SetInt(ExclusiveChoiceSmokeTest.FailuresKey, 0);
        SessionState.SetString(ExclusiveChoiceSmokeTest.FailedKey, string.Empty);

        if (interactive)
        {
            SaveOpenScenes();
        }

        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        new GameObject("ExclusiveChoiceSmokeTest").AddComponent<ExclusiveChoiceSmokeTest>();
        EditorApplication.isPlaying = true;
    }

    /// <summary> Anota os caminhos das cenas abertas (e qual era a ativa) para reabri-las depois do teste. </summary>
    private static void SaveOpenScenes()
    {
        List<string> paths = new List<string>();

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);

            if (scene.isLoaded && !string.IsNullOrEmpty(scene.path))
            {
                paths.Add(scene.path);
            }
        }

        SessionState.SetString(ExclusiveChoiceSmokeTest.ScenesKey, string.Join("|", paths));
        SessionState.SetString(ExclusiveChoiceSmokeTest.ActiveSceneKey, SceneManager.GetActiveScene().path);
    }

    /// <summary> Quando voltar para o modo de edição depois do teste, reabre as cenas e mostra o resultado. </summary>
    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredEditMode || !SessionState.GetBool(ExclusiveChoiceSmokeTest.PendingKey, false))
        {
            return;
        }

        SessionState.SetBool(ExclusiveChoiceSmokeTest.PendingKey, false);

        EditorApplication.delayCall += () =>
        {
            RestoreScenes();
            ShowResultDialog();
        };
    }

    /// <summary> Reabre as cenas que estavam abertas antes do teste. </summary>
    private static void RestoreScenes()
    {
        string saved = SessionState.GetString(ExclusiveChoiceSmokeTest.ScenesKey, string.Empty);
        if (string.IsNullOrEmpty(saved))
        {
            return;
        }

        string[] paths = saved.Split('|');

        for (int i = 0; i < paths.Length; i++)
        {
            EditorSceneManager.OpenScene(paths[i], i == 0 ? OpenSceneMode.Single : OpenSceneMode.Additive);
        }

        string active = SessionState.GetString(ExclusiveChoiceSmokeTest.ActiveSceneKey, string.Empty);
        Scene activeScene = SceneManager.GetSceneByPath(active);
        if (activeScene.IsValid())
        {
            SceneManager.SetActiveScene(activeScene);
        }
    }

    /// <summary> Mostra uma janela simples dizendo se o teste passou ou falhou e, se falhou, quais checagens. </summary>
    private static void ShowResultDialog()
    {
        int total = SessionState.GetInt(ExclusiveChoiceSmokeTest.TotalKey, 0);
        int failures = SessionState.GetInt(ExclusiveChoiceSmokeTest.FailuresKey, 0);
        string failed = SessionState.GetString(ExclusiveChoiceSmokeTest.FailedKey, string.Empty);

        string message;

        if (total == 0)
        {
            message = "O teste foi interrompido antes de terminar (você saiu do Play Mode ou houve um erro).\n\n" +
                      "Olhe o Console para ver o motivo. Sua cena foi reaberta.";
        }
        else if (failures == 0)
        {
            message = $"PASSOU!\n\nTodas as {total} checagens deram certo.\nSua cena foi reaberta.";
        }
        else
        {
            message = $"FALHOU: {failures} de {total} checagens falharam.\n\n{LimitLines(failed)}\n\n" +
                      "Veja o Console (digite [TESTE] no filtro) e o guia GuiaTestes.md. Sua cena foi reaberta.";
        }

        EditorUtility.DisplayDialog("Teste da Escolha Exclusiva e da Tabela", message, "OK");
    }

    /// <summary> Limita a quantidade de linhas mostradas na janela para ela não ficar gigante. </summary>
    /// <param name="lines">Linhas separadas por quebra de linha.</param>
    private static string LimitLines(string lines)
    {
        string[] all = lines.Split('\n');

        if (all.Length <= MaxFailuresInDialog)
        {
            return lines;
        }

        string shown = string.Join("\n", all, 0, MaxFailuresInDialog);
        return $"{shown}\n... e mais {all.Length - MaxFailuresInDialog}.";
    }
}
