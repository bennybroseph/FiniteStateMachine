using System;
using BennyBroseph;

namespace ConsoleApplication
{
    class Program
    {
        public enum PlayerStates { Init, Idle, Walk, Run };
        static void Main(string[] args)
        {
            FiniteStateMachine<PlayerStates> PlayerFSM = new FiniteStateMachine<PlayerStates>();

            PlayerFSM.TestMachine();         
        }
    }
}
