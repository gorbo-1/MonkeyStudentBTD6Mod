using BTD_Mod_Helper;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Api.Towers;
using BTD_Mod_Helper.Extensions;
using Il2CppAssets.Scripts.Models.Bloons;
using Il2CppAssets.Scripts.Models.Bloons.Behaviors;
using Il2CppAssets.Scripts.Models.Map;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Abilities;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Abilities.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Attack;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Attack.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Emissions;
using Il2CppAssets.Scripts.Models.Towers.Filters;
using Il2CppAssets.Scripts.Models.Towers.Projectiles;
using Il2CppAssets.Scripts.Models.Towers.Projectiles.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Weapons;
using Il2CppAssets.Scripts.Models.Towers.Weapons.Behaviors;
using Il2CppAssets.Scripts.Models.TowerSets;
using Il2CppAssets.Scripts.Simulation.Bloons;
using Il2CppAssets.Scripts.Unity;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using MelonLoader;
using MonkeyStudent;
using System;
using System.Linq;
using UnityEngine;

[assembly: MelonInfo(typeof(MonkeyStudent.Main), "Monkey Student", "1.0.0", "di0rgio")]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace MonkeyStudent;

public class Main : BloonsTD6Mod { }

public class MonkeyStudentTower : ModTower
{
    public override TowerSet TowerSet => TowerSet.Primary;
    public override string BaseTower => TowerType.NinjaMonkey;
    public override int Cost => 300;
    public override string DisplayName => "Monkey Student";
    public override string Description => "Pulls a random item out of his backpack and throws it at the bloons. Each attack has a ⅓ chance of activating.";

    public override void ModifyBaseTowerModel(TowerModel towerModel)
    {
        var attackModel = towerModel.GetAttackModel();

        // Create three separate weapons for the three different projectiles
        var pencilWeapon = attackModel.weapons[0].Duplicate();
        var calcWeapon = attackModel.weapons[0].Duplicate();
        var bookWeapon = attackModel.weapons[0].Duplicate();

        // Nerf base stats - much weaker
        pencilWeapon.Rate = 3.0f; // Slower attack speed
        calcWeapon.Rate = 2.0f;
        bookWeapon.Rate = 2.0f;

        // Creatprojectiles
        pencilWeapon.projectile = CreatePencilProjectile(pencilWeapon.projectile);
        calcWeapon.projectile = CreateCalcProjectile(calcWeapon.projectile);
        bookWeapon.projectile = CreateBookProjectile(bookWeapon.projectile);

        // Replace the single weapon with three weapons
        attackModel.weapons = new Il2CppReferenceArray<WeaponModel>(new WeaponModel[]
        {
            pencilWeapon,
            calcWeapon,
            bookWeapon
        });
    }

    private ProjectileModel CreatePencilProjectile(ProjectileModel baseProjectile)
    {
        var pencil = baseProjectile.Duplicate();
        pencil.id = "PencilProjectile";

        // Much weaker base stats
        pencil.pierce = 1; // Only 1 pierce
        pencil.GetDamageModel().damage = 1; // Low damage

        // Visual changes - use dart monkey's dart for pencil-like appearance
        pencil.display = Game.instance.model.GetTowerFromId("DartMonkey").GetWeapon().projectile.display;
        pencil.scale = 1.2f;

        return pencil;
    }

    private ProjectileModel CreateCalcProjectile(ProjectileModel baseProjectile)
    {
        var calc = baseProjectile.Duplicate();
        calc.id = "CalcProjectile";

        // Weak base stats
        calc.GetDamageModel().damage = 1; // Low damage
        calc.pierce = 1;

        // Visual changes - use bomb shooter projectile for calculator
        calc.display = Game.instance.model.GetTowerFromId("BombShooter").GetWeapon().projectile.display;
        calc.scale = 0.7f;

        // Create explosion projectile manually
        var explosionProjectile = baseProjectile.Duplicate();
        explosionProjectile.GetDamageModel().damage = 1;
        explosionProjectile.radius = 10f;
        explosionProjectile.pierce = 8;

        // Add explosion behavior
        var smallExplosion = new CreateProjectileOnContactModel(
            "CalcExplosion",
            explosionProjectile,
            new SingleEmissionModel("", null),
            true, false, false);
        calc.AddBehavior(smallExplosion);

        return calc;
    }

    private ProjectileModel CreateBookProjectile(ProjectileModel baseProjectile)
    {
        var book = baseProjectile.Duplicate();
        book.id = "BookProjectile";

        // Weak base stats
        book.GetDamageModel().damage = 1; // Low damage
        book.pierce = 1;

        // Visual changes - use boomerang for book
        var boomerangTower = Game.instance.model.GetTowerFromId("BoomerangMonkey");
        book.display = boomerangTower.GetWeapon().projectile.display;
        book.scale = 1.5f;

        // Create explosion projectile manually
        var explosionProjectile = baseProjectile.Duplicate();
        explosionProjectile.GetDamageModel().damage = 1;
        explosionProjectile.radius = 12f;
        explosionProjectile.pierce = 10;

        // Add explosion behavior
        var bookExplosion = new CreateProjectileOnContactModel(
            "BookExplosion",
            explosionProjectile,
            new SingleEmissionModel("", null),
            true, false, false);
        book.AddBehavior(bookExplosion);

        return book;
    }

    // Artist Path Upgrades
    public class ArtistTier1 : ModUpgrade<MonkeyStudentTower>
    {
        public override int Path => 0;
        public override int Tier => 1;
        public override int Cost => 250;
        public override string DisplayName => "Sharpened Pencil";
        public override string Description => "Pencil damage increased significantly";

        public override void ApplyUpgrade(TowerModel towerModel)
        {
            foreach (var weapon in towerModel.GetAttackModel().weapons)
            {
                if (weapon.projectile.id == "PencilProjectile")
                {
                    weapon.projectile.GetDamageModel().damage += 2; // 1 -> 3
                    weapon.projectile.pierce += 1; // 1 -> 2
                }
            }
        }
    }

    public class ArtistTier2 : ModUpgrade<MonkeyStudentTower>
    {
        public override int Path => 0;
        public override int Tier => 2;
        public override int Cost => 500;
        public override string DisplayName => "Mechanical Pencil";
        public override string Description => "Pencil damage and pierce increased further";

        public override void ApplyUpgrade(TowerModel towerModel)
        {
            foreach (var weapon in towerModel.GetAttackModel().weapons)
            {
                if (weapon.projectile.id == "PencilProjectile")
                {
                    weapon.projectile.GetDamageModel().damage += 3; // 3 -> 6
                    weapon.projectile.pierce += 2; // 2 -> 4
                }
            }
        }
    }

    public class ArtistTier3 : ModUpgrade<MonkeyStudentTower>
    {
        public override int Path => 0;
        public override int Tier => 3;
        public override int Cost => 900;
        public override string DisplayName => "Amateur Artist";
        public override string Description => "Pencil becomes paintbrush with splitting attack. Faster attack speed.";

        public override void ApplyUpgrade(TowerModel towerModel)
        {
            foreach (var weapon in towerModel.GetAttackModel().weapons)
            {
                weapon.Rate *= 0.75f; // Faster attack speed for all weapons

                if (weapon.projectile.id == "PencilProjectile")
                {
                    weapon.projectile.GetDamageModel().damage += 2; // 6 -> 8

                    // Add splitting behavior
                    var createProjectileModel = new CreateProjectileOnContactModel(
                        "PaintbrushSplit", weapon.projectile.Duplicate(),
                        new ArcEmissionModel("", 3, 0, 60, null, false, false),
                        true, false, false);
                    weapon.projectile.AddBehavior(createProjectileModel);
                }
            }
        }
    }

    public class ArtistTier4 : ModUpgrade<MonkeyStudentTower>
    {
        public override int Path => 0;
        public override int Tier => 4;
        public override int Cost => 5000;
        public override string DisplayName => "Art Club Member";
        public override string Description => "Paintbrush splits into 5 projectiles. Massive damage increase.";

        public override void ApplyUpgrade(TowerModel towerModel)
        {
            foreach (var weapon in towerModel.GetAttackModel().weapons)
            {
                weapon.Rate *= 0.8f; // Even faster attack speed
                weapon.projectile.GetDamageModel().damage += 5; // Big damage boost to all

                if (weapon.projectile.id == "PencilProjectile")
                {
                    weapon.projectile.GetDamageModel().damage += 10; // 13 -> 23 total

                    // Update splitting to 5 projectiles
                    var splitBehavior = weapon.projectile.GetBehavior<CreateProjectileOnContactModel>();
                    if (splitBehavior != null)
                    {
                        splitBehavior.emission = new ArcEmissionModel("", 5, 0, 90, null, false, false);
                        splitBehavior.projectile.GetDamageModel().damage = 8; // Split projectiles do good damage
                    }
                }
            }
        }
    }

    public class ArtistTier5 : ModUpgrade<MonkeyStudentTower>
    {
        public override int Path => 0;
        public override int Tier => 5;
        public override int Cost => 25000;
        public override string DisplayName => "Art Club President";
        public override string Description => "Extreme attack speed and damage. Paintbrush splits into 8 projectiles.";

        public override void ApplyUpgrade(TowerModel towerModel)
        {
            foreach (var weapon in towerModel.GetAttackModel().weapons)
            {
                weapon.Rate *= 0.25f; // 4x attack speed
                weapon.projectile.GetDamageModel().damage += 20; // Massive damage boost

                if (weapon.projectile.id == "PencilProjectile")
                {
                    weapon.projectile.GetDamageModel().damage += 30; // 53 -> 83 total
                    weapon.projectile.pierce += 10; // Lots of pierce

                    // Update splitting to 8 projectiles
                    var splitBehavior = weapon.projectile.GetBehavior<CreateProjectileOnContactModel>();
                    if (splitBehavior != null)
                    {
                        splitBehavior.emission = new ArcEmissionModel("", 8, 0, 120, null, false, false);
                        splitBehavior.projectile.GetDamageModel().damage = 25; // Split projectiles do massive damage
                    }
                }
            }
        }
    }

    // Mathematician Path Upgrades
    public class MathematicianTier1 : ModUpgrade<MonkeyStudentTower>
    {
        public override int Path => 1;
        public override int Tier => 1;
        public override int Cost => 150;
        public override string DisplayName => "Better Calc";
        public override string Description => "Calculator damage increased, slightly better range.";

        public override void ApplyUpgrade(TowerModel towerModel)
        {
            towerModel.range *= 1.15f;
            towerModel.GetAttackModel().range *= 1.15f;

            foreach (var weapon in towerModel.GetAttackModel().weapons)
            {
                if (weapon.projectile.id == "CalcProjectile")
                {
                    weapon.projectile.GetDamageModel().damage += 2; // 1 -> 3

                    // Improve explosion
                    var explosion = weapon.projectile.GetBehavior<CreateProjectileOnContactModel>();
                    if (explosion != null)
                    {
                        explosion.projectile.GetDamageModel().damage += 1; // 1 -> 2
                        explosion.projectile.radius += 5f; // 10 -> 15
                    }
                }
            }
        }
    }

    public class MathematicianTier2 : ModUpgrade<MonkeyStudentTower>
    {
        public override int Path => 1;
        public override int Tier => 2;
        public override int Cost => 400;
        public override string DisplayName => "Best Calc";
        public override string Description => "Calculator damage and explosion significantly improved.";

        public override void ApplyUpgrade(TowerModel towerModel)
        {
            towerModel.range *= 1.3f;
            towerModel.GetAttackModel().range *= 1.3f;

            foreach (var weapon in towerModel.GetAttackModel().weapons)
            {
                if (weapon.projectile.id == "CalcProjectile")
                {
                    weapon.projectile.GetDamageModel().damage += 3; // 3 -> 6
                    weapon.projectile.pierce += 2; // 1 -> 3

                    // Much better explosion
                    var explosion = weapon.projectile.GetBehavior<CreateProjectileOnContactModel>();
                    if (explosion != null)
                    {
                        explosion.projectile.GetDamageModel().damage += 3; // 2 -> 5
                        explosion.projectile.radius += 10f; // 15 -> 25
                    }
                }
            }
        }
    }

    public class MathematicianTier3 : ModUpgrade<MonkeyStudentTower>
    {
        public override int Path => 1;
        public override int Tier => 3;
        public override int Cost => 1250;
        public override string DisplayName => "Math Enjoyer";
        public override string Description => "Calculator becomes computer with massive explosion damage and pierce.";

        public override void ApplyUpgrade(TowerModel towerModel)
        {
            foreach (var weapon in towerModel.GetAttackModel().weapons)
            {
                weapon.Rate *= 0.8f; // Faster attack speed
                weapon.projectile.GetDamageModel().damage += 3; // Overall damage boost

                if (weapon.projectile.id == "CalcProjectile")
                {
                    weapon.projectile.GetDamageModel().damage += 8; // 9 -> 17 total
                    weapon.projectile.pierce += 5; // 3 -> 8

                    // Huge explosion improvement
                    var explosion = weapon.projectile.GetBehavior<CreateProjectileOnContactModel>();
                    if (explosion != null)
                    {
                        explosion.projectile.GetDamageModel().damage += 10; // 5 -> 15
                        explosion.projectile.radius += 20f; // 25 -> 45
                    }
                }
            }
        }
    }

    public class MathematicianTier4 : ModUpgrade<MonkeyStudentTower>
    {
        public override int Path => 1;
        public override int Tier => 4;
        public override int Cost => 7000;
        public override string DisplayName => "Calculus Club Member";
        public override string Description => "Massive range and damage increase. Computer explosions are devastating.";

        public override void ApplyUpgrade(TowerModel towerModel)
        {
            towerModel.range *= 2.0f; // Double range
            towerModel.GetAttackModel().range *= 2.0f;

            foreach (var weapon in towerModel.GetAttackModel().weapons)
            {
                weapon.Rate *= 0.7f; // Faster attack speed
                weapon.projectile.GetDamageModel().damage += 10; // Big damage boost

                if (weapon.projectile.id == "CalcProjectile")
                {
                    weapon.projectile.GetDamageModel().damage += 20; // 27 -> 47 total
                    weapon.projectile.pierce += 10; // 8 -> 18

                    // Devastating explosion
                    var explosion = weapon.projectile.GetBehavior<CreateProjectileOnContactModel>();
                    if (explosion != null)
                    {
                        explosion.projectile.GetDamageModel().damage += 25; // 15 -> 40
                        explosion.projectile.radius += 30f; // 45 -> 75
                    }
                }
            }
        }
    }

    public class MathematicianTier5 : ModUpgrade<MonkeyStudentTower>
    {
        public override int Path => 1;
        public override int Tier => 5;
        public override int Cost => 32400;
        public override string DisplayName => "Calculus Club President";
        public override string Description => "Extreme range, attack speed, and damage. Computer explosions can destroy entire waves.";

        public override void ApplyUpgrade(TowerModel towerModel)
        {
            towerModel.range *= 2.5f; // Massive range
            towerModel.GetAttackModel().range *= 2.5f;

            foreach (var weapon in towerModel.GetAttackModel().weapons)
            {
                weapon.Rate *= 0.3f; // 3x attack speed
                weapon.projectile.GetDamageModel().damage += 30; // Massive damage boost

                if (weapon.projectile.id == "CalcProjectile")
                {
                    weapon.projectile.GetDamageModel().damage += 60; // 77 -> 137 total
                    weapon.projectile.pierce += 20; // 18 -> 38

                    // Screen-clearing explosion
                    var explosion = weapon.projectile.GetBehavior<CreateProjectileOnContactModel>();
                    if (explosion != null)
                    {
                        explosion.projectile.GetDamageModel().damage += 80; // 40 -> 120
                        explosion.projectile.radius += 50f; // 75 -> 125
                    }
                }
            }
        }
    }

    // Literature Fan Path Upgrades
    public class LiteratureFan1 : ModUpgrade<MonkeyStudentTower>
    {
        public override int Path => 2;
        public override int Tier => 1;
        public override int Cost => 400;
        public override string DisplayName => "Lighter Books";
        public override string Description => "Book damage increased, faster attack speed.";

        public override void ApplyUpgrade(TowerModel towerModel)
        {
            foreach (var weapon in towerModel.GetAttackModel().weapons)
            {
                weapon.Rate *= 0.8f; // 25% attack speed increase

                if (weapon.projectile.id == "BookProjectile")
                {
                    weapon.projectile.GetDamageModel().damage += 2; // 1 -> 3

                    // Better book explosion
                    var explosion = weapon.projectile.GetBehavior<CreateProjectileOnContactModel>();
                    if (explosion != null)
                    {
                        explosion.projectile.GetDamageModel().damage += 1; // 1 -> 2
                        explosion.projectile.radius += 3f; // 12 -> 15
                    }
                }
            }
        }
    }

    public class LiteratureFan2 : ModUpgrade<MonkeyStudentTower>
    {
        public override int Path => 2;
        public override int Tier => 2;
        public override int Cost => 700;
        public override string DisplayName => "Super Light Books";
        public override string Description => "Much faster attack speed, camo detection, book damage increased.";

        public override void ApplyUpgrade(TowerModel towerModel)
        {
            foreach (var weapon in towerModel.GetAttackModel().weapons)
            {
                weapon.Rate *= 0.67f; // Much faster attack speed

                if (weapon.projectile.id == "BookProjectile")
                {
                    weapon.projectile.GetDamageModel().damage += 3; // 3 -> 6
                    weapon.projectile.pierce += 2; // 1 -> 3

                    // Much better explosion
                    var explosion = weapon.projectile.GetBehavior<CreateProjectileOnContactModel>();
                    if (explosion != null)
                    {
                        explosion.projectile.GetDamageModel().damage += 3; // 2 -> 5
                        explosion.projectile.radius += 8f; // 15 -> 23
                    }
                }
            }

            // Add camo detection
            towerModel.GetDescendants<FilterInvisibleModel>().ForEach(model => model.isActive = false);
        }
    }

    public class LiteratureFan3 : ModUpgrade<MonkeyStudentTower>
    {
        public override int Path => 2;
        public override int Tier => 3;
        public override int Cost => 1400;
        public override string DisplayName => "Reading Enthusiast";
        public override string Description => "Books deal massive damage with large explosions.";

        public override void ApplyUpgrade(TowerModel towerModel)
        {
            foreach (var weapon in towerModel.GetAttackModel().weapons)
            {
                weapon.Rate *= 0.75f; // Faster attack speed
                weapon.projectile.GetDamageModel().damage += 4; // Overall damage boost

                if (weapon.projectile.id == "BookProjectile")
                {
                    weapon.projectile.GetDamageModel().damage += 8; // 10 -> 18 total
                    weapon.projectile.pierce += 5; // 3 -> 8

                    // Large explosion
                    var explosion = weapon.projectile.GetBehavior<CreateProjectileOnContactModel>();
                    if (explosion != null)
                    {
                        explosion.projectile.GetDamageModel().damage += 10; // 5 -> 15
                        explosion.projectile.radius += 15f; // 23 -> 38
                    }
                }
            }
        }
    }

    public class LiteratureFan4 : ModUpgrade<MonkeyStudentTower>
    {
        public override int Path => 2;
        public override int Tier => 4;
        public override int Cost => 6000;
        public override string DisplayName => "Literature Club Member";
        public override string Description => "Extremely fast attack speed, books create devastating explosions.";

        public override void ApplyUpgrade(TowerModel towerModel)
        {
            foreach (var weapon in towerModel.GetAttackModel().weapons)
            {
                weapon.Rate *= 0.5f; // Very fast attack speed
                weapon.projectile.GetDamageModel().damage += 12; // Big damage boost

                if (weapon.projectile.id == "BookProjectile")
                {
                    weapon.projectile.GetDamageModel().damage += 25; // 30 -> 55 total
                    weapon.projectile.pierce += 10; // 8 -> 18

                    // Devastating explosion
                    var explosion = weapon.projectile.GetBehavior<CreateProjectileOnContactModel>();
                    if (explosion != null)
                    {
                        explosion.projectile.GetDamageModel().damage += 30; // 15 -> 45
                        explosion.projectile.radius += 25f; // 38 -> 63
                    }
                }
            }
        }
    }

    public class LiteratureFan5 : ModUpgrade<MonkeyStudentTower>
    {
        public override int Path => 2;
        public override int Tier => 5;
        public override int Cost => 40000;
        public override string DisplayName => "Literature Club President";
        public override string Description => "Extreme attack speed and damage. Books create screen-clearing explosions.";

        public override void ApplyUpgrade(TowerModel towerModel)
        {
            foreach (var weapon in towerModel.GetAttackModel().weapons)
            {
                weapon.Rate *= 0.25f; // 4x attack speed
                weapon.projectile.GetDamageModel().damage += 40; // Massive damage boost

                if (weapon.projectile.id == "BookProjectile")
                {
                    weapon.projectile.GetDamageModel().damage += 80; // 95 -> 175 total
                    weapon.projectile.pierce += 25; // 18 -> 43

                    // Screen-clearing explosion
                    var explosion = weapon.projectile.GetBehavior<CreateProjectileOnContactModel>();
                    if (explosion != null)
                    {
                        explosion.projectile.GetDamageModel().damage += 100; // 45 -> 145
                        explosion.projectile.radius += 50f; // 63 -> 113
                    }
                }
            }
        }
    }
}