// using UnityEngine;
// using Oculus.Haptics;
// using UnityEngine.SceneManagement;

// public class StartPanel : MonoBehaviour
// {
//     [Header("Glow Settings")]
//     public Color glowColor = Color.yellow;
//     public float glowIntensity = 2.0f;

//     private Renderer panelRenderer;
//     private Material panelMaterial;
//     private Color originalEmission;
//     private bool isGlowing = false;

//     [Header("UI Setting")]
//     public PanelAction panelAction = PanelAction.None;
//     public bool isselected = false;
//     public Material Selected;
//     public Material NotSelected;

//     [Header("Hover Animation")]
//     public Vector3 enlargedScale = new Vector3(1.2f, 1.2f, 1.2f);
//     public float floatHeight = 0.05f;
//     public float animationSpeed = 6f;

//     private Vector3 originalScale;
//     private Vector3 originalPosition;
//     private Vector3 targetScale;
//     private Vector3 targetPosition;

//     private bool isHovered = false;

//     public enum PanelAction
//     {
//         None,
//         Easy,
//         Intermediate,
//         ShapeOnly,
//         ColorOnly,
//         OneMin,
//         ThreeMin,
//         FiveMin,
//         TenMin,
//         Start,
//         Home
//     }

//     [Header("Haptics")]
//     public HapticClip clip1;
//     private HapticClipPlayer player;

//     void Awake()
//     {
//         player = new HapticClipPlayer(clip1);
//     }

//     void Start()
//     {
//         panelRenderer = GetComponent<Renderer>();
//         if (panelRenderer != null)
//         {
//             panelMaterial = panelRenderer.material;
//             panelMaterial.EnableKeyword("_EMISSION");
//             originalEmission = panelMaterial.GetColor("_EmissionColor");
//         }

//         // Save original transform states
//         originalScale = transform.localScale;
//         originalPosition = transform.localPosition;
//         targetScale = originalScale;
//         targetPosition = originalPosition;
//     }

//     void Update()
//     {
//         if (panelRenderer == null) return;

//         // Material selection visual
//         panelRenderer.material = isselected ? Selected : NotSelected;

//         // Smoothly interpolate to target position and scale
//         transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
//         transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * animationSpeed);
//     }

//     public void OnTriggerPressed()
//     {
//         Debug.Log("Start Panel Trigger Pressed!");

//         SceneSelection scene = FindObjectOfType<SceneSelection>();
//         UIManager ui = FindObjectOfType<UIManager>();
//         UIManager02 ui2 = FindObjectOfType<UIManager02>();
//         GameManager gm = FindObjectOfType<GameManager>();
//         GameManager02 gm2 = FindObjectOfType<GameManager02>();

//         switch (panelAction)
//         {
//             case PanelAction.Easy:
//                 scene?.loadEasy();
//                 break;

//             case PanelAction.Intermediate:
//                 scene?.loadIntermediate();
//                 break;

//             case PanelAction.ShapeOnly:
//                 ui?.SetShapeOnly();
//                 break;

//             case PanelAction.ColorOnly:
//                 ui?.SetColorOnly();
//                 break;

//             case PanelAction.OneMin:
//                 if (ui != null) ui.SetOneMin();
//                 else if (ui2 != null) ui2.SetOneMin();
//                 else Debug.LogWarning("No UIManager found!");
//                 break;

//             case PanelAction.ThreeMin:
//                 if (ui != null) ui.SetThreeMin();
//                 else if (ui2 != null) ui2.SetThreeMin();
//                 else Debug.LogWarning("No UIManager found!");
//                 break;

//             case PanelAction.FiveMin:
//                 if (ui != null) ui.SetFiveMin();
//                 else if (ui2 != null) ui2.SetFiveMin();
//                 else Debug.LogWarning("No UIManager found!");
//                 break;

//             case PanelAction.TenMin:
//                 if (ui != null) ui.SetTenMin();
//                 else if (ui2 != null) ui2.SetTenMin();
//                 else Debug.LogWarning("No UIManager found!");
//                 break;

//             case PanelAction.Start:
//                 if (gm != null)
//                     gm.startEasyGame();
//                 else if (gm2 != null)
//                     gm2.StartMidGame();
//                 else
//                     Debug.LogWarning("No GameManager found in this scene!");
//                 break;
            
//             case PanelAction.Home:
//                 SceneManager.LoadScene("MainMenu");
//                 break;


//             case PanelAction.None:
//             default:
//                 Debug.Log("No panel action assigned.");
//                 break;
//         }
//     }

//     void OnCollisionEnter(Collision collision)
//     {
//         if (collision.gameObject.CompareTag("GrabPole"))
//         {
//             SetGlow(true);
//             isHovered = true;
//             targetScale = originalScale * 1.1f;
//             targetPosition = originalPosition + new Vector3(0, floatHeight, 0);
//             PlayHapticClip1();
//         }
//     }

//     void OnCollisionExit(Collision collision)
//     {
//         if (collision.gameObject.CompareTag("GrabPole"))
//         {
//             SetGlow(false);
//             isHovered = false;
//             targetScale = originalScale;
//             targetPosition = originalPosition;
//         }
//     }

//     private void SetGlow(bool glow)
//     {
//         if (panelMaterial == null) return;

//         if (glow && !isGlowing)
//         {
//             panelMaterial.SetColor("_EmissionColor", glowColor * glowIntensity);
//             DynamicGI.SetEmissive(panelRenderer, glowColor * glowIntensity);
//             isGlowing = true;
//         }
//         else if (!glow && isGlowing)
//         {
//             panelMaterial.SetColor("_EmissionColor", originalEmission);
//             DynamicGI.SetEmissive(panelRenderer, originalEmission);
//             isGlowing = false;
//         }
//     }

//     public void PlayHapticClip1()
//     {
//         player.Play(Controller.Right);
//     }

//     public void StopHaptics()
//     {
//         player.Stop();
//     }
// }
