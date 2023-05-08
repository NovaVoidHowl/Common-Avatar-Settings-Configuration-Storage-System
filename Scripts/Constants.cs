using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.NovaVoidHowl.dev.unity.CASCSS
{
    public static class Constants
    {
    // version info
    public const string CASCSS_version = "0.0.1-alpha"; 
    public const int CASCSS_version_width = 105;



    // info messages
    public const string INFO_VIEW_ONLY_MODE = "Avatar is configured \n\nBelow interface set to view only mode" +
                                              "\nTo re-enable press the `Reset to un-configured` button";

    // error messages
    public const string ERROR_MISSING_SDK = "No compatible SDKs found. Please install ChilloutVR CCK or VRChat SDK3.";
    
    // warning messages
    public const string WARNING_MISSING_ANIMATOR = "Animator component is missing on the GameObject. Make sure you're " +
                                                   "attaching this script to the root of the avatar!";
    public const string WARNING_NOT_HUMANOID = "The Animator component is not set up with a Humanoid avatar.";
    public const string WARNING_EYE_BONE_NOT_FOUND = "Left or right eye bone not found. Please set the eye bones manually.";
    

    // VRC warning messages
    public const string VRC_WARNING_EYE_BONE_NOT_FOUND = "VRC needs left & right eye bones setting for use with Eye Look.";

    //sdk resource links
    public const string VRC_SDK_ULR = "https://vrchat.com/home/download";
    public const string CCK_CCK_ULR = "https://docs.abinteractive.net/cck/setup/";


    // avatar info
    public const string AVATAR_INFO_VIEW_POSITION = "The position of the avatar's eyes. This is used to position the camera in the world.";
    public const string AVATAR_INFO_VOICE_POSITION = "The position of the avatar's mouth. This is used to position the voice chat audio in the world.";
    public const string AVATAR_INFO_VISEMES_INFO = "Visemes are used to animate the face of the avatar. To use them you need to set the appropriate mesh in the box below";

    // dynamic import info
    public const string DYNAMIC_IMPORT_ENABLED = "Dynamic Config Import ENABLED \n\nThis feature dynamically feeds changes made to the Avatar Descriptor component back to this component";
    public const string DYNAMIC_IMPORT_DISABLED = "Dynamic Config Import DISABLED\n\nChanges made in the Avatar Descriptor component will not be written back to this component. \n\nThis feature dynamically feeds changes made to the Avatar Descriptor component back to this component";

    // import info
    public const string IMPORT_EXISTING_CONIFIG_DETECTED = "Existing avatar configuration component detected, press the following button to import it.";

    // import warning
    public const string WARNING_MORE_THAN_ONE_COMPONENT = "Warning \nMore that one avatar component detected, import disabled";
    public const string WARNING_NO_COMPONENTS = "No avatar components found, import disabled";
    }
}