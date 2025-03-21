using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class BlitzKrieg : Bot
{   int turnDirection = 1;
    int movementCounter = 0;
    bool movingForward;
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
            TurnRadarRight(360);
        }
    }

    // * SCAN
    public override void OnScannedBot(ScannedBotEvent e)
    {
        double targetX = e.X;
        double targetY = e.Y;
        double distance = DistanceTo(targetX, targetY);
        var bearingFromGun = GunBearingTo(targetX, targetY);


        // ! MOVEMINT
        if ((movementCounter+=1) % 10 == 0)
        {
            turnDirection = Random.Shared.Next(2) * 2 - 1;
        }
        SetForward(100 + Random.Shared.Next(60));
        SetTurnLeft(40 * turnDirection);


        // if (e.Energy < 20)
        // {
        //     TurnToFaceTarget(targetX, targetY, e.Direction);
        // }
        
        TurnGunLeft(bearingFromGun);

        SmartShoot(Energy, distance);
        SetRescan();
    }
    


    public override void OnHitBot(HitBotEvent e)
    {
    }
    public override void OnHitWall(HitWallEvent e)
    {
        ReverseDirection();
    }


    // ? ADDITIONAL FUNCTIONS
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

