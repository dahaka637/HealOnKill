using System;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace HealOnKill
{
    public class HealOnKillBehavior : MissionLogic
    {
        private readonly HealOnKillSettings _config;
        private Team _playerTeam;

        private const uint DefaultPlayerLogColor = 0xFF00FF00; // green
        private const uint DefaultTroopLogColor = 0xFF00B7FF;  // cyan/blue

        public HealOnKillBehavior(HealOnKillSettings config)
        {
            _config = config ?? HealOnKillSettings.Instance;
        }

        public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            _playerTeam = Mission?.MainAgent?.Team;
        }

        // ============================
        // LIFE LEECH POR DANO
        // ============================
        public override void OnScoreHit(
            Agent affectedAgent, Agent affectorAgent,
            WeaponComponentData attackerWeapon, bool isBlocked, bool isSiegeEngineHit,
            in Blow blow, in AttackCollisionData collisionData,
            float damagedHp, float hitDistance, float shotDifficulty)
        {
            if (!_config.EnableMod)
                return;

            ProcessHit(affectorAgent, affectedAgent, blow, in collisionData);
        }

        private void ProcessHit(Agent attacker, Agent victim, in Blow blow, in AttackCollisionData collisionData)
        {
            if (attacker == null || victim == null)
                return;

            if (!attacker.IsHuman || victim.IsMount)
                return;

            if (!attacker.IsEnemyOf(victim))
                return;

            if (collisionData.AttackBlockedWithShield)
                return;

            if (blow.InflictedDamage <= 0)
                return;

            if (attacker.Health <= 0f || attacker.HealthLimit <= 0f)
                return;

            bool isPlayer = attacker.IsMainAgent || attacker.IsPlayerControlled;
            bool isFriendly = IsFriendly(attacker);

            bool allowTroopHeal = _config.AllowNpcHeal && !isPlayer && isFriendly;

            if (!(isPlayer || allowTroopHeal))
                return;

            float pct = _config.GetLifeLeechFor(isPlayer);
            pct = Clamp01(pct);

            float heal = blow.InflictedDamage * pct;

            ApplyHeal(attacker, heal, inflictedDamage: blow.InflictedDamage);
        }

        // ============================
        // CHECAR TIME ALIADO
        // ============================
        private bool IsFriendly(Agent agent)
        {
            if (agent == null || agent.Team == null)
                return false;

            if (_playerTeam == null && Mission?.MainAgent != null)
                _playerTeam = Mission.MainAgent.Team;

            if (_playerTeam != null && agent.Team == _playerTeam)
                return true;

            return Mission != null &&
                   Mission.MainAgent != null &&
                   (agent.Team.IsPlayerTeam || agent.Team.IsPlayerAlly);
        }

        // ============================
        // APLICAR CURA
        // ============================
        private void ApplyHeal(Agent agent, float amount, int inflictedDamage)
        {
            if (agent == null || amount <= 0f || agent.Health <= 0f)
                return;

            float before = agent.Health;
            float after = MathF.Min(agent.HealthLimit, before + amount);

            if (after <= before)
                return;

            agent.Health = after;

            if (!ShouldLog(agent))
                return;

            float gained = after - before;
            if (gained <= 0.001f)
                return;

            bool isPlayer = agent.IsMainAgent || agent.IsPlayerControlled;

            uint color = isPlayer ? DefaultPlayerLogColor : DefaultTroopLogColor;

            string msg = string.Format(
                "{0} +{1:0.#} HP (Life Steal, dmg {2}) -> {3:0.#}/{4:0.#}",
                SafeName(agent),
                gained,
                inflictedDamage,
                after,
                agent.HealthLimit
            );

            InformationManager.DisplayMessage(
                new InformationMessage(msg, Color.FromUint(color))
            );
        }

        // ============================
        // LOGGING CONFIG
        // ============================
        private bool ShouldLog(Agent agent)
        {
            bool isPlayer = agent.IsMainAgent || agent.IsPlayerControlled;
            return isPlayer ? _config.LogPlayer : _config.LogTroops;
        }

        // ============================
        // UTILS
        // ============================
        private static float Clamp01(float v)
        {
            if (v < 0f) return 0f;
            if (v > 1f) return 1f;
            return v;
        }

        private static string SafeName(Agent agent)
        {
            try
            {
                string n = agent?.Name?.ToString();
                return string.IsNullOrWhiteSpace(n) ? "Unit" : n;
            }
            catch
            {
                return "Unit";
            }
        }
    }
}
