using RDRNetworkShared;

namespace ResuMPServer
{
    public class Pickup : Entity
    {
        internal Pickup(API father, NetHandle handle) : base(father, handle)
        {
        }

        #region Properties

        public int Amount
        {
            get => ((PickupProperties)Program.ServerInstance.NetEntityHandler.ToDict()[Value]).Amount;
        }

        public int CustomModel
        {
            get => GetPickupCustomModel();
        }

        public bool PickedUp
        {
            get => GetPickupPickedUp();
        }

        #endregion

        #region Methods

        public void Respawn()
        {
            var pic = Program.ServerInstance.NetEntityHandler.NetToProp<PickupProperties>(Value);
            if (pic != null && pic.PickedUp)
            {
                Program.ServerInstance.PickupManager.RespawnPickup(Value, pic);
            }
        }

        public int GetPickupCustomModel()
        {
            PickupProperties p;
            if ((p = Program.ServerInstance.NetEntityHandler.NetToProp<PickupProperties>(Value)) != null)
            {
                return p.CustomModel;
            }

            return 0;
        }

        public bool GetPickupPickedUp()
        {
            PickupProperties p;
            if ((p = Program.ServerInstance.NetEntityHandler.NetToProp<PickupProperties>(Value)) != null)
            {
                return p.PickedUp;
            }

            return false;
        }
        #endregion
    }
}