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

public class Bot4 : Bot
{   
    // Variabel target terbaik dan pengaturan radar
    private EnemyInfo bestTarget;
    private int turn_radar = 1;
    int turnDirection = 1;

    private EnemyInfo[] tank_list;
    private EnemyInfo lockedTarget;
    private EnemyInfo previousLockedTarget;

    static void Main(string[] args){
        new Bot4().Start();
    }

    Bot4() : base(BotInfo.FromFile("Bot4.json")) {}

    public override void Run()
    {
        // Konfigurasi independence untuk radar, gun, dan body tank
        AdjustRadarForBodyTurn = true;
        AdjustGunForBodyTurn = true;
        AdjustRadarForGunTurn = true;
        BodyColor = Color.Purple;

        while (IsRunning){
            // Default behavior scanner -> putar 360 derajat
            TurnRadarRight(30);
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

        // Membuat objek musuh dan menghitung skor potensial musuh tersebut
        EnemyInfo enemy = new EnemyInfo(targetX, targetY, targetEnergy, distance, direction);
        ComputeScorePotential(enemy);

        // Memperbarui target terbaik dengan skor potensial terbesar
        if (bestTarget == null || enemy.ScorePotential > bestTarget.ScorePotential) {
            bestTarget = enemy;
        } 

        // Memprioritaskan poin dari survival jika energi rendah dan lebih rendah dari musuh
        if (bestTarget != null) {
            if (Energy < 10) {
                if (Energy < bestTarget.HP) {
                    evadeSurvival();
                }
            } else {
                AttackTarget(bestTarget);
            }
        }
        turn_radar *= -1;
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

        double risk;
        if (Energy < 20) {
            risk = -50; // menjauhi pertarungan jika energi rendah
        } else if (Energy > 40) {
            risk = 10;  // lebih agresif ketika energi cukup
        } else {
            risk = 0;
        }

        enemy.ScorePotential = bulletDamage + ramDamage + bulletKillBonus + ramKillBonus + risk;
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
    public EnemyInfo sortPriorityTankPoint(ref EnemyInfo[] list)
    {

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
        if (Direction > 180 && Direction < 360){
            turnDirection = 1;
        }
        else if ( Direction < 180 && Direction > 0){
            turnDirection = -1;
        }

        TurnLeft(bearing);
    }

    // Fungsi untuk menghindar dan bertahan hidup
    private void evadeSurvival() {
        int direction = Random.Shared.Next(2) * 2 - 1;
        SetTurnLeft(90 * direction + Random.Shared.Next(-20, 20));
        SetForward(300);
    }
}
