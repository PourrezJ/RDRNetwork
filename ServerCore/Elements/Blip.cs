using RDRNetworkShared;

namespace ResuMPServer
{
    public class Blip : Entity
    {
        internal Blip(API father, NetHandle handle) : base(father, handle)
        {
        }

        #region Properties

        public override Vector3 Position
        {
            get { return base.Position; }
            set
            {
                Base.SetBlipPosition(Id, value);
            }
        }

        public int Color
        {
            get { return Base.GetBlipColor(Id); }
            set { Base.SetBlipColor(Id, value); }
        }

        public string Name
        {
            get { return Base.GetBlipName(this); }
            set { Base.SetBlipName(this, value); }
        }

        public override int Transparency
        {
            get { return base.Transparency; }
            set { Base.SetBlipTransparency(this, value); }
        }

        public bool ShortRange
        {
            get { return Base.GetBlipShortRange(this); }
            set { Base.SetBlipShortRange(this, value); }
        }

        public int Sprite
        {
            get { return Base.GetBlipSprite(this); }
            set { Base.SetBlipSprite(this, value); }
        }

        public float Scale
        {
            get { return Base.GetBlipScale(this); }
            set { Base.SetBlipScale(this, value); }
        }

        public bool RouteVisible
        {
            get { return Base.GetBlipRouteVisible(this); }
            set { Base.SetBlipRouteVisible(this, value); }
        }

        public int RouteColor
        {
            get { return Base.GetBlipRouteColor(this); }
            set { Base.SetBlipRouteColor(this, value); }
        }

        #endregion

        #region Methods

        #endregion
    }
}