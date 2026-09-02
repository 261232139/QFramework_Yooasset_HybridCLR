using System;

namespace CustomAssets.App.Game.App.MonoBehaviour
{
    public class AnimationEventReceiver : UnityEngine.MonoBehaviour
    {
        public Action AnimationEventTrigger;
        public Action<int> IntAnimationEventTrigger;
        public Action<string> StringAnimationEventTrigger;
        
        public void TriggerVoidAnimationEvent()
        {
            AnimationEventTrigger?.Invoke();
        }
        
        public void TriggerIntAnimationEvent(int value)
        {
            IntAnimationEventTrigger?.Invoke(value);
        }

        public void TriggerStringAnimationEvent(string value)
        {
            StringAnimationEventTrigger?.Invoke(value);
        }
    }
}
