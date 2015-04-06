﻿using System;
using System.Drawing;
using System.Reflection;

using LeagueSharp;
using LeagueSharp.Common;

namespace _xcsoft__ALL_IN_ONE
{
    class initializer
    {
        static Obj_AI_Hero Player = ObjectManager.Player;
        static Menu Menu { get { return xcsoftMenu.Menu; } }
        static Orbwalking.Orbwalker Orbwalker { get { return xcsoftMenu.Orbwalker; } }

        

        internal static void initialize()
        {
            xcsoftMenu.initialize("[xcsoft] ALL IN ONE");
            
            try 
            { 
                xcsoftFunc.sendDebugMsg("[xcsoft] ALL IN ONE: " + Type.GetType("_xcsoft__ALL_IN_ONE.champions." + Player.ChampionName).Name + " Supported."); 
            }
            catch
            {
                xcsoftFunc.sendDebugMsg("[xcsoft] ALL IN ONE: " + Player.ChampionName + " Not supported.");
                Game.PrintChat(xcsoftFunc.colorChat(Color.LightSkyBlue, "[xcsoft] ALL IN ONE: ") + xcsoftFunc.colorChat(Color.DarkGray, Player.ChampionName) + " Not supported.");

                xcsoftMenu.addItem("Sorry, " + Player.ChampionName + " Not supported");
                return;
            }

            xcsoftMenu.addOrbwalker(Player.ChampionName);
            xcsoftMenu.addTargetSelector(Player.ChampionName);
            xcsoftMenu.addSubMenu_ChampTemplate(Player.ChampionName);

            xcsoftMenu.addItem("blank", string.Empty);
            xcsoftMenu.addItem("Work In Progress!");
            xcsoftMenu.addItem("작업중입니다!");

            champLoader.Load(Player.ChampionName);

            Menu.SubMenu("Drawings").AddItem(new MenuItem("blank", string.Empty));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("txt", "--PUBLIC OPTIONS--"));

            Menu.SubMenu("Drawings").AddItem(new MenuItem("drawAARange", "Auto-Attack Real Range").SetValue(new Circle(true, Color.Silver)));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("drawAATarget", "Auto-Attack Target").SetValue(new Circle(true, Color.Red)));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("drawMinionLastHit", "Minion Last Hit").SetValue(new Circle(true, Color.GreenYellow)));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("drawMinionNearKill", "Minion Near Kill").SetValue(new Circle(true, Color.Gray)));
            Menu.SubMenu("Drawings").AddItem(new MenuItem("drawJunglePosition", "Jungle Position").SetValue(true));

            Drawing.OnDraw += Drawing_OnDraw;

            Game.PrintChat(xcsoftFunc.colorChat(Color.DodgerBlue, "[xcsoft] ALL IN ONE: ") + xcsoftFunc.colorChat(Color.Red, Player.ChampionName) + " Loaded");
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

            var drawMinionLastHit = Menu.Item("drawMinionLastHit").GetValue<Circle>();
            var drawMinionNearKill = Menu.Item("drawMinionNearKill").GetValue<Circle>();

            if (drawMinionLastHit.Active || drawMinionNearKill.Active)
            {
                foreach (var minion in MinionManager.GetMinions(Player.Position, Player.AttackRange + Player.BoundingRadius + 300))
                {
                    if (drawMinionLastHit.Active && Player.GetAutoAttackDamage(minion, true) >= minion.Health)
                        Render.Circle.DrawCircle(minion.Position, minion.BoundingRadius, drawMinionLastHit.Color, 5);
                    else
                    if (drawMinionNearKill.Active && Player.GetAutoAttackDamage(minion, true) * 2 >= minion.Health)
                        Render.Circle.DrawCircle(minion.Position, minion.BoundingRadius, drawMinionNearKill.Color, 5);
                }
            }

            if (Game.MapId == (GameMapId)11 && Menu.Item("drawJunglePosition").GetValue<bool>())
            {
                const byte circleRadius = 100;

                Render.Circle.DrawCircle(new SharpDX.Vector3(7461.018f, 3253.575f, 52.57141f), circleRadius, Color.Blue, 5); // blue team: red
                Render.Circle.DrawCircle(new SharpDX.Vector3(3511.601f, 8745.617f, 52.57141f), circleRadius, Color.Blue, 5); // blue team: blue
                Render.Circle.DrawCircle(new SharpDX.Vector3(7462.053f, 2489.813f, 52.57141f), circleRadius, Color.Blue, 5); // blue team: golems
                Render.Circle.DrawCircle(new SharpDX.Vector3(3144.897f, 7106.449f, 51.89026f), circleRadius, Color.Blue, 5); // blue team: wolfs
                Render.Circle.DrawCircle(new SharpDX.Vector3(7770.341f, 5061.238f, 49.26587f), circleRadius, Color.Blue, 5); // blue team: wariaths

                Render.Circle.DrawCircle(new SharpDX.Vector3(10930.93f, 5405.83f, -68.72192f), circleRadius, Color.Yellow, 5); // Dragon

                Render.Circle.DrawCircle(new SharpDX.Vector3(7326.056f, 11643.01f, 50.21985f), circleRadius, Color.Red, 5); // red team: red
                Render.Circle.DrawCircle(new SharpDX.Vector3(11417.6f, 6216.028f, 51.00244f), circleRadius, Color.Red, 5); // red team: blue
                Render.Circle.DrawCircle(new SharpDX.Vector3(7368.408f, 12488.37f, 56.47668f), circleRadius, Color.Red, 5); // red team: golems
                Render.Circle.DrawCircle(new SharpDX.Vector3(10342.77f, 8896.083f, 51.72742f), circleRadius, Color.Red, 5); // red team: wolfs
                Render.Circle.DrawCircle(new SharpDX.Vector3(7001.741f, 9915.717f, 54.02466f), circleRadius, Color.Red, 5); // red team: wariaths                    
            }

            var drawAA = Menu.Item("drawAARange").GetValue<Circle>();
            var drawTarget = Menu.Item("drawAATarget").GetValue<Circle>();

            if (drawAA.Active)
                Render.Circle.DrawCircle(Player.Position, Orbwalking.GetRealAutoAttackRange(Player), drawAA.Color);

            if (drawTarget.Active)
            {
                var aaTarget = Orbwalker.GetTarget();

                if (aaTarget != null)
                    Render.Circle.DrawCircle(aaTarget.Position, aaTarget.BoundingRadius + 15, drawTarget.Color, 6);
            }
        }
    }
}
