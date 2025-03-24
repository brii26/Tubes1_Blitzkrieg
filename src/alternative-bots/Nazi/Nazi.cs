using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Nazi : Bot
{
    private EnemyInfo[] tank_list; //array of tank object
    private bool allScanned; // boolean untuk mengecek apakah sudah selesai scan 360 derajat
    private int scanned_count = 0; //menghitung jumlah tank yang sudah di scan
    private EnemyInfo lockedTarget;

    static void Main(string[] args)
    {
        new Nazi().Start();
    }

    Nazi() : base(BotInfo.FromFile("Nazi.json")) { }

    public override void Run()
    {
        allScanned = false;
        EnemyInfo.NEFF = 0;
        BodyColor = Color.Black;
        TurretColor = Color.Green;
        RadarColor = Color.Green;
        BulletColor = Color.Black;
        ScanColor = Color.Green;
        AdjustGunForBodyTurn = true;
        lockedTarget = null;
        tank_list = new EnemyInfo[1000];

        while (IsRunning)
        {
            TurnRadarRight(360);
            SetRescan();
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        if(allScanned == false) //Membuat object dan push ke list of tank jika blom terscan semua
        {
            EnemyInfo newTank = new EnemyInfo(e.X, e.Y, DistanceTo(e.X, e.Y), DirectionTo(e.X, e.Y)); 
            tank_list[EnemyInfo.NEFF] = newTank;
            EnemyInfo.NEFF++;
            scanned_count++;
            if(scanned_count > EnemyCount-1) // Tank akan scan sebanyak EnemyCount-1, untuk memastikan setidaknya n-1 tank sudah di scan sebelum menentukan tank yang ingin di lock
            {
                scanned_count = 0; // variable menghitung banyak tank yang sudah berhasil di scan
                allScanned = true;
            }
            return;
        }

        lockedTarget = sortCompositeValue(ref tank_list); // sort berdasarkan delta angle dan distance untuk shooting efficiency
        var distance = DistanceTo(lockedTarget.X, lockedTarget.Y); // mengasign variable terhadap distance saat ini ke lockedTarget.
        double gunBearing = GunBearingTo(lockedTarget.X, lockedTarget.Y); 

        SetTurnLeft(BearingTo(lockedTarget.X, lockedTarget.Y)); // tank akan berputar ke kanan / ke kiri berdasarkan hasil greedy by efficiency shooting, dimana weightvalue gabungan dari distance dan delta rotation menjadi variablenya
        SetTurnGunLeft(gunBearing); // gun akan berputar ke kanan / ke kiri berdasarkan hasil greedy , value bearing range = [-180,180], akan otomatis terputar kekanan jika memang tank ada di kanan untuk value -
        
        SmartShoot(Energy, distance);
        ClearList(ref tank_list); // clear list untuk scan ulang, untuk setiap kali tank selesai menembak
        allScanned = false; // reset state allscanned untuk scan ulang sebelum menentukan locked tank untuk ditembak

        // Movement Mekanik
        if(distance <= 350)
        {
            if(distance > 300)
            {
                SetForward(10);
            }
            else
            {
                SetBack(25);
            }
        }
        else
        {
            SetForward(distance / 3.5);
        }
    }

    public override void OnHitBot(HitBotEvent e)
    {
        SetTurnLeft(500);
        SetBack(500);
    }

    public override void OnHitWall(HitWallEvent e)
    {
        SetForward(500);
    }

    public void EnergyAdjustedShoot(double health) // fungsi untuk menghemat shooting setelah health berada dibawah 3 pada smart shoot
    {
        if (health > 1.0)
        {
            SetFire(0.8);
        }
        else
        {
            SetFire(0.2);
        }
    }

    public void DistanceShoot(double distance)
    {
        double adjustShoot = distance/140; // minimum fire power .5 , maka 1/140 menjadi konstanta jarak tembak minimum pada 350 pixel
        SetFire(3.0 - adjustShoot);
    }

    public EnemyInfo sortCompositeValue(ref EnemyInfo[] list) // greedy by efficiency shooting, sort untuk menentukan tank yang memiliki composite value terendah sebagai prioritas utama 
    {
        EnemyInfo find_tank = null;
        double temp = double.MaxValue;
        for (int i = 0; i < EnemyInfo.NEFF; i++)
        {
            if (list[i] != null && list[i].CompositeValue < temp)
            {
                find_tank = list[i];
                temp = list[i].CompositeValue;
            }
        }
        return find_tank;
    }

    public void ClearList(ref EnemyInfo[] list) //clear tank list
    {
        for (int i = 0; i < EnemyInfo.NEFF; i++)
        {
            list[i] = null;
        }
        EnemyInfo.NEFF = 0;
    }

    public void SmartShoot(double health, double distance)
    {
        if (health < 3)
        {
            EnergyAdjustedShoot(health);
        }
        else
        {
            DistanceShoot(distance);
        }
    }
}

// Object untuk menyimpan informasi dari tank musuh
public class EnemyInfo
{
    public static int NEFF = 0;
    public double X { get; set; }
    public double Y { get; set; }
    public double distance { get; set; }
    public double deltaAngle { get; set; }
    public double CompositeValue { get; set; }

    public EnemyInfo(double x, double y, double dist, double delta_angle)
    {
        X = x;
        Y = y;
        distance = dist;
        deltaAngle = delta_angle % 180; // modulus 180, karena delta_angle merupakan absolute value dengan range [0,360], akan dibandingkan relative anglenya dengan value [-180,180].
        CompositeValue = distance + (6*deltaAngle); // WeightPriorityValue untuk menentukan tank yang memiliki distance + 6 * delta angle terendah, dengan pertimbangan 1:1 untuk maximum value tiap distance dan deltaAngle
    }
}