using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class BlitzKrieg : Bot
{   int turnDirection = 1;
    int movementCounter = 0;
    bool movingForward;
    private const double energyAdvantageRatio = 1.6;

    static void Main(string[] args)
    {
        new BlitzKrieg().Start();
    }

    BlitzKrieg() : base(BotInfo.FromFile("BlitzKrieg.json")) { }
    // * RUN
    public override void Run()
    {
        AdjustRadarForBodyTurn = true;
        AdjustGunForBodyTurn = true;
        AdjustRadarForGunTurn = true;
        BodyColor = Color.Pink;
        TurretColor = Color.Black;
        RadarColor = Color.LightPink;
        BulletColor = Color.Black;
        ScanColor = Color.Pink;
        movingForward = true;

        while (IsRunning)
        {
            // Scanner default 360 derajats
            TurnRadarRight(360);
        }
    }

    // * SCAN
    public override void OnScannedBot(ScannedBotEvent e)
    {
        // Inisialisasi variabel target, energy, jarak, dan bearing
        double targetX = e.X;
        double targetY = e.Y;
        double targetEnergy = e.Energy;
        double distance = DistanceTo(targetX, targetY);
        var bearingFromGun = GunBearingTo(targetX, targetY);

        // Mekanisme movement default
        if ((movementCounter+=1) % 10 == 0)
        {
            // Gerakan putar random dalam setiap 10 kemungkinan (antara 1 atau -1)
            turnDirection = Random.Shared.Next(2) * 2 - 1;
        }

        if (e.Energy < 20 && Energy > e.Energy * energyAdvantageRatio) {
            // Memutar badan ke arah target dengan energy rendah ketika ownEnergy masih kuat dan lebih agresif mendekati target
            TurnToFaceTarget(targetX, targetY, e.Direction);
            SetForward(150 + Random.Shared.Next(60));
        } 
        else {
            // Menghindari tembakan musuh dengan tambahan bergerak mutar untuk prioritas menghindar ketika energy musuh tinggi
            SetTurnLeft(40 * turnDirection);
            SetForward(100 + Random.Shared.Next(60));
        }
        
        // Memutar senjata ke arah target dan menembak dengan optimisasi energy
        TurnGunLeft(bearingFromGun);
        OptimizedEnergyShoot(Energy, targetEnergy, distance);

        // Setelah menembak, scan ulang
        SetRescan();
    }
    


    public override void OnHitBot(HitBotEvent e)
    {
    }
    public override void OnHitWall(HitWallEvent e)
    {
        // Balik arah ketika nabrak dinding
        ReverseDirection();
    }


    //* Main Methods
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

    // Fungsi optimisasi energy untuk menembak
    public void OptimizedEnergyShoot(double ourEnergy, double targetEnergy, double distance){
        
        double energyRatio = ourEnergy / targetEnergy;
        
        // Menaikkan fire power ketika energy kita lebih tinggi dan jarak dekat untuk efisiensi energi
        if (energyRatio >= energyAdvantageRatio && distance < 150) {
            SetFire(Math.Min(2.5, ourEnergy / 4));
        } else if (targetEnergy < 15 && distance < 100) {
            // Menaikkan fire power jika target energy rendah dan jarak dekat sekali, dengan batas maxFirePower, tetapi menyesuaikan juga dengan energy kita ketika rendah
            SetFire(Math.Min(2.8, ourEnergy / 5));
        } 
        // Menghemat energy pada firePower ketika jarak jauh dan target energy tinggi
        else if (ourEnergy > 20) {
            if (distance < 200) {
                SetFire(2);
            } else if (distance < 300) {
                SetFire(1.8);
            } else {
                SetFire(1.5);
            }
        } 
        // Semakin menurunkan fire power ketika energy kita rendah, tetap memanfaatkan "lifesteal" sehingga tidak stop menembak ketika energy kita rendah
        else if (ourEnergy > 10) SetFire(1.5);
        else if (ourEnergy > 5) SetFire(1);
        else if (ourEnergy > 2) SetFire(0.5);
        else if (ourEnergy > 1) SetFire(0.3);
        else if (ourEnergy > 0.3) {
            if (targetEnergy < 4) {
                SetFire(0.3);
            } else {
                SetFire(0.1);
            }
        }
    }

    // Fungsi helper untuk balik arah
    public void ReverseDirection()
    {
        if (movingForward)
        {
            SetBack(40000);
            movingForward = false;
        }
        else
        {
            SetForward(40000);
            movingForward = true;
        }
    }
}
