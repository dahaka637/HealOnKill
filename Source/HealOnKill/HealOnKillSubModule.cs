using System.IO;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace HealOnKill
{
    public class HealOnKillSubModule : MBSubModuleBase
    {
        private HealOnKillConfig _config;

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

            string path = Path.Combine(
                BasePath.Name,
                "Modules",
                "HealOnKill",
                "ModuleData",
                "HealOnKillConfig.xml");

            _config = HealOnKillConfig.Load(path);
        }

        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);
            mission.AddMissionBehavior(new HealOnKillBehavior(_config));
        }
    }
}
