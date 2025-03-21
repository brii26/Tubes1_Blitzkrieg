using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Dimitri : Bot
{   
    private double targetX = 0;
    private double targetY = 0;
    private double targetEnergy = 100;
    private bool isStrafingLeft = true;


    static void Main(string[] args)
    {
        new Dimitri().Start();
    }

    Dimitri() : base(BotInfo.FromFile("Dimitri.json")) { }

    public override void Run()
    {
        BodyColor = Color.Purple;
        TurretColor = Color.Yellow;
        RadarColor = Color.White;
        BulletColor = Color.Black;
        ScanColor = Color.Purple;
        
        // Independency
        AdjustGunForBodyTurn = true;


        while (IsRunning)
        {
            TurnRadarRight(360);
            SetRescan();
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        targetX = e.X;
        targetY = e.Y;
        targetEnergy = e.Energy;
        var distance = DistanceTo(targetX, targetY);

        SmartShoot(targetEnergy, distance);

        TurnToFaceTarget(targetX, targetY);
        var bearingFromGun = GunBearingTo(targetX, targetY);
        TurnGunLeft(bearingFromGun);

        if (distance >= 100) {
            SetForward(distance/2);
        } else {
            if (isStrafingLeft) {
                SetTurnRight(90);
                SetForward(distance/2);
            } else {
                SetTurnLeft(90);
                SetBack(distance/2);
            }

            if (TurnNumber % 20 == 0) {
                isStrafingLeft = !isStrafingLeft;
            }
        } 
    }

    public override void OnHitBot(HitBotEvent e)
    {
        Evade();
    }

    public override void OnHitWall(HitWallEvent e)
    {
        Back(50);
        TurnRight(30 + RandomAngle());
        Forward(80);
        TurnLeft(60 + RandomAngle());
        Forward(80);
    }


    //* Helper Functions
    private void Evade() {
        TurnRight(30 + RandomAngle());
        Forward(80);
        TurnLeft(60 + RandomAngle());
        Forward(80);
    }

    private void TurnToFaceTarget(double x, double y)
    {
        double angleToTarget = BearingTo(x, y);
        double angleToTurn = CalcBearing(angleToTarget);
        TurnRight(angleToTurn);
    }

    private double RandomAngle() {
        Random rand = new Random();
        return rand.Next(-15, 15);
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
}
