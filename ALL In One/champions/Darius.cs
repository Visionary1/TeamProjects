﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

using LeagueSharp;
using LeagueSharp.Common;

namespace ALL_In_One.champions
{
    class Darius//RL144
    {
        static Menu Menu {get{return AIO_Menu.MainMenu_Manual.SubMenu("Champion");}} //
        static Orbwalking.Orbwalker Orbwalker { get { return AIO_Menu.Orbwalker; } }
        static Obj_AI_Hero Player { get { return ObjectManager.Player; } }
        static Spell Q, W, E, R;

		

        public static void Load()
        {
            Q = new Spell(SpellSlot.Q, 425f, TargetSelector.DamageType.Physical);
            W = new Spell(SpellSlot.W);
            E = new Spell(SpellSlot.E, 550f, TargetSelector.DamageType.Physical);
            R = new Spell(SpellSlot.R, 460f, TargetSelector.DamageType.True);

			Q.SetTargetted(0.25f, float.MaxValue);
            E.SetSkillshot(0.1f, 50f * (float)Math.PI / 180, float.MaxValue, false, SkillshotType.SkillshotCone);
			R.SetTargetted(0.4f, float.MaxValue);
			
            AIO_Menu.Champion.Combo.addUseQ();
            Menu.SubMenu("Combo").AddItem(new MenuItem("ComboQD", "Q Min Distance", true).SetValue(new Slider(275, 0, 425)));
            Menu.SubMenu("Combo").AddItem(new MenuItem("ComboED", "E Min Distance", true).SetValue(new Slider(150, 0, 550)));
            AIO_Menu.Champion.Combo.addUseW();
            AIO_Menu.Champion.Combo.addUseE();

			AIO_Menu.Champion.Harass.addUseQ();
			AIO_Menu.Champion.Harass.addUseW();
			AIO_Menu.Champion.Harass.addUseE();
            AIO_Menu.Champion.Harass.addIfMana();

            AIO_Menu.Champion.Laneclear.addUseQ();
            AIO_Menu.Champion.Laneclear.addUseW();
            AIO_Menu.Champion.Laneclear.addIfMana();


            AIO_Menu.Champion.Jungleclear.addUseQ();
            AIO_Menu.Champion.Jungleclear.addUseW();
            AIO_Menu.Champion.Jungleclear.addIfMana();

            AIO_Menu.Champion.Misc.addItem("KillstealQ", true);
            AIO_Menu.Champion.Misc.addItem("KillstealR", true);
            AIO_Menu.Champion.Drawings.addQRange();
            AIO_Menu.Champion.Drawings.addERange(false);
            AIO_Menu.Champion.Drawings.addItem("E Safe Range", new Circle(true, Color.Red));
			AIO_Menu.Champion.Drawings.addRRange();
			
			AIO_Menu.Champion.Drawings.addDamageIndicator(getComboDamage);

            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
            Orbwalking.AfterAttack += Orbwalking_AfterAttack;
			
        }

        static void Game_OnUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            if (Orbwalking.CanMove(10))
            {
                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                    Combo();

                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
                    Harass();

                if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                {
                    Laneclear();
                    Jungleclear();
                }
            }

            #region Killsteal
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealQ"))
                KillstealQ();
            if (AIO_Menu.Champion.Misc.getBoolValue("KillstealR"))
                KillstealR();
            #endregion
			
			#region AfterAttack
			AIO_Func.AASkill(W);
			if(AIO_Func.AfterAttack())
			AA();
			#endregion
			
        }

        static void Drawing_OnDraw(EventArgs args)
        {
            if (Player.IsDead)
                return;

		var drawQ = AIO_Menu.Champion.Drawings.QRange;
		var drawE = AIO_Menu.Champion.Drawings.ERange;
		var drawR = AIO_Menu.Champion.Drawings.RRange;
		var drawEr = AIO_Menu.Champion.Drawings.getCircleValue("E Safe Range");
		var eTarget = TargetSelector.GetTarget(E.Range + Player.MoveSpeed * E.Delay, TargetSelector.DamageType.Magical);

	
		if (Q.IsReady() && drawQ.Active)
		Render.Circle.DrawCircle(Player.Position, Q.Range, drawQ.Color);
	
		if (E.IsReady() && drawE.Active)
		Render.Circle.DrawCircle(Player.Position, E.Range, drawE.Color);
		
		if (R.IsReady() && drawR.Active)
		Render.Circle.DrawCircle(Player.Position, R.Range, drawR.Color);
		
		if (E.IsReady() && drawEr.Active && eTarget != null)
		Render.Circle.DrawCircle(Player.Position, E.Range - eTarget.MoveSpeed*E.Delay, drawEr.Color);


        }
		
        static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!AIO_Menu.Champion.Misc.UseAntiGapcloser || Player.IsDead)
                return;


        }

        static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!AIO_Menu.Champion.Misc.UseInterrupter || Player.IsDead)
                return;

            if (E.IsReady()
			&& Player.Distance(sender.Position) <= E.Range)
                E.Cast(sender.Position);

        }

        static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (!sender.IsMe || Player.IsDead)
                return;
				

        }
		
		static void AA() // 챔피언 대상 평캔 ( 빼낸 이유는 AA방식 두개로 할시 두번 적어야 해서 단순화하기 위함.
		{
			var target = TargetSelector.GetTarget(Player.AttackRange + 50,TargetSelector.DamageType.Physical, true); //
			if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
			{
				if (AIO_Menu.Champion.Harass.UseW && W.IsReady() && utility.Activator.AfterAttack.ALLCancleItemsAreCasted
					&& HeroManager.Enemies.Any(x => Orbwalking.InAutoAttackRange(x)))
					W.Cast();
			}
				
			if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
			{
				if (AIO_Menu.Champion.Combo.UseW && W.IsReady() && utility.Activator.AfterAttack.ALLCancleItemsAreCasted
					&& HeroManager.Enemies.Any(x => Orbwalking.InAutoAttackRange(x)))
					W.Cast();					
			}
		}
		
        static void Orbwalking_AfterAttack(AttackableUnit unit, AttackableUnit target)
        {
            var Target = (Obj_AI_Base)target;
            if (!unit.IsMe || Target == null)
                return;

            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
			{
			AIO_Func.AALcJc(Q);
			AIO_Func.AALcJc(W);
			}
			if(!utility.Activator.AfterAttack.AIO)
			AA();
        }
		
        static void Combo()
        {
            if (AIO_Menu.Champion.Combo.UseQ && Q.IsReady())
            {
				var QD = Menu.Item("ComboQD", true).GetValue<Slider>().Value;
                var qTarget = TargetSelector.GetTarget(Q.Range - 10, Q.DamageType, true);
				if(qTarget.Distance(Player.Position) >= QD)
                Q.Cast();
            }
			
            if (AIO_Menu.Champion.Combo.UseE && E.IsReady())
            {
				var ED = Menu.Item("ComboED", true).GetValue<Slider>().Value;
                var ETarget = TargetSelector.GetTarget(E.Range, E.DamageType, true);
                if (ETarget.Distance(Player.Position) >= ED)
                E.Cast(ETarget);
            }

				
        }

        static void Harass()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Harass.IfMana))
                return;
				
            if (AIO_Menu.Champion.Harass.UseQ && Q.IsReady())
            {
				var QD = Menu.Item("ComboQD", true).GetValue<Slider>().Value;
                var qTarget = TargetSelector.GetTarget(Q.Range - 10, Q.DamageType, true);
				if(qTarget.Distance(Player.Position) >= QD)
                Q.Cast();
            }
				
            if (AIO_Menu.Champion.Harass.UseE && E.IsReady())
            {
				var ED = Menu.Item("ComboED", true).GetValue<Slider>().Value;
                var ETarget = TargetSelector.GetTarget(E.Range, E.DamageType, true);
                if (ETarget.Distance(Player.Position) >= ED)
                E.Cast(ETarget);
            }

        }
		
        static void Laneclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Laneclear.IfMana))
                return;

            var Minions = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Enemy);

            if (Minions.Count <= 0)
                return;

        }

        static void Jungleclear()
        {
            if (!(AIO_Func.getManaPercent(Player) > AIO_Menu.Champion.Jungleclear.IfMana))
                return;

            var Mobs = MinionManager.GetMinions(Q.Range, MinionTypes.All, MinionTeam.Neutral, MinionOrderTypes.MaxHealth);

            if (Mobs.Count <= 0)
                return;


        }

        static void KillstealQ()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
                if (Q.CanCast(target) && AIO_Func.isKillable(target, Q))
                    Q.Cast();
            }
        }
        static void KillstealR()
        {
            foreach (var target in HeroManager.Enemies.OrderByDescending(x => x.Health))
            {
				if(R.CanCast(target) && AIO_Func.isKillable(target, R))
				R.Cast(target);
				else
				{
					var buff = AIO_Func.getBuffInstance(target, "dariushemo");
					if (R.CanCast(target) && AIO_Func.isKillable(target, R.GetDamage(target)*(1 + (float)buff.Count / 5) - 10))
					R.Cast(target);
				}
			}
        }

        static float getComboDamage(Obj_AI_Base enemy)
        {
            float damage = 0;

            if (Q.IsReady())
			{
                damage += Q.GetDamage(enemy);
                damage += (float)Player.GetAutoAttackDamage(enemy, true);
			}
			
            if (Items.CanUseItem((int)ItemId.Tiamat_Melee_Only))
			{
			damage += (float)Player.GetItemDamage(enemy, Damage.DamageItems.Tiamat);
			damage += (float)Player.GetAutoAttackDamage(enemy, true);
			}
			
            if (Items.CanUseItem((int)ItemId.Ravenous_Hydra_Melee_Only))
			{
			damage += (float)Player.GetItemDamage(enemy, Damage.DamageItems.Hydra);
			damage += (float)Player.GetAutoAttackDamage(enemy, true);
			}

            if (W.IsReady())
                damage += W.GetDamage(enemy) + (float)Player.GetAutoAttackDamage(enemy, true);

            if (R.IsReady() && AIO_Menu.Champion.Combo.UseR)
                damage += R.GetDamage(enemy);

            if(!Player.IsWindingUp)
                damage += (float)Player.GetAutoAttackDamage(enemy, true);
				
            return damage;
        }
    }
}
