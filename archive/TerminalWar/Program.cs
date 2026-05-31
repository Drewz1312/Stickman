
using System;
using System.Collections.Generic;
using System.Threading;

namespace StickmanWar
{
    class Program
    {
        static GameEngine gameEngine = new GameEngine(100, 28);
        static bool isRunning = true;
        static bool isShopOpen = false;
        static readonly object consoleLock = new object();
        static int spawnCount = 1;

        static void Main()
        {
            Console.CursorVisible = false;
            Console.Title = "Stickman War";
            try { Console.SetWindowSize(120, 35); Console.SetBufferSize(120, 35); } catch { }

            new Thread(() =>
            {
                while (isRunning)
                {
                    gameEngine.Update();
                    Render();
                    Thread.Sleep(33);
                }
            }) { IsBackground = true }.Start();

            while (isRunning)
            {
                if (Console.KeyAvailable)
                {
                    var k = Console.ReadKey(true).Key;
                    
                    if (k == ConsoleKey.D1 || k == ConsoleKey.NumPad1)
                        for (int i = 0; i < spawnCount; i++) gameEngine.SpawnAlly(StickmanType.Warrior);
                    else if (k == ConsoleKey.D2 || k == ConsoleKey.NumPad2)
                        for (int i = 0; i < spawnCount; i++) gameEngine.SpawnAlly(StickmanType.Archer);
                    else if (k == ConsoleKey.D3 || k == ConsoleKey.NumPad3)
                        for (int i = 0; i < spawnCount; i++) gameEngine.SpawnAlly(StickmanType.Mage);
                    else if (k == ConsoleKey.UpArrow && spawnCount < 10) spawnCount++;
                    else if (k == ConsoleKey.DownArrow && spawnCount > 1) spawnCount--;
                    else if (k == ConsoleKey.D0) spawnCount = 1;
                    else if (k == ConsoleKey.B) isShopOpen = !isShopOpen;
                    else if (k == ConsoleKey.Spacebar) gameEngine.HealAllies();
                    else if (k == ConsoleKey.Escape) isRunning = false;
                }
                Thread.Sleep(16);
            }
        }

        static void Render()
        {
            // Копируем списки для безопасной отрисовки
            List<Stickman> alliesCopy;
            List<Stickman> enemiesCopy;
            int gold, kills, wave;
            
            lock (consoleLock)
            {
                alliesCopy = new List<Stickman>(gameEngine.Allies);
                enemiesCopy = new List<Stickman>(gameEngine.Enemies);
                gold = gameEngine.ResourceManager.Gold;
                kills = gameEngine.ResourceManager.Kills;
                wave = gameEngine.ResourceManager.Wave;
            }
            
            Console.SetCursorPosition(0, 0);
            
            // HUD
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(" ╔══════════════════════════════════════════════════════════════════════════════════════╗");
            Console.SetCursorPosition(0, 1);
            Console.Write(" ║ ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"Gold: {gold,4}G");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(" │ ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"Kills: {kills,3}");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(" │ ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"Wave: {wave,2}");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(" │ ");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write($"Allies: {alliesCopy.Count,2}");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(" │ ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"Enemies: {enemiesCopy.Count,2}");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(" │ ");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write($"Spawn x{spawnCount}");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(" ║");
            Console.WriteLine(" ╚══════════════════════════════════════════════════════════════════════════════════════╝");

            // Battlefield
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write(" ┌");
            for (int i = 0; i < 100; i++) Console.Write("─");
            Console.WriteLine("┐");
            
            for (int y = 0; y < 23; y++)
            {
                Console.Write(" │");
                for (int x = 0; x < 100; x++)
                {
                    bool drawn = false;

                    foreach (var a in alliesCopy)
                    {
                        if ((int)(a.Position.X / 12) == x && (int)(a.Position.Y / 28) == y && a.Health > 0)
                        {
                            float hpPercent = (float)a.Health / a.MaxHealth;
                            if (hpPercent > 0.7f) Console.ForegroundColor = ConsoleColor.Blue;
                            else if (hpPercent > 0.3f) Console.ForegroundColor = ConsoleColor.DarkCyan;
                            else Console.ForegroundColor = ConsoleColor.DarkBlue;
                            
                            Console.Write(a.Type == StickmanType.Warrior ? "W" : a.Type == StickmanType.Archer ? "A" : "M");
                            drawn = true;
                            break;
                        }
                    }

                    if (!drawn)
                    {
                        foreach (var e in enemiesCopy)
                        {
                            if ((int)(e.Position.X / 12) == x && (int)(e.Position.Y / 28) == y && e.Health > 0)
                            {
                                float hpPercent = (float)e.Health / e.MaxHealth;
                                if (hpPercent > 0.7f) Console.ForegroundColor = ConsoleColor.Red;
                                else if (hpPercent > 0.3f) Console.ForegroundColor = ConsoleColor.DarkRed;
                                else Console.ForegroundColor = ConsoleColor.DarkMagenta;
                                
                                Console.Write("E");
                                drawn = true;
                                break;
                            }
                        }
                    }

                    if (!drawn) Console.Write(" ");
                }
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("│");
            }
            
            Console.Write(" └");
            for (int i = 0; i < 100; i++) Console.Write("─");
            Console.WriteLine("┘");

            // Controls
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(" ╔══════════════════════════════════════════════════════════════════════════════════════╗");
            Console.Write(" ║ ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("[1] Warrior 10G  [2] Archer 15G  [3] Mage 20G  [↑↓] Count  [B] Shop  [Space] Heal 50G  [Esc] Exit");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(" ║");
            Console.WriteLine(" ╚══════════════════════════════════════════════════════════════════════════════════════╝");

            // Shop
            if (isShopOpen)
            {
                Console.SetCursorPosition(0, 29);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(" ╔══════════════════════════ SHOP ═══════════════════════════╗");
                Console.WriteLine(" ║                                                           ║");
                Console.Write(" ║  ⚔ WARRIOR  │  10G  │  ");
                Console.ForegroundColor = gold >= 10 * spawnCount ? ConsoleColor.Green : ConsoleColor.Red;
                Console.Write(gold >= 10 * spawnCount ? $"CAN BUY {spawnCount}" : "NO GOLD");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  │  Melee  HP:150 DMG:25          ║");
                
                Console.Write(" ║  🏹 ARCHER  │  15G  │  ");
                Console.ForegroundColor = gold >= 15 * spawnCount ? ConsoleColor.Green : ConsoleColor.Red;
                Console.Write(gold >= 15 * spawnCount ? $"CAN BUY {spawnCount}" : "NO GOLD");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  │  Range  HP:100 DMG:20 Range:300 ║");
                
                Console.Write(" ║  🔮 MAGE    │  20G  │  ");
                Console.ForegroundColor = gold >= 20 * spawnCount ? ConsoleColor.Green : ConsoleColor.Red;
                Console.Write(gold >= 20 * spawnCount ? $"CAN BUY {spawnCount}" : "NO GOLD");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  │  Magic  HP:80  DMG:35 Area      ║");
                
                Console.WriteLine(" ║                                                           ║");
                Console.Write(" ║  SPAWN COUNT: ");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.Write($"x{spawnCount}");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("  (↑↓ to change, 0 to reset)               ║");
                Console.WriteLine(" ╚═══════════════════════════════════════════════════════════╝");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
        }
    }

    public enum StickmanType { Warrior, Archer, Mage }

    public class Stickman
    {
        public (float X, float Y) Position;
        public int Health, MaxHealth, Damage, AttackCooldown, CurrentCooldown;
        public float Speed, AttackRange;
        public bool IsAlly;
        public StickmanType Type;

        public Stickman((float, float) pos, bool ally, StickmanType type, int hp, float spd, int dmg, float range, int cd)
        {
            Position = pos; IsAlly = ally; Type = type;
            MaxHealth = Health = hp; Speed = spd; Damage = dmg;
            AttackRange = range; AttackCooldown = cd;
        }

        public void Update(List<Stickman> enemies, List<Projectile> projectiles)
        {
            if (Health <= 0) return;
            Stickman t = null;
            float md = float.MaxValue;
            foreach (var e in enemies)
            {
                if (e.Health > 0)
                {
                    float d = (e.Position.X - Position.X) * (e.Position.X - Position.X) + (e.Position.Y - Position.Y) * (e.Position.Y - Position.Y);
                    if (d < md) { md = d; t = e; }
                }
            }
            if (t == null) return;
            float dx = t.Position.X - Position.X;
            float dy = t.Position.Y - Position.Y;
            float dist = MathF.Sqrt(dx * dx + dy * dy);
            if (dist > AttackRange)
            {
                Position = (Position.X + dx / dist * Speed, Position.Y + dy / dist * Speed);
            }
            else if (CurrentCooldown <= 0)
            {
                if (Type == StickmanType.Warrior) t.Health -= Damage;
                else projectiles.Add(new Projectile(Position, t.Position, IsAlly, Damage, Type == StickmanType.Mage ? 6 : 8, Type == StickmanType.Mage));
                CurrentCooldown = AttackCooldown;
            }
            if (CurrentCooldown > 0) CurrentCooldown--;
        }
    }

    public class Projectile
    {
        public (float X, float Y) Position, Target;
        public float Speed;
        public int Damage, Lifespan = 120;
        public bool IsAllied, IsExpired, IsMagic;

        public Projectile((float, float) start, (float, float) target, bool ally, int dmg, float spd, bool magic = false)
        {
            Position = start; Target = target; IsAllied = ally;
            Damage = dmg; Speed = spd; IsMagic = magic;
        }

        public void Update()
        {
            float dx = Target.X - Position.X;
            float dy = Target.Y - Position.Y;
            float dist = MathF.Sqrt(dx * dx + dy * dy);
            if (dist < Speed || Lifespan <= 0) IsExpired = true;
            else { Position = (Position.X + dx / dist * Speed, Position.Y + dy / dist * Speed); Lifespan--; }
        }
    }

    public class ResourceManager
    {
        public int Gold = 500, Kills = 0, Wave = 1;
        public bool SpendGold(int a) { if (Gold >= a) { Gold -= a; return true; } return false; }
        public void AddGold(int a) { Gold += a; }
    }

    public class GameEngine
    {
        public List<Stickman> Allies = new List<Stickman>();
        public List<Stickman> Enemies = new List<Stickman>();
        public List<Projectile> Projectiles = new List<Projectile>();
        public ResourceManager ResourceManager = new ResourceManager();
        Random rnd = new Random();
        int timer = 0;
        readonly object updateLock = new object();

        public GameEngine(int w, int h)
        {
            for (int i = 0; i < 5; i++) SpawnAlly(StickmanType.Warrior);
            SpawnAlly(StickmanType.Archer);
            SpawnAlly(StickmanType.Archer);
            for (int i = 0; i < 5; i++) SpawnEnemy();
        }

        public void Update()
        {
            lock (updateLock)
            {
                foreach (var a in Allies) a.Update(Enemies, Projectiles);
                foreach (var e in Enemies) e.Update(Allies, Projectiles);
                for (int i = Projectiles.Count - 1; i >= 0; i--)
                {
                    Projectiles[i].Update();
                    if (Projectiles[i].IsExpired) Projectiles.RemoveAt(i);
                }
                foreach (var p in Projectiles)
                {
                    var targets = p.IsAllied ? Enemies : Allies;
                    foreach (var t in targets)
                    {
                        if (t.Health > 0)
                        {
                            float dx = p.Position.X - t.Position.X;
                            float dy = p.Position.Y - t.Position.Y;
                            if (dx * dx + dy * dy < 400)
                            {
                                t.Health -= p.Damage;
                                p.IsExpired = true;
                                if (t.Health <= 0 && !t.IsAlly) { ResourceManager.AddGold(10); ResourceManager.Kills++; }
                                break;
                            }
                        }
                    }
                }
                Allies.RemoveAll(a => a.Health <= 0);
                Enemies.RemoveAll(e => e.Health <= 0);
                if (timer++ >= 60 && Enemies.Count < 20)
                {
                    timer = 0;
                    int spawnAmount = ResourceManager.Wave + 3;
                    for (int i = 0; i < spawnAmount; i++) SpawnEnemy();
                    ResourceManager.Wave++;
                }
            }
        }

        public void SpawnAlly(StickmanType t)
        {
            int c = t == StickmanType.Warrior ? 10 : t == StickmanType.Archer ? 15 : 20;
            lock (updateLock)
            {
                if (ResourceManager.SpendGold(c))
                {
                    Stickman ally;
                    float spreadX = rnd.Next(-30, 30);
                    float spreadY = rnd.Next(-20, 20);
                    if (t == StickmanType.Warrior) ally = new Stickman((rnd.Next(80, 200) + spreadX, 650 + spreadY), true, t, 150, 2.5f, 25, 40, 30);
                    else if (t == StickmanType.Archer) ally = new Stickman((rnd.Next(80, 200) + spreadX, 650 + spreadY), true, t, 100, 2f, 20, 300, 45);
                    else ally = new Stickman((rnd.Next(80, 200) + spreadX, 650 + spreadY), true, t, 80, 1.5f, 35, 250, 60);
                    Allies.Add(ally);
                }
            }
        }

        void SpawnEnemy()
        {
            var t = (StickmanType)rnd.Next(3);
            Stickman enemy;
            float spreadX = rnd.Next(-50, 50);
            float spreadY = rnd.Next(-30, 30);
            if (t == StickmanType.Warrior) enemy = new Stickman((rnd.Next(950, 1150) + spreadX, 650 + spreadY), false, t, 100, 2f, 15, 40, 35);
            else if (t == StickmanType.Archer) enemy = new Stickman((rnd.Next(950, 1150) + spreadX, 650 + spreadY), false, t, 70, 1.8f, 12, 280, 50);
            else enemy = new Stickman((rnd.Next(950, 1150) + spreadX, 650 + spreadY), false, t, 55, 1.5f, 25, 230, 65);
            Enemies.Add(enemy);
        }

        public void HealAllies()
        {
            lock (updateLock)
            {
                if (ResourceManager.SpendGold(50))
                    foreach (var a in Allies) a.Health = Math.Min(a.MaxHealth, a.Health + 50);
            }
        }
    }
}
