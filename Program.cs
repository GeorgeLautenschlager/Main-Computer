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
    partial class Program : MyGridProgram
    {
        List<IMyTextPanel> StatusMonitors = new List<IMyTextPanel>();

        List<IMyBatteryBlock> Batteries = new List<IMyBatteryBlock>();
        List<IMyGasTank> FuelTanks = new List<IMyGasTank>();
        List<IMyGasTank> OxygenTanks = new List<IMyGasTank>();
        
        List<IMyRadioAntenna> Antennas = new List<IMyRadioAntenna>();
        List<IMyGravityGenerator> GravityGenerators = new List<IMyGravityGenerator>();
        List<IMyGasGenerator> GasGenerators = new List<IMyGasGenerator>();
        List<IMyAssembler> Assemblers = new List<IMyAssembler>();
        List<IMyRefinery> Refineries = new List<IMyRefinery>();
        List<IMyOreDetector> OreDetectors = new List<IMyOreDetector>();

        //Antennas, Gravity Generators, Ore Detectors, Gas Generators, Assemblers, Refineries
        List<IMyTerminalBlock>[] PowerPriorities = new List<IMyTerminalBlock>[] { Antennas, GravityGenerators, OreDetectors, GasGenerators, Assemblers, Refineries };

        float currentBatteryInput, currentBatteryOutput, currentBatteryStored, maxBatteryStored;
        int activeGravityGenerators, activeRefineries, activeAssemblers, activeGasGenerators;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;

            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(StatusMonitors, lcd => MyIni.HasSection(lcd.CustomData, "status_monitor") && lcd.IsSameConstructAs(Me));
            GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(Batteries, battery => battery.IsSameConstructAs(Me));
            GridTerminalSystem.GetBlocksOfType<IMyGasTank>(FuelTanks, tank => BlockUtils.IsHydrogenTank(tank) && tank.IsSameConstructAs(Me));
            GridTerminalSystem.GetBlocksOfType<IMyGasTank>(OxygenTanks, tank => BlockUtils.IsOxygenTank(tank) && tank.IsSameConstructAs(Me));

            GridTerminalSystem.GetBlocksOfType<IMyGravityGenerator>(GravityGenerators, gravity => gravity.IsSameConstructAs(Me));
            GridTerminalSystem.GetBlocksOfType<IMyRefinery>(Refineries, refinery => refinery.IsSameConstructAs(Me));
            GridTerminalSystem.GetBlocksOfType<IMyAssembler>(Assemblers, assembler => assembler.IsSameConstructAs(Me));
            GridTerminalSystem.GetBlocksOfType<IMyGasGenerator>(GasGenerators, gasGenerator => gasGenerator.IsSameConstructAs(Me));
            GridTerminalSystem.GetBlocksOfType<IMyRadioAntenna>(Antennas, antenna => antenna.IsSameConstructAs(Me));
            GridTerminalSystem.GetBlocksOfType<IMyOreDetector>(OreDetectors, detector => detector.IsSameConstructAs(Me));
            
        }

        public void Main(string argument, UpdateType updateSource)
        {   
            // TODO: These need to operate together so as not to step on each other's toes
            OperateDoors();
            OperateAirlocks();

            WriteToStatusMonitors(StatusReport());
            ManagePower();
            // ManageIce();
            // ManageInventory();
        }

        public void ManagePower()
        {
            if(currentBatteryStored/maxBatteryStored < 0.2 && (currentBatteryOutput/currentBatteryInput) > 1.1)
            {
                StopNextConsumer();
            }
            else if((currentBatteryStored/maxBatteryStored > 0.6 && (currentBatteryOutput/currentBatteryInput) < 0.9))
            {
                StartNextConsumer();
            }
        }

        public void StopNextConsumer()
        {
            // Find the lowest priority list with blocks enabled
            List<IMyTerminalBlock> blocks = PowerPriorities.Reverse.Find(blockList => BlockUtils.CountEnabled(List<IMyTerminalBlock> blockList) > 0);
            // Turn off the first enabled block
            blocks.Find(block => block.Enabled).Enabled = false;
        }

        public void StarNextConsumer()
        {
            //Find the highest priority list with blocks disabled
            List<IMyTerminalBlock> blocks = PowerPriorities.Find(blockList => BlockUtils.CountEnabled(List<IMyTerminalBlock> blockList) < blockList.Count);
            // Turn on the first disabled block
            blocks.Find(block => !block.Enabled).Enabled = true;
        }

        public void WriteToStatusMonitors(StringBuilder input)
        {
            foreach (IMyTextPanel lcd in StatusMonitors)
            {
                lcd.WriteText(input);
            }
        }

        public StringBuilder StatusReport()
        {
            StringBuilder statusReport = new StringBuilder("STATUS REPORT");

            statusReport.Append(MainPowerReport());
            statusReport.Append(FuelReport());
            statusReport.Append(OxygenReport());
            statusReport.Append(EquipmentReport());
            statusReport.Append(DefenseSystemsReport());

            return statusReport;
        }
        public StringBuilder MainPowerReport()
        {
            currentBatteryInput = currentBatteryOutput = currentBatteryStored = maxBatteryStored = 0;

            foreach (IMyBatteryBlock battery in Batteries)
            {
                currentBatteryInput += battery.CurrentInput;
                currentBatteryOutput += battery.CurrentOutput;
                currentBatteryStored += battery.CurrentStoredPower;
                maxBatteryStored += battery.MaxStoredPower;
            }

            StringBuilder report = new StringBuilder("\n\nMain Power:");
            report.Append("\n====================");
            report.Append($"\nStored:\t{FormatNumber((currentStored / maxStored) * 100.0)}%");
            report.Append($"\nLoad:\t{FormatNumber((currentOutput / currentInput) * 100.0)}%");

            return report;
        }
        public StringBuilder FuelReport()
        {
            double currentStorage = 0;

            foreach(IMyGasTank tank in FuelTanks)
            {
                currentStorage += tank.FilledRatio;
            }

            StringBuilder report = new StringBuilder("\n\nFuel:");
            report.Append("\n====================");
            report.Append($"\n{FormatNumber((currentStorage/ FuelTanks.Count) * 100)}%");

            return report;
        }
        public StringBuilder OxygenReport()
        {
            double currentStorage = 0;

            foreach(IMyGasTank tank in OxygenTanks)
            {
                currentStorage += tank.FilledRatio;
            }

            StringBuilder report = new StringBuilder("\n\nFuel:");
            report.Append("\n====================");
            report.Append($"\n{FormatNumber((currentStorage/ OxygenTanks.Count) * 100)}%");

            return report;
        }
        public StringBuilder EquipmentReport()
        {
            StringBuilder report = new StringBuilder("\n\nEquipment:");
            report.Append("\n====================");
            
            //Active Gravity Generators
            activeGravityGenerators = GravityGenerators.Sum(grav => Convert.ToInt32(grav.Enabled));
            report.Append($"\nActive Gravity Generators:\t{activeGravityGenerators}/{GravityGenerators.Count}");

            //Active Refineries
            activeRefineries = Refineries.Sum(refinery => Convert.ToInt32(refinery.Enabled));
            report.Append($"\nActive Refineries:\t{activeRefineries}/{Refineries.Count}");

            //Active Assemblers
            activeAssemblers = Assemblers.Sum(assembler => Convert.ToInt32(assembler.Enabled));
            report.Append($"\nActive Assemblers:\t{activeAssemblers}/{Assemblers.Count}");

            //Active H2/O2 Generators
            activeGasGenerators = GasGenerators.Sum(gasGenerator => Convert.ToInt32(gasGenerator.Enabled));
            report.Append($"\nActive H2/O2 Generators:\t{activeGasGenerators}/{GasGenerators.Count}");

            //Active Antennas
            activeAntennas = Antennas.Sum(antenna => Convert.ToInt32(antenna.Enabled));
            report.Append($"\nActive Antennas:\t{activeAntennas}/{Antennas.Count}");

            //Active Ore Detectors
            activeOreDetectors = OreDetectors.Sum(oreDetector => Convert.ToInt32(oreDetector.Enabled));
            report.Append($"\nActive Ore Detectors:\t{activeOreDetectors}/{OreDetectors.Count}");

            return report;
        }
        public StringBuilder DefenseSystemsReport()
        {
            StringBuilder report = new StringBuilder("\n\nDefense Systems Report");
            report.Append("\n====================");
            report.Append("\nCome back when you've added some guns.");

            return report;
        }

        public string FormatNumber(double number)
        {
            return number.ToString("0.00");
        }
    }
}
