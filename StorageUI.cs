using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame;
using System.Collections.Generic;
using VRageMath;

namespace se_scripting
{
    public class StorageUI : MyGridProgram
    {
        // user configuration
        public const string MAIN_LCD = "LCD"; // block name of main lcd screen
        public const int BAR_LENGTH = 30; // maximum amount of characters for a bar
                                          // =================================================


        // implementation
        readonly List<string> errorList;

        private IMyTextPanel panel;

        private readonly Dictionary<string, DisplayFunc> displayFuncs;


        private string DisplayItemBar(DisplayElement e, Section sec)
        {
            return "";
        }


        private string DisplayBar(DisplayElement e, Section sec)
        {
            string bar = FormatBar(e.TotalVolume / e.MaxVolume);
            return "Used";
        }


        public StorageUI()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            errorList = new List<string>();

            panel = GridTerminalSystem.GetBlockWithName(MAIN_LCD) as IMyTextPanel;
            if (panel == null) errorList.Add($"Could not find main LCD with name: " + MAIN_LCD + ".");

            displayFuncs = new Dictionary<string, DisplayFunc>();
            displayFuncs.Add("ItemBar", DisplayItemBar);
        }


        public void Main(string argument, UpdateType updateSource)
        {
            errorList.Clear();
            DisplayElement e = new DisplayElement();
            e.TotalVolume = 0f;
            e.MaxVolume = 0f;
            e.ItemAmounts = new Dictionary<string, float>();
            e.Inventories = new List<IMyInventory>();

            List<IMyCargoContainer> cargoContainers = new List<IMyCargoContainer>();
            GridTerminalSystem.GetBlocksOfType<IMyCargoContainer>(cargoContainers, i => i.IsSameConstructAs(Me));
            foreach (IMyCargoContainer container in cargoContainers)
            {
                IMyInventory inv = container.GetInventory();
                e.Inventories.Add(inv);

                e.TotalVolume += (float)inv.MaxVolume;
                List<MyInventoryItem> items = new List<MyInventoryItem>();
                inv.GetItems(items);
                foreach (MyInventoryItem item in items)
                {
                    string type = item.Type.TypeId.ToString().Replace("MyObjectBuilder_", "");
                    string subType = item.Type.SubtypeId;
                    string name = subType + " " + type;
                    if (!e.ItemAmounts.ContainsKey(name)) e.ItemAmounts[name] = 0f;
                    e.ItemAmounts[name] += (float)item.Amount;
                }
            }

            // parsing custom data
            string customData = panel.CustomData;
            string[] lines = customData.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (!line.StartsWith("[") || !line.EndsWith("]"))
                {
                    errorList.Add("Invalid section on line " + (i + 1) + ".");
                    continue;
                }

                line = line.Substring(1, line.Length - 2);
                string[] parts = line.Split(new[] { ' ' }, 2);
                string sectionName = parts[0];
                string paramString = parts.Length > 0 ? parts[1] : "";
                Dictionary<string, string> parameters = ParseSectionParameters(paramString).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                Echo(sectionName);
                Echo(paramString);
            }

            if (errorList.Count < 1) Echo("No errors detected.");
            else Echo(string.Join("\n", errorList));
        }
        // =================================================


        // utility
        private struct DisplayElement
        {
            public float TotalVolume; // total volume of all used space
            public float MaxVolume; // total max volume of all inventories
            public Dictionary<string, float> ItemAmounts; // amount of items (e.g. { "Iron Ingot", 22000 kg })
            public List<IMyInventory> Inventories; // inventories of all cargo containers
        }


        private struct Section
        {
            public string Name; // name of section as defined in displayFuncs dictionary (e.g. Itembar)
            public List<string> Params; // params of specific section (e.g for Bar prefix="total used")
        }


        private delegate string DisplayFunc(DisplayElement e, Section sec);


        private string FormatBar(float percent)
        {
            int filled = (int)(percent * BAR_LENGTH);
            int empty = BAR_LENGTH - filled;
            return "[" + new string('|', filled) + new string('.', empty) + "]";
        }


        private List<string> ParseSections(string customData)
        {
            return new List<string>();
        }


        private List<KeyValuePair<string, string>> ParseSectionParameters(string _params)
        {
            return new List<KeyValuePair<string, string>>();
        }
        // =================================================
    }
}