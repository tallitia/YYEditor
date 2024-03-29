﻿using UnityEngine;
using System;
using System.Collections.Generic;
using LuaInterface;
using BindType = ToLuaMenu.BindType;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using PK_Game;
using Spine.Unity;

public static class CustomSettings
{
    public static string FrameworkPath = Application.dataPath + "/Game";
    public static string saveDir = FrameworkPath + "/ToLua/Source/Generate/";
    public static string luaDir = FrameworkPath + "/Lua/";
    public static string toluaBaseType = FrameworkPath + "/ToLua/BaseType/";
    public static string toluaLuaDir = FrameworkPath + "/ToLua/Lua";

    //导出时强制做为静态类的类型(注意customTypeList 还要添加这个类型才能导出)
    //unity 有些类作为sealed class, 其实完全等价于静态类
    public static List<Type> staticClassTypes = new List<Type>
    {
        typeof(UnityEngine.Application),
        typeof(UnityEngine.Time),
        typeof(UnityEngine.Screen),
        typeof(UnityEngine.SleepTimeout),
        typeof(UnityEngine.Input),
        typeof(UnityEngine.Resources),
        typeof(UnityEngine.Physics),
        typeof(UnityEngine.RenderSettings),
        typeof(UnityEngine.QualitySettings),
        typeof(UnityEngine.GL),
        typeof(UnityEngine.Graphics), 
        typeof(UnityEngine.PlayerPrefs),
    };

    //附加导出委托类型(在导出委托时, customTypeList 中牵扯的委托类型都会导出， 无需写在这里)
    public static DelegateType[] customDelegateList =
    {
        _DT(typeof(Action)),
        _DT(typeof(UnityEngine.Events.UnityAction)),
        _DT(typeof(System.Predicate<int>)),
        _DT(typeof(System.Action<int>)),
        _DT(typeof(System.Comparison<int>)),
		_DT(typeof(System.Func<int, int>)),
		_DT(typeof(DG.Tweening.TweenCallback)),
    };

    //在这里添加你要导出注册到lua的类型列表
    public static BindType[] customTypeList =
    {
        _GT(typeof(Debugger)).SetNameSpace(null),
        _GT(typeof(DG.Tweening.Ease)),
        _GT(typeof(DG.Tweening.DOTween)),
        _GT(typeof(DG.Tweening.Tween)).SetBaseType(typeof(System.Object)).AddExtendType(typeof(DG.Tweening.TweenExtensions)),
        _GT(typeof(DG.Tweening.Sequence)).AddExtendType(typeof(DG.Tweening.TweenSettingsExtensions)),
        _GT(typeof(DG.Tweening.Tweener)).AddExtendType(typeof(DG.Tweening.TweenSettingsExtensions)),
        _GT(typeof(DG.Tweening.LoopType)),
        _GT(typeof(DG.Tweening.PathMode)),
        _GT(typeof(DG.Tweening.PathType)),
        _GT(typeof(DG.Tweening.RotateMode)),
        _GT(typeof(Component)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
        _GT(typeof(Transform)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
        _GT(typeof(Material)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
        _GT(typeof(Rigidbody)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
        _GT(typeof(Camera)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
        _GT(typeof(AudioSource)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions)),
        _GT(typeof(AudioClip)),
        _GT(typeof(AudioListener)),
        _GT(typeof(PlayerPrefs)),
        _GT(typeof(ArrayList)),
        _GT(typeof(Behaviour)),
        _GT(typeof(MonoBehaviour)),
        _GT(typeof(GameObject)),
        _GT(typeof(TrackedReference)),
        _GT(typeof(Application)),
        _GT(typeof(Physics)),
        _GT(typeof(Collider)),
        _GT(typeof(Time)),
        _GT(typeof(Texture)),
        _GT(typeof(Texture2D)),
        _GT(typeof(Shader)),
        _GT(typeof(Renderer)),
        _GT(typeof(SpriteRenderer)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions43)),
        _GT(typeof(WWW)),
        _GT(typeof(Screen)),
        _GT(typeof(CameraClearFlags)),
        _GT(typeof(AssetBundle)),
        _GT(typeof(TextAsset)),
        _GT(typeof(ParticleSystem)),
        _GT(typeof(AsyncOperation)).SetBaseType(typeof(System.Object)),
        _GT(typeof(LightType)),
        _GT(typeof(SleepTimeout)),
        _GT(typeof(Animator)),
        _GT(typeof(Input)),
        _GT(typeof(KeyCode)),
        _GT(typeof(SkinnedMeshRenderer)),
        _GT(typeof(Space)),
        _GT(typeof(MeshRenderer)),

        _GT(typeof(BoxCollider)),
        _GT(typeof(MeshCollider)),
        _GT(typeof(SphereCollider)),
        _GT(typeof(CharacterController)),
        _GT(typeof(CapsuleCollider)),
        _GT(typeof(BoxCollider2D)),

        _GT(typeof(Animation)),
        _GT(typeof(AnimationClip)).SetBaseType(typeof(UnityEngine.Object)),
        _GT(typeof(AnimationState)),
        _GT(typeof(AnimationBlendMode)),
        _GT(typeof(QueueMode)),
        _GT(typeof(PlayMode)),
        _GT(typeof(WrapMode)),

        _GT(typeof(QualitySettings)),
        _GT(typeof(RenderSettings)),
        _GT(typeof(BlendWeights)),
        _GT(typeof(RenderTexture)),
        _GT(typeof(Resources)),  
          
		//ugui
		_GT(typeof(RectTransform)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions46)),
        _GT(typeof(Text)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions46)),
        _GT(typeof(InputField)),
        _GT(typeof(Image)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions46)),
        _GT(typeof(RawImage)),
        _GT(typeof(Rect)),
        _GT(typeof(Button)),
        _GT(typeof(ScrollRect)),
        _GT(typeof(UIScrollRect)),
        _GT(typeof(Scrollbar)),
        _GT(typeof(Sprite)),
        _GT(typeof(Canvas)),
        _GT(typeof(CanvasScaler)),
        _GT(typeof(Outline)),
        _GT(typeof(CanvasGroup)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions46)),
        _GT(typeof(ContentSizeFitter)),
        _GT(typeof(Toggle)),
        _GT(typeof(ToggleGroup)),
        _GT(typeof(Slider)).AddExtendType(typeof(DG.Tweening.ShortcutExtensions46)),
        _GT(typeof(TextAnchor)),
        _GT(typeof(LayoutElement)),
        _GT(typeof(LayoutGroup)),
        _GT(typeof(PointerEventData)),
        _GT(typeof(Dropdown)),


        //SkeletonGraphic
        _GT(typeof(SkeletonGraphic)),
        _GT(typeof(Spine.AnimationState)),
        _GT(typeof(Spine.TrackEntry)),
        _GT(typeof(Spine.Animation)),

        //Custom UI
        _GT(typeof(UILayout)),
        _GT(typeof(UIScrollPage)),
        _GT(typeof(UIImageAnimation)),
        _GT(typeof(UISpriteAnimation)),
        _GT(typeof(UISpriteRenderAnimation)),
        _GT(typeof(UIFollower)),
        _GT(typeof(UIInfiniteScroller)),
        _GT(typeof(UIDelayCall)),
        _GT(typeof(UIHollowMask)),
        _GT(typeof(UIDropdown)),
        _GT(typeof(UIFogImage)),
        _GT(typeof(UICharacter)),
        _GT(typeof(UIParticleAnimation)),
        _GT(typeof(UITextAnimation)),
        _GT(typeof(UIScrollNumber)),
        _GT(typeof(UIScrapeImage)),
        _GT(typeof(ScrapeImage)),
        _GT(typeof(UIScrollbar)),
        _GT(typeof(ScreenTouch)),
        _GT(typeof(UIImageLoader)),
        _GT(typeof(UIreversemask)),

        //framework
        _GT(typeof(Util)),
        _GT(typeof(AppConst)),
        _GT(typeof(Config)),
        _GT(typeof(LuaHelper)),
        _GT(typeof(LuaListener)),
        _GT(typeof(UIListener)),
        _GT(typeof(DragListener)),
        _GT(typeof(PointerListener)),
        _GT(typeof(ByteBuffer)),
        _GT(typeof(CharacterData)),

        _GT(typeof(MainGame)),
        _GT(typeof(LangManager)),
        _GT(typeof(LayerManager)),
        _GT(typeof(LuaManager)),
        _GT(typeof(AudioManager)),
        _GT(typeof(TimerManager)),
        _GT(typeof(ThreadManager)),
        _GT(typeof(NetworkManager)),
        _GT(typeof(AssetManager)),
        _GT(typeof(SDKAdapter)),
        _GT(typeof(PhoneDevice)),
        _GT(typeof(Disk)),
        _GT(typeof(RigidBodyBallRoot)),
        _GT(typeof(NewAStar)),
        _GT(typeof(MapPoint)),
        _GT(typeof(PKSpriteAnimation)),
        _GT(typeof(UUniversalComps)),
        _GT(typeof(UIResource)),
        _GT(typeof(UIResourceComponentItem)),
        _GT(typeof(UIHelper)),
    };

    public static List<Type> dynamicList = new List<Type>()
    {
        typeof(MeshRenderer),
#if !UNITY_5_4_OR_NEWER
        typeof(ParticleEmitter),
        typeof(ParticleRenderer),
        typeof(ParticleAnimator),
#endif

        typeof(BoxCollider),
        typeof(MeshCollider),
        typeof(SphereCollider),
        typeof(CharacterController),
        typeof(CapsuleCollider),

        typeof(Animation),
        typeof(AnimationClip),
        typeof(AnimationState),

        typeof(BlendWeights),
        typeof(RenderTexture),
        typeof(Rigidbody),
    };

    //重载函数，相同参数个数，相同位置out参数匹配出问题时, 需要强制匹配解决
    //使用方法参见例子14
    public static List<Type> outList = new List<Type>()
    {

    };

    //ngui优化，下面的类没有派生类，可以作为sealed class
    public static List<Type> sealedList = new List<Type>()
    {
        /*typeof(Transform),
        typeof(UIRoot),
        typeof(UICamera),
        typeof(UIViewport),
        typeof(UIPanel),
        typeof(UILabel),
        typeof(UIAnchor),
        typeof(UIAtlas),
        typeof(UIFont),
        typeof(UITexture),
        typeof(UISprite),
        typeof(UIGrid),
        typeof(UITable),
        typeof(UIWrapGrid),
        typeof(UIInput),
        typeof(UIScrollView),
        typeof(UIEventListener),
        typeof(UIScrollBar),
        typeof(UICenterOnChild),
        typeof(UIScrollView),        
        typeof(UIButton),
        typeof(UITextList),
        typeof(UIPlayTween),
        typeof(UIDragScrollView),
        typeof(UISpriteAnimation),
        typeof(UIWrapContent),
        typeof(TweenWidth),
        typeof(TweenAlpha),
        typeof(TweenColor),
        typeof(TweenRotation),
        typeof(TweenPosition),
        typeof(TweenScale),
        typeof(TweenHeight),
        typeof(TypewriterEffect),
        typeof(UIToggle),
        typeof(Localization),*/
    };

    public static BindType _GT(Type t)
    {
        return new BindType(t);
    }

    public static DelegateType _DT(Type t)
    {
        return new DelegateType(t);
    }
}
