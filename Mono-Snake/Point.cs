using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Snake
{
    /// <summary>
    /// Represents a point that the snakes will try to get to get longer.
    /// </summary>
    public class Point
    {
        private MainLoop game;
        //The position on the screen.
        public int xPos, yPos;
        //The amount of time that the point exists; see Update.
        private int tickDelay = 1000, ticks = 0;
        //Whether or not the point is ready to be deleted.
        public bool markedForDeletion = false;
        //The player who got the point.
        public Player playerWhoCaptured = Player.None;

        public Point(MainLoop game, int xPos, int yPos)
        {
            this.game = game;
            this.xPos = xPos;
            this.yPos = yPos;
        }

        /// <summary>
        /// Updates the point (mainly the timer).
        /// </summary>
        public void Update()
        {
            ticks++;
            if (ticks == tickDelay)
            {
                markedForDeletion = true;
            }
        }

        /// <summary>
        /// Draws the point to the screen with a centered origin.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(game.sprPoint,
                new Rectangle(xPos, yPos, (int)game.gridXPixels, (int)game.gridYPixels), //The destination rectangle
                Color.White);
        }
    }
}
