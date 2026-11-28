using TMPro;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(TMP_Text))]
public class CurvarTexto : MonoBehaviour
{
    [Range(-360f, 360f)]
    public float angulo = 60f; // positivo: ∩ | negativo: ∪

    private TMP_Text texto;

    void OnEnable()
    {
        texto = GetComponent<TMP_Text>();
        texto.OnPreRenderText += Curvar;
        Atualizar();
    }

    void OnDisable()
    {
        texto.OnPreRenderText -= Curvar;
        Atualizar();
    }

    void OnValidate() => Atualizar();

    // Chame este método se alterar o ângulo por código ou animação
    public void Atualizar()
    {
        if (texto == null) return;
        texto.havePropertiesChanged = true;
        texto.SetVerticesDirty();
    }

    void Curvar(TMP_TextInfo info)
    {
        if (Mathf.Abs(angulo) < 0.01f) return;

        // Descobre a largura do texto
        float minX = float.MaxValue;
        float maxX = float.MinValue;

        for (int i = 0; i < info.characterCount; i++)
        {
            TMP_CharacterInfo c = info.characterInfo[i];
            if (!c.isVisible) continue;

            minX = Mathf.Min(minX, c.bottomLeft.x);
            maxX = Mathf.Max(maxX, c.topRight.x);
        }

        if (maxX <= minX) return;

        float centro = (minX + maxX) / 2f;
        float raio = (maxX - minX) / (angulo * Mathf.Deg2Rad);

        // Posiciona e rotaciona cada letra
        for (int i = 0; i < info.characterCount; i++)
        {
            TMP_CharacterInfo c = info.characterInfo[i];
            if (!c.isVisible) continue;

            Vector3[] vertices = info.meshInfo[c.materialReferenceIndex].vertices;
            int v = c.vertexIndex;

            float meioX = (vertices[v].x + vertices[v + 2].x) / 2f;
            Vector3 pivo = new Vector3(meioX, c.baseLine, 0f);

            float teta = (meioX - centro) / raio;
            Vector3 novoPivo = new Vector3(
                centro + raio * Mathf.Sin(teta),
                c.baseLine - raio * (1f - Mathf.Cos(teta)),
                0f);

            Quaternion rotacao = Quaternion.Euler(0f, 0f, -teta * Mathf.Rad2Deg);

            for (int j = 0; j < 4; j++)
                vertices[v + j] = rotacao * (vertices[v + j] - pivo) + novoPivo;
        }
    }
}