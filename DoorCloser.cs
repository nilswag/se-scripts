using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace se_scripting
{
    public class DoorCloser : MyGridProgram
    {
        // user configuration
        private const float AUTO_CLOSE_TIME = 5f; // amount of seconds a door is allowed to be open
                                                   // =================================================


        // implementation
        private const float TICK_DURATION = 1f / 6f;
        private Dictionary<long, float> doorTimer;
        private List<IMyDoor> doors;

        public DoorCloser()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            doorTimer = new Dictionary<long, float>();
            doors = new List<IMyDoor>();
        }


        public void Main(string argument, UpdateType updateSource)
        {
            doors.Clear();
            GridTerminalSystem.GetBlocksOfType(doors, i => i.IsSameConstructAs(Me));
            foreach (IMyDoor door in doors)
            {
                if (!door.IsFunctional || !door.IsWorking) continue;
                if (door is IMyAirtightHangarDoor) continue;
                if (door.OpenRatio <= 0) continue;

                if (!doorTimer.ContainsKey(door.EntityId))
                    doorTimer[door.EntityId] = 0f;

                doorTimer[door.EntityId] += TICK_DURATION;

                if (doorTimer[door.EntityId] > AUTO_CLOSE_TIME)
                {
                    door.CloseDoor();
                    doorTimer.Remove(door.EntityId);
                }
            }
        }
        // =================================================

    }
}