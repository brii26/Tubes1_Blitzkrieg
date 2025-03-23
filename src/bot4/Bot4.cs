using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Bot4 : Bot
{   double lastEnemySpeed = 0;

    static void Main(string[] args){
        new Bot4().Start();
    }

    Bot4() : base(BotInfo.FromFile("Bot4.json")) {}

    public override void Run()
    {
        BodyColor = Color.Pink;

        MoveToWall();

        while (IsRunning){
            TurnRadarRight(360);
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        lastEnemySpeed = e.Speed;
        double distance = DistanceTo(e.X, e.Y);

        if (distance <= 300){
            AimAt(e.X, e.Y);
            FireSmart(distance);
        }

        if (distance < 100){
            Evade();
        }
    }

    public override void OnHitBot(HitBotEvent e)
    {
        double distance = DistanceTo(e.X, e.Y);

        if (lastEnemySpeed < 2 && distance < 200){
            AimAt(e.X, e.Y);
            Fire(3);
        } else if (lastEnemySpeed > 4){
            Evade();
        } else{
            AimAt(e.X, e.Y);
            FireSmart(distance);
        }
    }

    public override void OnHitWall(HitWallEvent e)
    {
        Back(50);
        TurnRight(90);
        Forward(100);
    }

    void MoveToWall()
    {
        Forward(500);
        TurnRight(90);
        Forward(500);
    }

    void AimAt(double x, double y)
    {
        var bearing = GunBearingTo(x, y);
        TurnGunLeft(bearing);
    }

    void FireSmart(double distance)
    {
        if (distance < 100)
            Fire(3);
        else if (distance < 200)
            Fire(2);
        else
            Fire(1.2);
    }

    void Evade()
    {
        Back(50);
        TurnRight(30);
        Forward(100);
    }
}
