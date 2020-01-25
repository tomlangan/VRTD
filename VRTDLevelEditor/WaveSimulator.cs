using System;
using VRTD.Gameplay;

namespace VRTD.LevelEditor
{
    public class WaveSimulatorDamageStats
    {
        public float DamageDealt = 0.0F;
        public float DamagePerEnemy = 0.0F;
        public int EnemiesKilled = 0;
    }

    public class WaveSimulator
    {
        EnemyWave Wave;
        LevelDescription LevelDesc;
        WaveSolution Solution;
        WaveInstance WI;
        WaveSimulatorDamageStats DamageStats = null;
        bool InvincibleEnemies = true;


        public WaveSimulator(LevelDescription desc)
        {
            LevelDesc = desc;
        }


        void SimulatorDamageCallback(EnemyInstance enemy, float damage)
        {
            if (InvincibleEnemies)
            {
                DamageStats.DamageDealt += damage;
            }
            else
            {
                if ((enemy.HealthRemaining > 0)
                    &&
                    (enemy.HealthRemaining <= damage))
                {
                    DamageStats.DamageDealt = enemy.Desc.HitPoints;
                    enemy.HealthRemaining = 0.0F;

                    // Enemy is dead
                    DamageStats.EnemiesKilled++;
                }
                else if (damage > 0.0F)
                {
                    DamageStats.DamageDealt += damage;
                    enemy.HealthRemaining -= damage;
                }
            }
        }

        private void ResetLevelDescState(LevelDescription desc)
        {
            for (int i=0; i<desc.Road.Count; i++)
            {
                desc.Road[i].EnemiesOccupying.Clear();
            }
        }
        
        public WaveSimulatorDamageStats SimulateDamageToEnemies(EnemyDescription enemy, WaveSolution solution, int enemyCount, bool invincible)
        {
            DamageStats = new WaveSimulatorDamageStats();
            InvincibleEnemies = invincible;

            float waveTime = 0.0F;
            float timeDelta = 1.0F / 72.0F;

            EnemyWave waveDesc = new EnemyWave();
            waveDesc.Count = enemyCount;
            waveDesc.Enemy = enemy.Name;
            waveDesc.DifficultyMultiplier = 1.0F;

            ResetLevelDescState(LevelDesc);
            TurretManager turrets = new TurretManager(LevelDesc);
            ProjectileManager projectiles = new ProjectileManager(LevelDesc, LevelManager.LookupProjectile);
            WaveInstance wave = new WaveInstance(LevelDesc, waveDesc, enemy, 0.0F, SimulatorDamageCallback);

            for (int i = 0; i < solution.Turrets.Count; i++)
            {
                turrets.AddTurret(LevelManager.LookupTurret(solution.Turrets[i].Name), solution.Turrets[i].pos, projectiles);
            }

            Debug.Log("----------------------------------------");
            Debug.Log("   Wave of " + enemyCount + " " + enemy.Name + "s");
            Debug.Log("----------------------------------------");

            do
            {
                wave.Advance(waveTime);
                projectiles.AdvanceAll(waveTime);
                turrets.Fire(waveTime);

                waveTime += timeDelta;
            } while (!wave.IsCompleted);

            DamageStats.DamagePerEnemy = DamageStats.DamageDealt / enemyCount;

            return DamageStats;
        }

    }
}
