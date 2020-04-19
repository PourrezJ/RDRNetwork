using ResuMPServer.Constant;
using RDRNetworkShared;
using System;

namespace ResuMPServer
{
    public class Vehicle : Entity
    {
        internal Vehicle(API father, NetHandle handle) : base(father, handle)
        {
        }

        #region Properties

        public int PrimaryColor
        {
            get { return GetPrimaryColor(); }
            set { SetPrimaryColor(value); }
        }

        public int SecondaryColor
        {
            get { return GetSecondaryColor(); }
            set { SetSecondaryColor(value); }
        }

        public Color CustomPrimaryColor
        {
            get { return GetCustomPrimaryColor(); }
            set { SetCustomPrimaryColor(value); }
        }

        public Color CustomSecondaryColor
        {
            get { return GetCustomSecondaryColor(); }
            set { SetCustomSecondaryColor(value); }
        }

        public float Health
        {
            get { return GetHealth(); }
            set { SetHealth(value); }
        }

        public int Livery
        {
            get { return GetLivery(); }
            set { SetLivery(value); }
        }

        public Vehicle Trailer
        {
            get { return new Vehicle(Base, GetTrailer()); }
        }

        public Vehicle TraileredBy
        {
            get { return new Vehicle(Base, GetTraileredBy()); }
        }

        public bool Siren
        {
            get { return GetSirenState(); }
        }

        public string NumberPlate
        {
            get { return GetNumberPlate(); }
            set { SetNumberPlate(value); }
        }

        public bool SpecialLight
        {
            get { return GetSpecialLightStatus(); }
            set { SetVehicleSpecialLightStatus(value); }
        }

        public bool BulletproofTyres
        {
            get { return GetMod(61) == 0; }
            set { SetMod(61, value ? 0 : 1); }
        }

        public int NumberPlateStyle
        {
            get { return GetMod(62); ; }
            set { SetMod(62, value); }
        }

        public int PearlescentColor
        {
            get { return GetMod(63); }
            set { SetMod(63, value); }
        }

        public int WheelColor
        {
            get { return GetMod(64); }
            set { SetMod(64, value); }
        }

        public int WheelType
        {
            get { return GetMod(65); }
            set { SetMod(65, value); }
        }

        public bool EngineStatus
        {
            get { return GetEngineStatus(); }
            set { SetEngineStatus(value); }
        }

        public Color TyreSmokeColor
        {
            get { return GetTyreSmokeColor(); }
            set { SetTyreSmokeColor(value.red, value.green, value.blue); }
        }

        public Color ModColor1
        {
            get { return GetModColor1(); }
            set { SetModColor1(value.red, value.green, value.blue); }
        }

        public Color ModColor2
        {
            get { return GetModColor2(); }
            set { SetModColor2(value.red, value.green, value.blue); }
        }

        public int WindowTint
        {
            get { return GetMod(69); }
            set { SetWindowTint(value); }
        }

        public float EnginePowerMultiplier
        {
            get { return GetEnginePowerMultiplier(); }
            set { SetEnginePowerMultiplier(value); }
        }

        public float EngineTorqueMultiplier
        {
            get { return GetEngineTorqueMultiplier(); }
            set { SetEngineTorqueMultiplier(value); }
        }

        public Color NeonColor
        {
            get { return GetNeonColor(); }
            set { SetNeonColor(value.red, value.green, value.blue); }
        }

        public int DashboardColor
        {
            get { return GetDashboardColor(); }
            set { SetDashboardColor(value); }
        }

        public int TrimColor
        {
            get { return GetTrimColor(); }
            set { SetTrimColor(value); }
        }

        public string DisplayName
        {
            get { return ConstantVehicleDataOrganizer.Get(Model).DisplayName; }
        }

        public Client[] Occupants
        {
            get { return GetOccupants(); }
        }

        public Client Driver
        {
            get { return GetDriver(); }
        }

        public float MaxOccupants
        {
            get { return ConstantVehicleDataOrganizer.Get(Model).MaxNumberOfPassengers; }
        }

        public float MaxSpeed
        {
            get { return ConstantVehicleDataOrganizer.Get(Model).MaxSpeed; }
        }

        public float MaxAcceleration
        {
            get { return ConstantVehicleDataOrganizer.Get(Model).MaxAcceleration; }
        }

        public float MaxTraction
        {
            get { return ConstantVehicleDataOrganizer.Get(Model).MaxTraction; }
        }

        public float MaxBraking
        {
            get { return ConstantVehicleDataOrganizer.Get(Model).MaxBraking; }
        }

        public bool Locked
        {
            get { return Base.GetVehicleLocked(this); }
            set { Base.SetVehicleLocked(this, value); }
        }

        public int Class
        {
            get { return ConstantVehicleDataOrganizer.Get(Model).VehicleClass; }
        }

        public string ClassName
        {
            get { return GetClassName(); }
        }

        #endregion

        #region Methods

        public void Repair()
        {
            if (DoesEntityExist())
            {
                ((VehicleProperties)Program.ServerInstance.NetEntityHandler.ToDict()[this.Value]).Health = 1000f;
                var delta = new Delta_VehicleProperties();
                delta.Health = 1000f;
                GameServer.UpdateEntityInfo(this.Value, EntityType.Vehicle, delta);
            }

            //Program.ServerInstance.SendNativeCallToAllPlayers(0x115722B1B9C14C1C, this);
            //Program.ServerInstance.SendNativeCallToAllPlayers(0x953DA1E1B12C0491, this);
        }

        public void PopTyre(int tyre)
        {
            if (DoesEntityExist())
            {
                Program.ServerInstance.NetEntityHandler.NetToProp<VehicleProperties>(this.Value).Tires |= (byte)(1 << tyre);
               // Base.SendNativeToAllPlayers(0xEC6A202EE4960385, this, tyre, false, 1000);

                var delta = new Delta_VehicleProperties();
                delta.Tires = Program.ServerInstance.NetEntityHandler.NetToProp<VehicleProperties>(this.Value).Tires;
                GameServer.UpdateEntityInfo(this.Value, EntityType.Vehicle, delta);
            }
        }

        public void FixTyre(int tyre)
        {
            if (DoesEntityExist())
            {
                Program.ServerInstance.NetEntityHandler.NetToProp<VehicleProperties>(this.Value).Tires &= (byte)~(1 << tyre);
               // Base.SendNativeToAllPlayers(0x6E13FC662B882D1D, this, tyre);

                var delta = new Delta_VehicleProperties();
                delta.Tires = Program.ServerInstance.NetEntityHandler.NetToProp<VehicleProperties>(this.Value).Tires;
                GameServer.UpdateEntityInfo(this.Value, EntityType.Vehicle, delta);
            }
        }

        public bool IsTyrePopped(int tyre)
        {
            if (DoesEntityExist())
            {
                return (Program.ServerInstance.NetEntityHandler.NetToProp<VehicleProperties>(this.Value).Tires &
                        1 << tyre) != 0;
            }

            return false;
        }

        public void BreakDoor(int door)
        {
            if (DoesEntityExist())
            {
                if (Program.ServerInstance.NetEntityHandler.NetToProp<VehicleProperties>(this.Value).DamageModel ==
                    null)
                {
                    Program.ServerInstance.NetEntityHandler.NetToProp<VehicleProperties>(this.Value).DamageModel = new VehicleDamageModel();
                    Program.ServerInstance.NetEntityHandler.NetToProp<VehicleProperties>(this.Value)
                        .DamageModel.BrokenDoors = 0;
                }

                Program.ServerInstance.NetEntityHandler.NetToProp<VehicleProperties>(this.Value).DamageModel.BrokenDoors |= (byte)(1 << door);


                var delta = new Delta_VehicleProperties();
                delta.DamageModel = Program.ServerInstance.NetEntityHandler.NetToProp<VehicleProperties>(this.Value).DamageModel;
                GameServer.UpdateEntityInfo(this.Value, EntityType.Vehicle, delta);
            }
        }

        public void FixDoor(int door)
        {
            if (DoesEntityExist())
            {
                if (Program.ServerInstance.NetEntityHandler.NetToProp<VehicleProperties>(this.Value).DamageModel ==
                    null)
                {
                    Program.ServerInstance.NetEntityHandler.NetToProp<VehicleProperties>(this.Value).DamageModel = new VehicleDamageModel();
                    Program.ServerInstance.NetEntityHandler.NetToProp<VehicleProperties>(this.Value)
                        .DamageModel.BrokenDoors = 0;
                }

                Program.ServerInstance.NetEntityHandler.NetToProp<VehicleProperties>(this.Value).DamageModel.BrokenDoors &= (byte)~(1 << door);

                var delta = new Delta_VehicleProperties();
                delta.DamageModel = Program.ServerInstance.NetEntityHandler.NetToProp<VehicleProperties>(this.Value).DamageModel;
                GameServer.UpdateEntityInfo(this.Value, EntityType.Vehicle, delta);
            }
        }

        public bool IsDoorBroken(int door)
        {
            if (DoesEntityExist())
            {
                if (Program.ServerInstance.NetEntityHandler.NetToProp<VehicleProperties>(this.Value).DamageModel ==
                    null) return false;

                return (Program.ServerInstance.NetEntityHandler.NetToProp<VehicleProperties>(this.Value).DamageModel.BrokenDoors & 1 << door) != 0;
            }

            return false;
        }

        public void OpenDoor(int door)
        {
            if (DoesEntityExist())
            {
                var prop = Program.ServerInstance.NetEntityHandler.NetToProp<VehicleProperties>(this.Value);

                prop.Doors |= (byte)(1 << door);

                Base.SendNativeToAllPlayers(Hash.SET_VEHICLE_DOOR_OPEN, this, door, false, false);
            }
        }

        public void CloseDoor(int door)
        {
            if (DoesEntityExist())
            {
                var prop = Program.ServerInstance.NetEntityHandler.NetToProp<VehicleProperties>(this.Value);

                prop.Doors &= (byte)~(1 << door);

                Base.SendNativeToAllPlayers(Hash.SET_VEHICLE_DOOR_SHUT, this, door, false);
            }
        }

        public bool IsDoorOpen(int door)
        {
            if (DoesEntityExist())
            {
                var prop = Program.ServerInstance.NetEntityHandler.NetToProp<VehicleProperties>(this.Value);

                return (prop.Doors & (1 << door)) != 0;
            }

            return false;
        }

        public void BreakWindow(int window)
        {
            if (DoesEntityExist())
            {
                if (Program.ServerInstance.NetEntityHandler.NetToProp<VehicleProperties>(this.Value).DamageModel ==
                    null)
                {
                    Program.ServerInstance.NetEntityHandler.NetToProp<VehicleProperties>(this.Value).DamageModel = new VehicleDamageModel();
                    Program.ServerInstance.NetEntityHandler.NetToProp<VehicleProperties>(this.Value)
                        .DamageModel.BrokenWindows = 0;
                }

                Program.ServerInstance.NetEntityHandler.NetToProp<VehicleProperties>(this.Value)
                    .DamageModel.BrokenDoors |= (byte)(1 << window);
               // Base.SendNativeToAllPlayers(0x9E5B5E4D2CCD2259, this, window);

                var delta = new Delta_VehicleProperties();
                delta.DamageModel = Program.ServerInstance.NetEntityHandler.NetToProp<VehicleProperties>(this.Value).DamageModel;
                GameServer.UpdateEntityInfo(this.Value, EntityType.Vehicle, delta);
            }
        }

        public void FixWindow(int window)
        {
            if (DoesEntityExist())
            {
                if (Program.ServerInstance.NetEntityHandler.NetToProp<VehicleProperties>(this.Value).DamageModel ==
                    null)
                {
                    Program.ServerInstance.NetEntityHandler.NetToProp<VehicleProperties>(this.Value).DamageModel = new VehicleDamageModel();
                    Program.ServerInstance.NetEntityHandler.NetToProp<VehicleProperties>(this.Value)
                        .DamageModel.BrokenWindows = 0;
                }

                Program.ServerInstance.NetEntityHandler.NetToProp<VehicleProperties>(this.Value)
                    .DamageModel.BrokenDoors &= (byte)~(1 << window);
               // Base.SendNativeToAllPlayers(0x772282EBEB95E682, this, window);

                var delta = new Delta_VehicleProperties();
                delta.DamageModel = Program.ServerInstance.NetEntityHandler.NetToProp<VehicleProperties>(this.Value).DamageModel;
                GameServer.UpdateEntityInfo(this.Value, EntityType.Vehicle, delta);
            }
        }

        public bool IsWindowBroken(int window)
        {
            if (DoesEntityExist())
            {
                if (Program.ServerInstance.NetEntityHandler.NetToProp<VehicleProperties>(this.Value).DamageModel ==
                    null) return false;

                return (Program.ServerInstance.NetEntityHandler.NetToProp<VehicleProperties>(this.Value).DamageModel.BrokenWindows & 1 << window) != 0;
            }

            return false;
        }

        public void SetExtra(int extra, bool enabled)
        {
            if (DoesEntityExist())
            {
                if (enabled)
                {
                    ((VehicleProperties)Program.ServerInstance.NetEntityHandler.ToDict()[this.Value])
                        .VehicleComponents |= (short)(1 << extra);
                }
                else
                {
                    ((VehicleProperties)Program.ServerInstance.NetEntityHandler.ToDict()[this.Value])
                        .VehicleComponents &= (short)(~(1 << extra));
                }

                //Base.SendNativeToAllPlayers(0x7EE3A3C5E4A40CC9, new EntityArgument(this.Value), extra, enabled ? 0 : -1);

                var delta = new Delta_VehicleProperties();
                delta.VehicleComponents = ((VehicleProperties)Program.ServerInstance.NetEntityHandler.ToDict()[this.Value])
                        .VehicleComponents;
                GameServer.UpdateEntityInfo(this.Value, EntityType.Vehicle, delta);
            }
        }

        public bool GetExtra(int extra)
        {
            if (DoesEntityExist())
            {
                return (((VehicleProperties)Program.ServerInstance.NetEntityHandler.ToDict()[this.Value])
                    .VehicleComponents & 1 << extra) != 0;
            }
            return false;
        }

        public void SetMod(int slot, int mod)
        {
            if (Program.ServerInstance.NetEntityHandler.ToDict().ContainsKey(this.Value))
            {
                ((VehicleProperties)Program.ServerInstance.NetEntityHandler.ToDict()[this.Value]).Mods.Set((byte)slot, mod);

                var delta = new Delta_VehicleProperties();

                delta.Mods = ((VehicleProperties)Program.ServerInstance.NetEntityHandler.ToDict()[this.Value]).Mods;
                GameServer.UpdateEntityInfo(this.Value, EntityType.Vehicle, delta);
            }
        }

        public int GetMod(int slot)
        {
            if (DoesEntityExist())
            {
                return ((VehicleProperties)Program.ServerInstance.NetEntityHandler.ToDict()[this.Value]).Mods.Get((byte)slot);
            }

            return 0;
        }

        public void RemoveMod(int slot)
        {
            if (DoesEntityExist())
            {
                ((VehicleProperties)Program.ServerInstance.NetEntityHandler.ToDict()[this.Value]).Mods.Remove((byte)slot);
            }

            //Program.ServerInstance.SendNativeCallToAllPlayers(false, 0x92D619E420858204, vehicle, modType);

            var delta = new Delta_VehicleProperties();
            delta.Mods = ((VehicleProperties)Program.ServerInstance.NetEntityHandler.ToDict()[this.Value]).Mods;
            GameServer.UpdateEntityInfo(this.Value, EntityType.Vehicle, delta);
        }

        public void SetNeons(int slot, bool turnedon)
        {
            var currentState = GetMod(72);

            if (turnedon)
                SetMod(72, currentState | 1 << slot);
            else
                SetMod(72, currentState & ~(1 << slot));
        }

        public bool GetNeons(int slot)
        {
            return (GetMod(72) & (1 << slot)) != 0;
        }

        public int GetPrimaryColor()
        {
            if (DoesEntityExist())
            {
                return ((VehicleProperties)Program.ServerInstance.NetEntityHandler.ToDict()[this.Value]).PrimaryColor;
            }
            return 0;
        }

        public void SetPrimaryColor(int color)
        {
            if (DoesEntityExist())
            {
                ((VehicleProperties)Program.ServerInstance.NetEntityHandler.ToDict()[this.Value]).PrimaryColor = color;
                //Program.ServerInstance.SendNativeCallToAllPlayers(false, 0x55E1D2758F34E437, vehicle);
                //Program.ServerInstance.SendNativeCallToAllPlayers(false, 0x4F1D4BE3A7F24601, vehicle, color, ((VehicleProperties)Program.ServerInstance.NetEntityHandler.ToDict()[vehicle.Value]).SecondaryColor);

                var delta = new Delta_VehicleProperties();
                delta.PrimaryColor = color;
                GameServer.UpdateEntityInfo(this.Value, EntityType.Vehicle, delta);
            }
        }

        public int GetSecondaryColor()
        {
            if (DoesEntityExist())
            {
                return ((VehicleProperties)Program.ServerInstance.NetEntityHandler.ToDict()[this.Value]).SecondaryColor;
            }
            return 0;
        }

        public void SetSecondaryColor(int color)
        {
            if (DoesEntityExist())
            {
                ((VehicleProperties)Program.ServerInstance.NetEntityHandler.ToDict()[this.Value]).SecondaryColor = color;
                //Program.ServerInstance.SendNativeCallToAllPlayers(false, 0x5FFBDEEC3E8E2009, vehicle);
                //Program.ServerInstance.SendNativeCallToAllPlayers(false, 0x4F1D4BE3A7F24601, vehicle, ((VehicleProperties)Program.ServerInstance.NetEntityHandler.ToDict()[vehicle.Value]).PrimaryColor, color);

                var delta = new Delta_VehicleProperties();
                delta.SecondaryColor = color;
                GameServer.UpdateEntityInfo(this.Value, EntityType.Vehicle, delta);
            }
        }

        public Color GetCustomPrimaryColor()
        {
            byte red = 0;
            byte green = 0;
            byte blue = 0;
            byte a;

            if (DoesEntityExist())
            {
                Extensions.ToArgb(((VehicleProperties)Program.ServerInstance.NetEntityHandler.ToDict()[this.Value]).PrimaryColor,
                    out a, out red, out green, out blue);
            }

            return new Color(red, green, blue, 255);
        }


        public void SetCustomPrimaryColor(Color color)
        {
            if (DoesEntityExist())
            {
                ((VehicleProperties)Program.ServerInstance.NetEntityHandler.ToDict()[this.Value]).PrimaryColor = Extensions.FromArgb((byte)color.alpha, (byte)color.red, (byte)color.green, (byte)color.blue);
                //Program.ServerInstance.SendNativeCallToAllPlayers(false, 0x7141766F91D15BEA, vehicle, red, green, blue);

                var delta = new Delta_VehicleProperties();
                delta.PrimaryColor = Extensions.FromArgb((byte)color.alpha, (byte)color.red, (byte)color.green, (byte)color.blue);
                GameServer.UpdateEntityInfo(this.Value, EntityType.Vehicle, delta);
            }
        }

        public Color GetCustomSecondaryColor()
        {
            byte red = 0;
            byte green = 0;
            byte blue = 0;
            byte a = 255;

            if (DoesEntityExist())
            {
                Extensions.ToArgb(((VehicleProperties)Program.ServerInstance.NetEntityHandler.ToDict()[this.Value]).SecondaryColor,
                    out a, out red, out green, out blue);
            }

            return new Color(red, green, blue, a);
        }

        public void SetCustomSecondaryColor(Color color)
        {
            if (DoesEntityExist())
            {
                ((VehicleProperties)Program.ServerInstance.NetEntityHandler.ToDict()[this.Value]).SecondaryColor = Extensions.FromArgb((byte)color.alpha, (byte)color.red, (byte)color.green, (byte)color.blue);
                
                //Program.ServerInstance.SendNativeCallToAllPlayers(false, 0x36CED73BFED89754, vehicle, red, green, blue);

                var delta = new Delta_VehicleProperties();
                delta.SecondaryColor = Extensions.FromArgb((byte)color.alpha, (byte)color.red, (byte)color.green, (byte)color.blue);
                GameServer.UpdateEntityInfo(this.Value, EntityType.Vehicle, delta);
            }
        }

        public float GetHealth()
        {
            if (DoesEntityExist())
            {
                return ((VehicleProperties)Program.ServerInstance.NetEntityHandler.ToDict()[this.Value]).Health;
            }
            return 0f;
        }


        public void SetHealth(float health)
        {
            if (Program.ServerInstance.NetEntityHandler.ToDict().ContainsKey(this.Value))
            {
                ((VehicleProperties)Program.ServerInstance.NetEntityHandler.ToDict()[this.Value]).Health = health;

                var delta = new Delta_VehicleProperties();
                delta.Health = health;
                GameServer.UpdateEntityInfo(this.Value, EntityType.Vehicle, delta);
            }

            //Program.ServerInstance.SendNativeCallToAllPlayers(false, 0x45F6D8EEF34ABEF1, vehicle, health);
        }

        public int GetLivery()
        {
            if (DoesEntityExist())
            {
                return ((VehicleProperties)Program.ServerInstance.NetEntityHandler.ToDict()[this.Value]).Livery;
            }
            return 0;
        }

        public void SetLivery(int livery)
        {
            if (DoesEntityExist())
            {
                ((VehicleProperties)Program.ServerInstance.NetEntityHandler.ToDict()[this.Value]).Livery = livery;
                //Program.ServerInstance.SendNativeCallToAllPlayers(false, 0x60BF608F1B8CD1B6, new EntityArgument(this.Value), livery);

                var delta = new Delta_VehicleProperties();
                delta.Livery = livery;
                GameServer.UpdateEntityInfo(this.Value, EntityType.Vehicle, delta);
            }
        }

        public NetHandle GetTrailer()
        {
            if (DoesEntityExist())
            {
                return
                    new NetHandle(((VehicleProperties)Program.ServerInstance.NetEntityHandler.ToDict()[this.Value]).Trailer);
            }

            return new NetHandle();
        }

        public NetHandle GetTraileredBy()
        {
            if (DoesEntityExist())
            {
                return
                    new NetHandle(((VehicleProperties)Program.ServerInstance.NetEntityHandler.ToDict()[this.Value]).TraileredBy);
            }

            return new NetHandle();
        }

        public bool GetSirenState()
        {
            if (DoesEntityExist())
            {
                return Program.ServerInstance.NetEntityHandler.NetToProp<VehicleProperties>(this.Value).Siren;
            }

            return false;
        }

        public string GetNumberPlate()
        {
            if (DoesEntityExist())
            {
                return ((VehicleProperties)Program.ServerInstance.NetEntityHandler.ToDict()[this.Value]).NumberPlate;
            }
            return null;
        }


        public void SetNumberPlate(string plate)
        {
            if (DoesEntityExist())
            {
                ((VehicleProperties)Program.ServerInstance.NetEntityHandler.ToDict()[this.Value]).NumberPlate = plate;
                //Program.ServerInstance.SendNativeCallToAllPlayers(false, 0x95A88F0B409CDA47, new EntityArgument(vehicle.Value), plate);

                var delta = new Delta_VehicleProperties();
                delta.NumberPlate = plate;
                GameServer.UpdateEntityInfo(this.Value, EntityType.Vehicle, delta);
            }
        }

        public bool GetEngineStatus()
        {
            if (DoesEntityExist())
            {
                return !PacketOptimization.CheckBit(Program.ServerInstance.NetEntityHandler.ToDict()[this.Value].Flag, EntityFlag.EngineOff);
            }

            return false;
        }

        public void SetEngineStatus(bool turnedOn)
        {
            if (DoesEntityExist())
            {
                //Program.ServerInstance.SendNativeCallToAllPlayers(false, 0x2497C4717C8B881E, vehicle, turnedOn, true, true);
                //Program.ServerInstance.SendNativeCallToAllPlayers(false, 0x8ABA6AF54B942B95, vehicle, !turnedOn);

                if (turnedOn)
                {
                    Program.ServerInstance.NetEntityHandler.ToDict()[this.Value].Flag = (byte)
                        PacketOptimization.ResetBit(Program.ServerInstance.NetEntityHandler.ToDict()[this.Value].Flag,
                            EntityFlag.EngineOff);
                }
                else
                {
                    Program.ServerInstance.NetEntityHandler.ToDict()[this.Value].Flag = (byte)
                        PacketOptimization.SetBit(Program.ServerInstance.NetEntityHandler.ToDict()[this.Value].Flag,
                            EntityFlag.EngineOff);

                }

                var delta = new Delta_EntityProperties();
                delta.Flag = Program.ServerInstance.NetEntityHandler.ToDict()[this.Value].Flag;
                GameServer.UpdateEntityInfo(this.Value, EntityType.Prop, delta);
            } 
        }

        public bool GetVehicleStatus()
        {
            if (DoesEntityExist())
            {
                return !PacketOptimization.CheckBit(Program.ServerInstance.NetEntityHandler.ToDict()[this.Value].Flag, EntityFlag.EngineOff);
            }

            return false;
        }

        public void SetVehicleSpecialLightStatus(bool turnedOn)
        {
            if (DoesEntityExist())
            {
                //Program.ServerInstance.SendNativeCallToAllPlayers(false, 0x14E85C5EE7A4D542, vehicle, turnedOn, true);
                //Program.ServerInstance.SendNativeCallToAllPlayers(false, 0x598803E85E8448D9, vehicle, turnedOn);

                if (turnedOn)
                {
                    Program.ServerInstance.NetEntityHandler.ToDict()[this.Value].Flag = (byte)
                        PacketOptimization.SetBit(Program.ServerInstance.NetEntityHandler.ToDict()[this.Value].Flag,
                            EntityFlag.SpecialLight);
                }
                else
                {
                    Program.ServerInstance.NetEntityHandler.ToDict()[this.Value].Flag = (byte)
                        PacketOptimization.ResetBit(Program.ServerInstance.NetEntityHandler.ToDict()[this.Value].Flag,
                            EntityFlag.SpecialLight);

                }

                var delta = new Delta_EntityProperties();
                delta.Flag = Program.ServerInstance.NetEntityHandler.ToDict()[this.Value].Flag;
                GameServer.UpdateEntityInfo(this.Value, EntityType.Prop, delta);
            }
        }

        public Color GetTyreSmokeColor()
        {
            var val = GetMod(68);
            byte a, red, green, blue;
            Extensions.ToArgb(val, out a, out red, out green, out blue);

            return new Color(red, green, blue);
        }

        public void SetTyreSmokeColor(int r, int g, int b)
        {
            SetMod(68, Extensions.FromArgb(255, (byte)r, (byte)g, (byte)b));
        }

        public bool GetSpecialLightStatus()
        {
            if (DoesEntityExist())
            {
                return !PacketOptimization.CheckBit(Program.ServerInstance.NetEntityHandler.ToDict()[this.Value].Flag, EntityFlag.SpecialLight);
            }

            return false;
        }

        public Color GetModColor1()
        {
            var val = GetMod(66);
            byte a, red, green, blue;
            Extensions.ToArgb(val, out a, out red, out green, out blue);
            return new Color(red, green, blue);
        }

        public void SetModColor1(int r, int g, int b)
        {
            SetMod(66, Extensions.FromArgb(0, (byte)r, (byte)g, (byte)b));
        }

        public Color GetModColor2()
        {
            var val = GetMod(67);
            byte a, red, green, blue;
            Extensions.ToArgb(val, out a, out red, out green, out blue);
            return new Color(red, green, blue);
        }

        public void SetModColor2( int r, int g, int b)
        {
            SetMod(67, Extensions.FromArgb(0, (byte)r, (byte)g, (byte)b));
        }

        public void SetWindowTint(int type)
        {
            SetMod(69, type);
        }

        public int GetWindowTint()
        {
            return GetMod(69);
        }

        public void SetEnginePowerMultiplier(float mult)
        {
            SetMod(70, BitConverter.ToInt32(BitConverter.GetBytes(mult), 0));
        }

        public float GetEnginePowerMultiplier()
        {
            return BitConverter.ToSingle(BitConverter.GetBytes(GetMod(70)), 0);
        }

        public float GetEngineTorqueMultiplier()
        {
            return BitConverter.ToSingle(BitConverter.GetBytes(GetMod(71)), 0);
        }

        public void SetEngineTorqueMultiplier(float mult)
        {
            SetMod(71, BitConverter.ToInt32(BitConverter.GetBytes(mult), 0));
        }

        public Color GetNeonColor()
        {
            var val = GetMod(73);
            byte a, red, green, blue;
            Extensions.ToArgb(val, out a, out red, out green, out blue);

            return new Color(red, green, blue);
        }

        public void SetNeonColor(int r, int g, int b)
        {
            SetMod(73, Extensions.FromArgb(0, (byte)r, (byte)g, (byte)b));
        }

        public int GetDashboardColor()
        {
            return GetMod(74);
        }

        public void SetDashboardColor(int type)
        {
            SetMod(74, type);
        }

        public void SetTrimColor(int type)
        {
            SetMod(75, type);
        }

        public int GetTrimColor()
        {
            return GetMod(75);
        }

        public Client[] GetOccupants()
        {
            if (Program.ServerInstance.VehicleOccupants.ContainsKey(this.Value))
            {
                return Program.ServerInstance.VehicleOccupants[this.Value].ToArray();
            }

            return new Client[0];
        }

        public Client GetDriver()
        {
            foreach (Client player in GetOccupants())
            {
                if (API.Shared.GetPlayerVehicleSeat(player) == -1)
                {
                    return player;
                }
            }

            return null;
        }

        public string GetClassName()
        {
            var classId = Model;
            if (classId < 0 || classId >= ConstantVehicleDataOrganizer.VehicleClasses.Length) return "";

            return ConstantVehicleDataOrganizer.VehicleClasses[classId];
        }
        #endregion
    }
}