using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Snake
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class MainLoop : Microsoft.Xna.Framework.Game
    {
        //The graphics device and batcher to handle graphics.
        GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        //Loads the fonts; see LoadContent.
        SpriteFont fntDefault, fntSmall;

        //For textures, see LoadContent.
        //The lines to draw the grid.
        private Texture2D sprLineHor, sprLineVer;

        //Blocks used to draw the snake.
        private Texture2D sprSpriteBlock;

        //Menu images.
        private Texture2D sprTitlePlay, sprTitleInfo, sprTitleOps;
        private Texture2D sprTitleLogo, sprTitleSnakeSelector;
        private Texture2D sprTitlePlayPlayers, sprTitlePlayPlayersOne, sprTitlePlayPlayersTwo;
        private Texture2D sprTitleGameStart, sprTitleGameStartBlue, sprTitleGameStartRed;
        private Texture2D sprTitleGamePaused, sprTitleInfoBack;
        private Texture2D sprTitleSizeSmall, sprTitleSizeMedium, sprTitleSizeLarge;
        private Texture2D sprTitleOpsToggleGrid, sprTitleOpsToggleSfx;

        //The sounds.
        SoundEffect sfxGamePoint, sfxGameEnd, sfxMenuClick, sfxMenuSwitch;
        SoundEffect sfxMusic01, sfxMusic02, sfxMusic03;

        //The point for collisions.
        public Texture2D sprPoint;

        //The menu selector's active button.
        public MenuButtons button = MenuButtons.play;

        //The number of squares in the grid.
        private int gridXSize = 20, gridYSize = 15;

        //The amount of pixels per grid square; see LoadContent.
        public float gridXPixels, gridYPixels;

        //Time until update.
        private float tickDelay = 12, tick = 0;

        //A randomizer for variation; see constructor.
        private Random chance;

        //The height and width of the screen; see LoadContent.
        private int scrWidth, scrHeight;

        //Stacks of Vector2s controlling the positions of occupied squares; see constructor.
        private Queue<Vector2> playerOneSnake, playerTwoSnake;

        //The last positions occupied by the players, used for growth.
        Vector2 playerOneDeleted, playerTwoDeleted;

        //A single list controlling the positions of points; see constructor.
        private List<Point> gamePoints, gamePointsDelete;

        //Gets the directions of the players.
        private Direction playerOneDir = Direction.none, playerTwoDir = Direction.none;

        //Who won, if any.
        private Player winner = Player.all;

        //The current controls.
        private GamePadState gpState1, gpState1Old, gpState2, gpState2Old;
        private KeyboardState kbState, kbStateOld;

        //Whether or not the game is paused.
        private bool paused = false;

        private bool soundEnabled = true; //Whether or not sound is being played.
        private bool gridEnabled = false; //Whether or not a grid is drawn.

        //The music player instance; see LoadContent().
        MusicPlayer musicList;

        //How many players are involved.
        private bool twoPlayer = false;

        //The current room.
        private RoomIndex room = RoomIndex.rmMenu;

        public MainLoop()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            //Sets up the random instance.
            chance = new Random();

            //Creates stacks of Vector2s controlling the positions of occupied squares.
            playerOneSnake = new Queue<Vector2>();
            playerTwoSnake = new Queue<Vector2>();

            //Creates a list of points (and a deletion list).
            gamePoints = new List<Point>();
            gamePointsDelete = new List<Point>();
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            fntDefault = Content.Load<SpriteFont>("fntDefault");
            fntSmall = Content.Load<SpriteFont>("fntSmall");

            sfxGamePoint = Content.Load<SoundEffect>("sfxGamePoint");
            sfxGameEnd = Content.Load<SoundEffect>("sfxGameEnd");
            sfxMenuClick = Content.Load<SoundEffect>("sfxMenuClick");
            sfxMenuSwitch = Content.Load<SoundEffect>("sfxMenuSwitch");
            sfxMusic01 = Content.Load<SoundEffect>("sfxMusic01");
            sfxMusic02 = Content.Load<SoundEffect>("sfxMusic02");
            sfxMusic03 = Content.Load<SoundEffect>("sfxMusic03");
            
            sprLineHor = Content.Load<Texture2D>("sprLineHorizontal");
            sprLineVer = Content.Load<Texture2D>("sprLineVertical");
            sprSpriteBlock = Content.Load<Texture2D>("sprSpriteBlock");
            sprTitleLogo = Content.Load<Texture2D>("sprTitleLogo");
            sprTitlePlay = Content.Load<Texture2D>("sprTitlePlay");
            sprTitleInfo = Content.Load<Texture2D>("sprTitleInfo");
            sprTitleOps = Content.Load<Texture2D>("sprTitleOps");
            sprTitlePlayPlayers = Content.Load<Texture2D>("sprTitlePlayPlayers");
            sprTitlePlayPlayersOne = Content.Load<Texture2D>("sprTitlePlayPlayersOne");
            sprTitlePlayPlayersTwo = Content.Load<Texture2D>("sprTitlePlayPlayersTwo");
            sprTitleSnakeSelector = Content.Load<Texture2D>("sprTitleSnakeSelector");
            sprTitleGameStart = Content.Load<Texture2D>("sprTitleGameStart");
            sprTitleGameStartRed = Content.Load<Texture2D>("sprTitleGameStartRed");
            sprTitleGameStartBlue = Content.Load<Texture2D>("sprTitleGameStartBlue");
            sprTitleGamePaused = Content.Load<Texture2D>("sprTitleGamePaused");
            sprTitleSizeSmall = Content.Load<Texture2D>("sprTitleSizeSmall");
            sprTitleSizeMedium = Content.Load<Texture2D>("sprTitleSizeMedium");
            sprTitleSizeLarge = Content.Load<Texture2D>("sprTitleSizeLarge");
            sprTitleInfoBack = Content.Load<Texture2D>("sprTitleInfoBack");
            sprTitleOpsToggleGrid = Content.Load<Texture2D>("sprTitleOpsToggleGrid");
            sprTitleOpsToggleSfx = Content.Load<Texture2D>("sprTitleOpsToggleSfx");
            sprPoint = Content.Load<Texture2D>("sprPoint");

            //Sets the height and width of the screen.
            scrWidth = GraphicsDevice.Viewport.Width;
            scrHeight = GraphicsDevice.Viewport.Height;

            //Sets up the music player.
            musicList = new MusicPlayer(sfxMusic01, sfxMusic02, sfxMusic03);
            musicList.NextSoundRandom();

            //Sets the default gamepad and keyboard states.
            gpState1 = GamePad.GetState(PlayerIndex.One);
            gpState2 = GamePad.GetState(PlayerIndex.Two);
            kbState = Keyboard.GetState();
        }

        protected override void Update(GameTime gameTime)
        {
            //Sets the old gamepad and keyboard states.
            gpState1Old = gpState1;
            gpState2Old = gpState2;
            kbStateOld = kbState;

            //Gets the current gamepad and keyboard states.
            gpState1 = GamePad.GetState(PlayerIndex.One);
            gpState2 = GamePad.GetState(PlayerIndex.Two);
            kbState = Keyboard.GetState();

            //Updates the music if sound is enabled.
            if (soundEnabled)
            {
                musicList.Update();
            }

            //If the room is the menu.
            if (room == RoomIndex.rmMenu)
            {
                //Moving upwards
                if ((kbState.IsKeyDown(Keys.Up) && kbStateOld.IsKeyUp(Keys.Up)) ||
                        gpState1.IsButtonDown(Buttons.LeftThumbstickUp) && gpState1Old.IsButtonUp(Buttons.LeftThumbstickUp) ||
                        gpState2.IsButtonDown(Buttons.LeftThumbstickUp) && gpState2Old.IsButtonUp(Buttons.LeftThumbstickUp))
                {
                    if (soundEnabled)
                    {
                        sfxMenuSwitch.Play();
                    }

                    switch (button)
                    {
                        case (MenuButtons.play):
                            button = MenuButtons.ops;
                            break;
                        case (MenuButtons.info):
                            button = MenuButtons.play;
                            break;
                        case (MenuButtons.ops):
                            button = MenuButtons.info;
                            break;
                    }
                }

                //Moving downwards
                if ((kbState.IsKeyDown(Keys.Down) && kbStateOld.IsKeyUp(Keys.Down)) ||
                        gpState1.IsButtonDown(Buttons.LeftThumbstickDown) && gpState1Old.IsButtonUp(Buttons.LeftThumbstickDown) ||
                        gpState2.IsButtonDown(Buttons.LeftThumbstickDown) && gpState2Old.IsButtonUp(Buttons.LeftThumbstickDown))
                {
                    if (soundEnabled)
                    {
                        sfxMenuSwitch.Play();
                    }

                    switch (button)
                    {
                        case (MenuButtons.play):
                            button = MenuButtons.info;
                            break;
                        case (MenuButtons.info):
                            button = MenuButtons.ops;
                            break;
                        case (MenuButtons.ops):
                            button = MenuButtons.play;
                            break;
                    }
                }
                
                //What happens when the buttons are clicked.
                if ((kbState.IsKeyDown(Keys.Enter) && kbStateOld.IsKeyUp(Keys.Enter)) ||
                        gpState1.IsButtonDown(Buttons.A) && gpState1Old.IsButtonUp(Buttons.A) ||
                        gpState2.IsButtonDown(Buttons.A) && gpState2Old.IsButtonUp(Buttons.A))
                {
                    if (soundEnabled)
                    {
                        sfxMenuClick.Play();
                    }

                    switch (button)
                    {
                        case (MenuButtons.play):
                            button = MenuButtons.onePlayer;
                            room = RoomIndex.rmMenuPlay;
                            break;
                        case (MenuButtons.info):
                            button = MenuButtons.info;
                            room = RoomIndex.rmMenuInfo;
                            break;
                        case (MenuButtons.ops):
                            button = MenuButtons.small;
                            room = RoomIndex.rmMenuOps;
                            break;
                    }
                }
            }
            //If the room is the play menu.
            else if (room == RoomIndex.rmMenuPlay)
            {
                //Switches from one to two-player.
                if ((kbState.IsKeyDown(Keys.Up) && kbStateOld.IsKeyUp(Keys.Up)) ||
                        gpState1.IsButtonDown(Buttons.LeftThumbstickUp) && gpState1Old.IsButtonUp(Buttons.LeftThumbstickUp) ||
                        gpState2.IsButtonDown(Buttons.LeftThumbstickUp) && gpState2Old.IsButtonUp(Buttons.LeftThumbstickUp) ||
                        kbState.IsKeyDown(Keys.Down) && kbStateOld.IsKeyUp(Keys.Down) ||
                        gpState1.IsButtonDown(Buttons.LeftThumbstickDown) && gpState1Old.IsButtonUp(Buttons.LeftThumbstickDown) ||
                        gpState2.IsButtonDown(Buttons.LeftThumbstickDown) && gpState2Old.IsButtonUp(Buttons.LeftThumbstickDown))
                {
                    if (soundEnabled)
                    {
                        sfxMenuSwitch.Play();
                    }

                    switch (button)
                    {
                        case (MenuButtons.onePlayer):
                            button = MenuButtons.twoPlayer;
                            break;
                        case (MenuButtons.twoPlayer):
                            button = MenuButtons.onePlayer;
                            break;
                    }
                }

                //What happens when the buttons are clicked.
                if ((kbState.IsKeyDown(Keys.Enter) && kbStateOld.IsKeyUp(Keys.Enter)) ||
                        gpState1.IsButtonDown(Buttons.A) && gpState1Old.IsButtonUp(Buttons.A) ||
                        gpState2.IsButtonDown(Buttons.A) && gpState2Old.IsButtonUp(Buttons.A))
                {
                    if (soundEnabled)
                    {
                        sfxMenuClick.Play();
                    }

                    //If the one or two-player buttons are pressed.
                    if (button == MenuButtons.onePlayer || button == MenuButtons.twoPlayer)
                    {
                        //Resets important pieces of information.
                        playerOneSnake.Clear(); //Clears the first snake's list.
                        playerTwoSnake.Clear(); //Clears the second snake's list.
                        gamePoints = new List<Point>(); //Removes all dots from the screen.
                        gamePointsDelete = new List<Point>(); //Clears the dot deletion list.
                        if (gridXSize == 20) //A small grid.
                        {
                            tickDelay = 12; //Resets the 'speed' of the game.
                        }
                        else if (gridXSize == 40) //A medium grid.
                        {
                            tickDelay = 10; //Resets the 'speed' of the game.
                        }
                        else if (gridXSize == 80) //A large grid.
                        {
                            tickDelay = 8; //Resets the 'speed' of the game.
                        }
                        tick = 0; //Resets the current tick.
                        playerOneDir = Direction.none; //Resets the first snake's direction.
                        playerTwoDir = Direction.none; //Resets the second snake's direction.

                        //Initializes the size of the grid in pixels.
                        gridXPixels = scrWidth / gridXSize;
                        gridYPixels = scrHeight / gridYSize; 

                        if (button == MenuButtons.onePlayer)
                        {
                            twoPlayer = false;

                            //Sets the position of the snake randomly.
                            playerOneSnake.Enqueue(new Vector2(
                                chance.Next(0, gridXSize) * gridXPixels,
                                chance.Next(0, gridYSize) * gridYPixels));
                        }
                        else if (button == MenuButtons.twoPlayer)
                        {
                            twoPlayer = true;

                            //Sets the default size of snakes to one and places them randomly.
                            float tempXPos1 = chance.Next(0, gridXSize) * gridXPixels;
                            float tempYPos1 = chance.Next(0, gridYSize) * gridYPixels;
                            float tempXPos2 = tempXPos1;
                            float tempYPos2 = tempYPos1;
                            while (tempXPos1 == tempXPos2 || tempYPos1 == tempYPos2)
                            {
                                tempXPos2 = chance.Next(0, gridXSize) * gridXPixels;
                                tempYPos2 = chance.Next(0, gridYSize) * gridYPixels;
                            }
                            //Sets the position for each snake.
                            playerOneSnake.Enqueue(new Vector2(tempXPos1, tempYPos1));
                            playerTwoSnake.Enqueue(new Vector2(tempXPos2, tempYPos2));
                        }

                        //Sets the button to 'play' when/if the player returns to the main menu.
                        button = MenuButtons.play;

                        //Sets the pixels in the grid and begins gameplay.                        
                        room = RoomIndex.rmMain;
                    }
                }
            }
            //If the room is the info menu.
            else if (room == RoomIndex.rmMenuInfo)
            {
                //Exits back to the main menu.
                if ((kbState.IsKeyDown(Keys.Enter) && kbStateOld.IsKeyUp(Keys.Enter)) ||
                        gpState1.IsButtonDown(Buttons.A) && gpState1Old.IsButtonUp(Buttons.A) ||
                        gpState2.IsButtonDown(Buttons.A) && gpState2Old.IsButtonUp(Buttons.A))
                {
                    if (soundEnabled)
                    {
                        sfxMenuClick.Play();
                    }
                    button = MenuButtons.play;
                    room = RoomIndex.rmMenu;
                }
            }
            //If the room is the options menu.
            else if (room == RoomIndex.rmMenuOps)
            {
                //Moving upwards
                if ((kbState.IsKeyDown(Keys.Up) && kbStateOld.IsKeyUp(Keys.Up)) ||
                        gpState1.IsButtonDown(Buttons.LeftThumbstickUp) && gpState1Old.IsButtonUp(Buttons.LeftThumbstickUp) ||
                        gpState2.IsButtonDown(Buttons.LeftThumbstickUp) && gpState2Old.IsButtonUp(Buttons.LeftThumbstickUp))
                {
                    if (soundEnabled)
                    {
                        sfxMenuSwitch.Play();
                    }

                    switch (button)
                    {
                        case (MenuButtons.medium):
                            button = MenuButtons.small;
                            break;
                        case (MenuButtons.large):
                            button = MenuButtons.medium;
                            break;
                        case (MenuButtons.toggleGrid):
                            button = MenuButtons.large;
                            break;
                        case (MenuButtons.toggleSfx):
                            button = MenuButtons.toggleGrid;
                            break;
                        case (MenuButtons.small):
                            button = MenuButtons.toggleSfx;
                            break;
                    }
                }

                //Moving downwards
                if ((kbState.IsKeyDown(Keys.Down) && kbStateOld.IsKeyUp(Keys.Down)) ||
                        gpState1.IsButtonDown(Buttons.LeftThumbstickDown) && gpState1Old.IsButtonUp(Buttons.LeftThumbstickDown) ||
                        gpState2.IsButtonDown(Buttons.LeftThumbstickDown) && gpState2Old.IsButtonUp(Buttons.LeftThumbstickDown))
                {
                    if (soundEnabled)
                    {
                        sfxMenuSwitch.Play();
                    }

                    switch (button)
                    {
                        case (MenuButtons.small):
                            button = MenuButtons.medium;
                            break;
                        case (MenuButtons.medium):
                            button = MenuButtons.large;
                            break;
                        case (MenuButtons.large):
                            button = MenuButtons.toggleGrid;
                            break;
                        case (MenuButtons.toggleGrid):
                            button = MenuButtons.toggleSfx;
                            break;
                        case (MenuButtons.toggleSfx):
                            button = MenuButtons.small;
                            break;
                    }
                }

                //Defines what happens when each button is pressed.
                if ((kbState.IsKeyDown(Keys.Enter) && kbStateOld.IsKeyUp(Keys.Enter)) ||
                        gpState1.IsButtonDown(Buttons.A) && gpState1Old.IsButtonUp(Buttons.A) ||
                        gpState2.IsButtonDown(Buttons.A) && gpState2Old.IsButtonUp(Buttons.A))
                {
                    if (soundEnabled)
                    {
                        sfxMenuClick.Play();
                    }

                    switch (button)
                    {
                    case (MenuButtons.small):
                        room = RoomIndex.rmMenu;
                        button = MenuButtons.play;
                        gridXSize = 20;
                        gridYSize = 15;
                        tickDelay = 12;
                        break;
                    case (MenuButtons.medium):
                        room = RoomIndex.rmMenu;
                        button = MenuButtons.play;
                        gridXSize = 40;
                        gridYSize = 30;
                        tickDelay = 10;
                        break;
                    case (MenuButtons.large):
                        room = RoomIndex.rmMenu;
                        button = MenuButtons.play;
                        gridXSize = 80;
                        gridYSize = 60;
                        tickDelay = 8;
                        break;
                    case (MenuButtons.toggleGrid):
                        room = RoomIndex.rmMenu;
                        button = MenuButtons.play;
                        gridEnabled = !gridEnabled; //inverts the boolean.
                        break;
                    case (MenuButtons.toggleSfx):
                        soundEnabled = !soundEnabled; //Inverts the boolean.

                        if (soundEnabled)
                        {
                            //Starts playing music again when turned back on.
                            musicList.NextSoundRandom();
                        }
                        else
                        {
                            //Stops music abruptly when turned off.
                            musicList.sound.Stop();
                        }
                        break;
                    }
                }
            }
            //If the room is the game.
            else if (room == RoomIndex.rmMain)
            {
                //Checks if a player has won.
                if (winner != Player.all)
                {
                    //Returns to the menu if enter or start is pressed.
                    if ((kbState.IsKeyUp(Keys.Enter) && kbStateOld.IsKeyDown(Keys.Enter)) ||
                        gpState1.IsButtonUp(Buttons.Start) && gpState1Old.IsButtonDown(Buttons.Start) ||
                        gpState2.IsButtonUp(Buttons.Start) && gpState2Old.IsButtonDown(Buttons.Start))
                    {
                        if (soundEnabled)
                        {
                            sfxMenuClick.Play();
                        }

                        //Resets the value of who won.
                        winner = Player.all;

                        //Changes the room to the menu.
                        room = RoomIndex.rmMenu;
                    }
                    //Suspends functionality when a player has won (like pausing).
                    return;
                }

                //Gets whether or not the game is paused.
                if ((kbState.IsKeyDown(Keys.P) && kbStateOld.IsKeyUp(Keys.P)) ||
                        gpState1.IsButtonDown(Buttons.B) && gpState1Old.IsButtonUp(Buttons.B) ||
                        gpState2.IsButtonDown(Buttons.B) && gpState2Old.IsButtonUp(Buttons.B))
                {
                    if (paused && winner == Player.all)
                    {
                        paused = false;
                    }
                    else if (!paused && winner == Player.all)
                    {
                        paused = true;
                    }
                }

                //Suspends functionality while paused.
                if (paused)
                {
                    return;
                }

                //Processes gamepad input for player 1.
                if (gpState1.ThumbSticks.Left.X > 0 &&
                    gpState1.ThumbSticks.Left.X > Math.Abs(gpState1.ThumbSticks.Left.Y))
                {
                    playerOneDir = Direction.right;
                }
                else if (gpState1.ThumbSticks.Left.X < 0 &&
                    gpState1.ThumbSticks.Left.X < -Math.Abs(gpState1.ThumbSticks.Left.Y))
                {
                    playerOneDir = Direction.left;
                }
                else if (gpState1.ThumbSticks.Left.Y < 0 &&
                    gpState1.ThumbSticks.Left.Y < Math.Abs(gpState1.ThumbSticks.Left.X))
                {
                    playerOneDir = Direction.down;
                }
                else if (gpState1.ThumbSticks.Left.Y > 0 &&
                    gpState1.ThumbSticks.Left.Y > Math.Abs(gpState1.ThumbSticks.Left.X))
                {
                    playerOneDir = Direction.up;
                }
                if (twoPlayer)
                {
                    //Processes gamepad input for player 2.
                    if (gpState2.ThumbSticks.Left.X > 0 &&
                        gpState2.ThumbSticks.Left.X > Math.Abs(gpState2.ThumbSticks.Left.Y))
                    {
                        playerTwoDir = Direction.right;
                    }
                    else if (gpState2.ThumbSticks.Left.X < 0 &&
                        gpState2.ThumbSticks.Left.X < -Math.Abs(gpState2.ThumbSticks.Left.Y))
                    {
                        playerTwoDir = Direction.left;
                    }
                    else if (gpState2.ThumbSticks.Left.Y < 0 &&
                        gpState2.ThumbSticks.Left.Y < Math.Abs(gpState2.ThumbSticks.Left.X))
                    {
                        playerTwoDir = Direction.down;
                    }
                    else if (gpState2.ThumbSticks.Left.Y > 0 &&
                        gpState2.ThumbSticks.Left.Y > Math.Abs(gpState2.ThumbSticks.Left.X))
                    {
                        playerTwoDir = Direction.up;
                    }
                }
                //Updates the keyboard for player 1.
                if (kbState.IsKeyDown(Keys.D))
                {
                    playerOneDir = Direction.right;
                }
                if (kbState.IsKeyDown(Keys.W))
                {
                    playerOneDir = Direction.up;
                }
                if (kbState.IsKeyDown(Keys.A))
                {
                    playerOneDir = Direction.left;
                }
                if (kbState.IsKeyDown(Keys.S))
                {
                    playerOneDir = Direction.down;
                }
                if (twoPlayer)
                {
                    //Updates the keyboard for player 2.
                    if (kbState.IsKeyDown(Keys.Right))
                    {
                        playerTwoDir = Direction.right;
                    }
                    if (kbState.IsKeyDown(Keys.Up))
                    {
                        playerTwoDir = Direction.up;
                    }
                    if (kbState.IsKeyDown(Keys.Left))
                    {
                        playerTwoDir = Direction.left;
                    }
                    if (kbState.IsKeyDown(Keys.Down))
                    {
                        playerTwoDir = Direction.down;
                    }
                }

                //Doesn't start the game until both players have a direction.
                if (twoPlayer)
                {
                    if (playerOneDir == Direction.none || playerTwoDir == Direction.none)
                    {
                        return;
                    }
                }
                else
                {
                    if (playerOneDir == Direction.none)
                    {
                        return;
                    }
                }

                //Updates the timer that affects the overall update.
                tick--;
                if (tick <= 0)
                {
                    tick = tickDelay;
                    UpdateMovement();
                    UpdateCollision();
                }

                //Spawns points randomly and spawns a point if there are none.
                if ((gamePoints.Count < 2 && chance.Next(0, 1000) == 1) ||
                    gamePoints.Count == 0)
                {
                    //temporary variables to store the position on the screen.
                    int tempPosX, tempPosY;
                    bool tempvalid = false;

                    //As long as there is open space on the screen.
                    if (playerOneSnake.Count + playerTwoSnake.Count < gridXSize * gridYSize)
                    {
                        //If tempvalid is false.
                        while (!tempvalid)
                        {
                            //Gets new values for the positions.
                            tempPosX = (int)(chance.Next(0, gridXSize) * gridXPixels);
                            tempPosY = (int)(chance.Next(0, gridYSize) * gridYPixels);

                            //Checks to see if those positions are occupied.
                            //If so, moves to another spot on the screen.
                            for (int i = 0; i < playerOneSnake.Count - 1; i++)
                            {
                                if (new Vector2(tempPosX, tempPosY) == playerOneSnake.ElementAt<Vector2>(i))
                                {
                                    continue;
                                }
                            }
                            if (twoPlayer)
                            {
                                for (int i = 0; i < playerTwoSnake.Count - 1; i++)
                                {
                                    if (new Vector2(tempPosX, tempPosY) == playerTwoSnake.ElementAt<Vector2>(i))
                                    {
                                        continue;
                                    }
                                }
                            }

                            //If the point doesn't intersect with anything on the screen.
                            tempvalid = true;

                            gamePoints.Add(new Point(this, tempPosX, tempPosY));
                        }
                    }
                }

                //Updates points and deletes them.
                for (int i = 0; i < gamePoints.Count; i++)
                {
                    //Updates all points.
                    gamePoints[i].Update();
                    //Checks for deleted points.
                    if (gamePoints[i].markedForDeletion == true)
                    {
                        if (soundEnabled)
                        {
                            sfxGamePoint.Play();
                        }

                        gamePointsDelete.Add(gamePoints[i]);

                        //Keeps track of the player's direction.
                        float tempXPlayer1 = 0, tempYPlayer1 = 0;
                        float tempXPlayer2 = 0, tempYPlayer2 = 0;

                        //Tries to determine the movement of player one.
                        switch (playerOneDir)
                        {
                            case (Direction.right):
                                tempXPlayer1 = gridXPixels;
                                break;
                            case (Direction.down):
                                tempYPlayer1 = gridYPixels;
                                break;
                            case (Direction.left):
                                tempXPlayer1 = -gridXPixels;
                                break;
                            case (Direction.up):
                                tempYPlayer1 = -gridYPixels;
                                break;
                        }

                        //Tries to determine the movement of player two.
                        switch (playerTwoDir)
                        {
                            case (Direction.right):
                                tempXPlayer2 = gridXPixels;
                                break;
                            case (Direction.down):
                                tempYPlayer2 = gridYPixels;
                                break;
                            case (Direction.left):
                                tempXPlayer2 = -gridXPixels;
                                break;
                            case (Direction.up):
                                tempYPlayer2 = -gridYPixels;
                                break;
                        }

                        if (gamePoints[i].playerWhoCaptured == Player.one)
                        {
                            //Reverses the snake so that a block is added to the end of it.
                            playerOneSnake = new Queue<Vector2>(playerOneSnake.Reverse<Vector2>());
                            playerOneSnake.Enqueue(playerOneDeleted);
                            playerOneSnake = new Queue<Vector2>(playerOneSnake.Reverse<Vector2>());

                            if (gridXSize == 20) //A small grid.
                            {
                                //Increases the speed of the game.
                                if (tickDelay > 10)
                                {
                                    tickDelay -= 0.5f;
                                }
                                else if (tickDelay > 5)
                                {
                                    tickDelay -= 0.5f;
                                }
                                else if (tickDelay > 2)
                                {
                                    tickDelay -= 0.25f;
                                }
                            }
                            else if (gridXSize == 40) //A medium grid.
                            {
                                //Increases the speed of the game.
                                if (tickDelay > 10)
                                {
                                    tickDelay -= 0.75f;
                                }
                                else if (tickDelay > 5)
                                {
                                    tickDelay -= 0.5f;
                                }
                                else if (tickDelay > 2)
                                {
                                    tickDelay -= 0.25f;
                                }
                            }
                            else if (gridXSize == 80) //A large grid.
                            {
                                //Increases the speed of the game.
                                if (tickDelay > 10)
                                {
                                    tickDelay -= 1;
                                }
                                else if (tickDelay > 3)
                                {
                                    tickDelay -= 0.5f;
                                }
                                else if (tickDelay > 1)
                                {
                                    tickDelay -= 0.25f;
                                }
                            }
                        }
                        else if (gamePoints[i].playerWhoCaptured == Player.two)
                        {
                            //Reverses the snake so that a block is added to the end of it.
                            playerTwoSnake = new Queue<Vector2>(playerTwoSnake.Reverse<Vector2>());
                            playerTwoSnake.Enqueue(playerTwoDeleted);
                            playerTwoSnake = new Queue<Vector2>(playerTwoSnake.Reverse<Vector2>());

                            //Increases the speed of the game.
                            if (tickDelay > 10)
                            {
                                tickDelay -= 0.5f;
                            }
                            else if (tickDelay > 5)
                            {
                                tickDelay -= 0.25f;
                            }
                        }
                    }
                }
                //Goes through the deletion list.
                for (int i = 0; i < gamePointsDelete.Count; i++)
                {
                    //Removes items from the gamePoints array.
                    gamePoints.Remove(gamePointsDelete[i]);
                }
                //Clears the deletion array.
                gamePointsDelete.Clear();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);
            //Makes it so that there is no interpolation for stretching (nearest-neighbor algorithm).
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);

            //Displays the title in all of the menus.
            if (room != RoomIndex.rmMain)
            {
                //Draws the title
                spriteBatch.Draw(sprTitleLogo,
                    new Rectangle(scrWidth / 2, scrHeight / 5, sprTitleLogo.Width, sprTitleLogo.Height),
                    sprTitleLogo.Bounds,
                    Color.White,
                    0,
                    new Vector2(sprTitleLogo.Width / 2, sprTitleLogo.Height / 2),
                    SpriteEffects.None,
                    0);
            }

            //If the room is the menu.
            if (room == RoomIndex.rmMenu)
            {
                //Draws the play button.
                spriteBatch.Draw(sprTitlePlay,
                    new Rectangle(scrWidth / 2, scrHeight / 5 + 96, sprTitlePlay.Width, sprTitlePlay.Height),
                    sprTitlePlay.Bounds,
                    Color.White,
                    0,
                    new Vector2(sprTitlePlay.Width / 2, sprTitlePlay.Height / 2),
                    SpriteEffects.None,
                    0);

                //Draws the info button.
                spriteBatch.Draw(sprTitleInfo,
                    new Rectangle(scrWidth / 2, scrHeight / 5 + 192, sprTitleInfo.Width, sprTitleInfo.Height),
                    sprTitleInfo.Bounds,
                    Color.White,
                    0,
                    new Vector2(sprTitleInfo.Width / 2, sprTitleInfo.Height / 2),
                    SpriteEffects.None,
                    0);

                //Draws the options button.
                spriteBatch.Draw(sprTitleOps,
                    new Rectangle(scrWidth / 2, scrHeight / 5 + 288, sprTitleOps.Width, sprTitleOps.Height),
                    sprTitleOps.Bounds,
                    Color.White,
                    0,
                    new Vector2(sprTitleOps.Width / 2, sprTitleOps.Height / 2),
                    SpriteEffects.None,
                    0);

                //Sets up a temporary y-coordinate for the selector image.
                //Based on the currently active button.
                int tempYPos = 0;
                
                switch (button)
                {
                    case (MenuButtons.play):                        
                        tempYPos = scrHeight / 5 + 96;
                        break;
                    case (MenuButtons.info):
                        tempYPos = scrHeight / 5 + 192;
                        break;
                    case (MenuButtons.ops):
                        tempYPos = scrHeight / 5 + 288;
                        break;
                }

                //Draws the selector image.
                spriteBatch.Draw(sprTitleSnakeSelector,
                    new Rectangle(scrWidth / 2 - 128, tempYPos, sprTitleSnakeSelector.Width, sprTitleSnakeSelector.Height),
                    sprTitleSnakeSelector.Bounds,
                    Color.White,
                    0,
                    new Vector2(sprTitleSnakeSelector.Width / 2, sprTitleSnakeSelector.Height / 2),
                    SpriteEffects.None,
                    0);

            }
            //If the room is the info room.
            else if (room == RoomIndex.rmMenuInfo)
            {
                //Draws the instructions
                spriteBatch.DrawString(fntSmall,
                    "Collect the dots as they appear and stay on-screen. Use the",
                    new Vector2(scrWidth / 2
                        - (fntSmall.MeasureString("Collect the dots as they appear and stay on-screen. Use the").X / 2),
                        scrHeight / 5 + 96),
                    Color.Black);

                spriteBatch.DrawString(fntSmall,
                    "left thumbstick for controllers, WASD for player one, and",
                    new Vector2(scrWidth / 2
                        - (fntSmall.MeasureString("left thumbstick for controllers, WASD for player one, and").X / 2),
                        scrHeight / 5 + 120),
                    Color.Black);

                spriteBatch.DrawString(fntSmall,
                     "arrowkeys for player two.  Don't run into yourself or another",
                     new Vector2(scrWidth / 2
                         - (fntSmall.MeasureString("arrowkeys for player two.  Don't run into yourself or another").X / 2),
                         scrHeight / 5 + 144),
                     Color.Black);

                spriteBatch.DrawString(fntSmall,
                     "player. Cut off the other player to win in two-player mode.",
                     new Vector2(scrWidth / 2
                         - (fntSmall.MeasureString("player. Cut off the other player to win in two-player mode.").X / 2),
                         scrHeight / 5 + 168),
                     Color.Black);

                spriteBatch.DrawString(fntSmall,
                     "P (keyboard) or B (gamepad) to pause. Y toggles music.",
                     new Vector2(scrWidth / 2
                         - (fntSmall.MeasureString("P (keyboard) or B (gamepad) to pause. Y toggles music.").X / 2),
                         scrHeight / 5 + 192),
                     Color.Black);

                //Draws the back button.
                spriteBatch.Draw(sprTitleInfoBack,
                    new Rectangle(scrWidth / 2, scrHeight / 5 + 288, sprTitleInfoBack.Width, sprTitleInfoBack.Height),
                    sprTitleInfoBack.Bounds,
                    Color.White,
                    0,
                    new Vector2(sprTitleInfoBack.Width / 2, sprTitleInfoBack.Height / 2),
                    SpriteEffects.None,
                    0);

                //Draws the selector image on the same line as the only button, the back button.
                spriteBatch.Draw(sprTitleSnakeSelector,
                    new Rectangle(scrWidth / 2 - 128, scrHeight / 5 + 288, sprTitleSnakeSelector.Width, sprTitleSnakeSelector.Height),
                    sprTitleSnakeSelector.Bounds,
                    Color.White,
                    0,
                    new Vector2(sprTitleSnakeSelector.Width / 2, sprTitleSnakeSelector.Height / 2),
                    SpriteEffects.None,
                    0);
            }
            else if (room == RoomIndex.rmMenuOps)
            {
                //Draws the small button.
                spriteBatch.Draw(sprTitleSizeSmall,
                    new Rectangle(scrWidth / 2, scrHeight / 5 + 96, sprTitleSizeSmall.Width, sprTitleSizeSmall.Height),
                    sprTitleSizeSmall.Bounds,
                    Color.White,
                    0,
                    new Vector2(sprTitleSizeSmall.Width / 2, sprTitleSizeSmall.Height / 2),
                    SpriteEffects.None,
                    0);

                //Draws the medium button.
                spriteBatch.Draw(sprTitleSizeMedium,
                    new Rectangle(scrWidth / 2, scrHeight / 5 + 160, sprTitleSizeMedium.Width, sprTitleSizeMedium.Height),
                    sprTitleSizeMedium.Bounds,
                    Color.White,
                    0,
                    new Vector2(sprTitleSizeMedium.Width / 2, sprTitleSizeMedium.Height / 2),
                    SpriteEffects.None,
                    0);

                //Draws the large button.
                spriteBatch.Draw(sprTitleSizeLarge,
                    new Rectangle(scrWidth / 2, scrHeight / 5 + 224, sprTitleSizeLarge.Width, sprTitleSizeLarge.Height),
                    sprTitleSizeLarge.Bounds,
                    Color.White,
                    0,
                    new Vector2(sprTitleSizeLarge.Width / 2, sprTitleSizeLarge.Height / 2),
                    SpriteEffects.None,
                    0);

                //Draws the toggle grid button.
                spriteBatch.Draw(sprTitleOpsToggleGrid,
                    new Rectangle(scrWidth / 2, scrHeight / 5 + 288, sprTitleOpsToggleGrid.Width, sprTitleOpsToggleGrid.Height),
                    sprTitleOpsToggleGrid.Bounds,
                    Color.White,
                    0,
                    new Vector2(sprTitleOpsToggleGrid.Width / 2, sprTitleOpsToggleGrid.Height / 2),
                    SpriteEffects.None,
                    0);

                //Draws the toggle sfx button.
                spriteBatch.Draw(sprTitleOpsToggleSfx,
                    new Rectangle(scrWidth / 2, scrHeight / 5 + 352, sprTitleOpsToggleGrid.Width, sprTitleOpsToggleGrid.Height),
                    sprTitleOpsToggleGrid.Bounds,
                    Color.White,
                    0,
                    new Vector2(sprTitleOpsToggleSfx.Width / 2, sprTitleOpsToggleSfx.Height / 2),
                    SpriteEffects.None,
                    0);

                //Sets up a temporary y-coordinate for the selector image.
                //Based on the currently active button.
                int tempYPos = 0;

                switch (button)
                {
                    case (MenuButtons.small):
                        tempYPos = scrHeight / 5 + 96;
                        break;
                    case (MenuButtons.medium):
                        tempYPos = scrHeight / 5 + 160;
                        break;
                    case (MenuButtons.large):
                        tempYPos = scrHeight / 5 + 224;
                        break;
                    case (MenuButtons.toggleGrid):
                        tempYPos = scrHeight / 5 + 288;
                        break;
                    case (MenuButtons.toggleSfx):
                        tempYPos = scrHeight / 5 + 352;
                        break;
                }

                //Draws the selector image.
                spriteBatch.Draw(sprTitleSnakeSelector,
                    new Rectangle(scrWidth / 2 - 128, tempYPos, sprTitleSnakeSelector.Width, sprTitleSnakeSelector.Height),
                    sprTitleSnakeSelector.Bounds,
                    Color.White,
                    0,
                    new Vector2(sprTitleSnakeSelector.Width / 2, sprTitleSnakeSelector.Height / 2),
                    SpriteEffects.None,
                    0);
            }
            //if the room is the play menu. 
            else if (room == RoomIndex.rmMenuPlay)
            {
                //Draws the 'players' text.
                spriteBatch.Draw(sprTitlePlayPlayers,
                    new Rectangle(scrWidth / 2, scrHeight / 5 + 96, sprTitlePlayPlayers.Width, sprTitlePlayPlayers.Height),
                    sprTitlePlayPlayers.Bounds,
                    Color.White,
                    0,
                    new Vector2(sprTitlePlayPlayers.Width / 2, sprTitlePlayPlayers.Height / 2),
                    SpriteEffects.None,
                    0);

                //Draws the one button.
                spriteBatch.Draw(sprTitlePlayPlayersOne,
                    new Rectangle(scrWidth / 2, scrHeight / 5 + 192, sprTitlePlayPlayersOne.Width, sprTitlePlayPlayersOne.Height),
                    sprTitlePlayPlayersOne.Bounds,
                    Color.White,
                    0,
                    new Vector2(sprTitlePlayPlayersOne.Width / 2, sprTitlePlayPlayersOne.Height / 2),
                    SpriteEffects.None,
                    0);

                //Draws the two button.
                spriteBatch.Draw(sprTitlePlayPlayersTwo,
                    new Rectangle(scrWidth / 2, scrHeight / 5 + 288, sprTitlePlayPlayersTwo.Width, sprTitlePlayPlayersTwo.Height),
                    sprTitlePlayPlayersTwo.Bounds,
                    Color.White,
                    0,
                    new Vector2(sprTitlePlayPlayersTwo.Width / 2, sprTitlePlayPlayersTwo.Height / 2),
                    SpriteEffects.None,
                    0);

                //Sets up a temporary y-coordinate for the selector image.
                //Based on the currently active button.
                int tempYPos = 0;

                switch (button)
                {
                    case (MenuButtons.onePlayer):
                        tempYPos = scrHeight / 5 + 192;
                        break;
                    case (MenuButtons.twoPlayer):
                        tempYPos = scrHeight / 5 + 288;
                        break;
                }

                //Draws the selector image.
                spriteBatch.Draw(sprTitleSnakeSelector,
                    new Rectangle(scrWidth / 2 - 128, tempYPos, sprTitleSnakeSelector.Width, sprTitleSnakeSelector.Height),
                    sprTitleSnakeSelector.Bounds,
                    Color.White,
                    0,
                    new Vector2(sprTitleSnakeSelector.Width / 2, sprTitleSnakeSelector.Height / 2),
                    SpriteEffects.None,
                    0);
            }
            //If the room is the game.
            else if (room == RoomIndex.rmMain)
            {
                if (gridEnabled)
                {
                    //Draws the grid to the screen.
                    //Draws all vertical rows.
                    for (float i = 0; i < scrHeight; i += gridYPixels)
                    {
                        spriteBatch.Draw(sprLineHor, new Rectangle(0, (int)i, scrWidth, 1), Color.Black);
                    }

                    //Draws all horizontal rows.
                    for (float i = 0; i < scrWidth; i += gridXPixels)
                    {
                        spriteBatch.Draw(sprLineVer, new Rectangle((int)i, 0, 1, scrHeight), Color.Black);
                    }
                }

                //Draws all points.
                for (int i = 0; i < gamePoints.Count; i++)
                {
                    gamePoints[i].Draw(spriteBatch);
                }
                
                //Draws snake one.
                for (int i = 0; i < playerOneSnake.Count; i++)
                {
                    spriteBatch.Draw(sprSpriteBlock, new Rectangle(
                        (int)playerOneSnake.ElementAt<Vector2>(i).X,
                        (int)playerOneSnake.ElementAt<Vector2>(i).Y,
                        (int)gridXPixels,
                        (int)gridYPixels),
                    Color.Red);
                }

                if (twoPlayer)
                {
                    //Draws snake two.
                    for (int i = 0; i < playerTwoSnake.Count; i++)
                    {
                        spriteBatch.Draw(sprSpriteBlock, new Rectangle(
                            (int)playerTwoSnake.ElementAt<Vector2>(i).X,
                            (int)playerTwoSnake.ElementAt<Vector2>(i).Y,
                            (int)gridXPixels,
                            (int)gridYPixels),
                        Color.Blue);
                    }
                }

                //If the game hasn't yet started...
                if (twoPlayer && (playerOneDir == Direction.none || playerTwoDir == Direction.none))
                {
                    if (playerOneDir == Direction.none && playerTwoDir == Direction.none)
                    {
                        spriteBatch.Draw(sprTitleGameStart,
                            new Rectangle(scrWidth / 2, scrHeight / 2, sprTitleGameStart.Width, sprTitleGameStart.Height),
                            sprTitleGameStart.Bounds,
                            Color.White,
                            0,
                            new Vector2(sprTitleGameStart.Width / 2, sprTitleGameStart.Height / 2),
                            SpriteEffects.None,
                            0);
                    }
                    else if (playerOneDir != Direction.none)
                    {
                        spriteBatch.Draw(sprTitleGameStartBlue,
                            new Rectangle(scrWidth / 2, scrHeight / 2, sprTitleGameStartBlue.Width, sprTitleGameStartBlue.Height),
                            sprTitleGameStartBlue.Bounds,
                            Color.White,
                            0,
                            new Vector2(sprTitleGameStartBlue.Width / 2, sprTitleGameStartBlue.Height / 2),
                            SpriteEffects.None,
                            0);
                    }
                    else if (playerTwoDir != Direction.none)
                    {
                        spriteBatch.Draw(sprTitleGameStartRed,
                            new Rectangle(scrWidth / 2, scrHeight / 2, sprTitleGameStartRed.Width, sprTitleGameStartRed.Height),
                            sprTitleGameStartRed.Bounds,
                            Color.White,
                            0,
                            new Vector2(sprTitleGameStartRed.Width / 2, sprTitleGameStartRed.Height / 2),
                            SpriteEffects.None,
                            0);
                    }
                }
                else if (!twoPlayer && playerOneDir == Direction.none)
                {
                    spriteBatch.Draw(sprTitleGameStart,
                            new Rectangle(scrWidth / 2, scrHeight / 2, sprTitleGameStart.Width, sprTitleGameStart.Height),
                            sprTitleGameStart.Bounds,
                            Color.White,
                            0,
                            new Vector2(sprTitleGameStart.Width / 2, sprTitleGameStart.Height / 2),
                            SpriteEffects.None,
                            0);
                }

                //Draws the word 'paused' when the game is paused
                if (paused)
                {
                    //Draws the selector image.
                    spriteBatch.Draw(sprTitleGamePaused,
                        new Rectangle(scrWidth / 2, scrHeight / 2, sprTitleGamePaused.Width, sprTitleGamePaused.Height),
                        sprTitleGamePaused.Bounds,
                        Color.White,
                        0,
                        new Vector2(sprTitleGamePaused.Width / 2, sprTitleGamePaused.Height / 2),
                        SpriteEffects.None,
                        0);
                }

                //Draws who won to the screen.
                if (winner != Player.all && twoPlayer)
                {
                    if (winner == Player.one)
                    {
                        spriteBatch.DrawString(
                            fntDefault,
                            "Red wins!",
                            new Vector2(scrWidth / 2 - (fntDefault.MeasureString("Red wins!").X / 2), scrHeight / 2),
                            Color.Red);
                    }
                    else if (winner == Player.two)
                    {
                        spriteBatch.DrawString(
                            fntDefault,
                            "Blue wins!",
                            new Vector2(scrWidth / 2 - (fntDefault.MeasureString("Blue wins!").X / 2), scrHeight / 2),
                            Color.Blue);
                    }
                    else if (winner == Player.none)
                    {
                        spriteBatch.DrawString(
                            fntDefault,
                            "Tie game",
                            new Vector2(scrWidth / 2 - (fntDefault.MeasureString("Tie game").X / 2), scrHeight / 2),
                            Color.Black);
                    }

                    spriteBatch.DrawString(
                        fntDefault,
                        "Red size: " + playerOneSnake.Count.ToString() + "\nBlue size: " + playerTwoSnake.Count.ToString(),
                        new Vector2(scrWidth / 2 -
                            (fntDefault.MeasureString("Red size: " + playerOneSnake.Count.ToString() +
                            "\nBlue size: " + playerTwoSnake.Count.ToString()).X / 2),
                            scrHeight / 2 + 64),
                        Color.Black);

                    spriteBatch.DrawString(
                        fntDefault,
                        "Press Enter or Start to exit to menu.",
                        new Vector2(scrWidth / 2 - (fntDefault.MeasureString("Press Enter or Start to exit to menu.").X / 2), scrHeight / 2 + 160),
                        Color.Black);
                }
                else if (winner != Player.all && !twoPlayer)
                {
                    spriteBatch.DrawString(
                        fntDefault,
                        "size: " + playerOneSnake.Count.ToString(),
                        new Vector2(scrWidth / 2 - (fntDefault.MeasureString("size: " + playerOneSnake.Count.ToString()).X / 2),
                            scrHeight / 2 + 64),
                        Color.Black);

                    spriteBatch.DrawString(
                        fntDefault,
                        "Press Enter or Start to exit to menu.",
                        new Vector2(scrWidth / 2 - (fntDefault.MeasureString("Press Enter or Start to exit to menu.").X / 2), scrHeight / 2 + 160),
                        Color.Black);
                }
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }

        /// <summary>
        /// Updates the snake positions based on direction.
        /// </summary>
        public void UpdateMovement()
        {
            //Creates temporary variables for the x and y additions to be made.
            float tempXPlayer1 = 0, tempYPlayer1 = 0;
            float tempXPlayer2 = 0, tempYPlayer2 = 0;

            //Affects the movement of player one.
            if (playerOneDir == Direction.right)
            {
                tempXPlayer1 = gridXPixels;
            }
            else if (playerOneDir == Direction.down)
            {
                tempYPlayer1 = gridYPixels;
            }
            else if (playerOneDir == Direction.left)
            {
                tempXPlayer1 = -gridXPixels;
            }
            else if (playerOneDir == Direction.up)
            {
                tempYPlayer1 = -gridYPixels;
            }

            if (twoPlayer)
            {
                //Affects the movement of player 2.
                if (playerTwoDir == Direction.right)
                {
                    tempXPlayer2 = gridXPixels;
                }
                else if (playerTwoDir == Direction.down)
                {
                    tempYPlayer2 = gridYPixels;
                }
                else if (playerTwoDir == Direction.left)
                {
                    tempXPlayer2 = -gridXPixels;
                }
                else if (playerTwoDir == Direction.up)
                {
                    tempYPlayer2 = -gridYPixels;
                }
            }

            //Adds a new block to players based on their direction.
            //This just simulates movement of blocks.
            playerOneSnake.Enqueue(new Vector2(
                playerOneSnake.ElementAt<Vector2>(playerOneSnake.Count - 1).X + tempXPlayer1,
                playerOneSnake.ElementAt<Vector2>(playerOneSnake.Count - 1).Y + tempYPlayer1));

            if (twoPlayer)
            {
                playerTwoSnake.Enqueue(new Vector2(
                    playerTwoSnake.ElementAt<Vector2>(playerTwoSnake.Count - 1).X + tempXPlayer2,
                    playerTwoSnake.ElementAt<Vector2>(playerTwoSnake.Count - 1).Y + tempYPlayer2));
            }
            //Removes the first block from the stack.
            playerOneDeleted = playerOneSnake.Dequeue();
            if (twoPlayer)
            {
                playerTwoDeleted = playerTwoSnake.Dequeue();
            }

            //Checks to see if the new block is outside of the screen.
            if (playerOneSnake.ElementAt<Vector2>(playerOneSnake.Count - 1).X >= scrWidth ||
                playerOneSnake.ElementAt<Vector2>(playerOneSnake.Count - 1).X < 0 ||
                playerOneSnake.ElementAt<Vector2>(playerOneSnake.Count - 1).Y >= scrHeight ||
                playerOneSnake.ElementAt<Vector2>(playerOneSnake.Count - 1).Y < 0)
            {
                if (soundEnabled)
                {
                    sfxGameEnd.Play();
                }
                winner = Player.two;

                //Checks to see if the other snake is off-screen too (would be a tie-game).
                if (twoPlayer)
                {
                    if (playerTwoSnake.ElementAt<Vector2>(playerTwoSnake.Count - 1).X >= scrWidth ||
                    playerTwoSnake.ElementAt<Vector2>(playerTwoSnake.Count - 1).X < 0 ||
                    playerTwoSnake.ElementAt<Vector2>(playerTwoSnake.Count - 1).Y >= scrHeight ||
                    playerTwoSnake.ElementAt<Vector2>(playerTwoSnake.Count - 1).Y < 0)
                    {
                        winner = Player.none;
                    }
                }
            }
            if (twoPlayer && winner == Player.all)
            {
                if (playerTwoSnake.ElementAt<Vector2>(playerTwoSnake.Count - 1).X >= scrWidth ||
                    playerTwoSnake.ElementAt<Vector2>(playerTwoSnake.Count - 1).X < 0 ||
                    playerTwoSnake.ElementAt<Vector2>(playerTwoSnake.Count - 1).Y >= scrHeight ||
                    playerTwoSnake.ElementAt<Vector2>(playerTwoSnake.Count - 1).Y < 0)
                {
                    if (soundEnabled)
                    {
                        sfxGameEnd.Play();
                    }
                    winner = Player.one;
                }
            }
        }

        /// <summary>
        /// Checks for collisions between the snakes and points.
        /// </summary>
        public void UpdateCollision()
        {
            //Checks for collision between the first snake and itself.
            for (int i = 0; i < playerOneSnake.Count - 1; i++)
            {
                if ((playerOneSnake.Count - 1) != 0)
                {
                    if (
                        playerOneSnake.ElementAt<Vector2>(playerOneSnake.Count - 1).X == playerOneSnake.ElementAt<Vector2>(i).X &&
                        playerOneSnake.ElementAt<Vector2>(playerOneSnake.Count - 1).Y == playerOneSnake.ElementAt<Vector2>(i).Y)
                    {
                        if (soundEnabled)
                        {
                            sfxGameEnd.Play();
                        }
                        winner = Player.two;
                    }
                }
            }

            if (twoPlayer)
            {
                //Checks for collision between the second snake and itself.
                for (int i = 0; i < playerTwoSnake.Count - 1; i++)
                {
                    if ((playerTwoSnake.Count - 1) != 0)
                    {
                        if (
                            playerTwoSnake.ElementAt<Vector2>(playerTwoSnake.Count - 1).X == playerTwoSnake.ElementAt<Vector2>(i).X &&
                            playerTwoSnake.ElementAt<Vector2>(playerTwoSnake.Count - 1).Y == playerTwoSnake.ElementAt<Vector2>(i).Y)
                        {
                            if (soundEnabled)
                            {
                                sfxGameEnd.Play();
                            }
                            winner = Player.one;
                        }
                    }
                }

                //Checks if the first player hits the second.
                for (int i = 0; i < playerTwoSnake.Count; i++)
                {
                    if (
                        playerOneSnake.ElementAt<Vector2>(playerOneSnake.Count - 1).X == playerTwoSnake.ElementAt<Vector2>(i).X &&
                        playerOneSnake.ElementAt<Vector2>(playerOneSnake.Count - 1).Y == playerTwoSnake.ElementAt<Vector2>(i).Y)
                    {
                        if (soundEnabled)
                        {
                            sfxGameEnd.Play();
                        }
                        winner = Player.two;
                    }
                }

                //Checks if the second player hits the first player.
                for (int i = 0; i < playerOneSnake.Count; i++)
                {
                    if (
                        playerOneSnake.ElementAt<Vector2>(i).X == playerTwoSnake.ElementAt<Vector2>(playerTwoSnake.Count - 1).X &&
                        playerOneSnake.ElementAt<Vector2>(i).Y == playerTwoSnake.ElementAt<Vector2>(playerTwoSnake.Count - 1).Y)
                    {
                        if (soundEnabled)
                        {
                            sfxGameEnd.Play();
                        }
                        winner = Player.one;
                    }
                }

                //If both hit each other dead-on.
                if (
                    playerOneSnake.ElementAt<Vector2>(playerOneSnake.Count - 1).X == playerTwoSnake.ElementAt<Vector2>(playerTwoSnake.Count - 1).X &&
                    playerOneSnake.ElementAt<Vector2>(playerOneSnake.Count - 1).Y == playerTwoSnake.ElementAt<Vector2>(playerTwoSnake.Count - 1).Y)
                {
                    if (soundEnabled)
                    {
                        sfxGameEnd.Play();
                    }
                    winner = Player.none;
                }
            }

            //Updates the point array.
            for (int i = 0; i < gamePoints.Count; i++)
            {
                if (!gamePoints[i].markedForDeletion)
                {
                    //If there's a direct collision between player one and a point.
                    if ((playerOneSnake.ElementAt<Vector2>(playerOneSnake.Count - 1).X == gamePoints[i].xPos) &&
                        (playerOneSnake.ElementAt<Vector2>(playerOneSnake.Count - 1).Y == gamePoints[i].yPos))
                    {
                        gamePoints[i].playerWhoCaptured = Player.one;
                        gamePoints[i].markedForDeletion = true;
                    }

                    if (twoPlayer)
                    {
                        //If there's a direct collision between player two and a point.
                        if ((playerTwoSnake.ElementAt<Vector2>(playerTwoSnake.Count - 1).X == gamePoints[i].xPos) &&
                            (playerTwoSnake.ElementAt<Vector2>(playerTwoSnake.Count - 1).Y == gamePoints[i].yPos))
                        {
                            gamePoints[i].playerWhoCaptured = Player.two;
                            gamePoints[i].markedForDeletion = true;
                        }
                    }
                }
            }
        }
    }
}