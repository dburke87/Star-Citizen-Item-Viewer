﻿using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Star_Citizen_Item_Viewer.Classes
{
    public class Weapon : Item
    {
        public decimal Firerate { get; set; }

        public int MaximumTemperature { get; set; }
        public decimal HeatPerShot { get; set; }
        public decimal HeatPerSecond
        {
            get
            {
                return HeatPerShot * Firerate;
            }
        }
        public decimal HeatUptime
        {
            get
            {
                return MaximumTemperature / HeatPerSecond;
            }
        }


        public int PowerBase { get; set; }
        public int PowerDraw { get; set; }
        public int PowerPerShot
        {
            get
            {
                return PowerBase + PowerDraw;
            }
        }
        

        // Ammo
        public decimal Lifetime { get; set; }
        public int Speed { get; set; }
        public decimal DamageBiochemical {get;set;}
        public decimal DamageDistortion { get; set; }
        public decimal DamageEnergy { get; set; }
        public decimal DamagePhysical { get; set; }
        public decimal DamageThermal { get; set; }
        public decimal DamageTotal {
            get
            {
                return DamageBiochemical + DamageDistortion + DamageEnergy + DamagePhysical + DamageThermal;
            }
        }
        public decimal DamagePerSecond
        {
            get
            {
                return DamageTotal * Firerate;
            }
        }
        public int MaxRange
        {
            get
            {
                return Convert.ToInt32(Speed * Lifetime);
            }
        }
        public decimal DamagePerPower
        {
            get
            {
                return DamagePerSecond / (PowerBase + PowerDraw);
            }
        }
        public decimal DamagePerHeat
        {
            get
            {
                return DamageTotal / HeatPerShot;
            }
        }

        public decimal MaxSpread { get; set; }
        public decimal InitialSpread { get; set; }
        public decimal SpreadGrowth { get; set; }
        public decimal SpreadDecay { get; set; }
        public decimal SpreadPerSecond
        {
            get
            {
                return (Firerate * SpreadGrowth) - SpreadDecay > 0 ? (Firerate * SpreadGrowth) - SpreadDecay : 0;
            }
        }
        public decimal TimeUntilMaxSpread
        {
            get
            {
                return SpreadPerSecond != 0 && (MaxSpread-InitialSpread) / SpreadPerSecond >= 0 ? (MaxSpread - InitialSpread) / SpreadPerSecond : 999;
            }
        }

        public Weapon(dynamic Json, string File)
        {
            Id = Json.__ref;
            Name = string.IsNullOrEmpty((string)Json.name_local) ? File : Json.name_local;
                
            Size = Json.size;
            Filename = File;

            Firerate = Json.Components.SCItemWeaponComponentParams.fire.fireRate / 60M;
            //HeatPerShot = Json.Components.SCItemWeaponComponentParams.fire.heatPerShot;
            //MaximumTemperature = Json.Components.EntityComponentHeatConnection.MaximumTemperature;

            PowerBase = Json.Components.EntityComponentPowerConnection.PowerBase;
            PowerDraw = Json.Components.EntityComponentPowerConnection.PowerDraw;

            MaxSpread = Json.Components.SCItemWeaponComponentParams.fire.launchParams.SProjectileLauncher.spreadParams.max;
            InitialSpread = Json.Components.SCItemWeaponComponentParams.fire.launchParams.SProjectileLauncher.spreadParams.firstAttack;
            SpreadGrowth = Json.Components.SCItemWeaponComponentParams.fire.launchParams.SProjectileLauncher.spreadParams.attack;
            SpreadDecay = Json.Components.SCItemWeaponComponentParams.fire.launchParams.SProjectileLauncher.spreadParams.decay;

            Lifetime = Json.ammo.lifetime;
            Speed = Json.ammo.speed;
            DamageBiochemical = Json.ammo.bullet.damage.DamageInfo.DamageBiochemical;
            DamageDistortion = Json.ammo.bullet.damage.DamageInfo.DamageDistortion;
            DamageEnergy = Json.ammo.bullet.damage.DamageInfo.DamageEnergy;
            DamagePhysical = Json.ammo.bullet.damage.DamageInfo.DamagePhysical;
            DamageThermal = Json.ammo.bullet.damage.DamageInfo.DamageThermal;

            // Explosive ammo
            DamageBiochemical += Json.ammo.bullet.detonation != null ? (int)Json.ammo.bullet.detonation.explosion.damage.DamageInfo.DamageBiochemical : 0;
            DamageDistortion += Json.ammo.bullet.detonation != null ? (int)Json.ammo.bullet.detonation.explosion.damage.DamageInfo.DamageDistortion : 0;
            DamageEnergy += Json.ammo.bullet.detonation != null ? (int)Json.ammo.bullet.detonation.explosion.damage.DamageInfo.DamageEnergy : 0;
            DamagePhysical += Json.ammo.bullet.detonation != null ? (int)Json.ammo.bullet.detonation.explosion.damage.DamageInfo.DamagePhysical : 0;
            DamageThermal += Json.ammo.bullet.detonation != null ? (int)Json.ammo.bullet.detonation.explosion.damage.DamageInfo.DamageThermal : 0;
        }

        public static Dictionary<string,object> parseAll(string filePath)
        {
            ConcurrentDictionary<string, object> output = new ConcurrentDictionary<string, object>();
            Parallel.ForEach(Directory.GetFiles(filePath), new ParallelOptions { MaxDegreeOfParallelism = 5 }, path =>
            {
                try
                {
                    string raw = File.ReadAllText(path).Replace("@", "");
                    dynamic json = JsonConvert.DeserializeObject(raw);
                    Weapon w = new Weapon(json, path.Replace(filePath + "\\", "").Replace(".json", ""));
                    output.TryAdd(w.Id, w);
                }
                catch (Exception ex) { }
            });
            return new Dictionary<string, object>(output);
        }

        public static Column[] GetColumns()
        {
            return new Column[] {
                new Column("Id", "Id", false, false, "", false),
                new Column("Name", "Name", false),
                new Column("Size", "Size", false),
                new Column("Alpha Damage", "DamageTotal", true, true),
                new Column("Damage Per Second", "DamagePerSecond", true, true, "N2"),
                new Column("Firerate", "Firerate", true, true, "N2"),
                new Column("Biochemical Damage", "DamageBiochemical", true, true),
                new Column("Distortion Damage", "DamageDistortion", true, true),
                new Column("Energy Damage", "DamageEnergy", true, true),
                new Column("Physical Damage", "DamagePhysical", true, true),
                new Column("Thermal Damage", "DamageThermal", true, true),
                //new Column("Damage Per Power", "DamagePerPower", true, true, "N2"),
                //new Column("Damage Per Heat", "DamagePerHeat", true, true, "N2"),
                //new Column("Power Per Shot", "PowerPerShot", true, false),
                //new Column("Heat Per Shot", "HeatPerShot", true, false),
                //new Column("Heat Per Second", "HeatPerSecond", true, false, "N2"),
                //new Column("Heat Uptime", "HeatUptime", true, true, "N2"),
                new Column("Projectile Velocity", "Speed", true, true),
                new Column("Max Range", "MaxRange", true, true),
                new Column("Max Spread", "MaxSpread", true, false, "N3"),
                new Column("Initial Spread", "InitialSpread", true, false, "N3"),
                new Column("Spread Growth", "SpreadGrowth", true, false, "N3"),
                new Column("Spread Decay", "SpreadDecay", true, true, "N3"),
                new Column("Spread Per Second", "SpreadPerSecond", true, false, "N3"),
                new Column("Time Until Max Spread", "TimeUntilMaxSpread", true, true, "N3"),
                new Column("Score", null, true, true, "N2", false),
            };
        }

        public static List<string[]> GetDownloadInfo(string FilePath)
        {
            return new List<string[]>
            {
                new string[] { "http://starcitizendb.com/api/components/df/WeaponGun", FilePath + "\\weapons" }
                //,new string[] {"http://starcitizendb.com/api/components/df/PowerPlant", FilePath + "\\power plants"}
                //,new string[] {"http://starcitizendb.com/api/components/df/Cooler", FilePath + "\\coolers"}
                //,new string[] {"http://starcitizendb.com/api/components/df/Shield", FilePath + "\\shields"}
            };
        }
    }
}
