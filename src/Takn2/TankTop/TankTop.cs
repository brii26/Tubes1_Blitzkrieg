using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class TankTop : Bot
{   
    static void Main(string[] args)
    {
        new TankTop().Start();
    }

    TankTop() : base(BotInfo.FromFile("TankTop.json")) { }

    public override void Run()
    {
        BodyColor = Color.Black;
        TurretColor = Color.Red;
        RadarColor = Color.Red;
        BulletColor = Color.Black;
        ScanColor = Color.Red;
        AdjustGunForBodyTurn = true;

        while (IsRunning)
        {
            TurnRadarRight(360);
            SetRescan();
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {

        var distance = DistanceTo(e.X, e.Y);
        SmartShoot(Energy,distance);

        TurnToFaceTarget(e.X, e.Y,e.Direction);
        var bearingFromGun = GunBearingTo(e.X, e.Y);
        TurnGunLeft(bearingFromGun);

        if(distance <= 350){ 
            if(distance >300){
                SetForward(2);
            }
            else{
                SetBack(15);
            }
        }
        else{
            SetForward(distance/3.5);
        }
    }


    public override void OnHitByBullet(HitByBulletEvent e)
    {

    }


    public override void OnHitBot(HitBotEvent e)
    {
        double angleToEnemy = BearingTo(e.X, e.Y);
        var distance = DistanceTo(e.X, e.Y);
        TurnGunRight(angleToEnemy - GunBearingTo(e.X, e.Y));
        SmartShoot(Energy, distance);
        Forward(40);
    }

    public override void OnHitWall(HitWallEvent e)
    {
        Console.WriteLine("Ouch! I hit a wall, must turn back!");
    }

    private void TurnToFaceTarget(double x, double y, double Direction)
    {
        var bearing = BearingTo(x, y);

        TurnLeft(bearing);
    }

    public void EnergyAdjustedShoot (double health){
        if (health > 16)
            SetFire(3);
        else if (health > 10)
            SetFire(2);
        else if (health > 4)
            SetFire(1);
        else if (health > 2)
            SetFire(.5);
        else if (health > .4)
            SetFire(.1);
    }

        public void DistanceShoot(double distance)
    {
        if ( distance <= 300){
            if(distance < 100){
                SetFire(1.7);
            }
            else if ( distance <200){
                SetFire(1.5);
            }
            else{
                SetFire(1.2);
            }
        }
        else{
            SetFire(1);
        }
    }

    public void SmartShoot(double health, double distance){
        if (distance < 30){
            EnergyAdjustedShoot(health);
        }
        if ( health < 20){
            EnergyAdjustedShoot(health);
        }
        else{
            DistanceShoot(distance);
        }
    }

}