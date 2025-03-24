# Tubes1_Blietzkrieg

> Tugas Besar IF2211 Strategi Algoritma

## Deskripsi

Proyek ini merupakan Tugas Besar 1 untuk mata kuliah IF2211 Strategi Algoritma di Institut Teknologi Bandung. Proyek ini mengimplementasikan berbagai strategi berbasis algoritma greedy dalam pembuatan bot untuk permainan **Robocode Tank Royale**.

## Fitur Utama

Bot ini dikembangkan dengan berbagai strategi greedy yang berbeda untuk mendapatkan skor tertinggi dalam permainan. Strategi-strategi yang diimplementasikan antara lain:

1. **Greedy by Distance** - Bot menyerang lawan yang memiliki jarak terdekat.
2. **Greedy by Energy** - Bot menyerang lawan dengan energi yang lebih rendah dan menyesuaikan kekuatan tembakan berdasarkan kondisi energi.
3. **Greedy by Shooting Efficiency** - Bot menargetkan musuh dengan perhitungan efisiensi tembakan berdasarkan sudut dan jarak.
4. **Greedy by Potential Points** - Bot memilih target berdasarkan potensi skor tertinggi yang dapat diperoleh dari eliminasi lawan.

## Cara Menjalankan

### Prasyarat

-   Java Development Kit (JDK) terinstall.
-   Robocode Tank Royale telah diunduh dan dikonfigurasi.
-   .NET Framework 9.0

### Setup and How to Run

1. Download the source code (.zip) from the latest release at [GitHub](https://github.com/brii26/Tubes1_Blitzkrieg)
2. Extract the zip file and open the terminal
3. **Run Game Engine**
    ```sh
    java -jar robocode-tankroyale-gui-0.30.0.jar
    ```
4. **Configure Bots**
    - Click **Config** > **Bot Root Directories**, then select the folder containing the bots.
5. **Start Battle**
    - Select the bots you want to play and boot them up.
    - Add the bots as battle participants.
    - Click **Start Battle** to begin the game.

## Project Structure

```
Tubes1_Blitzkrieg/
├── docs/
│   └── Blitzkrieg.pdf
├── README.md
├── src/
│   ├── alternative-bots/
│   │   ├── Adolf/
│   │   │   ├── Adolf.cs
│   │   │   └── Adolf.json
│   │   └── GreedyBastard/
│   │       ├── GreedyBastard.cs
│   │       └── GreedyBastard.json
│   └── main-bot/
│       ├── TankTop/
│       │   ├── TankTop.cs
│       │   └── TankTop.json
│       └── Nazi/
│           ├── Nazi.cs
│           └── Nazi.json
└── .gitignore
```

## Authors

| Nama                     | NIM      |
| ------------------------ | -------- |
| Shanice Feodora Tjahjono | 13523097 |
| Brian Ricardo Tamin      | 13523126 |
| Andrew Tedjapratama      | 13523148 |
