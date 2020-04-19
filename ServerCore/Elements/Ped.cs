using RDRNetworkShared;

namespace ResuMPServer
{
    public class Ped : Entity
    {
        internal Ped(API father, NetHandle handle) : base(father, handle)
        {
        }

        #region Properties

        #endregion

        #region Methods

        public void PlayAnimation(string dictionary, string name, bool looped)
        {
            Base.PlayPedAnimation(this, looped, dictionary, name);
        }

        public void PlayScenario(string scenario)
        {
            Base.PlayPedScenario(this, scenario);
        }

        public void StopAnimation()
        {
            Base.StopPedAnimation(this);
        }

        #endregion
    }
}