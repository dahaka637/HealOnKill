using TaleWorlds.MountAndBlade;

namespace HealOnKill
{
    public class HealOnKillSubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            // Nada para carregar — MCM gerencia salvamento/carregamento automaticamente.
        }

        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);

            // Obtém as configurações ativas do MCM
            var settings = HealOnKillSettings.Instance;

            // Injeta o comportamento na missão
            mission.AddMissionBehavior(new HealOnKillBehavior(settings));
        }
    }
}
