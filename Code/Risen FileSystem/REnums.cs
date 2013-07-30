using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RisenEditor.Code.RisenTypes
{
public enum gEDirection
{
    eEVelocityDirectionFrom_None = 0x00000000,
    eEVelocityDirectionFrom_StartPositionAndOwner = 0x00000001,
    eEVelocityDirectionFrom_OwnerAndStartPosition = 0x00000002,
    eEVelocityDirectionFrom_Owner = 0x00000003,
    eEVelocityDirectionFrom_World = 0x00000004
}

// eCVegetationBrush_PS.ColorNoiseTurbulence
// eCVegetationBrush_PS.ProbabilityNoiseTurbulence
public enum bENoiseTurbulence
{
    bETurbulence_FractalSum    = 0x00000000,
    bETurbulence_FractalAbsSum = 0x00000001
};

// eCImageFilterRTBase.ColorFormat
public enum eCImageFilterRTBase_eCGfxShared_eEColorFormat
{
    eEColorFormat_Unknown       = 0x00000000,
    eEColorFormat_A8R8G8B8      = 0x00000015,
    eEColorFormat_X1R5G5B5      = 0x00000018,
    eEColorFormat_A1R5G5B5      = 0x00000019,
    eEColorFormat_A4R4G4B4      = 0x0000001A,
    eEColorFormat_R16F          = 0x0000006F,
    eEColorFormat_G16R16F       = 0x00000070,
    eEColorFormat_A16B16G16R16F = 0x00000071,
    eEColorFormat_R32F          = 0x00000072,
    eEColorFormat_G32R32F       = 0x00000073,
    eEColorFormat_A32B32G32R32F = 0x00000074
};

// eCImageResource2.PixelFormat
public enum eCImageResource2_eCGfxShared_eEColorFormat
{
    eEColorFormat_Unknown  = 0x00000000,
    eEColorFormat_A8R8G8B8 = 0x00000015,
    eEColorFormat_X8R8G8B8 = 0x00000016,
    eEColorFormat_DXT1     = 0x31545844,
    eEColorFormat_DXT2     = 0x32545844,
    eEColorFormat_DXT3     = 0x33545844,
    eEColorFormat_DXT4     = 0x34545844,
    eEColorFormat_DXT5     = 0x35545844
};

// eCGuiWindow2.AnchorMode
public enum eEAnchorMode
{
    eEAnchorMode_Default  = 0x00000000,
    eEAnchorMode_Relative = 0x00000001
};

// eCAudioEmitter_PS.FallOff
// eCAudioRollOffPreset.FallOff
// gCEffectCommandPlaySound.FallOff
// gCEffectCommandPlayVoice.FallOff
public enum eEAudioChannelFallOff
{
    eEAudioChannelFallOff_Logarithmic = 0x00000000,
    eEAudioChannelFallOff_Linear      = 0x00000001
};

// gCAudioVolumeScrollBar2.AudioChannelGroup
// gCAudioVolumeTrackbar2.AudioChannelGroup
public enum eEAudioChannelGroup
{
    eEAudioChannelGroup_Master  = 0x00000000,
    eEAudioChannelGroup_Voice   = 0x00000001,
    eEAudioChannelGroup_Music   = 0x00000003,
    eEAudioChannelGroup_FX      = 0x00000004,
    eEAudioChannelGroup_Ambient = 0x00000007
};

// eCAudioEmitter_PS.SpawningMode
public enum eEAudioEmitterMode
{
    eEAudioEmitterMode_Once   = 0x00000000,
    eEAudioEmitterMode_Loop   = 0x00000001,
    eEAudioEmitterMode_Repeat = 0x00000002
};

// eCAudioEmitter_PS.Shape
public enum eEAudioEmitterShape
{
    eEAudioEmitterShape_Point = 0x00000000,
    eEAudioEmitterShape_Box   = 0x00000001
};

// eCBillboard_PS.TargetMode
public enum eEBillboardTargetMode
{
    eEBillboardTargetMode_Self   = 0x00000000,
    eEBillboardTargetMode_Parent = 0x00000001,
    eEBillboardTargetMode_Target = 0x00000002
};

// eCIlluminated_PS.CastDirLightShadowsOverwrite
// eCIlluminated_PS.CastPntLightShadowsOverwrite
// eCIlluminated_PS.CastStaticShadowsOverwrite
public enum eEBoolOverwrite
{
    eEBoolOverwrite_None  = 0x00000000,
    eEBoolOverwrite_True  = 0x00000001,
    eEBoolOverwrite_False = 0x00000002
};

// eCGuiRadioButton2.CheckState
public enum eCGuiRadioButton2_eECheckState
{
    eECheckState_Unchecked = 0x00000000,
    eECheckState_Checked   = 0x00000001
};

// eCGuiCheckBox2.CheckState
public enum eCGuiCheckBox2_eECheckState
{
    eECheckState_Unchecked     = 0x00000000,
    eECheckState_Checked       = 0x00000001,
    eECheckState_Indeterminate = 0x00000002
};

// eCCollisionShape_PS.Group
public enum eECollisionGroup : uint
{
    eECollisionGroup_Static           = 0x00000001,
    eECollisionGroup_Dynamic          = 0x00000002,
    eECollisionGroup_Player           = 0x00000003,
    eECollisionGroup_NPC              = 0x00000004,
    eECollisionGroup_Item_Equipped    = 0x00000005,
    eECollisionGroup_Item_World       = 0x00000006,
    eECollisionGroup_Item_Attack      = 0x00000007,
    eECollisionGroup_Interactable     = 0x00000008,
    eECollisionGroup_Trigger          = 0x00000009,
    eECollisionGroup_Zone             = 0x0000000A,
    eECollisionGroup_Camera           = 0x0000000B,
    eECollisionGroup_Tree             = 0x0000000C,
    eECollisionGroup_DownCharacter    = 0x0000000D,
    eECollisionGroup_PlayerTrigger    = 0x0000000E,
    eECollisionGroup_ObjectTrigger    = 0x0000000F,
    eECollisionGroup_Ghost            = 0x00000010,
    eECollisionGroup_Mover            = 0x00000011,
    eECollisionGroup_RagDoll          = 0x00000012,
    eECollisionGroup_CharacterTrigger = 0x00000013
};

// eCCollisionShape.ShapeType
public enum eECollisionShapeType
{
    eECollisionShapeType_Box        = 0x00000003,
    eECollisionShapeType_TriMesh    = 0x00000001,
    eECollisionShapeType_ConvexHull = 0x00000006,
    eECollisionShapeType_None       = 0x00000000,
    eECollisionShapeType_Plane      = 0x00000002,
    eECollisionShapeType_Capsule    = 0x00000004,
    eECollisionShapeType_Sphere     = 0x00000005,
    eECollisionShapeType_Point      = 0x00000007
};

// eCColorSrcCombiner.CombinerType
public enum eEColorSrcCombinerType
{
    eEColorSrcCombinerType_Add      = 0x00000000,
    eEColorSrcCombinerType_Subtract = 0x00000001,
    eEColorSrcCombinerType_Multiply = 0x00000002,
    eEColorSrcCombinerType_Max      = 0x00000003,
    eEColorSrcCombinerType_Min      = 0x00000004
};

// eCColorSrcSampler.TexRepeatU
// eCColorSrcSampler.TexRepeatV
public enum eEColorSrcSampleTexRepeat
{
    eEColorSrcSampleTexRepeat_Wrap   = 0x00000000,
    eEColorSrcSampleTexRepeat_Clamp  = 0x00000001,
    eEColorSrcSampleTexRepeat_Mirror = 0x00000002
};

// eCColorSrcConstantSwitch.SwitchRepeat
// eCColorSrcSampler.SwitchRepeat
public enum eEColorSrcSwitchRepeat
{
    eEColorSrcSwitchRepeat_Repeat   = 0x00000000,
    eEColorSrcSwitchRepeat_Clamp    = 0x00000001,
    eEColorSrcSwitchRepeat_PingPong = 0x00000002
};

// eCParticle_PS.CoordinateSystem
public enum eECoordinateSystem
{
    eECoordinateSystem_Independent = 0x00000000,
    eECoordinateSystem_Relative    = 0x00000001
};

// eCColorSrcCamDistance.DistanceType
public enum eEDistanceType
{
    eEDistanceType_Src   = 0x00000000,
    eEDistanceType_Dest  = 0x00000001,
    eEDistanceType_Delta = 0x00000002
};

// eCGuiWindow2.Dock
public enum eEDock
{
    eEDock_None   = 0x00000000,
    eEDock_Left   = 0x00000001,
    eEDock_Top    = 0x00000002,
    eEDock_Right  = 0x00000003,
    eEDock_Bottom = 0x00000004,
    eEDock_Fill   = 0x00000005
};

// eCPointLight_PS.Effect
public enum eEDynamicLightEffect
{
    eEDynamicLightEffect_Steady  = 0x00000000,
    eEDynamicLightEffect_Flicker = 0x00000003,
    eEDynamicLightEffect_Pulse   = 0x00000001,
    eEDynamicLightEffect_Blink   = 0x00000002,
    eEDynamicLightEffect_Strobe  = 0x00000004
};

// eCParticle_PS.FacingDirection
public enum eEFacingDirection
{
    eEFacingDirection_FacingCamera              = 0x00000000,
    eEFacingDirection_AlongMovementFacingCamera = 0x00000001,
    eEFacingDirection_SpecifiedNormal           = 0x00000002,
    eEFacingDirection_AlongMovementFacingNormal = 0x00000003,
    eEFacingDirection_PerpendicularToMovement   = 0x00000004
};

// eCColorSrcFresnel.Term
public enum eEFresnelTerm
{
    eEFresnelTerm_Simple  = 0x00000000,
    eEFresnelTerm_Quadric = 0x00000001,
    eEFresnelTerm_Power   = 0x00000002
};

// eCGuiCursor2.CursorSize
public enum eEGuiCursorSize
{
    eEGuiCursorSize_FromSystem  = 0x00000000,
    eEGuiCursorSize_FromImage   = 0x00000001,
    eEGuiCursorSize_Independent = 0x00000002
};

// eCImageFilterRTBase.OutputMode
public enum eEIFOutputMode
{
    eEIFOutputMode_Texture     = 0x00000000,
    eEIFOutputMode_FrameBuffer = 0x00000001
};

// eCImageFilterRTBase.SizeMode
public enum eEIFSizeMode
{
    eEIFSizeMode_Relative = 0x00000001,
    eEIFSizeMode_Absolute = 0x00000000
};

// eCImageFilterTexture.TextureMode
public enum eEIFTextureMode
{
    eEIFTextureMode_Custom          = 0x00000000,
    eEIFTextureMode_SolidBackBuffer = 0x00000001,
    eEIFTextureMode_DepthBuffer     = 0x00000002
};

// eCGuiImage2.BlendMode
public enum eEImageBlend
{
    eEImageBlend_AlphaBlend = 0x00000000,
    eEImageBlend_Add        = 0x00001000,
    eEImageBlend_AddScaled  = 0x00002000,
    eEImageBlend_Modulate   = 0x00003000,
    eEImageBlend_Modulate2X = 0x00004000,
    eEImageBlend_Overwrite  = 0x00005000
};

// eCGuiLayeredImage2.BackgroundBlendMode
// eCGuiLayeredImage2.OverlayBlendMode
public enum eEImageLayerBlend
{
    eEImageLayerBlend_AlphaBlend = 0x00010000,
    eEImageLayerBlend_Add        = 0x00020000,
    eEImageLayerBlend_AddScaled  = 0x00030000,
    eEImageLayerBlend_Modulate   = 0x00040000,
    eEImageLayerBlend_Modulate2X = 0x00050000,
    eEImageLayerBlend_Overwrite  = 0x00060000,
    eEImageLayerBlend_FromImage  = 0x00000000
};

// eCParticle_PS.LightingStyle
public enum eELightingStyle
{
    eELightingStyle_Disabled = 0x00000000,
    eELightingStyle_Flat     = 0x00000001,
    eELightingStyle_Particle = 0x00000002,
    eELightingStyle_System   = 0x00000003
};

// eCGuiListCtrl2.View
public enum eEListView
{
    eEListView_Icon      = 0x00000000,
    eEListView_Details   = 0x00000001,
    eEListView_SmallIcon = 0x00000002,
    eEListView_List      = 0x00000003,
    eEListView_Tile      = 0x00000004,
    eEListView_UserGrid  = 0x00000005
};

// eCGuiListCtrl2.UGIconAlignMode
// eCGuiListCtrl2.UGLabelAlignMode
// eCGuiListCtrl2.UGSubLabelAlignMode
public enum eEListViewAlign
{
    eEListViewAlign_LeftTop      = 0x00000000,
    eEListViewAlign_CenterTop    = 0x00000001,
    eEListViewAlign_RightTop     = 0x00000002,
    eEListViewAlign_LeftMiddle   = 0x00000003,
    eEListViewAlign_CenterMiddle = 0x00000004,
    eEListViewAlign_RightMiddle  = 0x00000005,
    eEListViewAlign_LeftBottom   = 0x00000006,
    eEListViewAlign_CenterBottom = 0x00000007,
    eEListViewAlign_RightBottom  = 0x00000008
};

// eCGuiListCtrl2.UGIconSizeMode
public enum eEListViewIconSize
{
    eEListViewIconSize_FromImageList = 0x00000000,
    eEListViewIconSize_FixedWidth    = 0x00000001,
    eEListViewIconSize_FixedHeight   = 0x00000002,
    eEListViewIconSize_FixedSize     = 0x00000003
};

// eCGuiListCtrl2.UGItemLayoutMode
public enum eEListViewItemLayout
{
    eEListViewItemLayout_LabelRight  = 0x00000000,
    eEListViewItemLayout_LabelBottom = 0x00000001,
    eEListViewItemLayout_LabelLeft   = 0x00000002,
    eEListViewItemLayout_LabelTop    = 0x00000003,
    eEListViewItemLayout_NoSplit     = 0x00000004
};

// eCGuiListCtrl2.TileSizeMode
// eCGuiListCtrl2.UGTileSizeMode
public enum eEListViewTileSize
{
    eEListViewTileSize_AutoSize    = 0x00000000,
    eEListViewTileSize_FixedWidth  = 0x00000001,
    eEListViewTileSize_FixedHeight = 0x00000002,
    eEListViewTileSize_FixedSize   = 0x00000003
};

// eCParticle_PS.StartLocationShape
public enum eELocationShape
{
    eELocationShape_Box    = 0x00000000,
    eELocationShape_Sphere = 0x00000001,
    eELocationShape_Mesh   = 0x00000002
};

// eCParticle_PS.StartLocationTarget
public enum eELocationTarget
{
    eELocationTarget_Self   = 0x00000000,
    eELocationTarget_Parent = 0x00000001,
    eELocationTarget_Target = 0x00000002
};

// eCMoverAnimationBase.PlayBackMode
public enum eEMoverPlayBackMode
{
    eEMoverPlayBackMode_Forward  = 0x00000000,
    eEMoverPlayBackMode_Backward = 0x00000001,
    eEMoverPlayBackMode_PingPong = 0x00000002
};

// eCGuiPictureBox2.OverlayMode
public enum eEOverlayMode
{
    eEOverlayMode_Disabled   = 0x00000000,
    eEOverlayMode_Background = 0x00000001,
    eEOverlayMode_Picture    = 0x00000002,
    eEOverlayMode_Text       = 0x00000003
};

// eCCollisionShape_PS.Range
public enum eEPhysicRangeType
{
    eEPhysicRangeType_World           = 0x00000000,
    eEPhysicRangeType_ProcessingRange = 0x00000001,
    eEPhysicRangeType_VisibilityRange = 0x00000002
};

// eCGuiPictureBox2.PictureMode
public enum eEPictureMode
{
    eEPictureMode_Scale  = 0x00000000,
    eEPictureMode_Center = 0x00000001,
    eEPictureMode_Repeat = 0x00000002,
    eEPictureMode_Fit    = 0x00000003
};

// gCEffectCommandModifyEntity.PropertySet
public enum eEPropertySetType
{
    eEPropertySetType_Particle     = 0x00000049,
    eEPropertySetType_AudioEmitter = 0x0000004B
};

// eCTexCoordSrcReflect.ReflectType
public enum eEReflectType
{
    eEReflectType_Reflect       = 0x00000000,
    eEReflectType_WorldEye      = 0x00000001,
    eEReflectType_WorldNormal   = 0x00000002,
    eEReflectType_TangentNormal = 0x00000003,
    eEReflectType_TangentEye    = 0x00000004
};

// eCRigidBody_PS.BodyFlag
public enum eERigidbody_Flag
{
    eERigidbody_Flag_NONE            = 0x00000000,
    eERigidbody_Flag_FROZEN          = 0x0000007E,
    eERigidbody_Flag_DISABLE_GRAVITY = 0x00000001,
    eERigidbody_Flag_Kinematic       = 0x00000080
};

// eCParticle_PS.UseRotationFrom
public enum eERotationFrom
{
    eERotationFrom_None   = 0x00000000,
    eERotationFrom_Entity = 0x00000001,
    eERotationFrom_Offset = 0x00000002,
    eERotationFrom_Normal = 0x00000003
};

// eCShaderDefault.BRDFLightingType
public enum eEShaderMaterialBRDFType
{
    eEShaderMaterialBRDFType_Simple     = 0x00000000,
    eEShaderMaterialBRDFType_Complex    = 0x00000001,
    eEShaderMaterialBRDFType_WrapAround = 0x00000002
};

// eCBillboard_PS.BlendMode
// eCShaderBase.BlendMode
public enum eEShaderMaterialBlendMode
{
    eEShaderMaterialBlendMode_Normal        = 0x00000000,
    eEShaderMaterialBlendMode_Masked        = 0x00000001,
    eEShaderMaterialBlendMode_AlphaBlend    = 0x00000002,
    eEShaderMaterialBlendMode_Modulate      = 0x00000003,
    eEShaderMaterialBlendMode_AlphaModulate = 0x00000004,
    eEShaderMaterialBlendMode_Translucent   = 0x00000005,
    eEShaderMaterialBlendMode_Brighten      = 0x00000007,
    eEShaderMaterialBlendMode_Darken        = 0x00000006,
    eEShaderMaterialBlendMode_Invisible     = 0x00000008
};

// eCShaderDefault.TransformationType
public enum eEShaderMaterialTransformation
{
    eEShaderMaterialTransformation_Default         = 0x00000000,
    eEShaderMaterialTransformation_Instanced       = 0x00000001,
    eEShaderMaterialTransformation_Skinned         = 0x00000002,
    eEShaderMaterialTransformation_Tree_Branches   = 0x00000003,
    eEShaderMaterialTransformation_Tree_Fronds     = 0x00000004,
    eEShaderMaterialTransformation_Tree_LeafCards  = 0x00000005,
    eEShaderMaterialTransformation_Tree_LeafMeshes = 0x00000006,
    eEShaderMaterialTransformation_Billboard       = 0x00000007,
    eEShaderMaterialTransformation_Morphed         = 0x00000008,
    eEShaderMaterialTransformation_Skinned_Morphed = 0x00000009,
    eEShaderMaterialTransformation_Vegetation      = 0x0000000A,
    eEShaderMaterialTransformation_Tree_Billboards = 0x0000000B
};

// eCShaderBase.MaxShaderVersion
public enum eEShaderMaterialVersion
{
    eEShaderMaterialVersion_1_1 = 0x00000000,
    eEShaderMaterialVersion_1_4 = 0x00000001,
    eEShaderMaterialVersion_2_0 = 0x00000002,
    eEShaderMaterialVersion_3_0 = 0x00000003
};

// eCIlluminated_PS.ShadowCasterType
public enum eEShadowCasterType
{
    eEShadowCasterType_Terrain  = 0x00000000,
    eEShadowCasterType_Building = 0x00000001,
    eEShadowCasterType_Object   = 0x00000002,
    eEShadowCasterType_None     = 0x00000003
};

// eCStaticPointLight_PS.ShadowMaskIndex
public enum eEShadowMaskIndex
{
    eEShadowMaskIndex_R = 0x00000002,
    eEShadowMaskIndex_G = 0x00000001,
    eEShadowMaskIndex_B = 0x00000000
};

// eCCollisionShape.ShapeAABBAdaptMode
public enum eEShapeAABBAdapt
{
    eEShapeAABBAdapt_None      = 0x00000000,
    eEShapeAABBAdapt_LocalNode = 0x00000001,
    eEShapeAABBAdapt_LocalTree = 0x00000002
};

// eCCollisionShape.Group
// gCStateGraphEventFilterCollisionShape.Group
public enum eEShapeGroup
{
    eEShapeGroup_Static            = 0x00000001,
    eEShapeGroup_Dynamic           = 0x00000002,
    eEShapeGroup_Shield            = 0x00000003,
    eEShapeGroup_MeleeWeapon       = 0x00000004,
    eEShapeGroup_Projectile        = 0x00000005,
    eEShapeGroup_Movement          = 0x00000006,
    eEShapeGroup_WeaponTrigger     = 0x00000007,
    eEShapeGroup_ParadeSphere      = 0x00000008,
    eEShapeGroup_Tree_Trunk        = 0x00000009,
    eEShapeGroup_Tree_Branches     = 0x0000000A,
    eEShapeGroup_Camera            = 0x0000000B,
    eEShapeGroup_Movement_ZoneNPC  = 0x0000000C,
    eEShapeGroup_HeightRepulsor    = 0x0000000D,
    eEShapeGroup_Cloth             = 0x0000000E,
    eEShapeGroup_PhysicalBodyPart  = 0x0000000F,
    eEShapeGroup_KeyframedBodyPart = 0x00000010,
    eEShapeGroup_Camera_Obstacle   = 0x00000011,
    eEShapeGroup_Projectile_Level  = 0x00000012,
    eEShapeGroup_Trigger           = 0x00000013,
    eEShapeGroup_Door              = 0x00000014
};

// eCMaterialResource2.PhysicMaterial
public enum eCMaterialResource2_eEShapeMaterial
{
    eEShapeMaterial_None             = 0x00000000,
    eEShapeMaterial_Wood             = 0x00000001,
    eEShapeMaterial_Metal            = 0x00000002,
    eEShapeMaterial_Water            = 0x00000003,
    eEShapeMaterial_Stone            = 0x00000004,
    eEShapeMaterial_Earth            = 0x00000005,
    eEShapeMaterial_Ice              = 0x00000006,
    eEShapeMaterial_Leather          = 0x00000007,
    eEShapeMaterial_Clay             = 0x00000008,
    eEShapeMaterial_Glass            = 0x00000009,
    eEShapeMaterial_Flesh            = 0x0000000A,
    eEShapeMaterial_Snow             = 0x0000000B,
    eEShapeMaterial_Debris           = 0x0000000C,
    eEShapeMaterial_Foliage          = 0x0000000D,
    eEShapeMaterial_Magic            = 0x0000000E,
    eEShapeMaterial_Grass            = 0x0000000F,
    eEShapeMaterial_SpringAndDamper1 = 0x00000010,
    eEShapeMaterial_SpringAndDamper2 = 0x00000011,
    eEShapeMaterial_SpringAndDamper3 = 0x00000012
};

// eCCollisionShape.Material
// gCStateGraphEventFilterCollisionShape.Material
public enum eEShapeMaterial
{
    eEShapeMaterial_None             = 0x00000000,
    eEShapeMaterial_Wood             = 0x00000001,
    eEShapeMaterial_Metal            = 0x00000002,
    eEShapeMaterial_Water            = 0x00000003,
    eEShapeMaterial_Stone            = 0x00000004,
    eEShapeMaterial_Earth            = 0x00000005,
    eEShapeMaterial_Ice              = 0x00000006,
    eEShapeMaterial_Leather          = 0x00000007,
    eEShapeMaterial_Clay             = 0x00000008,
    eEShapeMaterial_Glass            = 0x00000009,
    eEShapeMaterial_Flesh            = 0x0000000A,
    eEShapeMaterial_Snow             = 0x0000000B,
    eEShapeMaterial_Debris           = 0x0000000C,
    eEShapeMaterial_Foliage          = 0x0000000D,
    eEShapeMaterial_Magic            = 0x0000000E,
    eEShapeMaterial_Grass            = 0x0000000F,
    eEShapeMaterial_SpringAndDamper1 = 0x00000010,
    eEShapeMaterial_SpringAndDamper2 = 0x00000011,
    eEShapeMaterial_SpringAndDamper3 = 0x00000012,
    eEShapeMaterial_Damage           = 0x00000013,
    eEShapeMaterial_Sand             = 0x00000014,
    eEShapeMaterial_Movement         = 0x00000015,
    eEShapeMaterial_Axe              = 0x00000016
};

// eCGuiSplitImage2.DrawStyle
public enum eESplitImageStyle
{
    eESplitImageStyle_Scale          = 0x0000001B,
    eESplitImageStyle_Repeat         = 0x00000024,
    eESplitImageStyle_ScaleX_RepeatY = 0x00000023,
    eESplitImageStyle_RepeatX_ScaleY = 0x0000001C
};

// eCIlluminated_PS.StaticIlluminated
public enum eEStaticIlluminated
{
    eEStaticIlluminated_Static  = 0x00000000,
    eEStaticIlluminated_Dynamic = 0x00000001
};

// eCStrip_PS.SpawnMode
public enum eEStripSpawning
{
    eEStripSpawning_Movement   = 0x00000001,
    eEStripSpawning_Continuous = 0x00000000,
    eEStripSpawning_Timed      = 0x00000002
};

// eCTexCoordSrcOscillator.OscillatorTypeU
// eCTexCoordSrcOscillator.OscillatorTypeV
public enum eETexCoordSrcOscillatorType
{
    eETexCoordSrcOscillatorType_Pan           = 0x00000000,
    eETexCoordSrcOscillatorType_Stretch       = 0x00000001,
    eETexCoordSrcOscillatorType_StretchRepeat = 0x00000002,
    eETexCoordSrcOscillatorType_Jitter        = 0x00000003
};

// eCTexCoordSrcRotator.RotationType
public enum eETexCoordSrcRotatorType
{
    eETexCoordSrcRotatorType_Once      = 0x00000000,
    eETexCoordSrcRotatorType_Constant  = 0x00000001,
    eETexCoordSrcRotatorType_Oscillate = 0x00000002
};

// eCGuiButton2.TextAlign
// eCGuiStatic2.TextAlign
public enum eETextAlign
{
    eETextAlign_Left_Top      = 0x00000000,
    eETextAlign_Center_Top    = 0x00000001,
    eETextAlign_Right_Top     = 0x00000002,
    eETextAlign_Left_Middle   = 0x00000004,
    eETextAlign_Center_Middle = 0x00000005,
    eETextAlign_Right_Middle  = 0x00000006,
    eETextAlign_Left_Bottom   = 0x00000008,
    eETextAlign_Center_Bottom = 0x00000009,
    eETextAlign_Right_Bottom  = 0x0000000A
};

// eCParticle_PS.DrawStyle
public enum eETextureDrawStyle
{
    eETextureDrawStyle_Regular       = 0x00000000,
    eETextureDrawStyle_AlphaBlend    = 0x00000001,
    eETextureDrawStyle_Modulated     = 0x00000002,
    eETextureDrawStyle_Translucent   = 0x00000003,
    eETextureDrawStyle_AlphaModulate = 0x00000004,
    eETextureDrawStyle_Darken        = 0x00000005,
    eETextureDrawStyle_Brighten      = 0x00000006,
    eETextureDrawStyle_Invisible     = 0x00000007
};

// eCGuiTrackBar2.TicSide
public enum eETicSide
{
    eETicSide_Right = 0x00000000,
    eETicSide_Left  = 0x00000001,
    eETicSide_Both  = 0x00000002
};

// eCVegetationBrush_PS.ColorFunction
public enum eEVegetationBrushColorFunction
{
    eEVegetationBrushColorFunction_Random                         = 0x00000001,
    eEVegetationBrushColorFunction_PerlinNoise                    = 0x00000002,
    eEVegetationBrushColorFunction_PerlinNoise_Improved           = 0x00000003,
    eEVegetationBrushColorFunction_EbertNoise                     = 0x00000004,
    eEVegetationBrushColorFunction_PeacheyNoise                   = 0x00000005,
    eEVegetationBrushColorFunction_PeacheyNoise_Gradient          = 0x00000006,
    eEVegetationBrushColorFunction_PeacheyNoise_GradientValue     = 0x00000007,
    eEVegetationBrushColorFunction_PeacheyNoise_SparseConvolusion = 0x00000008,
    eEVegetationBrushColorFunction_PeacheyNoise_ValueConvolusion  = 0x00000009
};

// eCVegetationBrush_PS.Mode
public enum eEVegetationBrushMode
{
    eEVegetationBrushMode_Place    = 0x00000000,
    eEVegetationBrushMode_Remove   = 0x00000001,
    eEVegetationBrushMode_Colorize = 0x00000002
};

// eCVegetationBrush_PS.Placement
public enum eEVegetationBrushPlace
{
    eEVegetationBrushPlace_DistanceSelf  = 0x00000000,
    eEVegetationBrushPlace_DistanceOther = 0x00000001,
    eEVegetationBrushPlace_RemoveOther   = 0x00000002
};

// eCVegetationBrush_PS.ProbabilityFunction
public enum eEVegetationBrushProbabilityFunction
{
    eEVegetationBrushProbabilityFunction_None                           = 0x00000000,
    eEVegetationBrushProbabilityFunction_Shape                          = 0x00000001,
    eEVegetationBrushProbabilityFunction_PerlinNoise                    = 0x00000002,
    eEVegetationBrushProbabilityFunction_PerlinNoise_Improved           = 0x00000003,
    eEVegetationBrushProbabilityFunction_EbertNoise                     = 0x00000004,
    eEVegetationBrushProbabilityFunction_PeacheyNoise                   = 0x00000005,
    eEVegetationBrushProbabilityFunction_PeacheyNoise_Gradient          = 0x00000006,
    eEVegetationBrushProbabilityFunction_PeacheyNoise_GradientValue     = 0x00000007,
    eEVegetationBrushProbabilityFunction_PeacheyNoise_SparseConvolusion = 0x00000008,
    eEVegetationBrushProbabilityFunction_PeacheyNoise_ValueConvolusion  = 0x00000009
};

// eCVegetationBrush_PS.Shape
public enum eEVegetationBrushShape
{
    eEVegetationBrushShape_Circle = 0x00000000,
    eEVegetationBrushShape_Rect   = 0x00000001,
    eEVegetationBrushShape_Single = 0x00000002
};

// eCVegetation_Mesh.MeshShading
public enum eEVegetationMeshShading
{
    eEVegetationMeshShading_MeshNormal       = 0x00000000,
    eEVegetationMeshShading_EntryOrientation = 0x00000001
};

// eCParticle_PS.VelocityDirectionFrom
public enum eEVelocityDirectionFrom
{
    eEVelocityDirectionFrom_None                  = 0x00000000,
    eEVelocityDirectionFrom_StartPositionAndOwner = 0x00000001,
    eEVelocityDirectionFrom_OwnerAndStartPosition = 0x00000002,
    eEVelocityDirectionFrom_Owner                 = 0x00000003,
    eEVelocityDirectionFrom_World                 = 0x00000004
};

// eCWeatherZone_PS.AmbientBackLightOverwrite
// eCWeatherZone_PS.AmbientGeneralOverwrite
// eCWeatherZone_PS.AmbientIntensityOverwrite
// eCWeatherZone_PS.CloudColorOverwrite
// eCWeatherZone_PS.CloudThicknessrOverwrite
// eCWeatherZone_PS.FogColorOverwrite
// eCWeatherZone_PS.FogDensityOverwrite
// eCWeatherZone_PS.FogEndrOverwrite
// eCWeatherZone_PS.FogStartrOverwrite
// eCWeatherZone_PS.HazeColorOverwrite
// eCWeatherZone_PS.LightDiffuseOverwrite
// eCWeatherZone_PS.LightIntensityOverwrite
// eCWeatherZone_PS.LightSpecularOverwrite
// eCWeatherZone_PS.SkyColorOverwrite
public enum eEWeatherZoneOverwrite
{
    eEWeatherZoneOverwrite_None      = 0x00000000,
    eEWeatherZoneOverwrite_Overwrite = 0x00000001,
    eEWeatherZoneOverwrite_Modulate  = 0x00000002,
    eEWeatherZoneOverwrite_Add       = 0x00000003
};

// eCWeatherZone_PS.Shape
public enum eEWeatherZoneShape
{
    eEWeatherZoneShape_2D_Circle = 0x00000000,
    eEWeatherZoneShape_2D_Rect   = 0x00000001,
    eEWeatherZoneShape_3D_Sphere = 0x00000002,
    eEWeatherZoneShape_3D_Box    = 0x00000003
};

// gCScriptRoutine_PS.AIMode
public enum gEAIMode
{
    gEAIMode_None     = 0x00000000,
    gEAIMode_Sender   = 0x00000001,
    gEAIMode_Routine  = 0x00000002,
    gEAIMode_Sleep    = 0x00000003,
    gEAIMode_Observe  = 0x00000004,
    gEAIMode_Talk     = 0x00000005,
    gEAIMode_GotoBody = 0x00000006,
    gEAIMode_Watch    = 0x00000007,
    gEAIMode_Avoid    = 0x00000008,
    gEAIMode_Threaten = 0x00000009,
    gEAIMode_Attack   = 0x0000000A,
    gEAIMode_Down     = 0x0000000B,
    gEAIMode_Dead     = 0x0000000C
};

// gCAchievementBar.ViewMode
public enum gEAchievementViewMode
{
    gEAchievementViewMode_Counter = 0x00000000,
    gEAchievementViewMode_Credits = 0x00000001
};

// gCCombatMoveMelee.AlignToTarget
public enum gEAlignToTarget
{
    gEAlignToTarget_None      = 0x00000000,
    gEAlignToTarget_TargetDir = 0x00000001,
    gEAlignToTarget_Target    = 0x00000002,
    gEAlignToTarget_Free      = 0x00000003
};

// gCScriptRoutine_PS.AmbientAction
public enum gEAmbientAction
{
    gEAmbientAction_Ambient = 0x00000000,
    gEAmbientAction_Listen  = 0x00000001,
    gEAmbientAction_EMPTY   = 0x00000002
};

// gCInvAmountPicbox.Type
public enum gEAmountType
{
    gEAmountType_Gold     = 0x00000000,
    gEAmountType_Sale     = 0x00000001,
    gEAmountType_Purchase = 0x00000002
};

// gCAnchor_PS.AnchorType
public enum gEAnchorType
{
    gEAnchorType_Local  = 0x00000000,
    gEAnchorType_Roam   = 0x00000001,
    gEAnchorType_Patrol = 0x00000002,
    gEAnchorType_Event  = 0x00000003
};

// gCScriptRoutine_PS.AniState
// gCScriptRoutine_PS.FallbackAniState
public enum gEAniState
{
    gEAniState_Dummy0        = 0x00000000,
    gEAniState_Dummy1        = 0x00000001,
    gEAniState_Stand         = 0x00000002,
    gEAniState_Sneak         = 0x00000003,
    gEAniState_Attack        = 0x00000004,
    gEAniState_Parade        = 0x00000005,
    gEAniState_Kneel         = 0x00000006,
    gEAniState_SitGround     = 0x00000007,
    gEAniState_SitStool      = 0x00000008,
    gEAniState_SitBench      = 0x00000009,
    gEAniState_SitThrone     = 0x0000000A,
    gEAniState_SleepBed      = 0x0000000B,
    gEAniState_SleepGround   = 0x0000000C,
    gEAniState_SitBathtub    = 0x0000000D,
    gEAniState_Down          = 0x0000000E,
    gEAniState_DownBack      = 0x0000000F,
    gEAniState_Dead          = 0x00000010,
    gEAniState_DeadBack      = 0x00000011,
    gEAniState_Finished      = 0x00000012,
    gEAniState_FinishedBack  = 0x00000013,
    gEAniState_TalkStand     = 0x00000014,
    gEAniState_TalkSitGround = 0x00000015,
    gEAniState_TalkSitStool  = 0x00000016,
    gEAniState_TalkSitBench  = 0x00000017,
    gEAniState_TalkSitThrone = 0x00000018,
    gEAniState_Wade          = 0x00000019,
    gEAniState_Swim          = 0x0000001A,
    gEAniState_Dive          = 0x0000001B,
    gEAniState_Stumble       = 0x0000001C,
    gEAniState_Levitate      = 0x0000001D
};

// gCCombatMoveStumble.ResultingAniState
public enum gCCombatMoveStumble_gEAniState
{
    gEAniState_Stand    = 0x00000002,
    gEAniState_Down     = 0x0000000E,
    gEAniState_DownBack = 0x0000000F,
    gEAniState_Dead     = 0x00000010,
    gEAniState_DeadBack = 0x00000011
};

// gCArena_PS.Status
public enum gEArenaStatus
{
    gEArenaStatus_None    = 0x00000000,
    gEArenaStatus_Running = 0x00000001
};

// gCNPC_PS.AttitudeLock
public enum gEAttitude
{
    gEAttitude_None     = 0x00000000,
    gEAttitude_Friendly = 0x00000001,
    gEAttitude_Neutral  = 0x00000002,
    gEAttitude_Angry    = 0x00000003,
    gEAttitude_Hostile  = 0x00000004
};

// gCInfoCommandBoostAttribs.BoostTarget
public enum gEBoostTarget
{
    gEBoostTarget_Strength     = 0x00000000,
    gEBoostTarget_Dexterity    = 0x00000001,
    gEBoostTarget_Intelligence = 0x00000002
};

// gCNPC_PS.BraveryOverride
public enum gEBraveryOverride
{
    gEBraveryOverride_None   = 0x00000000,
    gEBraveryOverride_Brave  = 0x00000001,
    gEBraveryOverride_Coward = 0x00000002
};

// gCCombatMoveMelee.Action
public enum gCCombatMoveMelee_gECombatAction
{
    gECombatAction_Attack  = 0x00000001,
    gECombatAction_Parade  = 0x00000002,
    gECombatAction_Stumble = 0x00000003
};

// gCCombatMoveScriptState.MoveCombatAction
public enum gCCombatMoveScriptState_gECombatAction
{
    gECombatAction_None    = 0x00000000,
    gECombatAction_Attack  = 0x00000001,
    gECombatAction_Parade  = 0x00000002,
    gECombatAction_Stumble = 0x00000003
};

// gCCombatMoveMelee.AttackStumbleType
public enum gECombatAttackStumble
{
    gECombatAttackStumble_None       = 0x00000000,
    gECombatAttackStumble_ParadeType = 0x00000001
};

// gCCombatMoveMelee.ComboParade
public enum gECombatComboParade
{
    gECombatComboParade_None    = 0x00000000,
    gECombatComboParade_Parade  = 0x00000001,
    gECombatComboParade_Parade1 = 0x00000002
};

// gCCombatSystem_PS.FightAIMode
public enum gECombatFightAIMode
{
    gECombatFightAIMode_Active  = 0x00000000,
    gECombatFightAIMode_Passive = 0x00000001
};

// gCCombatMoveMelee.HitDirection
public enum gECombatHitDirection
{
    gECombatHitDirection_Fore  = 0x00000000,
    gECombatHitDirection_Left  = 0x00000001,
    gECombatHitDirection_Right = 0x00000002
};

// gCCombatStyle.CombatMode
public enum gECombatMode
{
    gECombatMode_None   = 0x00000000,
    gECombatMode_Carry  = 0x00000001,
    gECombatMode_Melee  = 0x00000002,
    gECombatMode_Ranged = 0x00000003,
    gECombatMode_Magic  = 0x00000004,
    gECombatMode_Cast   = 0x00000005
};

// gCCombatMoveMelee.ParadeReaction
public enum gCCombatMoveMelee_gECombatMove
{
    gECombatMove_None               = 0x00000000,
    gECombatMove_ParadeStumble      = 0x0000000E,
    gECombatMove_ParadeStumbleHeavy = 0x0000000F,
    gECombatMove_AttackStumble      = 0x0000000B,
    gECombatMove_AttackStumbleLeft  = 0x0000000C,
    gECombatMove_AttackStumbleRight = 0x0000000D
};

// gCCombatMoveMelee.StumbleReaction
public enum gCCombatMoveMelee2_gECombatMove
{
    gECombatMove_Stumble            = 0x00000019,
    gECombatMove_StumbleLight       = 0x0000001A,
    gECombatMove_StumbleHeavy       = 0x0000001B,
    gECombatMove_StumbleBack        = 0x0000001F,
    gECombatMove_StumbleDown        = 0x00000020,
    gECombatMove_ParadeStumble      = 0x0000000E,
    gECombatMove_ParadeStumbleHeavy = 0x0000000F
};

// gCCombatMoveMelee.WeaponSide
// gCCombatMoveParade.WeaponSide
public enum gECombatMoveSide
{
    gECombatMoveSide_Left  = 0x00000000,
    gECombatMoveSide_Right = 0x00000001
};

// gCCombatMoveMelee.AttackType
// gCCombatStyle.AttackType
// gCCombatStyle.ParadeType
public enum gECombatParadeType
{
    gECombatParadeType_None    = 0x00000000,
    gECombatParadeType_Fist    = 0x00000001,
    gECombatParadeType_Weapon  = 0x00000002,
    gECombatParadeType_Magic   = 0x00000004,
    gECombatParadeType_Ranged  = 0x00000008,
    gECombatParadeType_Monster = 0x00000010,
    gECombatParadeType_Shield  = 0x0000001B
};

// gCCombatMoveMeleePhase.PhaseType
public enum gECombatPhaseType
{
    gECombatPhaseType_Raise         = 0x00000001,
    gECombatPhaseType_PowerRaise    = 0x00000002,
    gECombatPhaseType_Hit           = 0x00000003,
    gECombatPhaseType_Recover       = 0x00000005,
    gECombatPhaseType_Hoover        = 0x00000004,
    gECombatPhaseType_Parade        = 0x00000006,
    gECombatPhaseType_Strafe        = 0x00000007,
    gECombatPhaseType_CounterParade = 0x00000008,
    gECombatPhaseType_CounterAttack = 0x00000009
};

// gCCombatStyleAniPose.Pose
public enum gECombatPose
{
    gECombatPose_P0 = 0x00000000,
    gECombatPose_P1 = 0x00000001,
    gECombatPose_P2 = 0x00000002
};

// gCNPC_PS.LastPlayerComment
public enum gEComment
{
    gEComment_None              = 0x00000000,
    gEComment_DefeatInquisition = 0x00000001,
    gEComment_Theft             = 0x00000002,
    gEComment_Livestock         = 0x00000003,
    gEComment_Defeat            = 0x00000004,
    gEComment_Count             = 0x00000005
};

// gCInfoConditionSkillValue.CompareOperation
public enum gECompareOperation
{
    gECompareOperation_Equal        = 0x00000000,
    gECompareOperation_NotEqual     = 0x00000001,
    gECompareOperation_Less         = 0x00000002,
    gECompareOperation_LessEqual    = 0x00000003,
    gECompareOperation_Greater      = 0x00000004,
    gECompareOperation_GreaterEqual = 0x00000005
};

// gCNPC_PS.LastPlayerCrime
public enum gECrime
{
    gECrime_None            = 0x00000000,
    gECrime_MurderLivestock = 0x00000001,
    gECrime_Theft           = 0x00000002,
    gECrime_Murder          = 0x00000003,
    gECrime_Count           = 0x00000004
};

// gCNPC_PS.DamageCalculationType
public enum gEDamageCalculationType
{
    gEDamageCalculationType_Normal   = 0x00000000,
    gEDamageCalculationType_Monster  = 0x00000001,
    gEDamageCalculationType_Immortal = 0x00000002
};

// gCDamage_PS.DamageType
public enum gEDamageType
{
    gEDamageType_None    = 0x00000000,
    gEDamageType_Edge    = 0x00000001,
    gEDamageType_Blunt   = 0x00000002,
    gEDamageType_Point   = 0x00000003,
    gEDamageType_Fire    = 0x00000004,
    gEDamageType_Ice     = 0x00000005,
    gEDamageType_Magic   = 0x00000006,
    gEDamageType_Physics = 0x00000007
};

// gCDoor_PS.Status
public enum gEDoorStatus
{
    gEDoorStatus_Open   = 0x00000000,
    gEDoorStatus_Closed = 0x00000001
};

// gCEffect_PS.DecayMode
public enum gEEffectDecayMode
{
    gEEffectDecayMode_Decay = 0x00000000,
    gEEffectDecayMode_Kill  = 0x00000001
};

// gCEffectCommandKillEntityRange.Range
public enum gEEffectKillRange
{
    gEEffectKillRange_All   = 0x00000000,
    gEEffectKillRange_Range = 0x00000001
};

// gCEffectCommandPlaySound.CoordinateSystem
// gCEffectCommandSpawnEntity.CoordinateSystem
// gCEffectCommandSpawnEntityList.CoordinateSystem
// gCEffectCommandSpawnEntitySwitch.CoordinateSystem
public enum gEEffectLink
{
    gEEffectLink_Independent  = 0x00000000,
    gEEffectLink_TargetEntity = 0x00000001,
    gEEffectLink_TargetBone   = 0x00000002
};

// gCEffect_PS.LoopMode
public enum gEEffectLoopMode
{
    gEEffectLoopMode_Once   = 0x00000000,
    gEEffectLoopMode_Loop   = 0x00000001,
    gEEffectLoopMode_Repeat = 0x00000002
};

// gCEffectCommandRunScript.OtherType
public enum gEEffectScriptOtherType
{
    gEEffectScriptOtherType_TemplateEntity = 0x00000001,
    gEEffectScriptOtherType_Entity         = 0x00000000
};

// gCEffectCommandRunScript.ParamType
public enum gEEffectScriptParamType
{
    gEEffectScriptParamType_UseEffectCommandTime = 0x00000001,
    gEEffectScriptParamType_UseParam             = 0x00000000
};

// gCEffect_PS.StopMode
public enum gEEffectStopMode
{
    gEEffectStopMode_Decay   = 0x00000000,
    gEEffectStopMode_Disable = 0x00000001,
    gEEffectStopMode_Kill    = 0x00000002
};

// gCEffect_PS.TargetMode
public enum gEEffectTargetMode
{
    gEEffectTargetMode_Self   = 0x00000000,
    gEEffectTargetMode_Parent = 0x00000001,
    gEEffectTargetMode_Script = 0x00000002
};

// gCDynamicLayer.EntityType
public enum gEEntityType
{
    gEEntityType_Game      = 0x00000000,
    gEEntityType_Temporary = 0x00000001
};

// gCEquipPicbox2.EquipSlot
public enum gCEquipPicbox2_gEEquipSlot
{
    gEEquipSlot_MeleeWeapon  = 0x00000001,
    gEEquipSlot_MeleeShield  = 0x00000002,
    gEEquipSlot_RangedWeapon = 0x00000003,
    gEEquipSlot_RangedAmmo   = 0x00000004,
    gEEquipSlot_Amulet       = 0x00000005,
    gEEquipSlot_Ring1        = 0x00000006,
    gEEquipSlot_Ring2        = 0x00000007,
    gEEquipSlot_Armor        = 0x00000008,
    gEEquipSlot_Helmet       = 0x00000009
};

// gCInventoryStack.EquipSlot
public enum gEEquipSlot
{
    gEEquipSlot_None         = 0x00000000,
    gEEquipSlot_MeleeWeapon  = 0x00000001,
    gEEquipSlot_MeleeShield  = 0x00000002,
    gEEquipSlot_RangedWeapon = 0x00000003,
    gEEquipSlot_RangedAmmo   = 0x00000004,
    gEEquipSlot_Amulet       = 0x00000005,
    gEEquipSlot_Ring1        = 0x00000006,
    gEEquipSlot_Ring2        = 0x00000007,
    gEEquipSlot_Armor        = 0x00000008,
    gEEquipSlot_Helmet       = 0x00000009
};

// gCNPC_PS.LastFightAgainstPlayer
public enum gEFight
{
    gEFight_None    = 0x00000000,
    gEFight_Lost    = 0x00000001,
    gEFight_Won     = 0x00000002,
    gEFight_Cancel  = 0x00000003,
    gEFight_Running = 0x00000004
};

// gCProjectile2_PS.FlightPathType
public enum gEFlightPathType
{
    gEFlightPathType_Ballistic = 0x00000000,
    gEFlightPathType_Seeking   = 0x00000001
};

// gCInteraction_PS.FocusNameType
public enum gEFocusNameType
{
    gEFocusNameType_Skeleton = 0x00000000,
    gEFocusNameType_Entity   = 0x00000001,
    gEFocusNameType_Bone     = 0x00000002,
    gEFocusNameType_Disable  = 0x00000003,
    gEFocusNameType_Center   = 0x00000004
};

// gCInteraction_PS.FocusPriority
public enum gEFocusPriority
{
    gEFocusPriority_None    = 0x00000000,
    gEFocusPriority_Lowest  = 0x00000001,
    gEFocusPriority_Low     = 0x00000002,
    gEFocusPriority_Normal  = 0x00000003,
    gEFocusPriority_High    = 0x00000004,
    gEFocusPriority_Highest = 0x00000005
};

// gCFocusInteractFilter2.Source
public enum gEFocusSource
{
    gEFocusSource_Camera             = 0x00000000,
    gEFocusSource_Player             = 0x00000001,
    gEFocusSource_PlayerPosCameraDir = 0x00000002,
    gEFocusSource_CameraPosPlayerDir = 0x00000003,
    gEFocusSource_Auto               = 0x00000004
};

// gCGUIFilter.FilterType
public enum gEGUIFilterType
{
    gEGUIFilterType_Status      = 0x00000001,
    gEGUIFilterType_NPCInfoType = 0x00000002,
    gEGUIFilterType_Location    = 0x00000003,
    gEGUIFilterType_Category    = 0x00000004,
    gEGUIFilterType_InfoType    = 0x00000005,
    gEGUIFilterType_Details     = 0x00000006
};

// gCGammaScrollBar2.GammaRamp
// gCGammaTrackbar2.GammaRamp
public enum gEGammaRamp
{
    gEGammaRamp_Brightness = 0x00000000,
    gEGammaRamp_Contrast   = 0x00000001,
    gEGammaRamp_Red        = 0x00000002,
    gEGammaRamp_Green      = 0x00000003,
    gEGammaRamp_Blue       = 0x00000004
};

// gCNPC_PS.Gender
public enum gEGender
{
    gEGender_Male   = 0x00000000,
    gEGender_Female = 0x00000001
};

// gCInfoCommandSetGuardStatus.GuardStatus
public enum gEGuardStatus
{
    gEGuardStatus_Active          = 0x00000000,
    gEGuardStatus_FirstWarnGiven  = 0x00000001,
    gEGuardStatus_SecondWarnGiven = 0x00000002,
    gEGuardStatus_Inactive        = 0x00000003
};

// gCNPC_PS.GuardStatus
public enum gCNPC_PS_gEGuardStatus
{
    gEGuardStatus_Active          = 0x00000000,
    gEGuardStatus_FirstWarnGiven  = 0x00000001,
    gEGuardStatus_SecondWarnGiven = 0x00000002,
    gEGuardStatus_Inactive        = 0x00000003,
    gEGuardStatus_Behind          = 0x00000004
};

// gCAIZone_PS.Guild
public enum gEGuild
{
    gEGuild_None = 0x00000000,
    gEGuild_Don  = 0x00000001,
    gEGuild_Dig  = 0x00000002,
    gEGuild_Grd  = 0x00000003,
    gEGuild_Cit  = 0x00000004,
    gEGuild_Inq  = 0x00000005,
    gEGuild_Mag  = 0x00000006,
    gEGuild_Pir  = 0x00000007
};

// gCNPC_PS.Guild
public enum gCNPC_PS_gEGuild
{
    gEGuild_None  = 0x00000000,
    gEGuild_Don   = 0x00000001,
    gEGuild_Dig   = 0x00000002,
    gEGuild_Grd   = 0x00000003,
    gEGuild_Cit   = 0x00000004,
    gEGuild_Inq   = 0x00000005,
    gEGuild_Mag   = 0x00000006,
    gEGuild_Pir   = 0x00000007,
    gEGuild_Count = 0x00000008
};

// gCScriptRoutine_PS.HitDirection
public enum gEHitDirection
{
    gEHitDirection_Left  = 0x00000000,
    gEHitDirection_Right = 0x00000001
};

// gCHudPage2.PageID
public enum gEHudPage
{
    gEHudPage_None             = 0x00000000,
    gEHudPage_Game             = 0x00000001,
    gEHudPage_Inventory        = 0x00000002,
    gEHudPage_Character        = 0x00000003,
    gEHudPage_Log              = 0x00000004,
    gEHudPage_Map              = 0x00000005,
    gEHudPage_CraftSelect      = 0x00000006,
    gEHudPage_ItemSelect       = 0x00000007,
    gEHudPage_Loot             = 0x00000008,
    gEHudPage_Pickpocket       = 0x00000009,
    gEHudPage_Trade            = 0x0000000A,
    gEHudPage_Dialog           = 0x0000000B,
    gEHudPage_Talk             = 0x0000000C,
    gEHudPage_Menu_Back        = 0x0000001A,
    gEHudPage_Menu_Main        = 0x0000000D,
    gEHudPage_Menu_Game        = 0x0000000E,
    gEHudPage_Menu_Load        = 0x0000000F,
    gEHudPage_Menu_Save        = 0x00000010,
    gEHudPage_Menu_Achievement = 0x00000011,
    gEHudPage_Menu_Options     = 0x00000012,
    gEHudPage_Menu_Video       = 0x00000013,
    gEHudPage_Menu_Audio       = 0x00000014,
    gEHudPage_Menu_Input       = 0x00000015,
    gEHudPage_Menu_Settings    = 0x00000016,
    gEHudPage_Menu_System      = 0x00000017,
    gEHudPage_Menu_Credits     = 0x00000018,
    gEHudPage_Menu_Cheats      = 0x00000019,
    gEHudPage_Outro            = 0x0000001B,
    gEHudPage_Loading          = 0x0000001C
};

// gCPageTimerProgressBar.HudPage
public enum gCPageTimerProgressBar_gEHudPage
{
    gEHudPage_Pickpocket  = 0x00000009,
    gEHudPage_CraftSelect = 0x00000006,
    gEHudPage_ItemSelect  = 0x00000007,
    gEHudPage_Loot        = 0x00000008
};

// gCInventoryList.Icon
public enum gEIcon
{
    gEIcon_Inventory = 0x00000000,
    gEIcon_Craft     = 0x00000001
};

// gCInfo.ConditionType
public enum gEInfoCondType
{
    gEInfoCondType_Crime          = 0x00000000,
    gEInfoCondType_Duel           = 0x00000001,
    gEInfoCondType_Hello          = 0x00000002,
    gEInfoCondType_General        = 0x00000003,
    gEInfoCondType_Overtime       = 0x00000004,
    gEInfoCondType_Open           = 0x00000005,
    gEInfoCondType_Activator      = 0x00000006,
    gEInfoCondType_Running        = 0x00000007,
    gEInfoCondType_Delivery       = 0x00000008,
    gEInfoCondType_PartDelivery   = 0x00000009,
    gEInfoCondType_Success        = 0x0000000A,
    gEInfoCondType_DoCancel       = 0x0000000B,
    gEInfoCondType_Failed         = 0x0000000C,
    gEInfoCondType_Cancelled      = 0x0000000D,
    gEInfoCondType_Join           = 0x0000000E,
    gEInfoCondType_Dismiss        = 0x0000000F,
    gEInfoCondType_Trade          = 0x00000011,
    gEInfoCondType_PickPocket     = 0x00000012,
    gEInfoCondType_Ready          = 0x00000013,
    gEInfoCondType_Lost           = 0x00000014,
    gEInfoCondType_Reactivator    = 0x00000015,
    gEInfoCondType_Won            = 0x00000016,
    gEInfoCondType_MasterThief    = 0x00000017,
    gEInfoCondType_FirstWarn      = 0x0000001A,
    gEInfoCondType_SecondWarn     = 0x0000001B,
    gEInfoCondType_MobJoin        = 0x0000001C,
    gEInfoCondType_SlaveJoin      = 0x0000001D,
    gEInfoCondType_LongTimeNoSee  = 0x0000001E,
    gEInfoCondType_PoliticalCrime = 0x00000020,
    gEInfoCondType_MobDismiss     = 0x00000021,
    gEInfoCondType_Wait           = 0x00000022,
    gEInfoCondType_Heal           = 0x00000023,
    gEInfoCondType_NothingToSay   = 0x00000032,
    gEInfoCondType_End            = 0x00000033,
    gEInfoCondType_Back           = 0x00000034,
    gEInfoCondType_Finished       = 0x00000036,
    gEInfoCondType_NotYetFinished = 0x00000038,
};

// gCInfoConditionQuestStatus.CondType
public enum gCInfoConditionQuestStatus_gEInfoCondType
{
    gEInfoCondType_Open           = 0x00000005,
    gEInfoCondType_Overtime       = 0x00000004,
    gEInfoCondType_Running        = 0x00000007,
    gEInfoCondType_Success        = 0x0000000A,
    gEInfoCondType_Failed         = 0x0000000C,
    gEInfoCondType_Cancelled      = 0x0000000D,
    gEInfoCondType_Ready          = 0x00000013,
    gEInfoCondType_Lost           = 0x00000014,
    gEInfoCondType_Won            = 0x00000016,
    gEInfoCondType_NotYetFinished = 0x00000035,
    gEInfoCondType_Finished       = 0x00000036
};

// gCInfoCommandPickPocket.Gesture
// gCInfoCommandSay.Gesture
// gCInfoCommandSaySVM.Gesture
// gCInfoCommandThink.Gesture
public enum gEInfoGesture
{
    gEInfoGesture_Ambient  = 0x00000000,
    gEInfoGesture_Me       = 0x00000001,
    gEInfoGesture_You      = 0x00000002,
    gEInfoGesture_Threaten = 0x00000003,
    gEInfoGesture_Yes      = 0x00000004,
    gEInfoGesture_No       = 0x00000005,
    gEInfoGesture_All      = 0x00000006,
    gEInfoGesture_Back     = 0x00000007,
    gEInfoGesture_Tell     = 0x00000008,
    gEInfoGesture_Admonish = 0x00000009,
    gEInfoGesture_Secret   = 0x0000000A,
    gEInfoGesture_Recall   = 0x0000000B,
    gEInfoGesture_YouAndMe = 0x0000000C
};

// gCInfoCommandAddNPCInfo.Location
// gCInfoCommandRemoveNPCInfo.Location
// gCQuest.LocationInfo
public enum gEInfoLocation
{
    gEInfoLocation_Main      = 0x00000000,
    gEInfoLocation_Harbor    = 0x00000001,
    gEInfoLocation_Monastery = 0x00000002,
    gEInfoLocation_Don       = 0x00000003
};

// gCInfoConditionNPCStatus.SecondaryNPCStatus
public enum gEInfoNPCStatus
{
    gEInfoNPCStatus_Alive             = 0x00000000,
    gEInfoNPCStatus_UnHarmed          = 0x00000001,
    gEInfoNPCStatus_Defeated          = 0x00000002,
    gEInfoNPCStatus_Dead              = 0x00000003,
    gEInfoNPCStatus_TalkedToPlayer    = 0x00000004,
    gEInfoNPCStatus_NotTalkedToPlayer = 0x00000005
};

// gCInfoCommandAddNPCInfo.Type
// gCInfoCommandRemoveNPCInfo.Type
public enum gEInfoNPCType
{
    gEInfoNPCType_Vendor  = 0x00000000,
    gEInfoNPCType_Teacher = 0x00000001,
    gEInfoNPCType_VIP     = 0x00000002
};

// gCInfo.Type
public enum gEInfoType
{
    gEInfoType_Refuse    = 0x00000000,
    gEInfoType_Important = 0x00000001,
    gEInfoType_News      = 0x00000002,
    gEInfoType_Info      = 0x00000003,
    gEInfoType_Parent    = 0x00000004,
    gEInfoType_Comment   = 0x00000005
};

// gCItemInfo.ItemDetails
public enum gEInfoView
{
    gEInfoView_Header              = 0x00000000,
    gEInfoView_Description         = 0x00000001,
    gEInfoView_Damage              = 0x00000002,
    gEInfoView_RequiredSkills      = 0x00000003,
    gEInfoView_ModifySkills        = 0x00000004,
    gEInfoView_RequiredIngredients = 0x00000005,
    gEInfoView_GoldValue           = 0x00000006,
    gEInfoView_CraftRequiredSkills = 0x00000007,
    gEInfoView_RequiredMana        = 0x00000008
};

// gCSkillInfo.ItemDetails
public enum gCSkillInfo_gEInfoView
{
    gEInfoView_Header      = 0x00000000,
    gEInfoView_Description = 0x00000001,
    gEInfoView_Level       = 0x00000002,
    gEInfoView_NextLevel   = 0x00000003
};

// gCDialogInfo.View
public enum gCDialogInfo_gCDialogInfo_gEInfoView
{
    gEInfoView_Header      = 0x00000000,
    gEInfoView_Description = 0x00000001,
    gEInfoView_NextLevel   = 0x00000002,
    gEInfoView_Learnpoints = 0x00000003,
    gEInfoView_GoldCosts   = 0x00000004
};

// gCHintStatic.View
public enum gCHintStatic_gEInfoView
{
    gEInfoView_Image     = 0x00000000,
    gEInfoView_Text      = 0x00000001,
    gEInfoView_ImageText = 0x00000002
};

// gCInteraction.Type
public enum gEInteractionType
{
    gEInteractionType_Interact_NPC        = 0x00000000,
    gEInteractionType_Interact_Player     = 0x00000001,
    gEInteractionType_InventoryUse_Player = 0x00000002,
    gEInteractionType_QuickUse_Player     = 0x00000003,
    gEInteractionType_Magic               = 0x00000004
};

// gCFocusInteractFilter2.UseType
public enum SPECIAL_gEInteractionUseType
{
    gEInteractionUseType_None = 0x00000000,
    gEInteractionUseType_Item = 0x00000001,
    gEInteractionUseType_NPC  = 0x00000002
};

// gCInteraction_PS.UseType
public enum gEInteractionUseType
{
    gEInteractionUseType_None           = 0x00000000,
    gEInteractionUseType_Item           = 0x00000001,
    gEInteractionUseType_NPC            = 0x00000002,
    gEInteractionUseType_Roam           = 0x00000003,
    gEInteractionUseType_CoolWeapon     = 0x00000004,
    gEInteractionUseType_Anvil          = 0x00000005,
    gEInteractionUseType_Forge          = 0x00000006,
    gEInteractionUseType_GrindStone     = 0x00000007,
    gEInteractionUseType_Cauldron       = 0x00000008,
    gEInteractionUseType_Barbecue       = 0x00000009,
    gEInteractionUseType_Alchemy        = 0x0000000A,
    gEInteractionUseType_LookAt         = 0x0000000B,
    gEInteractionUseType_Bookstand      = 0x0000000C,
    gEInteractionUseType_TakeCarryable  = 0x0000000D,
    gEInteractionUseType_DropCarryable  = 0x0000000E,
    gEInteractionUseType_PickOre        = 0x0000000F,
    gEInteractionUseType_PickGround     = 0x00000010,
    gEInteractionUseType_DigGround      = 0x00000011,
    gEInteractionUseType_Fieldwork      = 0x00000012,
    gEInteractionUseType_WriteScroll    = 0x00000013,
    gEInteractionUseType_SawLog         = 0x00000014,
    gEInteractionUseType_PracticeStaff  = 0x00000015,
    gEInteractionUseType_Bed            = 0x00000016,
    gEInteractionUseType_SleepGround    = 0x00000017,
    gEInteractionUseType_SweepFloor     = 0x00000018,
    gEInteractionUseType_Dance          = 0x00000019,
    gEInteractionUseType_Flute          = 0x0000001A,
    gEInteractionUseType_Boss           = 0x0000001B,
    gEInteractionUseType_Throne         = 0x0000001C,
    gEInteractionUseType_Pace           = 0x0000001D,
    gEInteractionUseType_Bard           = 0x0000001E,
    gEInteractionUseType_Stool          = 0x0000001F,
    gEInteractionUseType_Bench          = 0x00000020,
    gEInteractionUseType_Waterpipe      = 0x00000021,
    gEInteractionUseType_Fountain       = 0x00000022,
    gEInteractionUseType_PirateTreasure = 0x00000023,
    gEInteractionUseType_Campfire       = 0x00000024,
    gEInteractionUseType_Stove          = 0x00000025,
    gEInteractionUseType_SitGround      = 0x00000026,
    gEInteractionUseType_Smalltalk      = 0x00000027,
    gEInteractionUseType_Preach         = 0x00000028,
    gEInteractionUseType_Spectator      = 0x00000029,
    gEInteractionUseType_Stand          = 0x0000002A,
    gEInteractionUseType_Guard          = 0x0000002B,
    gEInteractionUseType_Trader         = 0x0000002C,
    gEInteractionUseType_Listener       = 0x0000002D,
    gEInteractionUseType_Cupboard       = 0x0000002E,
    gEInteractionUseType_Pee            = 0x0000002F,
    gEInteractionUseType_Bathtub        = 0x00000030,
    gEInteractionUseType_Door           = 0x00000031,
    gEInteractionUseType_Chest          = 0x00000032,
    gEInteractionUseType_Flee           = 0x00000033,
    gEInteractionUseType_Lever          = 0x00000034,
    gEInteractionUseType_Button         = 0x00000035,
    gEInteractionUseType_Winch          = 0x00000036,
    gEInteractionUseType_Destructable   = 0x00000037,
    gEInteractionUseType_Goldsmith      = 0x00000038,
    gEInteractionUseType_Altar          = 0x00000039,
    gEInteractionUseType_Sarcophagus    = 0x0000003A,
    gEInteractionUseType_SecretRing     = 0x0000003B,
    gEInteractionUseType_MagicOrb       = 0x0000003C,
    gEInteractionUseType_RedBarrier     = 0x0000003D,
    gEInteractionUseType_BlueBarrier    = 0x0000003E,
    gEInteractionUseType_LizardBook     = 0x0000003F,
    gEInteractionUseType_PracticeSword  = 0x00000040,
    gEInteractionUseType_PracticeMagic  = 0x00000041,
    gEInteractionUseType_Plunder        = 0x00000042,
    gEInteractionUseType_Loo            = 0x00000043,
    gEInteractionUseType_Attack         = 0x00000044,
    gEInteractionUseType_Keyhole        = 0x00000045
};

// gCItem_PS.Category
public enum gEItemCategory
{
    gEItemCategory_None       = 0x00000000,
    gEItemCategory_Weapon     = 0x00000001,
    gEItemCategory_Armor      = 0x00000002,
    gEItemCategory_Consumable = 0x00000003,
    gEItemCategory_Empty_D    = 0x00000004,
    gEItemCategory_Magic      = 0x00000005,
    gEItemCategory_Misc       = 0x00000006,
    gEItemCategory_Written    = 0x00000007,
    gEItemCategory_Empty_B    = 0x00000008,
    gEItemCategory_Empty_E    = 0x00000009,
    gEItemCategory_Empty_F    = 0x0000000A,
    gEItemCategory_Count      = 0x0000000B
};

// gCItem_PS.HoldType
public enum gEItemHoldType
{
    gEItemHoldType_None          = 0x00000000,
    gEItemHoldType_1H            = 0x00000001,
    gEItemHoldType_2H            = 0x00000002,
    gEItemHoldType_BS            = 0x00000003,
    gEItemHoldType_Arrow         = 0x00000004,
    gEItemHoldType_Bow           = 0x00000005,
    gEItemHoldType_CrossBow      = 0x00000006,
    gEItemHoldType_Bolt          = 0x00000007,
    gEItemHoldType_Fist          = 0x00000008,
    gEItemHoldType_Shield        = 0x00000009,
    gEItemHoldType_Armor         = 0x0000000A,
    gEItemHoldType_Helmet        = 0x0000000B,
    gEItemHoldType_Staff         = 0x0000000C,
    gEItemHoldType_Amulet        = 0x0000000D,
    gEItemHoldType_Ring          = 0x0000000E,
    gEItemHoldType_Rune          = 0x0000000F,
    gEItemHoldType_Torch         = 0x00000010,
    gEItemHoldType_CarryFront    = 0x00000011,
    gEItemHoldType_Axe           = 0x00000012,
    gEItemHoldType_Cast          = 0x0000001D,
    gEItemHoldType_FocusCast     = 0x0000001F,
    gEItemHoldType_Magic         = 0x0000001E,
    gEItemHoldType_Apple         = 0x00000013,
    gEItemHoldType_Bread         = 0x00000014,
    gEItemHoldType_Jar           = 0x00000015,
    gEItemHoldType_Joint         = 0x00000016,
    gEItemHoldType_Meat          = 0x00000017,
    gEItemHoldType_Potion        = 0x00000018,
    gEItemHoldType_Saringda      = 0x00000019,
    gEItemHoldType_Saw           = 0x0000001A,
    gEItemHoldType_Scoop         = 0x0000001B,
    gEItemHoldType_Stew          = 0x0000001C,
    gEItemHoldType_MagicMissile  = 0x00000020,
    gEItemHoldType_MagicFireball = 0x00000021,
    gEItemHoldType_MagicIcelance = 0x00000022,
    gEItemHoldType_Flute         = 0x00000023
};

// gCItem_PS.UseType
public enum gEItemUseType
{
    gEItemUseType_None          = 0x00000000,
    gEItemUseType_1H            = 0x00000001,
    gEItemUseType_2H            = 0x00000002,
    gEItemUseType_BS            = 0x00000003,
    gEItemUseType_Arrow         = 0x00000004,
    gEItemUseType_Bow           = 0x00000005,
    gEItemUseType_CrossBow      = 0x00000006,
    gEItemUseType_Bolt          = 0x00000007,
    gEItemUseType_Fist          = 0x00000008,
    gEItemUseType_Shield        = 0x00000009,
    gEItemUseType_Armor         = 0x0000000A,
    gEItemUseType_Helmet        = 0x0000000B,
    gEItemUseType_Staff         = 0x0000000C,
    gEItemUseType_Amulet        = 0x0000000D,
    gEItemUseType_Ring          = 0x0000000E,
    gEItemUseType_Rune          = 0x0000000F,
    gEItemUseType_Torch         = 0x00000010,
    gEItemUseType_CarryFront    = 0x00000011,
    gEItemUseType_Axe           = 0x00000012,
    gEItemUseType_Cast          = 0x00000013,
    gEItemUseType_FocusCast     = 0x00000014,
    gEItemUseType_MagicMissile  = 0x00000015,
    gEItemUseType_MagicFireball = 0x00000016,
    gEItemUseType_MagicIcelance = 0x00000017,
    gEItemUseType_MagicAmmo     = 0x00000018,
    gEItemUseType_MagicFrost    = 0x00000019,
    gEItemUseType_Head          = 0x0000001A
};

// gCCombatWeaponConfig.LeftUseType
// gCCombatWeaponConfig.RightUseType
public enum gCCombatWeaponConfig_gEItemUseType
{
    gEItemUseType_None          = 0x00000000,
    gEItemUseType_1H            = 0x00000001,
    gEItemUseType_2H            = 0x00000002,
    gEItemUseType_BS            = 0x00000003,
    gEItemUseType_Arrow         = 0x00000004,
    gEItemUseType_Bow           = 0x00000005,
    gEItemUseType_CrossBow      = 0x00000006,
    gEItemUseType_Bolt          = 0x00000007,
    gEItemUseType_Fist          = 0x00000008,
    gEItemUseType_Shield        = 0x00000009,
    gEItemUseType_Staff         = 0x0000000C,
    gEItemUseType_Axe           = 0x00000012,
    gEItemUseType_Torch         = 0x00000010,
    gEItemUseType_CarryFront    = 0x00000011,
    gEItemUseType_Cast          = 0x00000013,
    gEItemUseType_FocusCast     = 0x00000014,
    gEItemUseType_MagicMissile  = 0x00000015,
    gEItemUseType_MagicFireball = 0x00000016,
    gEItemUseType_MagicIcelance = 0x00000017,
    gEItemUseType_MagicAmmo     = 0x00000018,
    gEItemUseType_MagicFrost    = 0x00000019
};

// gCLock_PS.Status
public enum gELockStatus
{
    gELockStatus_Locked   = 0x00000000,
    gELockStatus_Unlocked = 0x00000001
};

// gCMiscLabel.InfoType
public enum gEMiscInfo
{
    gEMiscInfo_Guild = 0x00000000
};

// gCMiscProgressBar.InfoType
public enum gCMiscProgressBar_gEMiscInfo
{
    gEMiscInfo_StatusEffect = 0x00000001,
    gEMiscInfo_Health       = 0x00000002,
    gEMiscInfo_Mana         = 0x00000003
};

// gCMouseInvAxisTrackbar2.MouseAxis
// gCMouseSensitivityTrackbar2.MouseAxis
public enum gEMouseAxis
{
    gEMouseAxis_X = 0x00000000,
    gEMouseAxis_Y = 0x00000001
};

// gCCollisionCircle_PS.Type
public enum gENavObstacleType
{
    gENavObstacleType_Obstacle  = 0x00000000,
    gENavObstacleType_Climbable = 0x00000001
};

// gCAIHelper_FreePoint_PS.NavTestResult
// gCNavOffset_PS.NavTestResult
// gCNavPath_PS.NavTestResult
// gCNavZone_PS.NavTestResult
// gCNegZone_PS.NavTestResult
// gCPrefPath_PS.NavTestResult
public enum gENavTestResult
{
    gENavTestResult_Succeeded                        = 0x00000000,
    gENavTestResult_NavPathWithOneDeadEnd            = 0x00000001,
    gENavTestResult_NavPathWithTwoDeadEnds           = 0x00000002,
    gENavTestResult_NavPathBlockedByCollisionCircle  = 0x00000003,
    gENavTestResult_NavPathIllegalBuild              = 0x00000004,
    gENavTestResult_PrefPathOutOfNavZone             = 0x00000005,
    gENavTestResult_PrefPathBlockedByCollisionCircle = 0x00000006,
    gENavTestResult_PrefPathIllegalBuild             = 0x00000007,
    gENavTestResult_NavZoneInConflict                = 0x00000008,
    gENavTestResult_NavZoneIllegalBuild              = 0x00000009,
    gENavTestResult_NegZoneOutOfNavZone              = 0x0000000A,
    gENavTestResult_NegZoneIllegalBuild              = 0x0000000B,
    gENavTestResult_FreePointOutOfNavArea            = 0x0000000C,
    gENavTestResult_FreePointInNegZone               = 0x0000000D,
    gENavTestResult_FreePointInCollisionCircle       = 0x0000000E,
    gENavTestResult_NavOffsetOutOfNavArea            = 0x00000010
};

// gCInfoCommandRunScript.OtherType
public enum gEOtherType
{
    gEOtherType_TemplateEntity = 0x00000001,
    gEOtherType_Entity         = 0x00000000
};

// gCLootStatic2.VisibleMode
public enum gCLootStatic2_gEPageMode
{
    gEPageMode_Dialog    = 0x00000003,
    gEPageMode_UserMin   = 0x00000004,
    gEPageMode_UserSlots = 0x00000006,
    gEPageMode_UserMax   = 0x00000007
};

// gCHudPage2.PageMode
public enum gCHudPage2_gEPageMode
{
    gEPageMode_None      = 0x00000000,
    gEPageMode_Panorama  = 0x00000001,
    gEPageMode_Dialog    = 0x00000003,
    gEPageMode_UserMin   = 0x00000004,
    gEPageMode_UserSlots = 0x00000006,
    gEPageMode_UserMax   = 0x00000007
};

// gCCompassPicbox2.VisibleMode
// gCEquipPicbox2.VisibleMode
// gCLockpickStatic2.VisibleMode
// gCMiscProgressBar.VisibleMode
// gCQuickPicbox2.VisibleMode
// gCTutorialLabel2.VisibleMode
public enum gEPageMode
{
    gEPageMode_UserMin   = 0x00000004,
    gEPageMode_UserSlots = 0x00000006,
    gEPageMode_UserMax   = 0x00000007
};

// gCHintStatic.PaintArea
public enum gEPaintArea
{
    gEPaintArea_Client  = 0x00000000,
    gEPaintArea_Window  = 0x00000001,
    gEPaintArea_Desktop = 0x00000002
};

// gCParty_PS.PartyMemberType
public enum gEPartyMemberType
{
    gEPartyMemberType_None        = 0x00000000,
    gEPartyMemberType_Party       = 0x00000001,
    gEPartyMemberType_Mob         = 0x00000002,
    gEPartyMemberType_Slave       = 0x00000003,
    gEPartyMemberType_Controlled  = 0x00000004,
    gEPartyMemberType_Summoned    = 0x00000005,
    gEPartyMemberType_PlayerGuide = 0x00000006
};

// gCQuestActor.ActorType
public enum gEQuestActor
{
    gEQuestActor_Client = 0x00000000,
    gEQuestActor_Target = 0x00000001
};

// gCQuest.Status
public enum gEQuestStatus
{
    gEQuestStatus_Open      = 0x00000000,
    gEQuestStatus_Running   = 0x00000001,
    gEQuestStatus_Success   = 0x00000002,
    gEQuestStatus_Failed    = 0x00000003,
    gEQuestStatus_Obsolete  = 0x00000004,
    gEQuestStatus_Cancelled = 0x00000005,
    gEQuestStatus_Lost      = 0x00000006,
    gEQuestStatus_Won       = 0x00000007
};

// gCQuest.Type
public enum gEQuestType
{
    gEQuestType_HasItems  = 0x00000000,
    gEQuestType_Report    = 0x00000001,
    gEQuestType_Kill      = 0x00000002,
    gEQuestType_Defeat    = 0x00000003,
    gEQuestType_DriveAway = 0x00000004,
    gEQuestType_Arena     = 0x00000005,
    gEQuestType_BringNpc  = 0x00000006,
    gEQuestType_FollowNpc = 0x00000007,
    gEQuestType_EnterArea = 0x00000008,
    gEQuestType_Plunder   = 0x0000000B,
    gEQuestType_Sparring  = 0x0000000C,
    gEQuestType_Duel      = 0x0000000D
};

// gCQuickPicbox2.QuickSlot
public enum gEQuickSlot
{
    gEQuickSlot_1  = 0x00000000,
    gEQuickSlot_2  = 0x00000001,
    gEQuickSlot_3  = 0x00000002,
    gEQuickSlot_4  = 0x00000003,
    gEQuickSlot_5  = 0x00000004,
    gEQuickSlot_6  = 0x00000005,
    gEQuickSlot_7  = 0x00000006,
    gEQuickSlot_8  = 0x00000007,
    gEQuickSlot_9  = 0x00000008,
    gEQuickSlot_10 = 0x00000009
};

// gCNPC_PS.LastPlayerAR
// gCNPC_PS.Reason
public enum gEReason
{
    gEReason_None            = 0x00000000,
    gEReason_SVM_Ambient     = 0x00000001,
    gEReason_SVM_Combat      = 0x00000002,
    gEReason_SVM_Party       = 0x00000003,
    gEReason_PlayerTalk      = 0x00000004,
    gEReason_ImportantInfo   = 0x00000005,
    gEReason_PlayerSneaking  = 0x00000006,
    gEReason_PlayerWeapon    = 0x00000007,
    gEReason_PlayerRoom      = 0x00000008,
    gEReason_PlayerUseBed    = 0x00000009,
    gEReason_Eat             = 0x0000000A,
    gEReason_Ransack         = 0x0000000B,
    gEReason_Fighter         = 0x0000000C,
    gEReason_Attacker        = 0x0000000D,
    gEReason_Nuisance        = 0x0000000E,
    gEReason_Joke            = 0x0000000F,
    gEReason_Frost           = 0x00000010,
    gEReason_Damage          = 0x00000011,
    gEReason_DamageLivestock = 0x00000012,
    gEReason_MurderLivestock = 0x00000013,
    gEReason_Theft           = 0x00000014,
    gEReason_Illusion        = 0x00000015,
    gEReason_GateGuard       = 0x00000016,
    gEReason_Defeat          = 0x00000017,
    gEReason_Inspect         = 0x00000018,
    gEReason_Finish          = 0x00000019,
    gEReason_Raider          = 0x0000001A,
    gEReason_Enemy           = 0x0000001B,
    gEReason_Murder          = 0x0000001C,
    gEReason_Duel            = 0x0000001D,
    gEReason_Arena           = 0x0000001E,
    gEReason_Kill            = 0x0000001F,
    gEReason_Count           = 0x00000020
};

// gCRecipe_PS.Craft
public enum gERecipeCategory
{
    gERecipeCategory_Alchemy          = 0x00000000,
    gERecipeCategory_Cooking          = 0x00000001,
    gERecipeCategory_Frying           = 0x00000003,
    gERecipeCategory_Goldsmith        = 0x00000004,
    gERecipeCategory_WriteScroll      = 0x00000005,
    gERecipeCategory_Smith_Forge      = 0x00000006,
    gERecipeCategory_Smith_Anvil      = 0x00000007,
    gERecipeCategory_Smith_CoolWeapon = 0x00000008,
    gERecipeCategory_Smith_GrindStone = 0x00000009
};

// gCCreditsLabel2.ScrollStart
public enum gEScrollStart
{
    gEScrollStart_Top    = 0x00000000,
    gEScrollStart_Bottom = 0x00000001
};

// gCSession.State
public enum gESession_State
{
    gESession_State_None          = 0x00000000,
    gESession_State_Movement      = 0x00000001,
    gESession_State_Fight         = 0x00000002,
    gESession_State_Ride_Movement = 0x00000003,
    gESession_State_Ride_Fight    = 0x00000004,
    gESession_State_ItemUse       = 0x00000005,
    gESession_State_Inventory     = 0x00000006,
    gESession_State_Dialog        = 0x00000007,
    gESession_State_Trade         = 0x00000008,
    gESession_State_InteractObj   = 0x00000009,
    gESession_State_Journal       = 0x0000000A,
    gESession_State_Editor        = 0x0000000B
};

// gCSkillPicbox.SkillType
public enum gESkill : uint
{
    gESkill_None            = 0xFFFFFFFF,
    gESkill_Atrib_HP        = 0x00000000,
    gESkill_Atrib_MP        = 0x00000001,
    gESkill_Stat_LV         = 0x00000002,
    gESkill_Stat_XP         = 0x00000003,
    gESkill_Stat_LP         = 0x00000004,
    gESkill_Stat_HP         = 0x00000005,
    gESkill_Stat_MP         = 0x00000006,
    gESkill_Stat_STR        = 0x00000007,
    gESkill_Stat_DEX        = 0x00000008,
    gESkill_Stat_INT        = 0x00000009,
    gESkill_Prot_Edge       = 0x0000000A,
    gESkill_Prot_Blunt      = 0x0000000B,
    gESkill_Prot_Point      = 0x0000000C,
    gESkill_Prot_Fire       = 0x0000000D,
    gESkill_Prot_Ice        = 0x0000000E,
    gESkill_Prot_Magic      = 0x0000000F,
    gESkill_Combat_Sword    = 0x00000010,
    gESkill_Combat_Axe      = 0x00000011,
    gESkill_Combat_Staff    = 0x00000012,
    gESkill_Combat_Bow      = 0x00000013,
    gESkill_Combat_CrossBow = 0x00000014,
    gESkill_Magic_Circle    = 0x00000015,
    gESkill_Magic_Fireball  = 0x00000016,
    gESkill_Magic_Frost     = 0x00000017,
    gESkill_Magic_Missile   = 0x00000018,
    gESkill_Misc_Scribe     = 0x00000020,
    gESkill_Misc_Alchemy    = 0x0000001F,
    gESkill_Misc_Smith      = 0x00000019,
    gESkill_Misc_Mining     = 0x0000001A,
    gESkill_Misc_Sneak      = 0x0000001D,
    gESkill_Misc_Lockpick   = 0x0000001B,
    gESkill_Misc_Pickpocket = 0x0000001C,
    gESkill_Misc_Acrobat    = 0x0000001E,
    gESkill_Misc_Trophy     = 0x00000021
};

// gCSkillValueBase.Skill
public enum gCSkillValueBase_gESkill : uint
{
    gESkill_None            = 0xFFFFFFFF,
    gESkill_Atrib_HP        = 0x00000000,
    gESkill_Atrib_MP        = 0x00000001,
    gESkill_Stat_LV         = 0x00000002,
    gESkill_Stat_XP         = 0x00000003,
    gESkill_Stat_LP         = 0x00000004,
    gESkill_Stat_HP         = 0x00000005,
    gESkill_Stat_MP         = 0x00000006,
    gESkill_Stat_STR        = 0x00000007,
    gESkill_Stat_DEX        = 0x00000008,
    gESkill_Stat_INT        = 0x00000009,
    gESkill_Prot_Edge       = 0x0000000A,
    gESkill_Prot_Blunt      = 0x0000000B,
    gESkill_Prot_Point      = 0x0000000C,
    gESkill_Prot_Fire       = 0x0000000D,
    gESkill_Prot_Ice        = 0x0000000E,
    gESkill_Prot_Magic      = 0x0000000F,
    gESkill_Combat_Sword    = 0x00000010,
    gESkill_Combat_Axe      = 0x00000011,
    gESkill_Combat_Staff    = 0x00000012,
    gESkill_Combat_Bow      = 0x00000013,
    gESkill_Combat_CrossBow = 0x00000014,
    gESkill_Magic_Circle    = 0x00000015,
    gESkill_Magic_Fireball  = 0x00000016,
    gESkill_Magic_Frost     = 0x00000017,
    gESkill_Magic_Missile   = 0x00000018,
    gESkill_Misc_Smith      = 0x00000019,
    gESkill_Misc_Mining     = 0x0000001A,
    gESkill_Misc_Lockpick   = 0x0000001B,
    gESkill_Misc_Pickpocket = 0x0000001C,
    gESkill_Misc_Sneak      = 0x0000001D,
    gESkill_Misc_Acrobat    = 0x0000001E,
    gESkill_Misc_Alchemy    = 0x0000001F,
    gESkill_Misc_Scribe     = 0x00000020,
    gESkill_Misc_Trophy     = 0x00000021
};

// gCSkillProgressBar.SkillType
public enum gCSkillProgressBar_gESkill
{
    gESkill_Stat_LV         = 0x00000002,
    gESkill_Stat_XP         = 0x00000003,
    gESkill_Stat_LP         = 0x00000004,
    gESkill_Stat_HP         = 0x00000005,
    gESkill_Stat_MP         = 0x00000006,
    gESkill_Stat_STR        = 0x00000007,
    gESkill_Stat_DEX        = 0x00000008,
    gESkill_Stat_INT        = 0x00000009,
    gESkill_Prot_Edge       = 0x0000000A,
    gESkill_Prot_Blunt      = 0x0000000B,
    gESkill_Prot_Point      = 0x0000000C,
    gESkill_Prot_Fire       = 0x0000000D,
    gESkill_Prot_Ice        = 0x0000000E,
    gESkill_Prot_Magic      = 0x0000000F,
    gESkill_Combat_Sword    = 0x00000010,
    gESkill_Combat_Axe      = 0x00000011,
    gESkill_Combat_Staff    = 0x00000012,
    gESkill_Combat_Bow      = 0x00000013,
    gESkill_Combat_CrossBow = 0x00000014,
    gESkill_Magic_Circle    = 0x00000015,
    gESkill_Magic_Fireball  = 0x00000016,
    gESkill_Magic_Frost     = 0x00000017,
    gESkill_Magic_Missile   = 0x00000018,
    gESkill_Misc_Scribe     = 0x00000020,
    gESkill_Misc_Alchemy    = 0x0000001F,
    gESkill_Misc_Smith      = 0x00000019,
    gESkill_Misc_Mining     = 0x0000001A,
    gESkill_Misc_Sneak      = 0x0000001D,
    gESkill_Misc_Lockpick   = 0x0000001B,
    gESkill_Misc_Pickpocket = 0x0000001C,
    gESkill_Misc_Acrobat    = 0x0000001E,
    gESkill_Misc_Trophy     = 0x00000021
};

// gCModifySkill.Modifier
public enum gESkillModifier
{
    gESkillModifier_AddValue        = 0x00000000,
    gESkillModifier_SetToMax        = 0x00000001,
    gESkillModifier_SetToValue      = 0x00000002,
    gESkillModifier_AddPercentOfMax = 0x00000003
};

// gCInventoryList.Character
// gCMiscProgressBar.Character
public enum gESpecialEntity
{
    gESpecialEntity_Player    = 0x00000000,
    gESpecialEntity_Focus     = 0x00000001,
    gESpecialEntity_Interact  = 0x00000002,
    gESpecialEntity_Trader    = 0x00000003,
    gESpecialEntity_DialogNPC = 0x00000004
};

// gCNPC_PS.Species
public enum gESpecies
{
    gESpecies_None          = 0x00000000,
    gESpecies_Human         = 0x00000001,
    gESpecies_Lizard        = 0x00000002,
    gESpecies_Brontok       = 0x00000003,
    gESpecies_Wolf_Tame     = 0x00000004,
    gESpecies_Lurker        = 0x00000005,
    gESpecies_Ashbeast      = 0x00000006,
    gESpecies_Nautilus      = 0x00000007,
    gESpecies_Dragonfly     = 0x00000008,
    gESpecies_Mantis        = 0x00000009,
    gESpecies_Scorpion      = 0x0000000A,
    gESpecies_Skeleton      = 0x0000000B,
    gESpecies_Swampmummy    = 0x0000000C,
    gESpecies_Rotworm       = 0x0000000D,
    gESpecies_Skeleton_Tame = 0x0000000E,
    gESpecies_Gnome         = 0x0000000F,
    gESpecies_Boar          = 0x00000010,
    gESpecies_Wolf          = 0x00000011,
    gESpecies_Stingrat      = 0x00000012,
    gESpecies_Vulture       = 0x00000013,
    gESpecies_Thundertail   = 0x00000014,
    gESpecies_Ogre          = 0x00000015,
    gESpecies_Ogre_Tame     = 0x00000016,
    gESpecies_Cow           = 0x00000017,
    gESpecies_Pig           = 0x00000018,
    gESpecies_Chicken       = 0x00000019,
    gESpecies_Ghost         = 0x0000001A,
    gESpecies_Count         = 0x0000001B
};

// gCSpinButton.SpinType
public enum gESpinButtonType
{
    gESpinButtonType_Prev  = 0x00000000,
    gESpinButtonType_Cycle = 0x00000001,
    gESpinButtonType_Next  = 0x00000002
};

// gCInventoryStack.Type
public enum gEStackType
{
    gEStackType_Normal = 0x00000000,
    gEStackType_Trade  = 0x00000001,
    gEStackType_Hidden = 0x00000002
};

// gCStateGraphAction.EventType
public enum gEStateGraphEventType
{
    gEStateGraphEventType_None      = 0x00000000,
    gEStateGraphEventType_Trigger   = 0x00000001,
    gEStateGraphEventType_Untrigger = 0x00000002,
    gEStateGraphEventType_Touch     = 0x00000003,
    gEStateGraphEventType_Untouch   = 0x00000004,
    gEStateGraphEventType_Damage    = 0x00000005
};

// gCSkillPicbox.ViewMode
public enum gEViewMode
{
    gEViewMode_Name        = 0x00000000,
    gEViewMode_Description = 0x00000001,
    gEViewMode_Value       = 0x00000002,
    gEViewMode_Icon        = 0x00000003,
    gEViewMode_IconValue   = 0x00000004,
    gEViewMode_NameValue   = 0x00000005
};

// gCEquipPicbox2.ViewMode
public enum gCEquipPicbox2_gEViewMode
{
    gEViewMode_Value     = 0x00000002,
    gEViewMode_Icon      = 0x00000003,
    gEViewMode_IconValue = 0x00000004,
    gEViewMode_Damage    = 0x00000006
};

// gCQuickPicbox2.ViewMode
public enum gCQuickPicbox2_gEViewMode
{
    gEViewMode_Value     = 0x00000002,
    gEViewMode_Icon      = 0x00000003,
    gEViewMode_IconValue = 0x00000004,
    gEViewMode_Hold      = 0x00000007
};

// gCCombatAI.CombatWalkMode
// gCNavigation_PS.GuideWalkMode
// gCQuest.GuideWalkMode
public enum gEWalkMode
{
    gEWalkMode_Run    = 0x00000002,
    gEWalkMode_Sneak  = 0x00000000,
    gEWalkMode_Walk   = 0x00000001,
    gEWalkMode_Sprint = 0x00000003
};

// gCWrittenStatic2.WrittenType
public enum gEWrittenType : uint
{
    gEWrittenType_Invalid = 0xFFFFFFFF,
    gEWrittenType_Book    = 0x00000000,
    gEWrittenType_Letter  = 0x00000001,
    gEWrittenType_Recipe  = 0x00000002,
    gEWrittenType_Map     = 0x00000003
};
}
