using UnityEngine;

public class HurtFlashRenderer : MonoBehaviour
{
    [Header(" ‹…À∑∫∫Ï")]
    [Tooltip("µ•Œª∫¡√Î")]
    public int FlashMs = 250;
    [Tooltip("∑∫∫Ï«ø∂»")]
    public float EmissionPower = 3f;
    [Header("π«˜¿‰÷»æ∆˜")]
    public SkinnedMeshRenderer TargetSkinRender;

    public Material[] InstanceMats;
    public TimeMgr.TimerTask ActiveTimer;
    public void OnEnable()
    {
        if (InstanceMats != null)
        {
            foreach (var mat in InstanceMats)
            {
                mat.SetFloat("_Emission", 0f);
            }
        }
    }
    public void Start()
    {
        ClearTimer();
    }
    public void InitMaterial()
    {
        if (TargetSkinRender == null)
        {
            return;
        }

        InstanceMats = new Material[TargetSkinRender.materials.Length];
        for (int i = 0; i < TargetSkinRender.materials.Length; i++)
        {
            Material srcMat = TargetSkinRender.materials[i];
            Material newMat = new Material(srcMat);
            newMat.EnableKeyword("_EMISSION");
            newMat.SetColor("_EmissionColor", Color.red);
            newMat.DisableKeyword("_EMISSION");
            //newMat.SetFloat("_Emission", 0f);
            InstanceMats[i] = newMat;
        }
        TargetSkinRender.materials = InstanceMats;
    }
    public void PlayFlashRed()
    {
        if (InstanceMats == null || InstanceMats.Length == 0)
        {
            return;
        }
        if (ActiveTimer != null)
        {
            TimeMgr.instance.StopTimer(ActiveTimer);
        }
        foreach (var mat in InstanceMats)
        {
            Debug.Log("±‰∫Ï");
            mat.EnableKeyword("_EMISSION");
            mat.SetFloat("_Emission", EmissionPower);
        }

        float durationSec = FlashMs / 1000f;
        ActiveTimer = TimeMgr.instance.CreateTimer(
            TimeMgr.TimerMode.DeltaTime,
            0,
            durationSec,
            null,
            OnFlashEnd
        );
    }
    private void OnFlashEnd()
    {
        foreach (var mat in InstanceMats)
        {
            mat.DisableKeyword("_EMISSION");
        }
        ActiveTimer = null;
    }
    public void ClearTimer()
    {
        if (ActiveTimer != null)
        {
            TimeMgr.instance.StopTimer(ActiveTimer);
            ActiveTimer = null;
        }
    }
}