using UnityEngine;
using QFramework;

namespace HotUpdate
{
    internal class StateEnterLobby : AbstractState<HotUpdateState, HotUpdateRunner>
    {
        public StateEnterLobby(FSM<HotUpdateState> fsm, HotUpdateRunner target) : base(fsm, target)
        {
        }

        protected override void OnEnter()
        {
            var context = Launch.LaunchContext.Instance;
            context.Progress = 1f;

            if (Launch.LoadingUI.Instance != null)
                Launch.LoadingUI.Instance.Hide();

            UI.LobbyController.Instance.Open(mTarget);
            Debug.Log("[EnterLobby] Lobby opened through UIKit.");
        }
    }
}
