using System;
using System.Drawing;
using Robocode.TankRoyale.BotApi;
using Robocode.TankRoyale.BotApi.Events;

public class TankTop : Bot
{ 
    private int turn_direction = 1;
    private double tolerance; // toleransi radius dari posisi trakhir jarak tank terdekat
    private int error_count; // variable untuk menghitung jumlah fail scan 
    private EnemyInfo[] tank_list; // untuk menyimpan tank musuh sementara sebagai pembanding untuk implementasi greedy by distance
    private EnemyInfo lockedTarget; // tank yang di lock
    private EnemyInfo previousLockedTarget; // tank yang di lock sebelumnya
    static void Main(string[] args)
    {
        new TankTop().Start();
    }

    TankTop() : base(BotInfo.FromFile("TankTop.json")) { }
    public override void Run()
    {
        tolerance = 1200; // inisitalisasi toleransi scan default 1200
        error_count = 0; 
        EnemyInfo.NEFF = 0;
        BodyColor = Color.Black;
        TurretColor = Color.Red;
        RadarColor = Color.Red;
        BulletColor = Color.Black;
        ScanColor = Color.Red;
        AdjustGunForBodyTurn = true;
        lockedTarget = null;
        previousLockedTarget = null;
        tank_list = new EnemyInfo[1000];

        while (IsRunning)
        {
            TurnRadarRight(360);
            SetRescan();
            ClearList(ref tank_list);
        }
    }

    public override void OnScannedBot(ScannedBotEvent e)
    {
        if(error_count == 2){ // kalau tidak ke scan 2x karena distance tolerance scan  error , reset state (rescan)
            previousLockedTarget = null;
            error_count = 0;
        }

        EnemyInfo newTank = new EnemyInfo(e.X,e.Y,DistanceTo(e.X,e.Y), e.Direction); // membuat object tank baru untuk setiap tank yang di deteksi
        tank_list[EnemyInfo.NEFF] = newTank;
        EnemyInfo.NEFF++;

        lockedTarget = sortPriorityTankDistance(ref tank_list); // greedy memanggil fungsi untuk sorting dari distance terendah semua tank yang sudah di scan me return tank terdekat

        if(previousLockedTarget != null){ // jika tank baru mendeteksi sekali atau event seperti tank mati, previousLockedTarget akan menjadi null dan tidak dibandingkan dengan current lockedTarget untuk djadikan tolerance scan greedy
            if (Math.Abs(DistanceTo(previousLockedTarget.X,previousLockedTarget.Y) - DistanceTo(lockedTarget.X, lockedTarget.Y)) > tolerance){
                error_count++; // setiap tank berhenti, error_count bertambah
                return;
            }
        }
        else{
            previousLockedTarget = lockedTarget; //menjadikan previousLockedTarget menjadi LockedTarget, untuk nantinya dibandingkan dengan lockedtarget selanjutnya di onscannedevent selanjutnya
            return ;
        }

        tolerance = (DistanceTo(previousLockedTarget.X,previousLockedTarget.Y) < (DistanceTo(lockedTarget.X, lockedTarget.Y)) ) ? DistanceTo(previousLockedTarget.X,previousLockedTarget.Y)+100 : DistanceTo(lockedTarget.X, lockedTarget.Y) + 100; // membandingkan jarak saat ini ke current locked dan previous, apakah tank yang dideteksi adalah benar tank yang sebelumnya dengan toleransi jarak saat ini / sebelumnya ditambah 70 asumsi tank sudah menjauh sebanyak 70 unit setelah delay
        previousLockedTarget = lockedTarget;

        if(lockedTarget == null){return;} // jika miss scan atau tidak terdeteksi , rescan lagi 


        // greedy by predictive angle movement
        double heading = lockedTarget.direction;
        double gunRotationAdjustment = 15;
        var distance = DistanceTo(lockedTarget.X, lockedTarget.Y);

        SetTurnLeft(BearingTo(lockedTarget.X,lockedTarget.Y));

        if(heading > 15){
            SetTurnGunLeft(GunBearingTo(lockedTarget.X-gunRotationAdjustment,lockedTarget.Y));
        }
        else if (heading < -15){
            SetTurnGunLeft(GunBearingTo(lockedTarget.X+gunRotationAdjustment,lockedTarget.Y));
        }
        else{
            SetTurnGunLeft(GunBearingTo(lockedTarget.X ,lockedTarget.Y));
        }

        SmartShoot(Energy, distance);

        // mekanik movement
        if(distance <= 120){ 
            if(distance >80){
                SetForward(5_00);
                SetTurnLeft(turn_direction * 1_000);
            }
            else{
                SetForward(1_000);
            }
        }
        else{
            SetForward(distance/2.5);
        }
    }

    public override void OnHitBot(HitBotEvent e)
    {
        SetTurnGunLeft(GunBearingTo(e.X,e.Y));
        AggressiveShoot(Energy);
        SetForward(40);
    }

    public void AggressiveShoot (double health) //Aggressive shoot health adjusted
    {
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

        public void DistanceShoot(double distance) //greedy shooting by distance
    {
        double distanceAdjustedShoot = (3.0 - (0.008 * distance));
        SetFire(distanceAdjustedShoot);
    }

    public void SmartShoot(double health, double distance) // kombinasi greedy by distance & greedy by energy shooting
    {
        if (distance < 20){
            AggressiveShoot(health);
        }
        else{
            DistanceShoot(distance);
        }
    }

    public void ClearList(ref EnemyInfo[] list) // fungsi untuk clear list tank hasil scan tiap 360 rotation
    {
        for (int i = 0 ; i < EnemyInfo.NEFF ; i++){
            list[i] = null;
        }
        EnemyInfo.NEFF = 0;
    }

    public EnemyInfo sortPriorityTankDistance(ref EnemyInfo[] list) // fungsi greedy sorting by closest distance
    {
        EnemyInfo find_tank = null;
        double temp = double.MaxValue;
        for (int i = 0 ; i < EnemyInfo.NEFF; i++){
            if(list[i] != null && list[i].distance< temp){
                temp = list[i].distance;
                find_tank = list[i];
            }
        }
        return find_tank;
    }

    public override void OnBotDeath(BotDeathEvent botDeathEvent) // bagian dari heuristik greedy dimana jika musuh mati, reset tolerance ke default value 1200
    {
        tolerance = 1200;
        previousLockedTarget = null;
    }

  public override void OnHitWall(HitWallEvent e)
    {
     turn_direction *=-1;
     SetBack(-2_00);
    }

}

// Class sebagai enemy tank identifier untuk Greedy Heuristik by distance
public class EnemyInfo{

   public static int NEFF = 0 ;
   public double X{get; set;}
   public double Y{get;set;}
   public double distance {get;set;}
   public double direction{get;set;}

   public EnemyInfo(double x, double y, double dist, double direct){
        X = x;
        Y = y;
        distance = dist;
        direction = direct;
   }
}
