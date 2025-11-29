using System;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace HealOnKill
{
    public class HealOnKillBehavior : MissionLogic
    {
        private readonly HealOnKillConfig _config;
        private Team _playerTeam;

        // Defaults de cor (ARGB)
        private const uint DefaultPlayerLogColor = 0xFF00FF00; // green
        private const uint DefaultTroopLogColor = 0xFF00B7FF;  // cyan/blue

        public HealOnKillBehavior(HealOnKillConfig config)
        {
            _config = config ?? new HealOnKillConfig();
        }

        public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            _playerTeam = Mission?.MainAgent?.Team;
        }

        // Roubo de vida baseado no dano
        public override void OnScoreHit(
            Agent affectedAgent, Agent affectorAgent,
            WeaponComponentData attackerWeapon, bool isBlocked, bool isSiegeEngineHit,
            in Blow blow, in AttackCollisionData collisionData,
            float damagedHp, float hitDistance, float shotDifficulty)
        {
            if (!_config.EnableMod)
                return;

            if (_config.HealOnlyOnKill)
                return;

            ProcessHit(affectorAgent, affectedAgent, blow, in collisionData);
        }

        // Cura extra ao matar
        public override void OnAgentRemoved(Agent victim, Agent attacker, AgentState state, KillingBlow killingBlow)
        {
            if (!_config.EnableMod)
                return;

            if (attacker == null || victim == null)
                return;

            if (!attacker.IsHuman)
                return;

            if (state != AgentState.Killed)
                return;

            if (!attacker.IsEnemyOf(victim))
                return;

            if (attacker.Health <= 0f || attacker.HealthLimit <= 0f)
                return;

            bool isPlayer = attacker.IsMainAgent || attacker.IsPlayerControlled;
            bool isFriendly = IsFriendly(attacker);
            bool allowNpcHeal = _config.AllowNpcHeal && !isPlayer && isFriendly;

            if (!(isPlayer || allowNpcHeal))
                return;

            float killPct = Clamp01(_config.KillHealPercent);
            float heal = MathF.Max(1f, attacker.HealthLimit * killPct);

            if (isPlayer)
                heal += MathF.Max(0f, _config.PlayerExtraHeal);
            else
                heal += MathF.Max(0f, _config.TroopExtraHeal);

            ApplyHeal(attacker, heal, HealReason.Kill, inflictedDamage: 0);
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
            bool allowNpcHeal = _config.AllowNpcHeal && !isPlayer && isFriendly;

            if (!(isPlayer || allowNpcHeal))
                return;

            float leechPct = Clamp01(_config.LifeLeechPercent);
            float heal = blow.InflictedDamage * leechPct;

            if (isPlayer)
                heal += MathF.Max(0f, _config.PlayerExtraHeal);
            else
                heal += MathF.Max(0f, _config.TroopExtraHeal);

            ApplyHeal(attacker, heal, HealReason.Leech, inflictedDamage: blow.InflictedDamage);
        }

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

        private void ApplyHeal(Agent agent, float amount, HealReason reason, int inflictedDamage)
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
            uint colorUint = isPlayer
                ? _config.GetPlayerLogColorOr(DefaultPlayerLogColor)
                : _config.GetTroopLogColorOr(DefaultTroopLogColor);

            string tag = reason == HealReason.Kill ? "Kill Heal" : "Life Steal";

            // Exemplo:
            // Player +3.2 HP (Life Steal, dmg 21) -> 45.0/55.0
            // Legionary +2.0 HP (Kill Heal) -> 38.0/52.0
            string msg;

            if (reason == HealReason.Leech)
            {
                msg = string.Format(
                    "{0} +{1:0.#} HP ({2}, dmg {3}) -> {4:0.#}/{5:0.#}",
                    SafeName(agent),
                    gained,
                    tag,
                    inflictedDamage,
                    after,
                    agent.HealthLimit
                );
            }
            else
            {
                msg = string.Format(
                    "{0} +{1:0.#} HP ({2}) -> {3:0.#}/{4:0.#}",
                    SafeName(agent),
                    gained,
                    tag,
                    after,
                    agent.HealthLimit
                );
            }

            InformationManager.DisplayMessage(new InformationMessage(msg, Color.FromUint(colorUint)));
        }

        private bool ShouldLog(Agent agent)
        {
            // Compatibilidade:
            // - Se o usuário só tiver ShowFeedback=true (config antiga), loga tudo.
            // - Se tiver LogPlayer/LogTroops (config nova), usa os toggles separados.
            bool legacy = _config.ShowFeedback && !_config.LogPlayer && !_config.LogTroops;
            if (legacy)
                return true;

            bool isPlayer = agent.IsMainAgent || agent.IsPlayerControlled;
            return isPlayer ? _config.LogPlayer : _config.LogTroops;
        }

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

        private enum HealReason
        {
            Leech,
            Kill
        }
    }
}
