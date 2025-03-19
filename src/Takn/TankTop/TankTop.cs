using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class TankTop : Bot
{   
    int turnDirection = 1;
    string change_dir = "forward";
    bool pressed = false;
    int pressed_count = 0;
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

        while (IsRunning)
        {
            TurnGunLeft(20);
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {

        if (pressed_count == 2){pressed = false; pressed_count = 0;};
        TurnToFaceTarget(e.X, e.Y,e.Direction);
        var bearingFromGun = GunBearingTo(e.X, e.Y);
        TurnGunLeft(bearingFromGun);


        var distance = DistanceTo(e.X, e.Y);

        if(distance <= 220){ //jarak lebih kecil dari 150 pixel shoot max + mundur 10 pixel
            distanceShoot(distance);
            if(pressed){
                distanceShoot(distance);
                pressed_count++;
            }
            else{
                if ( change_dir == "forward"){
                    change_dir = "back";
                    TurnLeft(100);
                    Forward(100);
                }
                else{
                    change_dir = "forward";
                    TurnRight(-100);
                    Forward(-100);
                }
            }
        }
        else{
            Forward(distance/2.7);
        }
        Rescan();
    }

    public override void OnHitByBullet(HitByBulletEvent e)
    {
        if (pressed){
            Fire(2.5);
        }
        else{
            turnDirection *= -1;
        }
    }


    public override void OnHitBot(HitBotEvent e)
    {
        pressed = true;
        double angleToEnemy = BearingTo(e.X, e.Y);
        var distance = DistanceTo(e.X, e.Y);
        TurnGunRight(angleToEnemy - GunBearingTo(e.X, e.Y));
        pressed_count = 0;
        EnergyShoot(e.Energy);
        Forward(40);
    }

    public override void OnHitWall(HitWallEvent e)
    {
        Console.WriteLine("Ouch! I hit a wall, must turn back!");
    }

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

    public void EnergyShoot (double health){
        if (health > 16)
            Fire(3);
        else if (health > 10)
            Fire(2);
        else if (health > 4)
            Fire(1);
        else if (health > 2)
            Fire(.5);
        else if (health > .4)
            Fire(.1);
    }

    public void distanceShoot(double distance){
        if ( distance <= 200){
            if(distance < 100){
                Fire(2.5);
            }
            else if ( distance <150){
                Fire(2);
            }
            else{
                Fire(.7);
            }
        }
    }


}
