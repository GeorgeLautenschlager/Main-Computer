using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRage;
using VRageMath;

namespace IngameScript
{
    partial class BlockUtils : MyGridProgram
    {
        static readonly MyDefinitionId Hydrogen = MyDefinitionId.Parse("MyObjectBuilder_GasProperties/Hydrogen");
        static readonly MyDefinitionId Oxygen = MyDefinitionId.Parse("MyObjectBuilder_GasProperties/Oxygen");
          
        public static enum GasType
        {
          None,
          Hydrogen,
          Oxygen
        }

        public static GasType GetContentType(IMyGasTank tank)
        {
          MyResourceSinkComponent sinkComponent;
          if (tank.Components.TryGet(out sinkComponent))
          {
            var resources = sinkComponent.AcceptedResources;
            if (resources.Contains(Hydrogen)) 
              return GasType.Hydrogen;

            if (resources.Contains(Oxygen))
              return GasType.Oxygen;
          }
          return GasType.None;
        }

        public static bool IsHydrogenTank(IMyGasTank tank)
        {
            GetContentType(tank) == GasType.Hydrogen;
        }

        public static bool IsOxygenTank(IMyGasTank tank)
        {
            GetContentType(tank) == GasType.Oxygen;
        }

        public static bool TotallyNotEitherAHydrogenTankOrAOxygenTankWTF(IMyGasTank tank)
        {
            GetContentType(tank) == GasType.None;
        }

        public static int CountEnabled(List<IMyTerminalBlock> blocks)
        {
          blocks.Sum(block => Convert.ToInt32(block.Enabled));
        }
    }
}