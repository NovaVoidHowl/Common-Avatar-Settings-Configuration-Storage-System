#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Animations;
using AnimatorController = UnityEditor.Animations.AnimatorController;
using AnimatorControllerParameterType = UnityEngine.AnimatorControllerParameterType;

namespace com.NovaVoidHowl.dev.unity.CASCSS
{
  [CustomEditor(typeof(AvatarsSetup))]
  public class AvatarsSetupEditor : Editor
  {
    private AvatarsSetup AvatarSetupScript;
    Color voicePositionColour = new Color(0.941f, 0.553f, 0.02f, 1.0f);
    Color viewPositionColour = new Color(0.545f, 0.09f, 0.106f, 1.0f);
    Color foldoutTitleBackgroundColor = new Color(0.18f, 0.18f, 0.18f, 1.0f);

    private static string[] _visemeNames = new[]
    {
      "sil",
      "PP",
      "FF",
      "TH",
      "DD",
      "kk",
      "CH",
      "SS",
      "nn",
      "RR",
      "aa",
      "E",
      "ih",
      "oh",
      "ou"
    };
    private List<string> _blendShapeNames = null;

    private Transform realLeftEye;
    private Transform realRightEye;

    private void OnEnable()
    {
      AvatarSetupScript = (AvatarsSetup)target;
    }

    public override void OnInspectorGUI()
    {
      AvatarSetupScript = (AvatarsSetup)target;
      bool compatibleSDKsExist = IsCVR_CCKExists() || IsVRC_SDK_VRCSDK3Exists();

      // Get the Animator component on the same GameObject
      var animator = AvatarSetupScript.GetComponent<Animator>();

      if (!compatibleSDKsExist)
      {
        EditorGUILayout.HelpBox(Constants.ERROR_MISSING_SDK, MessageType.Error);
        if (GUILayout.Button("ChilloutVR CCK Download"))
        {
          Application.OpenURL(Constants.CCK_CCK_ULR);
        }
        if (GUILayout.Button("VRChat SDK3 Download"))
        {
          Application.OpenURL(Constants.VRC_SDK_ULR);
        }
        return; // Hide the rest of the GUI
      }
      if (animator == null)
      {
        EditorGUILayout.HelpBox(Constants.WARNING_MISSING_ANIMATOR, MessageType.Warning);
        return; // Hide the rest of the GUI
      }

      if (!animator.isHuman)
      {
        EditorGUILayout.HelpBox(Constants.WARNING_NOT_HUMANOID, MessageType.Warning);
      }

      // Get the eye bones from the Animator component
      realLeftEye = animator.GetBoneTransform(HumanBodyBones.LeftEye);
      realRightEye = animator.GetBoneTransform(HumanBodyBones.RightEye);

      if (realLeftEye == null || realRightEye == null)
      {
        EditorGUILayout.HelpBox(Constants.WARNING_EYE_BONE_NOT_FOUND, MessageType.Warning);
      }

      if (AvatarSetupScript.use_BlinkBlendshapes)
      {
#if VRC_SDK_VRCSDK3
        if (realLeftEye == null || realRightEye == null)
        {
          EditorGUILayout.HelpBox(Constants.VRC_WARNING_EYE_BONE_NOT_FOUND, MessageType.Warning);
        }
#endif
      }

      //// check if any avatar components found
      int avatarComponentsFound = GetAvatarComponentCount();

      // cleanup settings if no avatar components found
      SettingsCleanupByComponentsFound(avatarComponentsFound);

      RenderSetupButtonsSection(AvatarSetupScript, avatarComponentsFound);

      EditorGUILayout.Space();
      EditorGUILayout.Space();

      renderAvatarComponentImportSection(AvatarSetupScript, avatarComponentsFound);
      if (AvatarSetupScript.GetIsConfigured())
      {
        EditorGUILayout.Space();
        renderHorizontalSeparator();
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(Constants.INFO_VIEW_ONLY_MODE, MessageType.Info);
      }

      EditorGUILayout.Space();

      renderVoiceAndViewPosition(AvatarSetupScript);

      EditorGUILayout.Space();
      EditorGUI.BeginDisabledGroup(AvatarSetupScript.GetIsConfigured());
      EditorGUILayout.LabelField("Face viseme options ", EditorStyles.boldLabel);
      if (AvatarSetupScript.bodyMesh == null)
      {
        EditorGUILayout.HelpBox(Constants.AVATAR_INFO_VISEMES_INFO, MessageType.Info);
      }
      AvatarSetupScript.bodyMesh = (SkinnedMeshRenderer)
        EditorGUILayout.ObjectField("Face Visemes Mesh", AvatarSetupScript.bodyMesh, typeof(SkinnedMeshRenderer), true);
      EditorGUI.EndDisabledGroup();
      if (AvatarSetupScript.bodyMesh != null)
      {
        if (!AvatarSetupScript.use_BlinkBlendshapes)
        {
          clearBlinkBlendshapes();
        }
        if (!AvatarSetupScript.use_visemes)
        {
          clearVisemeBlendshapes();
        }

        if (_blendShapeNames == null)
        {
          _blendShapeNames = GetBlendShapeNames(AvatarSetupScript.bodyMesh.GetComponent<SkinnedMeshRenderer>());
        }

        EditorGUILayout.Space();
        EditorGUI.BeginDisabledGroup(AvatarSetupScript.GetIsConfigured());
        EditorGUILayout.LabelField("Face Settings ", EditorStyles.boldLabel);
        GUI.backgroundColor = Color.white; // prevent bleed through from previous GUI.backgroundColor
        AvatarSetupScript.use_BlinkBlendshapes = EditorGUILayout.Toggle(
          "Use Blink Blendshapes",
          AvatarSetupScript.use_BlinkBlendshapes
        );
        EditorGUI.EndDisabledGroup();
        if (AvatarSetupScript.use_BlinkBlendshapes)
        {
          RenderFoldoutSection(
            "Chillout VR Specific",
            ref AvatarSetupScript.cvrBlinkShowSection,
            () =>
            {
              renderCVRBlinkLayers();
            }
          );

          RenderFoldoutSection(
            "VRChat Specific",
            ref AvatarSetupScript.vrcBlinkShowSection,
            () =>
            {
              renderVRCblinkBlendshape();
              renderVRClookUpBlendshape();
              renderVRClookDownBlendshape();
            }
          );
        }
        RenderFoldoutSection(
          "Eye Look Settings",
          ref AvatarSetupScript.eyeLookShowSection,
          () =>
          {
            renderEyeBoneConfig();
            renderEyeLookRotationConfig();
          }
        );

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUI.BeginDisabledGroup(AvatarSetupScript.GetIsConfigured());
        EditorGUILayout.LabelField("Viseme Settings ", EditorStyles.boldLabel);
        GUI.backgroundColor = Color.white; // prevent bleed through from previous GUI.backgroundColor
        AvatarSetupScript.use_visemes = EditorGUILayout.Toggle("Use Speech Visemes", AvatarSetupScript.use_visemes);
        EditorGUI.EndDisabledGroup();
        if (AvatarSetupScript.use_visemes)
        {
          if (AvatarSetupScript.bodyMesh != null)
          {
            RenderFoldoutSection(
              "Viseme List",
              ref AvatarSetupScript.VisemesShowSection,
              () =>
              {
                renderVisemeParameters();
              }
            );
          }
        }
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUI.BeginDisabledGroup(AvatarSetupScript.GetIsConfigured());
        EditorGUILayout.LabelField("Animation Layers ", EditorStyles.boldLabel);
        EditorGUI.EndDisabledGroup();
        RenderFoldoutSection(
          "VRChat Specific",
          ref AvatarSetupScript.vrcLayerShowSection,
          () =>
          {
            renderVRCLayers();
          }
        );
      }
      else
      {
        EditorGUI.BeginDisabledGroup(AvatarSetupScript.GetIsConfigured()); // if configured disable the fields
        EditorGUILayout.LabelField("To use visemes you need to set the `Face Visemes Mesh` first ");
        AvatarSetupScript.use_visemes = false;
        AvatarSetupScript.use_BlinkBlendshapes = false;
        EditorGUI.EndDisabledGroup();
        clearVisemeBlendshapes();
        clearBlinkBlendshapes();
      }

      EditorGUILayout.Space();
      renderAvatarMenuSection(AvatarSetupScript);
      GUI.enabled = true; // prevent the GUI disable from persisting outside of this script

      renderCredits();
    }

    private void renderEyeLookRotationConfig()
    {
      EditorGUI.BeginDisabledGroup(AvatarSetupScript.GetIsConfigured()); // if configured disable the fields
      GUILayoutOption[] eyeAngleBoxOptions = new GUILayoutOption[] { GUILayout.Width(75), };

      AvatarSetupScript.perEyeRotations = EditorGUILayout.Toggle(
        "Custom Limits for each eye",
        AvatarSetupScript.perEyeRotations
      );

      EditorGUILayout.LabelField(
        $"{(AvatarSetupScript.perEyeRotations ? "Left" : "")}Eye Angle Limits:",
        EditorStyles.boldLabel
      );

      EditorGUILayout.BeginHorizontal();
      GUILayout.FlexibleSpace();
      AvatarSetupScript.lMinY = Mathf.Abs(EditorGUILayout.FloatField(AvatarSetupScript.lMinY, eyeAngleBoxOptions));
      AvatarSetupScript.leftEyeMinY = -AvatarSetupScript.lMinY;
      GUILayout.FlexibleSpace();
      EditorGUILayout.EndHorizontal();

      EditorGUILayout.BeginHorizontal();
      GUILayout.FlexibleSpace();
      AvatarSetupScript.lMaxX = Mathf.Abs(EditorGUILayout.FloatField(AvatarSetupScript.lMaxX, eyeAngleBoxOptions));
      AvatarSetupScript.leftEyeMaxX = AvatarSetupScript.lMaxX;

      GUILayout.FlexibleSpace();

      AvatarSetupScript.lMinX = Mathf.Abs(EditorGUILayout.FloatField(AvatarSetupScript.lMinX, eyeAngleBoxOptions));
      AvatarSetupScript.leftEyeMinX = -AvatarSetupScript.lMinX;
      GUILayout.FlexibleSpace();
      EditorGUILayout.EndHorizontal();

      EditorGUILayout.BeginHorizontal();
      GUILayout.FlexibleSpace();
      AvatarSetupScript.lMaxY = Mathf.Abs(EditorGUILayout.FloatField(AvatarSetupScript.lMaxY, eyeAngleBoxOptions));
      AvatarSetupScript.leftEyeMaxY = AvatarSetupScript.lMaxY;
      GUILayout.FlexibleSpace();
      EditorGUILayout.EndHorizontal();

      EditorGUILayout.Space();

      if (AvatarSetupScript.perEyeRotations)
      {
        EditorGUILayout.Separator();

        EditorGUILayout.LabelField($"Right Eye Angle Limits:", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        AvatarSetupScript.rMinY = Mathf.Abs(EditorGUILayout.FloatField(AvatarSetupScript.rMinY, eyeAngleBoxOptions));
        AvatarSetupScript.rightEyeMinY = -AvatarSetupScript.rMinY;
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        AvatarSetupScript.rMaxX = Mathf.Abs(EditorGUILayout.FloatField(AvatarSetupScript.rMaxX, eyeAngleBoxOptions));
        AvatarSetupScript.rightEyeMaxX = AvatarSetupScript.rMaxX;

        GUILayout.FlexibleSpace();

        AvatarSetupScript.rMinX = Mathf.Abs(EditorGUILayout.FloatField(AvatarSetupScript.rMinX, eyeAngleBoxOptions));
        AvatarSetupScript.rightEyeMinX = -AvatarSetupScript.rMinX;
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        AvatarSetupScript.rMaxY = Mathf.Abs(EditorGUILayout.FloatField(AvatarSetupScript.rMaxY, eyeAngleBoxOptions));
        AvatarSetupScript.rightEyeMaxY = AvatarSetupScript.rMaxY;
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
      }
      else
      {
        AvatarSetupScript.rightEyeMinX = -AvatarSetupScript.lMaxX;
        AvatarSetupScript.rightEyeMaxX = AvatarSetupScript.lMinX;
        AvatarSetupScript.rightEyeMinY = -AvatarSetupScript.lMinY;
        AvatarSetupScript.rightEyeMaxY = AvatarSetupScript.lMaxY;
      }
      EditorGUI.EndDisabledGroup();
    }

    private void renderEyeBoneConfig()
    {
      EditorGUI.BeginDisabledGroup(AvatarSetupScript.GetIsConfigured()); // if configured disable the fields
      if (realLeftEye != null && realRightEye != null)
      {
        if (GUILayout.Button("Auto select eye bones"))
        {
          AutoSetEyeBones();
        }
      }
      AvatarSetupScript.leftEyeBone = (Transform)
        EditorGUILayout.ObjectField("Left Eye Bone", AvatarSetupScript.leftEyeBone, typeof(Transform), true);
      AvatarSetupScript.rightEyeBone = (Transform)
        EditorGUILayout.ObjectField("Right Eye Bone", AvatarSetupScript.rightEyeBone, typeof(Transform), true);
      EditorGUI.EndDisabledGroup();
    }

    private void renderVRCblinkBlendshape()
    {
      EditorGUI.BeginDisabledGroup(AvatarSetupScript.GetIsConfigured()); // if configured disable the fields
      int counterVRCblink = 0;
      for (int j = 0; j < _blendShapeNames.Count; ++j)
        if (AvatarSetupScript.vrcBlinkBlendshape == j)
          counterVRCblink = j;

      int vrcBlink = EditorGUILayout.Popup("Blink Blendshape", counterVRCblink, _blendShapeNames.ToArray());
      AvatarSetupScript.vrcBlinkBlendshape = vrcBlink;
      AvatarSetupScript.vrcEyeBlendshapes[0] = vrcBlink - 1;
      EditorGUI.EndDisabledGroup();
    }

    private void renderVRClookUpBlendshape()
    {
      EditorGUI.BeginDisabledGroup(AvatarSetupScript.GetIsConfigured()); // if configured disable the fields
      int counterVRClookUp = 0;
      for (int j = 0; j < _blendShapeNames.Count; ++j)
        if (AvatarSetupScript.vrcLookUpBlendshape == j)
          counterVRClookUp = j;

      int vrcLookUp = EditorGUILayout.Popup("Look Up Blendshape", counterVRClookUp, _blendShapeNames.ToArray());
      AvatarSetupScript.vrcLookUpBlendshape = vrcLookUp;
      AvatarSetupScript.vrcEyeBlendshapes[1] = vrcLookUp - 1;
      EditorGUI.EndDisabledGroup();
    }

    private void renderVRClookDownBlendshape()
    {
      EditorGUI.BeginDisabledGroup(AvatarSetupScript.GetIsConfigured()); // if configured disable the fields
      int counterVRClookDown = 0;
      for (int j = 0; j < _blendShapeNames.Count; ++j)
        if (AvatarSetupScript.vrcLookDownBlendshape == j)
          counterVRClookDown = j;

      int vrcLookDown = EditorGUILayout.Popup("Look Down Blendshape", counterVRClookDown, _blendShapeNames.ToArray());
      AvatarSetupScript.vrcLookDownBlendshape = vrcLookDown;
      AvatarSetupScript.vrcEyeBlendshapes[2] = vrcLookDown - 1;
      EditorGUI.EndDisabledGroup();
    }

    private void renderCVRBlinkLayers()
    {
      EditorGUI.BeginDisabledGroup(AvatarSetupScript.GetIsConfigured()); // if configured disable the fields
      for (int i = 0; i < 4; i++)
      {
        int counterCVRblink = 0;
        for (int j = 0; j < _blendShapeNames.Count; ++j)
          if (AvatarSetupScript.blinkBlendshape[i] == _blendShapeNames[j])
            counterCVRblink = j;

        int cvrBlink = EditorGUILayout.Popup("Blink " + (i + 1), counterCVRblink, _blendShapeNames.ToArray());
        AvatarSetupScript.blinkBlendshape[i] = _blendShapeNames[cvrBlink];
      }
      EditorGUI.EndDisabledGroup();
    }

    private void renderVRCLayers()
    {
      EditorGUI.BeginDisabledGroup(AvatarSetupScript.GetIsConfigured()); // if configured disable the fields
      AvatarSetupScript.vrcAction = (RuntimeAnimatorController)
        EditorGUILayout.ObjectField("Action", AvatarSetupScript.vrcAction, typeof(RuntimeAnimatorController), true);
      AvatarSetupScript.vrcBase = (RuntimeAnimatorController)
        EditorGUILayout.ObjectField("Base", AvatarSetupScript.vrcBase, typeof(RuntimeAnimatorController), true);
      AvatarSetupScript.vrcGesture = (RuntimeAnimatorController)
        EditorGUILayout.ObjectField("Gesture", AvatarSetupScript.vrcGesture, typeof(RuntimeAnimatorController), true);
      AvatarSetupScript.vrcAdditive = (RuntimeAnimatorController)
        EditorGUILayout.ObjectField("Additive", AvatarSetupScript.vrcAdditive, typeof(RuntimeAnimatorController), true);
      AvatarSetupScript.vrcFX = (RuntimeAnimatorController)
        EditorGUILayout.ObjectField("FX", AvatarSetupScript.vrcFX, typeof(RuntimeAnimatorController), true);
      AvatarSetupScript.vrcSitting = (RuntimeAnimatorController)
        EditorGUILayout.ObjectField("Sitting", AvatarSetupScript.vrcSitting, typeof(RuntimeAnimatorController), true);
      AvatarSetupScript.vrcIKPose = (RuntimeAnimatorController)
        EditorGUILayout.ObjectField("IKPose", AvatarSetupScript.vrcIKPose, typeof(RuntimeAnimatorController), true);
      AvatarSetupScript.vrcTPose = (RuntimeAnimatorController)
        EditorGUILayout.ObjectField("TPose", AvatarSetupScript.vrcTPose, typeof(RuntimeAnimatorController), true);
      EditorGUI.EndDisabledGroup();
    }

    private void renderFoldoutStart()
    {
      EditorGUI.indentLevel++;
      EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
      GUILayout.Space(EditorGUI.indentLevel * 16);
      EditorGUILayout.BeginVertical();
      EditorGUI.indentLevel++;
      GUILayout.Space(4);
    }

    private void renderFoldoutEnd()
    {
      GUILayout.Space(10);
      EditorGUI.indentLevel--;
      EditorGUILayout.EndVertical();
      EditorGUILayout.EndHorizontal();
      EditorGUI.indentLevel--;
    }

    private void RenderFoldoutSection(string title, ref bool showSection, Action content)
    {
      Rect foldoutRect = GUILayoutUtility.GetRect(
        GUIContent.none,
        EditorStyles.helpBox,
        GUILayout.ExpandWidth(true),
        GUILayout.Height(30f)
      );

      // Draw background
      GUIStyle bannerStyle = new GUIStyle(GUI.skin.box);
      bannerStyle.normal.background = Texture2D.whiteTexture;
      bannerStyle.normal.textColor = foldoutTitleBackgroundColor;
      GUI.backgroundColor = foldoutTitleBackgroundColor;
      GUI.Box(foldoutRect, GUIContent.none, bannerStyle);
      GUI.backgroundColor = Color.white;

      GUI.DrawTexture(
        new Rect(foldoutRect.x + 4f, foldoutRect.y + 7f, 16f, 16f),
        (
          showSection
            ? EditorGUIUtility.IconContent("IN Foldout on").image
            : EditorGUIUtility.IconContent("IN Foldout").image
        )
      );
      Rect labelRect = new Rect(foldoutRect.x + 20f, foldoutRect.y + 7f, foldoutRect.width - 20f, 16f);
      showSection = GUI.Toggle(labelRect, showSection, title, EditorStyles.boldLabel);
      if (Event.current.type == EventType.MouseDown && foldoutRect.Contains(Event.current.mousePosition))
      {
        showSection = !showSection;
        Event.current.Use();
      }

      if (showSection)
      {
        renderFoldoutStart();

        GUI.backgroundColor = Color.white;
        content.Invoke();
        GUI.backgroundColor = foldoutTitleBackgroundColor;

        renderFoldoutEnd();
      }
    }

    private void renderHorizontalSeparator()
    {
      GUILayout.Space(4);
      GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(2));
      GUILayout.Space(4);
    }

    private void renderAvatarComponentImportSection(AvatarsSetup AvatarSetupScript, int avatarComponentsFound)
    {
      EditorGUILayout.LabelField("Import Settings For Avatar Component", EditorStyles.boldLabel);

      renderAvatarComponentErrorMessages(avatarComponentsFound);

      if (AvatarSetupScript.GetUpdateFromTarget())
      {
        EditorGUILayout.HelpBox(Constants.DYNAMIC_IMPORT_ENABLED, MessageType.Info);
        if (GUILayout.Button("Disable Dynamic Config Import (Experimental)"))
        {
          AvatarSetupScript.SetUpdateFromTarget(false);
        }
      }
      else
      {
        EditorGUILayout.HelpBox(Constants.DYNAMIC_IMPORT_DISABLED, MessageType.Info);

        // only if configured via this component
        EditorGUI.BeginDisabledGroup(!AvatarSetupScript.GetIsConfigured());
        if (GUILayout.Button("Enable Dynamic Config Import (Experimental)"))
        {
          AvatarSetupScript.SetUpdateFromTarget(true);
        }
        EditorGUI.EndDisabledGroup();
      }

      if (!AvatarSetupScript.GetIsConfigured() && avatarComponentsFound == 1)
      {
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(Constants.IMPORT_EXISTING_CONIFIG_DETECTED, MessageType.Info);
        // only show the import button if avatar not configured
        if (GUILayout.Button("Import From Existing Component"))
        {
          AvatarSetupScript.TriggerUpdateFromTargetToCore();
        }
      }
    }

    private void RenderSetupButtonsSection(AvatarsSetup AvatarSetupScript, int avatarComponentsFound)
    {
      bool setupButtonsEnabled = true;
      if (avatarComponentsFound > 0 && !AvatarSetupScript.GetIsConfigured())
      {
        EditorGUILayout.HelpBox("Existing avatar settings detected, setup buttons disabled.", MessageType.Info);
        setupButtonsEnabled = false; // Disable the setup buttons
      }

      EditorGUI.BeginDisabledGroup(!IsCVR_CCKExists() || !setupButtonsEnabled || AvatarSetupScript.GetIsConfigured());
      if (GUILayout.Button("Setup for Chillout VR"))
      {
        AvatarSetupScript.SetIsConfigured(true);
        collapseAllFoldouts();
        AvatarSetupScript.SetNumberOfAllowedParameters(3200);

        // Add the ChilloutVRComponent
        AvatarSetupScript.AddChilloutVRComponent();
      }
      EditorGUI.EndDisabledGroup();

      EditorGUI.BeginDisabledGroup(
        !IsVRC_SDK_VRCSDK3Exists() || !setupButtonsEnabled || AvatarSetupScript.GetIsConfigured()
      );
      if (GUILayout.Button("Setup for VRChat"))
      {
        AvatarSetupScript.SetIsConfigured(true);
        collapseAllFoldouts();
        AvatarSetupScript.SetNumberOfAllowedParameters(256);

        // Add the VRChatComponent
        AvatarSetupScript.AddVRChatComponent();
      }
      EditorGUI.EndDisabledGroup();

      EditorGUI.BeginDisabledGroup(!AvatarSetupScript.GetIsConfigured() || !setupButtonsEnabled);
      if (GUILayout.Button("Reset to un-configured"))
      {
        AvatarSetupScript.SetIsConfigured(false);
        // Remove any added components
        AvatarSetupScript.RemoveAvatarComponents();
        AvatarSetupScript.SetNumberOfAllowedParameters(0);
      }
      EditorGUI.EndDisabledGroup();
    }

    private void renderAvatarComponentErrorMessages(int avatarComponentsFound)
    {
      if (avatarComponentsFound > 1)
      {
        // Oh dear more that one component found. Unsupported as we don't know what to pull from, show warning
        EditorGUILayout.HelpBox(Constants.WARNING_MORE_THAN_ONE_COMPONENT, MessageType.Warning);
      }
      if (avatarComponentsFound == 0)
      {
        // No component found. Unsupported as there is nothing to pull from, show warning
        EditorGUILayout.HelpBox(Constants.WARNING_NO_COMPONENTS, MessageType.Warning);
      }
    }

    private void renderVoiceAndViewPosition(AvatarsSetup AvatarSetupScript)
    {
      EditorGUI.BeginDisabledGroup(AvatarSetupScript.GetIsConfigured()); // if configured disable the fields
      EditorGUILayout.LabelField("Avatar Viewpoint & Voice Settings", EditorStyles.boldLabel);
      AvatarSetupScript.viewPosition = EditorGUILayout.Vector3Field("View Position", AvatarSetupScript.viewPosition);
      EditorGUILayout.HelpBox(Constants.AVATAR_INFO_VIEW_POSITION, MessageType.Info);
      AvatarSetupScript.voicePosition = EditorGUILayout.Vector3Field("Voice Position", AvatarSetupScript.voicePosition);
      EditorGUILayout.HelpBox(Constants.AVATAR_INFO_VOICE_POSITION, MessageType.Info);
      EditorGUI.EndDisabledGroup();
    }

    private void renderAvatarMenuSection(AvatarsSetup AvatarSetupScript)
    {
      EditorGUI.BeginDisabledGroup(AvatarSetupScript.GetIsConfigured()); // if configured disable the fields
      EditorGUILayout.LabelField("Avatar Menu Settings", EditorStyles.boldLabel);
      AvatarSetupScript.use_AvatarMenu = EditorGUILayout.Toggle(
        "Enable Custom Avatar Menu",
        AvatarSetupScript.use_AvatarMenu
      );
      EditorGUI.EndDisabledGroup();
      if (AvatarSetupScript.use_AvatarMenu)
      {
        RenderFoldoutSection(
          "VRChat Specific",
          ref AvatarSetupScript.vrcMenuShowSection,
          () =>
          {
            EditorGUI.BeginDisabledGroup(AvatarSetupScript.GetIsConfigured()); // if configured disable the fields
            AvatarSetupScript.vrcBaseMenu = EditorGUILayout.ObjectField(
              "Avatar Menu Path",
              AvatarSetupScript.vrcBaseMenu,
              typeof(UnityEngine.Object),
              false
            );
            AvatarSetupScript.vrcParameters = EditorGUILayout.ObjectField(
              "Avatar Parameters Path",
              AvatarSetupScript.vrcParameters,
              typeof(UnityEngine.Object),
              false
            );
            EditorGUI.EndDisabledGroup();
          }
        );
      }
    }

    private void renderCredits()
    {
      EditorGUILayout.Space();
      renderHorizontalSeparator();
      EditorGUILayout.BeginHorizontal();
      GUILayout.FlexibleSpace();
      EditorGUILayout.LabelField(
        "Powered By NVH's CASCS System",
        GUILayout.Width(200)
      );
      EditorGUILayout.EndHorizontal();
      EditorGUILayout.BeginHorizontal();
      GUILayout.FlexibleSpace();
      EditorGUILayout.LabelField("Build " + Constants.CASCSS_version, GUILayout.Width(Constants.CASCSS_version_width));
      EditorGUILayout.EndHorizontal();
    }

    private void collapseAllFoldouts()
    {
      AvatarSetupScript.eyeLookShowSection = false;
      AvatarSetupScript.vrcLayerShowSection = false;
      AvatarSetupScript.cvrLayerShowSection = false; // not yet in use, may be removed in future
      AvatarSetupScript.vrcBlinkShowSection = false;
      AvatarSetupScript.cvrBlinkShowSection = false;
      AvatarSetupScript.VisemesShowSection = false;
    }

    private void clearVisemeBlendshapes()
    {
      for (int i = 0; i < AvatarSetupScript.visemeBlendshapes.Length; i++)
      {
        AvatarSetupScript.visemeBlendshapes[i] = null;
      }
    }

    private void clearBlinkBlendshapes()
    {
      for (int i = 0; i < AvatarSetupScript.blinkBlendshape.Length; i++)
      {
        AvatarSetupScript.blinkBlendshape[i] = null;
      }
      AvatarSetupScript.vrcBlinkBlendshape = 0;
      AvatarSetupScript.vrcLookUpBlendshape = 0;
      AvatarSetupScript.vrcLookDownBlendshape = 0;
    }

    private void renderVisemeParameters()
    {
      EditorGUI.BeginDisabledGroup(AvatarSetupScript.GetIsConfigured()); // if configured disable the fields
      if (GUILayout.Button("Auto select Visemes"))
      {
        FindVisemes();
      }
      // viseme parameters
      for (int i = 0; i < _visemeNames.Length; i++)
      {
        int current = 0;
        for (int j = 0; j < _blendShapeNames.Count; ++j)
          if (AvatarSetupScript.visemeBlendshapes[i] == _blendShapeNames[j])
            current = j;

        int viseme = EditorGUILayout.Popup("Viseme: " + _visemeNames[i], current, _blendShapeNames.ToArray());
        AvatarSetupScript.visemeBlendshapes[i] = _blendShapeNames[viseme];
      }
      EditorGUI.EndDisabledGroup();
    }

    private bool IsCVR_CCKExists()
    {
      bool exists = false;
#if CVR_CCK_EXISTS
      exists = true;
#endif
      return exists;
    }

    private bool IsVRC_SDK_VRCSDK3Exists()
    {
      bool exists = false;
#if VRC_SDK_VRCSDK3
      exists = true;
#endif
      return exists;
    }

    private int GetAvatarComponentCount()
    {
      int avatarComponentsFound = 0;

      if (AvatarSetupScript.CheckForVRChatAvatarComponent())
      {
        // VRC Component detected
        avatarComponentsFound = avatarComponentsFound + 1;
      }
      if (AvatarSetupScript.CheckForCVRAvatarComponent())
      {
        // CVR Component detected
        avatarComponentsFound = avatarComponentsFound + 1;
      }

      return avatarComponentsFound;
    }

    private void SettingsCleanupByComponentsFound(int avatarComponentsFound)
    {
      if (avatarComponentsFound < 1)
      {
        // No component found. turn off dynamic import
        AvatarSetupScript.SetUpdateFromTarget(false);
        // No component found. turn off isConfigured state
        AvatarSetupScript.SetIsConfigured(false);
      }
      if (avatarComponentsFound > 1)
      {
        // More than one component found, turn off dynamic import
        AvatarSetupScript.SetUpdateFromTarget(false);
      }
    }

    protected virtual void OnSceneGUI()
    {
      if (AvatarSetupScript != null)
      {
        var avatarTransform = AvatarSetupScript.transform;
        var scale = avatarTransform.localScale;
        var inverseScale = new Vector3(1 / scale.x, 1 / scale.y, 1 / scale.z);

        // only draw the handles if the avatar is not configured
        if (!AvatarSetupScript.GetIsConfigured())
        {
          //View Position
          GUIStyle style = new GUIStyle();
          style.normal.textColor = viewPositionColour;
          style.fontSize = 20;
          Handles.BeginGUI();
          Vector3 pos = avatarTransform.TransformPoint(Vector3.Scale(AvatarSetupScript.viewPosition, inverseScale));
          Vector2 pos2D = HandleUtility.WorldToGUIPoint(pos);
          GUI.Label(new Rect(pos2D.x + 20, pos2D.y - 10, 100, 20), "View Position", style);
          Handles.EndGUI();

          EditorGUI.BeginChangeCheck();
          Vector3 viewPos = Handles.PositionHandle(pos, avatarTransform.rotation);
          if (EditorGUI.EndChangeCheck())
          {
            Undo.RecordObject(AvatarSetupScript, "View Position Change");
            AvatarSetupScript.viewPosition = Vector3.Scale(avatarTransform.InverseTransformPoint(viewPos), scale);
          }

          //Voice Position
          style.normal.textColor = voicePositionColour;
          Handles.BeginGUI();
          pos = avatarTransform.TransformPoint(Vector3.Scale(AvatarSetupScript.voicePosition, inverseScale));
          pos2D = HandleUtility.WorldToGUIPoint(pos);
          GUI.Label(new Rect(pos2D.x + 20, pos2D.y - 10, 100, 20), "Voice Position", style);
          Handles.EndGUI();

          EditorGUI.BeginChangeCheck();
          Vector3 voicePos = Handles.PositionHandle(pos, avatarTransform.rotation);
          if (EditorGUI.EndChangeCheck())
          {
            Undo.RecordObject(AvatarSetupScript, "Voice Position Change");
            AvatarSetupScript.voicePosition = Vector3.Scale(avatarTransform.InverseTransformPoint(voicePos), scale);
          }
        }
      }
    }

    List<string> GetBlendShapeNames(SkinnedMeshRenderer smr)
    {
      List<string> blendShapeNames = new List<string>();
      blendShapeNames.Add("-none-");
      Mesh mesh = smr.sharedMesh;
      for (int i = 0; i < mesh.blendShapeCount; i++)
      {
        blendShapeNames.Add(mesh.GetBlendShapeName(i));
      }
      return blendShapeNames;
    }

    void FindVisemes()
    {
      for (int i = 0; i < _visemeNames.Length; i++)
      {
        for (int j = 0; j < _blendShapeNames.Count; ++j)
        {
          if (
            _blendShapeNames[j].ToLower().Contains("v_" + _visemeNames[i].ToLower())
            || _blendShapeNames[j].ToLower().Contains("viseme_" + _visemeNames[i].ToLower())
          )
          {
            AvatarSetupScript.visemeBlendshapes[i] = _blendShapeNames[j];
          }
        }
      }
    }

    void AutoSetEyeBones()
    {
      AvatarSetupScript.leftEyeBone = realLeftEye;
      AvatarSetupScript.rightEyeBone = realRightEye;
    }
  }
}
#endif