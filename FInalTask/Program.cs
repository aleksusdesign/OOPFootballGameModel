using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FInalTask
{
    public class Footballer
    {
        private int age;
        private readonly Random r = new Random();
        public string Name { get; set; }
        public int Age
        {
            get
            {
                return age;
            }
            private set
            {
                if (value < 6) throw new ArgumentException("Маленький футболист, подрости, приходи");
                else if (value > 99) throw new ArgumentException("Извините, но к сожалению мы не берем в наши команды Легенд футбола");
                else age = value;
            }
        }        
        
        public int Level{get; private set;}

        public Footballer(string name, int age)
        {
            Name = name;
            Age = age;
            Level = r.Next(0, 100);
        }

        public override string ToString()
        {
            return $"Футболист {Name} - {Age} лет - Уровень: {Level}";
        }
    }

    public class Coach
    {
        private readonly Random r = new Random();
        public string Name { get; private set; }
        public double Level { get; private set; }

        public Coach(string name)
        {
            Name = name;
            Level = r.NextDouble() * (1.5 - 0.5) + 0.5;
        }
    }

    public class Referee
    {
        private readonly Random r = new Random();
        public string Name { get; private set; }
        public Preferences Preference { get; private set; }

        public enum Preferences
        {
            Neutral,
            JudgesTheFirstTeam,
            JudgesTheSecondTeam
        }
        public Referee(string name)
        {
            Name = name;
            Preference = (Preferences)r.Next(0, 3);
        }

        public void HandlerEvents(object sender, GameEventArgs e) 
        { 
            Console.WriteLine(e.Message); 
        }
    }

    public class Team
    {
        private readonly Random r = new Random();
        public string Team_name { get; private set;}
        public List<Footballer> List_footballers { get; private set;}
        
        public Coach Team_coach { get; private set; }
        public double Team_level{ get; set;}
        public int Fans_index{ get; set;}

        public Team(string name, Coach coach)
        {
            Team_name = name;
            List_footballers = new List<Footballer>();
            Team_coach = coach;
            Fans_index = r.Next(1, 11);
            Team_level += Fans_index;
        }

        public void AddFootballer(Footballer f)
        {
            List_footballers.Add(f);
            Team_level += f.Level*Team_coach.Level;
        }

        public List<Footballer> ListAll()
        {
            return List_footballers.OrderBy(a => a.Name).ToList();
        }        
        
        public List<Footballer> ListOverThirty()
        {
            return List_footballers.Where(f => f.Age > 30)
                .OrderByDescending(f => f.Level).ToList();
        }

        public void DopingTest()
        {
            if(r.Next(0, 100) < 25) throw new GameException(this);
        }

        public override string ToString()
        {
            return $"Уровень комманды {Team_name} - {Math.Round(Team_level,2)}" +
                $"\nТренер комманды: {Team_coach.Name} - прогноз удачи {Math.Round(Team_coach.Level, 2)}\n";
        }
    }

    public class GameEventArgs: EventArgs
    {

        public string Message{ get; private set; }

        public GameEventArgs(string message){ Message = message; }

    }


    [Serializable]
    public class GameException : Exception
    {
        public GameException(Team t1)
            : base($"По результатам анализов после матча:" +
                  $"\nкомманда {t1.Team_name.ToUpper()} была поймана на доппинге" +
                  $"\nПОДЛЕЖИТ ДИСКВАЛИФИКАЦИИ")
        {}

        public GameException(string s)
            : base(s)
        {}

        protected GameException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : 
            base(info, context) { }
    }



public class Game
    {
        public event EventHandler<GameEventArgs> Foul;
        public event EventHandler<GameEventArgs> Goal;
        private readonly Random r = new Random();
        public Team Team1{ get;}
        public Team Team2{ get;}
        public Referee Game_referee { get; }

        public enum Events
        {
            Foul,
            Goal
        }
        public Game(Team team1, Team team2, Referee referee)
        {
            Team1 = team1;
            Team2 = team2;
            Game_referee = referee;
        }

        public void CheckCountFootballers()
        {
            if (Team1.List_footballers.Count > Team2.List_footballers.Count) throw new GameException($"В комманде {Team2.Team_name} количество футболистов меньше" +
                $"\nМатч не состоялся");
            else if (Team1.List_footballers.Count < Team2.List_footballers.Count) throw new GameException($"В комманде {Team1.Team_name} количество футболистов меньше" +
               $"\nМатч не состоялся");
        }

        public void StartGame()
        {
            try
            {
                Team1.DopingTest();
                Team2.DopingTest();
            }
            catch(GameException e)
            {
                Console.WriteLine(e.Message);
            }
            try
            {
                CheckCountFootballers();
            }
            catch (GameException e)
            {
                Console.WriteLine(e.Message);
                System.Environment.Exit(1);
            }

            Console.WriteLine($"Матч {Team1.Team_name} - {Team2.Team_name} начался!\n{Team1}\n{Team2}\nСудья матча: {Game_referee.Name}");

            for (int i = 0; i < r.Next(3, 6); i++)
            {
                switch ((Events)r.Next(0, 2))
                {
                    case Game.Events.Foul:
                        Foul?.Invoke(this, new GameEventArgs("Это фол, неспортивное поведение!\nСудья показывает желтую карточку\n"));
                        break;
                    case Game.Events.Goal:
                        Goal?.Invoke(this, new GameEventArgs("Опасный момент... и... ГОООООЛ!\nСудья показывает на центр поля\n"));
                        break;
                }
            }
        }

        public void ResultGame()
        {
            switch (Game_referee.Preference)
            {
                case Referee.Preferences.JudgesTheFirstTeam:
                    Team1.Team_level += 40;
                    Console.WriteLine("Cудья подсуживает первой комманде!\n");
                    break;
                case Referee.Preferences.JudgesTheSecondTeam:
                    Team2.Team_level += 40;
                    Console.WriteLine("Cудья подсуживает второй комманде!\n");
                    break;
                default:
                    Console.WriteLine("Судья нетрален!\n");
                    break;
            }

            switch (Team1.Fans_index.CompareTo(Team2.Fans_index))
            {
                case 0:
                    Console.WriteLine("Армии поддержки одинаковы");
                    break;
                case 1:
                    Console.WriteLine($"Армия фанатов команды {Team1.Team_name} сильнее");
                    Team1.Team_level += Team1.Fans_index;
                    break;
                case -1:
                    Console.WriteLine($"Армия фанатов команды {Team2.Team_name} сильнее");
                    Team2.Team_level += Team2.Fans_index;
                    break;

            }

            if (Math.Abs(Team1.Team_level - Team2.Team_level) < Math.Max(Team1.Team_level, Team2.Team_level) * 0.1f) Console.WriteLine("Ничья\n");
            else if (Team1.Team_level - Team2.Team_level > 0) Console.WriteLine($"Победила комманда {Team1.Team_name}\n");
            else Console.WriteLine($"Победила комманда {Team2.Team_name}\n");
        }
    }

    public static class GameExtensions
    {
        public static void PrintFootballerList(this List<Footballer> f) {
            foreach (Footballer footboler in f)
                Console.WriteLine(footboler);
            Console.WriteLine();
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            Team a = new Team("Мурзилки", new Coach("Шевченко"));
            Team b = new Team("Пупсики", new Coach("Блохин"));

            a.AddFootballer(new Footballer("Петров", 22));
            a.AddFootballer(new Footballer("Семенов", 19));
            a.AddFootballer(new Footballer("Иванов", 25));
            a.AddFootballer(new Footballer("Алёшкин", 39));
            a.AddFootballer(new Footballer("Щербаков", 30));

            b.AddFootballer(new Footballer("Ярмоленко", 30));
            b.AddFootballer(new Footballer("Коноплянка", 29));
            b.AddFootballer(new Footballer("Тайсон", 35));
            b.AddFootballer(new Footballer("Марлос", 20));
            b.AddFootballer(new Footballer("Девич", 45));

            Referee r = new Referee("Киркоров");

            Game g = new Game(a, b, r);

            g.Foul += new EventHandler<GameEventArgs>(r.HandlerEvents);
            g.Goal += new EventHandler<GameEventArgs>(r.HandlerEvents);

            g.StartGame();
            g.ResultGame();
            Console.WriteLine($"Состав комманды {a.Team_name} :");
            a.ListAll().PrintFootballerList();

            Console.WriteLine($"Состав комманды {b.Team_name}, кому за 30 :");
            b.ListOverThirty().PrintFootballerList();
        }
    }
}
