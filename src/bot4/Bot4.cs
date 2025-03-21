using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class Bot4 : Bot
{   
    /* A bot that drives forward and backward, and fires a bullet */
    static void Main(string[] args){
        new Bot4().Start();
    }

    Bot4() : base(BotInfo.FromFile("Bot4.json")) {}

    public override void Run()
    {
        /* Customize bot colors, read the documentation for more information */
        BodyColor = Color.Pink;

        MoveToWall();

        while (IsRunning){
            TurnRadarRight(360);
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
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
        if (e.Energy < 10){
            Fire(3);
        } else{
            Evade();
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
    /* Read the documentation for more events and methods */
}
