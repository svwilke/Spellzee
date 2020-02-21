namespace RetroBlitDemoBrickBust
{
    using UnityEngine;

    /// <summary>
    /// Extra life powerup
    /// </summary>
    public class PowerUpExtraLife : PowerUp
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pos">Position</param>
        public PowerUpExtraLife(Vector2i pos) : base("P", C.COLOR_BLACK_BRICK, pos)
        {
        }

        /// <summary>
        /// Activate the power up
        /// </summary>
        protected override void Activate()
        {
            base.Activate();

            BrickBustGame game = (BrickBustGame)RB.Game;
            var level = game.Level;

            level.Lives++;
        }
    }
}
