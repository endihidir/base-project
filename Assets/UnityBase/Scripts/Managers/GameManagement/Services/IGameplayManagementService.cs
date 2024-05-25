﻿using UnityBase.Manager;

namespace UnityBase.Service
{
    public interface IGameplayManagementService
    {
        public GameState CurrentGameState { get; }
        public void ChangeGameState(GameState nextGameState, float transitionDuration, float startDelay = 0f);
    }
}