using UnityEngine;
using UnityEngine.Experimental.Rendering;
using System;
using System.Reflection;
using UnityEditor;

#if VRC_SDK_VRCSDK3
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
#endif
#if CVR_CCK_EXISTS
using ABI.CCK.Components;
using ABI.CCK.Scripts;
#endif
#if KAFE_CVR_CCK_EYE_MOVEMENT_FIX_EXISTS
using EyeMovementFix.CCK;
#endif

namespace com.NovaVoidHowl.dev.unity.CASCSS
{
  [DisallowMultipleComponent]
  [ExecuteInEditMode]
  public class AvatarsSetup : MonoBehaviour
  {
    [HideInInspector]
    private bool updateFromTarget = false;
    private bool isConfigured = false;
    private int NumberOfAllowedParameters;
    Color voicePositionColour = new Color(0.941f, 0.553f, 0.02f, 1.0f);
    Color viewPositionColour = new Color(0.545f, 0.09f, 0.106f, 1.0f);

    public Vector3 viewPosition = new Vector3(0, 0.1f, 0);
    public Vector3 voicePosition = new Vector3(0, 0.1f, 0);

    // Actual Rotation Limits for the eyes
    public float leftEyeMinX = -25;
    public float leftEyeMaxX = 25;
    public float leftEyeMinY = -25;
    public float leftEyeMaxY = 25;

    public float rightEyeMinX = -25;
    public float rightEyeMaxX = 25;
    public float rightEyeMinY = -25;
    public float rightEyeMaxY = 25;

    // Inspector Saved Fields
    public float lMinY = 25f;
    public float lMaxX = 25f;
    public float lMinX = 25f;
    public float lMaxY = 25f;

    public float rMinY = 25f;
    public float rMaxX = 25f;
    public float rMinX = 25f;
    public float rMaxY = 25f;

    // place holder for the eye rotation center
    // note the following 4 floats are not used in the current version
    public float rCenterY = 0f;
    public float rCenterX = 0f;
    public float lCenterY = 0f;
    public float lCenterX = 0f;

    public bool perEyeRotations;

    public SkinnedMeshRenderer bodyMesh;
    public bool use_BlinkBlendshapes;
    public bool use_visemes = false;
    public bool use_AvatarMenu = false;

    public Transform leftEyeBone = null;
    public Transform rightEyeBone = null;
    public int vrcBlinkBlendshape = 0;
    public int vrcLookUpBlendshape = 0;
    public int vrcLookDownBlendshape = 0;
    public int[] vrcEyeBlendshapes = new int[3];

    public RuntimeAnimatorController vrcAction;
    public RuntimeAnimatorController vrcBase;
    public RuntimeAnimatorController vrcAdditive;
    public RuntimeAnimatorController vrcGesture;
    public RuntimeAnimatorController vrcFX;
    public RuntimeAnimatorController vrcIKPose;
    public RuntimeAnimatorController vrcSitting;
    public RuntimeAnimatorController vrcTPose;

    public string[] visemeBlendshapes = new string[15];
    public string[] blinkBlendshape = new string[4];

    public UnityEngine.Object vrcBaseMenu = null;
    public UnityEngine.Object vrcParameters = null;

    // ----------------- Show Sections -----------------

    public bool eyeLookShowSection = true;
    public bool vrcLayerShowSection = true;
    public bool cvrLayerShowSection = true; // not yet in use, may be removed in future
    public bool vrcBlinkShowSection = true;
    public bool cvrBlinkShowSection = true;
    public bool VisemesShowSection = true;
    public bool MenusShowSection = true; // for avatar menus --- not yet in use
    public bool vrcMenuShowSection = true;

#if CVR_CCK_EXISTS
    private CVRAvatar chilloutVRComponent;
#endif
#if KAFE_CVR_CCK_EYE_MOVEMENT_FIX_EXISTS
    private EyeRotationLimits eyeRotationLimitComponent;
#endif
#if VRC_SDK_VRCSDK3
    private VRCAvatarDescriptor vrchatComponent;
    private VRCAvatarDescriptor.CustomAnimLayer[] vrchatAnimLayerSetBase;
    private VRCAvatarDescriptor.CustomAnimLayer[] vrchatAnimLayerSetSpecial;
#endif

    public void SetUpdateFromTarget(bool value)
    {
      updateFromTarget = value;
    }

    public bool GetUpdateFromTarget()
    {
      return updateFromTarget;
    }

    public void SetIsConfigured(bool value)
    {
      isConfigured = value;
    }

    public void SetNumberOfAllowedParameters(int value)
    {
      NumberOfAllowedParameters = value;
    }

    public bool GetIsConfigured()
    {
      return isConfigured;
    }

    public int GetNumberOfAllowedParameters()
    {
      return NumberOfAllowedParameters;
    }

    // add ChilloutVR Avatar Component to the gameObject
    public void AddChilloutVRComponent()
    {
      // only if ChilloutVR SDK is installed
#if CVR_CCK_EXISTS
      // Check if the component already exists
      chilloutVRComponent = GetComponent<CVRAvatar>();

      if (chilloutVRComponent == null)
      {
        // Add the component to the gameObject
        chilloutVRComponent = gameObject.AddComponent<CVRAvatar>();
      }
#if KAFE_CVR_CCK_EYE_MOVEMENT_FIX_EXISTS
            // only can add the eye rotation limits if the fix is installed
            eyeRotationLimitComponent = GetComponent<EyeRotationLimits>();

            if (eyeRotationLimitComponent == null)
            {
                // Add the component to the gameObject
                eyeRotationLimitComponent = gameObject.AddComponent<EyeRotationLimits>();
            }
            eyeRotationLimitComponent.LeftEyeMinX = leftEyeMinX;
            eyeRotationLimitComponent.LeftEyeMaxX = leftEyeMaxX;
            eyeRotationLimitComponent.LeftEyeMinY = leftEyeMinY;
            eyeRotationLimitComponent.LeftEyeMaxY = leftEyeMaxY;
            eyeRotationLimitComponent.RightEyeMinX = rightEyeMinX;
            eyeRotationLimitComponent.RightEyeMaxX = rightEyeMaxX;
            eyeRotationLimitComponent.RightEyeMinY = rightEyeMinY;
            eyeRotationLimitComponent.RightEyeMaxY = rightEyeMaxY;
            eyeRotationLimitComponent.lMinY = lMinY;
            eyeRotationLimitComponent.lMaxX = lMaxX;
            eyeRotationLimitComponent.lMinX = lMinX;
            eyeRotationLimitComponent.lMaxY = lMaxY;
            eyeRotationLimitComponent.rMinY = rMinY;
            eyeRotationLimitComponent.rMaxX = rMaxX;
            eyeRotationLimitComponent.rMinX = rMinX;
            eyeRotationLimitComponent.rMaxY = rMaxY;
            eyeRotationLimitComponent.individualEyeRotations = perEyeRotations;

#endif

      // Set any necessary properties or parameters on the component here
      chilloutVRComponent.viewPosition = viewPosition;
      chilloutVRComponent.voicePosition = voicePosition;
      if (bodyMesh != null)
      {
        chilloutVRComponent.bodyMesh = bodyMesh;
        if (use_visemes)
        {
          chilloutVRComponent.visemeBlendshapes = visemeBlendshapes;
          chilloutVRComponent.useVisemeLipsync = true;
        }
        else
        {
          chilloutVRComponent.visemeBlendshapes = new string[15];
          chilloutVRComponent.useVisemeLipsync = false;
        }
        if (use_BlinkBlendshapes)
        {
          chilloutVRComponent.blinkBlendshape = blinkBlendshape;
          chilloutVRComponent.useBlinkBlendshapes = true;
        }
        else
        {
          chilloutVRComponent.blinkBlendshape = new string[4];
          chilloutVRComponent.useBlinkBlendshapes = false;
        }
      }

      // menu setup
      chilloutVRComponent.avatarUsesAdvancedSettings = use_AvatarMenu;

#endif
    }

    public void AddVRChatComponent()
    {
      // only if VRChat SDK3 is installed
#if VRC_SDK_VRCSDK3
      // Check if the component already exists
      vrchatComponent = GetComponent<VRCAvatarDescriptor>();

      if (vrchatComponent == null)
      {
        // Add the component to the gameObject
        vrchatComponent = gameObject.AddComponent<VRCAvatarDescriptor>();
      }

      // Set any necessary properties or parameters on the component here
      vrchatComponent.ViewPosition = viewPosition;
      if (bodyMesh != null)
      {
        vrchatComponent.VisemeSkinnedMesh = bodyMesh;
        if (use_visemes)
        {
          vrchatComponent.VisemeBlendShapes = visemeBlendshapes;
          vrchatComponent.lipSync = VRC.SDKBase.VRC_AvatarDescriptor.LipSyncStyle.VisemeBlendShape;
        }
        else
        {
          vrchatComponent.VisemeBlendShapes = new string[15];
          vrchatComponent.lipSync = VRC.SDKBase.VRC_AvatarDescriptor.LipSyncStyle.Default;
        }
        if (use_BlinkBlendshapes)
        {
          vrchatComponent.enableEyeLook = true;
          vrchatComponent.customEyeLookSettings.eyelidsSkinnedMesh = bodyMesh;
          vrchatComponent.customEyeLookSettings.eyelidType = VRCAvatarDescriptor.EyelidType.Blendshapes;
          vrchatComponent.customEyeLookSettings.leftEye = leftEyeBone;
          vrchatComponent.customEyeLookSettings.rightEye = rightEyeBone;
          vrchatComponent.customEyeLookSettings.eyelidsBlendshapes = vrcEyeBlendshapes;

          // convert eye look degrees from left/right to up/down per eye to vrchat's system
          // first instantiate the look sections, they are null by default for some reason
          vrchatComponent.customEyeLookSettings.eyesLookingStraight =
            new VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.CustomEyeLookSettings.EyeRotations();
          vrchatComponent.customEyeLookSettings.eyesLookingUp =
            new VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.CustomEyeLookSettings.EyeRotations();
          vrchatComponent.customEyeLookSettings.eyesLookingDown =
            new VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.CustomEyeLookSettings.EyeRotations();
          vrchatComponent.customEyeLookSettings.eyesLookingLeft =
            new VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.CustomEyeLookSettings.EyeRotations();
          vrchatComponent.customEyeLookSettings.eyesLookingRight =
            new VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.CustomEyeLookSettings.EyeRotations();

          // straight ahead should be 0,0,0 so hardcode that
          vrchatComponent.customEyeLookSettings.eyesLookingStraight.linked = !perEyeRotations;
          Quaternion straightRotL = Quaternion.Euler(0f, 0f, 0f);
          Quaternion straightRotR = Quaternion.Euler(0f, 0f, 0f);
          vrchatComponent.customEyeLookSettings.eyesLookingStraight.left = straightRotL;
          vrchatComponent.customEyeLookSettings.eyesLookingStraight.right = straightRotR;

          // up/down only use the X value (first value)
          vrchatComponent.customEyeLookSettings.eyesLookingUp.linked = !perEyeRotations;
          Quaternion upRotL = Quaternion.Euler(-lMinY, 0f, 0f);
          Quaternion upRotR = Quaternion.Euler(-rMinY, 0f, 0f);
          vrchatComponent.customEyeLookSettings.eyesLookingUp.left = upRotL;
          vrchatComponent.customEyeLookSettings.eyesLookingUp.right = upRotR;

          vrchatComponent.customEyeLookSettings.eyesLookingDown.linked = !perEyeRotations;
          Quaternion downRotL = Quaternion.Euler(lMaxY, 0f, 0f);
          Quaternion downRotR = Quaternion.Euler(rMaxY, 0f, 0f);
          vrchatComponent.customEyeLookSettings.eyesLookingDown.left = downRotL;
          vrchatComponent.customEyeLookSettings.eyesLookingDown.right = downRotR;

          // left/right only use the Y value (second value)
          vrchatComponent.customEyeLookSettings.eyesLookingLeft.linked = !perEyeRotations;
          Quaternion leftRotL = Quaternion.Euler(0f, -lMinX, 0f);
          Quaternion leftRotR = Quaternion.Euler(0f, -rMinX, 0f);
          vrchatComponent.customEyeLookSettings.eyesLookingLeft.left = leftRotL;
          vrchatComponent.customEyeLookSettings.eyesLookingLeft.right = leftRotR;

          vrchatComponent.customEyeLookSettings.eyesLookingRight.linked = !perEyeRotations;
          Quaternion rightRotL = Quaternion.Euler(0f, lMaxX, 0f);
          Quaternion rightRotR = Quaternion.Euler(0f, rMaxX, 0f);
          vrchatComponent.customEyeLookSettings.eyesLookingRight.left = rightRotL;
          vrchatComponent.customEyeLookSettings.eyesLookingRight.right = rightRotR;
        }
        else
        {
          vrchatComponent.enableEyeLook = false;
        }

        //What the heck VRC why did you use your own special components for this?!. Why not just an int?
        // setup base and special animation layers
        vrchatComponent.customizeAnimationLayers = true;
        vrchatComponent.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[5];
        for (int i = 0; i < vrchatComponent.baseAnimationLayers.Length; i++)
        {
          vrchatComponent.baseAnimationLayers[i] = new VRCAvatarDescriptor.CustomAnimLayer();
          vrchatComponent.baseAnimationLayers[0].isEnabled = false;
          vrchatComponent.baseAnimationLayers[0].isDefault = false;
        }
        vrchatComponent.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[3];
        for (int i = 0; i < vrchatComponent.specialAnimationLayers.Length; i++)
        {
          vrchatComponent.specialAnimationLayers[i] = new VRCAvatarDescriptor.CustomAnimLayer();
          vrchatComponent.specialAnimationLayers[0].isEnabled = false;
          vrchatComponent.specialAnimationLayers[0].isDefault = false;
        }

        vrchatComponent.baseAnimationLayers[0].type = VRCAvatarDescriptor.AnimLayerType.Base;
        vrchatComponent.baseAnimationLayers[0].animatorController = vrcBase;
        vrchatComponent.baseAnimationLayers[1].type = VRCAvatarDescriptor.AnimLayerType.Additive;
        vrchatComponent.baseAnimationLayers[1].animatorController = vrcAdditive;
        vrchatComponent.baseAnimationLayers[2].type = VRCAvatarDescriptor.AnimLayerType.Gesture;
        vrchatComponent.baseAnimationLayers[2].animatorController = vrcGesture;
        vrchatComponent.baseAnimationLayers[3].type = VRCAvatarDescriptor.AnimLayerType.Action;
        vrchatComponent.baseAnimationLayers[3].animatorController = vrcAction;
        vrchatComponent.baseAnimationLayers[4].type = VRCAvatarDescriptor.AnimLayerType.FX;
        vrchatComponent.baseAnimationLayers[4].animatorController = vrcFX;

        vrchatComponent.specialAnimationLayers[0].type = VRCAvatarDescriptor.AnimLayerType.Sitting;
        vrchatComponent.specialAnimationLayers[0].animatorController = vrcSitting;
        vrchatComponent.specialAnimationLayers[1].type = VRCAvatarDescriptor.AnimLayerType.TPose;
        vrchatComponent.specialAnimationLayers[1].animatorController = vrcTPose;
        vrchatComponent.specialAnimationLayers[2].type = VRCAvatarDescriptor.AnimLayerType.IKPose;
        vrchatComponent.specialAnimationLayers[2].animatorController = vrcIKPose;
      }

      // menus setup
      vrchatComponent.customExpressions = use_AvatarMenu;

      try
      {
        VRCExpressionsMenu vrcMenu = (VRCExpressionsMenu)vrcBaseMenu;

        if (vrcMenu != null)
        {
          vrchatComponent.expressionsMenu = vrcMenu;
        }
      }
      catch (InvalidCastException e)
      {
        EditorUtility.DisplayDialog("Invalid Asset Type", "The selected asset is not a VRCExpressionsMenu.", "OK");
      }

      try
      {
        VRCExpressionParameters vrcParam = (VRCExpressionParameters)vrcParameters;

        if (vrcParam != null)
        {
          vrchatComponent.expressionParameters = vrcParam;
        }
      }
      catch (InvalidCastException e)
      {
        EditorUtility.DisplayDialog("Invalid Asset Type", "The selected asset is not a VRCExpressionParameters.", "OK");
      }

#endif
    }

    public void TriggerUpdateFromTargetToCore()
    {
      // save current import allow state
      bool updateFromTargetTemp = updateFromTarget;

      updateFromTarget = true;
      Update();
      // restore real import allow state
      updateFromTarget = updateFromTargetTemp;
    }

    private void Update()
    {
      Debug.Log("Update trigger detected");
      if (updateFromTarget)
      {
        Debug.Log("running update");
        // only pull data back from CVR/VRC components if allowed
        UpdateCVRToCore();
        UpdateVRChatToCore();
      }
    }

    // update cvr component data on core
    private void UpdateCVRToCore()
    {
      // only if ChilloutVR SDK is installed
#if CVR_CCK_EXISTS
      // Check if the component already exists
      chilloutVRComponent = GetComponent<CVRAvatar>();

      if (chilloutVRComponent != null)
      {
        // Update parameters from cvr avatar component
        viewPosition = chilloutVRComponent.viewPosition;
        voicePosition = chilloutVRComponent.voicePosition;
        bodyMesh = chilloutVRComponent.bodyMesh;
        if (chilloutVRComponent.useVisemeLipsync)
        {
          use_visemes = true;
          visemeBlendshapes = chilloutVRComponent.visemeBlendshapes;
        }
        else
        {
          use_visemes = false;
          visemeBlendshapes = new string[15];
        }
        if (chilloutVRComponent.useBlinkBlendshapes)
        {
          use_BlinkBlendshapes = true;
          blinkBlendshape = chilloutVRComponent.blinkBlendshape;
        }
        else
        {
          use_BlinkBlendshapes = false;
          blinkBlendshape = new string[4];
        }

        // menu setup
        use_AvatarMenu = chilloutVRComponent.avatarUsesAdvancedSettings;
      }
#if KAFE_CVR_CCK_EYE_MOVEMENT_FIX_EXISTS
            // Check if the eye rotation component exists
            eyeRotationLimitComponent = GetComponent<EyeRotationLimits>();

            if (eyeRotationLimitComponent != null)
            {
                // Update parameters from eye rotation component

                leftEyeMinX = eyeRotationLimitComponent.LeftEyeMinX;
                leftEyeMaxX = eyeRotationLimitComponent.LeftEyeMaxX;
                leftEyeMinY = eyeRotationLimitComponent.LeftEyeMinY;
                leftEyeMaxY = eyeRotationLimitComponent.LeftEyeMaxY;
                rightEyeMinX = eyeRotationLimitComponent.RightEyeMinX;
                rightEyeMaxX = eyeRotationLimitComponent.RightEyeMaxX;
                rightEyeMinY = eyeRotationLimitComponent.RightEyeMinY;
                rightEyeMaxY = eyeRotationLimitComponent.RightEyeMaxY;
                lMinY = eyeRotationLimitComponent.lMinY;
                lMaxX = eyeRotationLimitComponent.lMaxX;
                lMinX = eyeRotationLimitComponent.lMinX;
                lMaxY = eyeRotationLimitComponent.lMaxY;
                rMinY = eyeRotationLimitComponent.rMinY;
                rMaxX = eyeRotationLimitComponent.rMaxX;
                rMinX = eyeRotationLimitComponent.rMinX;
                rMaxY = eyeRotationLimitComponent.rMaxY;
                perEyeRotations = eyeRotationLimitComponent.individualEyeRotations;
            }
#endif
#endif
    }

    // update vrc component date on core
    private void UpdateVRChatToCore()
    {
      // only if VRChat SDK3 is installed
#if VRC_SDK_VRCSDK3

      // Check if the component already exists
      vrchatComponent = GetComponent<VRCAvatarDescriptor>();

      if (vrchatComponent != null)
      {
        // Update parameters from vrchat avatar component
        viewPosition = vrchatComponent.ViewPosition;
        bodyMesh = vrchatComponent.VisemeSkinnedMesh;

        if (vrchatComponent.lipSync == VRC.SDKBase.VRC_AvatarDescriptor.LipSyncStyle.VisemeBlendShape)
        {
          use_visemes = true;
          visemeBlendshapes = vrchatComponent.VisemeBlendShapes;
        }
        else
        {
          use_visemes = false;
          visemeBlendshapes = new string[15];
        }

        if (vrchatComponent.enableEyeLook)
        {
          use_BlinkBlendshapes = true;
          leftEyeBone = vrchatComponent.customEyeLookSettings.leftEye;
          rightEyeBone = vrchatComponent.customEyeLookSettings.rightEye;
          LoadAndConvertVRCBlinkBlendshapeToCore();
        }
        else
        {
          use_BlinkBlendshapes = false;
          leftEyeBone = null;
          rightEyeBone = null;
        }

        // write base and special animation layers changes back to core
        if (vrchatComponent.customizeAnimationLayers)
        {
          vrcBase = vrchatComponent.baseAnimationLayers[0].animatorController;
          vrcAdditive = vrchatComponent.baseAnimationLayers[1].animatorController;
          vrcGesture = vrchatComponent.baseAnimationLayers[2].animatorController;
          vrcAction = vrchatComponent.baseAnimationLayers[3].animatorController;
          vrcFX = vrchatComponent.baseAnimationLayers[4].animatorController;

          vrcSitting = vrchatComponent.specialAnimationLayers[0].animatorController;
          vrcTPose = vrchatComponent.specialAnimationLayers[1].animatorController;
          vrcIKPose = vrchatComponent.specialAnimationLayers[2].animatorController;
        }

        // eye rotation back to core
        // need to check if each one is populated, as they are not required

        // note these 2 get reused in each if statement
        Vector3 eulerRight; // used to store the euler values for the right eye
        Vector3 eulerLeft; // used to store the euler values for the left eye

        int foundEnabledPerEyeRotations = 0;
        if (vrchatComponent.customEyeLookSettings.eyesLookingStraight != null)
        {
          (eulerLeft, eulerRight) = ProcessEyeRotationData(vrchatComponent.customEyeLookSettings.eyesLookingStraight);

          // convert to core format
          rCenterY = eulerRight.y;
          rCenterX = eulerRight.x;
          lCenterY = eulerLeft.y;
          lCenterX = eulerLeft.x;

          // note while stored the above are not used in this version as the center values are locked to 0,0,0
          if (!vrchatComponent.customEyeLookSettings.eyesLookingStraight.linked)
          {
            foundEnabledPerEyeRotations = foundEnabledPerEyeRotations + 1;
          }
        }

        if (vrchatComponent.customEyeLookSettings.eyesLookingUp != null)
        {
          (eulerLeft, eulerRight) = ProcessEyeRotationData(vrchatComponent.customEyeLookSettings.eyesLookingUp);

          // convert to core format
          // negative rotation value so convert

          rMinY = 360 - eulerRight.x;
          lMinY = 360 - eulerLeft.x;

          if (!vrchatComponent.customEyeLookSettings.eyesLookingUp.linked)
          {
            foundEnabledPerEyeRotations = foundEnabledPerEyeRotations + 1;
          }
        }
        if (vrchatComponent.customEyeLookSettings.eyesLookingDown != null)
        {
          (eulerLeft, eulerRight) = ProcessEyeRotationData(vrchatComponent.customEyeLookSettings.eyesLookingDown);

          // convert to core format


          rMaxY = eulerRight.x;
          lMaxY = eulerLeft.x;

          if (!vrchatComponent.customEyeLookSettings.eyesLookingDown.linked)
          {
            foundEnabledPerEyeRotations = foundEnabledPerEyeRotations + 1;
          }
        }
        if (vrchatComponent.customEyeLookSettings.eyesLookingLeft != null)
        {
          (eulerLeft, eulerRight) = ProcessEyeRotationData(vrchatComponent.customEyeLookSettings.eyesLookingLeft);

          // convert to core format
          // negative rotation value so convert

          rMinX = 360 - eulerRight.y;
          lMinX = 360 - eulerLeft.y;

          if (!vrchatComponent.customEyeLookSettings.eyesLookingLeft.linked)
          {
            foundEnabledPerEyeRotations = foundEnabledPerEyeRotations + 1;
          }
        }
        if (vrchatComponent.customEyeLookSettings.eyesLookingRight != null)
        {
          (eulerLeft, eulerRight) = ProcessEyeRotationData(vrchatComponent.customEyeLookSettings.eyesLookingRight);

          // convert to core format

          rMaxX = eulerRight.y;
          lMaxX = eulerLeft.y;

          if (!vrchatComponent.customEyeLookSettings.eyesLookingRight.linked)
          {
            foundEnabledPerEyeRotations = foundEnabledPerEyeRotations + 1;
          }
        }
        if (foundEnabledPerEyeRotations > 0)
        {
          perEyeRotations = true;
        }
        else
        {
          perEyeRotations = false;
        }

        // menus setup
        use_AvatarMenu = vrchatComponent.customExpressions;

        // VRC interface should ensure that the variables are ok to use
        // so we can just copy them over
        vrcBaseMenu = (UnityEngine.Object)vrchatComponent.expressionsMenu;
        vrcParameters = (UnityEngine.Object)vrchatComponent.expressionParameters;
      }
#endif
    }

    // check if vrc component is present on same game object
    public bool CheckForVRChatAvatarComponent()
    {
      bool Output = false;

#if VRC_SDK_VRCSDK3
      vrchatComponent = GetComponent<VRCAvatarDescriptor>();

      if (vrchatComponent != null)
      {
        Output = true;
      }
#endif
      return Output;
    }

    public bool CheckForCVRAvatarComponent()
    {
      bool Output = false;

#if CVR_CCK_EXISTS
      chilloutVRComponent = GetComponent<CVRAvatar>();

      if (chilloutVRComponent != null)
      {
        Output = true;
      }
#endif
      return Output;
    }

    // extract VRchat eye rotation data back to core compatible format
#if VRC_SDK_VRCSDK3
    public Tuple<Vector3, Vector3> ProcessEyeRotationData(
      VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.CustomEyeLookSettings.EyeRotations eyeRotations
    )
    {
      Vector3 eulerLeft = eyeRotations.left.eulerAngles;
      Vector3 eulerRight;
      if (eyeRotations.linked)
      {
        eulerRight = eulerLeft;
      }
      else
      {
        eulerRight = eyeRotations.right.eulerAngles;
      }

      return new Tuple<Vector3, Vector3>(eulerLeft, eulerRight);
    }
#endif

    // convert VRchat blink blendshape data back to core format
    public void LoadAndConvertVRCBlinkBlendshapeToCore()
    {
      // only if VRChat SDK3 is installed
#if VRC_SDK_VRCSDK3
      // Check if the component already exists
      vrchatComponent = GetComponent<VRCAvatarDescriptor>();

      if (vrchatComponent != null)
      {
        // pull data from vrchat avatar component
        vrcEyeBlendshapes = vrchatComponent.customEyeLookSettings.eyelidsBlendshapes;
        // add the +1 to the blendshape index, to allow for the "none" option
        vrcBlinkBlendshape = vrcEyeBlendshapes[0] + 1;
        vrcLookUpBlendshape = vrcEyeBlendshapes[1] + 1;
        vrcLookDownBlendshape = vrcEyeBlendshapes[2] + 1;
      }
#endif
    }

    // remove ChilloutVR/VRChat Avatar Components on the gameObject
    public void RemoveAvatarComponents()
    {
      // only if ChilloutVR SDK is installed
#if CVR_CCK_EXISTS
      // Check if the component exists
      chilloutVRComponent = GetComponent<CVRAvatar>();

      if (chilloutVRComponent != null)
      {
        // Remove the component from the gameObject
        DestroyImmediate(chilloutVRComponent);
        // report that the component was removed to console
        Debug.Log("CVRAvatar component removed from avatar");
      }

#if KAFE_CVR_CCK_EYE_MOVEMENT_FIX_EXISTS
            // Check if the eye rotation component exists
            eyeRotationLimitComponent = GetComponent<EyeRotationLimits>();

            if (eyeRotationLimitComponent != null)
            {
                // Remove the component from the gameObject
                DestroyImmediate(eyeRotationLimitComponent);
                // report that the component was removed to console
                Debug.Log("EyeRotationLimits component removed from avatar");
            }
            else
            {
                // report that the component was not found to console
                Debug.Log("EyeRotationLimits component not found on avatar");
            }
#endif
#endif

      // only if vrchat SDK is installed
#if VRC_SDK_VRCSDK3
      // Check if the VRChat Avatar component exists
      vrchatComponent = GetComponent<VRCAvatarDescriptor>();

      if (vrchatComponent != null)
      {
        // Remove the component from the gameObject
        DestroyImmediate(vrchatComponent);
      }
#endif
    }

    void OnDrawGizmosSelected()
    {
      if (!isConfigured)
      {
        var scale = transform.localScale;
        scale.x = 1 / scale.x;
        scale.y = 1 / scale.y;
        scale.z = 1 / scale.z;

        Gizmos.color = viewPositionColour;
        Gizmos.DrawSphere(transform.TransformPoint(Vector3.Scale(viewPosition, scale)), 0.01f);

        Gizmos.color = voicePositionColour;
        Gizmos.DrawSphere(transform.TransformPoint(Vector3.Scale(voicePosition, scale)), 0.01f);
      }
    }
  }
}
