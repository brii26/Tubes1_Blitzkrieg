using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class TemplateBot : Bot
{   int turnDirection = 1;
    bool movingForward;
    static void Main(string[] args)
    {
        new TemplateBot().Start();
    }

    TemplateBot() : base(BotInfo.FromFile("BlitzKrieg.json")) { }
    // * RUN
    public override void Run()
    {
        /* Customize bot colors, read the documentation for more information */
        BodyColor = Color.Pink;
        TurretColor = Color.Black;
        RadarColor = Color.LightPink;
        BulletColor = Color.Black;
        ScanColor = Color.Pink;
        movingForward = true;

        while (IsRunning)
        {
            Forward(20 + Random.Shared.Next(90));
            TurnLeft(Random.Shared.Next(50, 120));
        }
    }

    // * SCAN
    public override void OnScannedBot(ScannedBotEvent e)
    {
        double distance = DistanceTo(e.X, e.Y);
        double gunTurn = BearingTo(e.X, e.Y) - GunDirection;
        var bearingFromGun = GunBearingTo(e.X, e.Y);


        TurnToFaceTarget(e.X, e.Y, e.Direction);
        TurnGunLeft(bearingFromGun);

        SetTurnGunRight(gunTurn);

        // Firepower logic (Greedy for finishing weak enemies)
        double firePower = Math.Min(3, Math.Max(0.1, 200 / distance));
        if (e.Energy < 20) firePower = Math.Min(3, firePower + 0.5);

        SmartFire(distance);
        SetFire(firePower);
    }
    
        // Console.WriteLine("I see a bot at " + e.X + ", " + e.Y);
    
    
    public override void OnHitBot(HitBotEvent e)
    {
        // Console.WriteLine("Ouch! I hit a bot at " + e.X + ", " + e.Y);
    }
    public override void OnHitWall(HitWallEvent e)
    {
        ReverseDirection();
        // Console.WriteLine("Ouch! I hit a wall, must turn back!");
    }


    // ? ADDITIONAL FUNCTIONS
    private void TurnToFaceTarget(double x, double y, double Direction)
    {
        var bearing = BearingTo(x, y);
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
    private void SmartFire(double distance)
    {
        if (distance > 200 || Energy < 15)
            Fire(1);
        else if (distance > 50)
            Fire(2);
        else
            Fire(3);
    }
    /* Read the documentation for more events and methods */
}
