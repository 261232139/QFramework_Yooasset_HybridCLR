using System.ComponentModel;
using SRF.Service;
using UnityEngine;
using UnityEngine.Scripting;

public delegate void SROptionsPropertyChanged(object sender, string propertyName);

#if !DISABLE_SRDEBUGGER
[Preserve]
#endif
public partial class SROptions : INotifyPropertyChanged
{
    private static readonly SROptions _current = new SROptions();

    public static SROptions Current
    {
        get { return _current; }
    }

    //#if !DISABLE_SRDEBUGGER
    //    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    //    public static void OnStartup()
    //    {
    //        SRServiceManager.GetService<SRDebugger.Internal.InternalOptionsRegistry>().AddOptionContainer(Current);
    //    }
    //#endif

//#if !DISABLE_SRDEBUGGER
    //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void OnStartup()
    {
        var registry = SRServiceManager.GetService<SRDebugger.Internal.InternalOptionsRegistry>();
        if (registry == null) return;

        // 1. 注册插件主容器
        registry.AddOptionContainer(Current);

        // 2. 动态实例化并注册 HotUpdate 中的 SROptions_BD
        string typeName = "SROptions_BD, HotUpdate";
        System.Type bdType = System.Type.GetType(typeName);
        
        if (bdType != null)
        {
            try
            {
                object bdInstance = System.Activator.CreateInstance(bdType);
                if (bdInstance != null)
                {
                    registry.AddOptionContainer(bdInstance);
                }
            }
            catch (System.Exception e)
            {
                // 常见的错误包括：没有无参构造函数、类是抽象类等
                Debug.LogWarning($"[SRDebugger] 实例化 SROptions_BD 失败: {e.Message}");
            }
        }
    }
//#endif

    public event SROptionsPropertyChanged PropertyChanged;
    
#if UNITY_EDITOR
    [JetBrains.Annotations.NotifyPropertyChangedInvocator]
#endif
    public void OnPropertyChanged(string propertyName)
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, propertyName);
        }

        if (InterfacePropertyChangedEventHandler != null)
        {
            InterfacePropertyChangedEventHandler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    private event PropertyChangedEventHandler InterfacePropertyChangedEventHandler;

    event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
    {
        add { InterfacePropertyChangedEventHandler += value; }
        remove { InterfacePropertyChangedEventHandler -= value; }
    }
}
