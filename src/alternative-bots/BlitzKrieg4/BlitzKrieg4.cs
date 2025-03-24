using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

// Kelas untuk informasi musuh dan skor potensial
public class EnemyInfo
{
    public static int NEFF = 0;
    public double X, Y, HP, Distance, ScorePotential, Direction;
    public EnemyInfo(double x, double y, double hp, double distance, double direction)
    {
        X = x;
        Y = y;
        HP = hp;
        Distance = distance;
        Direction = direction;
        ScorePotential = 0;
    }
}

public class BlitzKrieg4 : Bot
{   
    // Variabel target terbaik dan pengaturan radar
    private EnemyInfo bestTarget; // temporary variable untuk target dengan skor potensial terbesar tiap sweep radar, best target digunakan saat membutuhkan respons lebih cepat sebelum full sweep radar
    private EnemyInfo lockedTarget; // target dengan skor potensial terbesar dalam list

    private EnemyInfo[] tank_list;

    static void Main(string[] args){
        new BlitzKrieg4().Start();
    }

    BlitzKrieg4() : base(BotInfo.FromFile("BlitzKrieg4.json")) {}

    public override void Run()
    {
        // Konfigurasi independence untuk radar, gun, dan body tank
        AdjustRadarForBodyTurn = true;
        AdjustGunForBodyTurn = true;
        AdjustRadarForGunTurn = true;
        BodyColor = Color.Purple;
        
        tank_list = new EnemyInfo[1000]; // Tank list statik untuk menampung objek musuh
        
        while (IsRunning){
            // clear list tank sebelum scan
            ClearList(ref tank_list);
            
            // Scan 360 derajat, lalu lock ke tank dengan prioritas potensial skor terbesar
            TurnRadarRight(360);
            lockedTarget = sortPriorityScorePotential(ref tank_list);
            
            // setelah radar sweep, attack target jika energi cukup, jika tidak maka fokus survival
            if (lockedTarget != null) {
                if (Energy < 10 && Energy < lockedTarget.HP) {
                    evadeSurvival();
                } else {
                    AttackTarget(lockedTarget);
                }
            }
            
            TurnRadarRight(360);
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        // Data target, energy, jarak, arah, dan bearing
        double targetX = e.X;
        double targetY = e.Y;
        double targetEnergy = e.Energy;
        double direction = e.Direction;
        double distance = DistanceTo(targetX, targetY);
        var bearingFromGun = GunBearingTo(targetX, targetY);

        TurnGunLeft(bearingFromGun);

        // Membuat objek musuh dan compute skor potensial musuh tersebut
        EnemyInfo enemy = new EnemyInfo(targetX, targetY, targetEnergy, distance, direction);
        ComputeScorePotential(enemy);
        
        // Selama list masih ada space, tambah tank musuh ke list
        if (EnemyInfo.NEFF < tank_list.Length) {
            tank_list[EnemyInfo.NEFF] = enemy;
            EnemyInfo.NEFF++;
        }
        
        // Update lock target dan best target
        if (lockedTarget != null){
            lockedTarget = enemy;
        }

        if (bestTarget == null || enemy.ScorePotential > bestTarget.ScorePotential) {
            bestTarget = enemy;
        } 

        // Memprioritaskan poin dari survival jika energi rendah dan lebih rendah dari musuh
        if (bestTarget != null && lockedTarget == null) {
            if (Energy < 10) {
                if (Energy < bestTarget.HP) {
                    evadeSurvival();
                }
            } else {
                AttackTarget(bestTarget);
            }
        }
        SetRescan();
    }

    public override void OnHitBot(HitBotEvent e)
    {
        SetBack(100);
        SetTurnRight(45);
        SetForward(150);
    }

    public override void OnHitWall(HitWallEvent e)
    {
        SetBack(50);
        SetTurnRight(90);
        SetForward(100);
    }

    // Menghitung skor potensial berdasarkan berbagai faktor bonus
    private void ComputeScorePotential(EnemyInfo enemy) 
    {
        double bulletDamage = 100 - enemy.HP;
        double ramDamage = 2 * (100 - enemy.HP);

        // Bullet dan ram kill bonus sesuai spesifikasi game
        double bulletKillBonus;
        if (enemy.HP < 10) {
            bulletKillBonus = 0.2 * bulletDamage;
        } else {
            bulletKillBonus = 0;
        }

        double ramKillBonus;
        if (enemy.HP < 10) {
            ramKillBonus = 0.3 * ramDamage;
        } else {
            ramKillBonus = 0;
        }

        enemy.ScorePotential = bulletDamage + ramDamage + bulletKillBonus + ramKillBonus;
    }

    // Fungsi untuk clear list tank hasil scan tiap 360 rotation
    public void ClearList(ref EnemyInfo[] list) 
    {
        for (int i = 0 ; i < EnemyInfo.NEFF ; i++){
            list[i] = null;
        }
        EnemyInfo.NEFF = 0;
    }

    // Fungsi greedy sorting by points
    public EnemyInfo sortPriorityScorePotential(ref EnemyInfo[] list)
    {
        if (EnemyInfo.NEFF <= 0)
        {
            return null;
        }
        
        // Cari tank dengan skor potensial terbesar dalam list
        EnemyInfo highestScoreTank = list[0];
        for (int i = 1; i < EnemyInfo.NEFF; i++)
        {
            if (list[i].ScorePotential > highestScoreTank.ScorePotential)
            {
                highestScoreTank = list[i];
            }
        }
        
        return highestScoreTank;
    }

    // Fungsi untuk menyerang target yang dipilih
    private void AttackTarget(EnemyInfo target) {
        double distance = target.Distance;

        // Normal attack behavior sesuai HP musuh
        if (target.HP > 10) {
            TurnToFaceTarget(target.X, target.Y, target.Direction);
            SetForward(80);
        } else {
            TurnToFaceTarget(target.X, target.Y, target.Direction);
            SetForward(300);
        }

        TurnToFaceTarget(target.X, target.Y, target.Direction);

        // Hanya menembak jika energi cukup, jika tidak maka fokus survival
        if (Energy > 20) {
            if (distance < 50 && target.HP < 20) {
                SetFire(3);
            } else if (distance < 100 && target.HP > 20) {
                SetFire(2.8);
            } else if (distance < 100 && target.HP < 10) {
                SetFire(2.5);
            } else {
                SetFire(2);
            }
        }

        bestTarget = null;
    }

    // Fungsi untuk menghitung angle target dan menghadap ke arahnya
    private void TurnToFaceTarget(double targetX, double targetY, double Direction)
    {
        var bearing = BearingTo(targetX, targetY);
        TurnLeft(bearing);
    }

    // Fungsi untuk menghindar dan bertahan hidup
    private void evadeSurvival() {
        int direction = Random.Shared.Next(2) * 2 - 1;
        SetTurnLeft(90 * direction + Random.Shared.Next(-20, 20));
        SetForward(300);
    }
}
