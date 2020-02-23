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
        List<IMyTextPanel> LCDs = new List<IMyTextPanel>();
        List<IMyBatteryBlock> Batteries = new List<IMyBatteryBlock>();
        List<IMyGasTank> FuelTanks = new List<IMyGasTank>();
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;

            GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(LCDs, lcd => MyIni.HasSection(lcd.CustomData, "status_monitor"));

            GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(Batteries, battery => battery.IsSameConstructAs(Me));
            //TODO: Use this instead, better yet package it as a utility https://forum.keenswh.com/threads/hydrogen-tanks-vs-oxygen-tanks.7403348/#post-1287110133
            GridTerminalSystem.GetBlocksOfType<IMyGasTank>(FuelTanks, tank => tank.IsSameConstructAs(Me) && tank.DetailedInfo.ToString().Split('\n')[0].Split(':')[1].Trim() == "Hydrogen Tank");
        }

        public void Save()
        {

        }

        public void Main(string argument, UpdateType updateSource)
        {
            StringBuilder statusReport = new StringBuilder("STATUS REPORT");

            statusReport.Append(MainPowerReport());
            statusReport.Append(FuelReport());

            foreach (IMyTextPanel lcd in LCDs)
            {
                lcd.WriteText(statusReport);
            }
        }

        public StringBuilder MainPowerReport()
        {
            float currentInput = 0;
            float currentOutput = 0;
            float currentStored = 0;
            float maxStored = 0;

            foreach (IMyBatteryBlock battery in Batteries)
            {
                currentInput += battery.CurrentInput;
                currentOutput += battery.CurrentOutput;
                currentStored += battery.CurrentStoredPower;
                maxStored += battery.MaxStoredPower;
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

        public string FormatNumber(double number)
        {
            return number.ToString("0.00");
        }
    }
}
