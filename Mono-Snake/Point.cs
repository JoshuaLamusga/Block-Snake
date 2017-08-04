using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BlockSnake
{
    /// <summary>
    /// Represents a point that the snakes will try to get to get longer.
    /// </summary>
    public class GamePoint
    {
        #region Members
        /// <summary>
        /// A reference to the game loop.
        /// </summary>
        private MainLoop game;

        /// <summary>
        /// The position on the screen.
        /// </summary>
        public Vector2 position;
        
        /// <summary>
        /// The number of frames that must pass until the point is marked for deletion.
        /// </summary>
        private int tickDelay = 1000;

        /// <summary>
        /// The number of frames executed since the point was created.
        /// </summary>
        private int ticks = 0;
        
        /// <summary>
        /// A point marked for deletion is removed from any lists managing it.
        /// </summary>
        public bool markedForDeletion = false;
        
        /// <summary>
        /// The index of the player that captured the point.
        /// </summary>
        public PlayerNum playerWhoCaptured = PlayerNum.None;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new game point at the specified location.
        /// </summary>
        public GamePoint(MainLoop game, Vector2 position)
        {
            this.game = game;
            this.position = position;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Updates the point; it will be automatically deleted after some
        /// time.
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
                new Rectangle((int)position.X, (int)position.Y,
                    (int)game.GetGridCellSizes(game.gridSize).X,
                    (int)game.GetGridCellSizes(game.gridSize).Y),
                Color.White);
        }
        #endregion
    }
}