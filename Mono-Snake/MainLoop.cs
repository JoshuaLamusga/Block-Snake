using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BlockSnake
{
    /// <summary>
    /// Handles execution of the game.
    /// </summary>
    public class MainLoop : Game
    {
        #region Members
        //The graphics device and batcher to handle graphics.
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        //Fonts.
        private SpriteFont fntDefault, fntSmall, fntTiny;

        //Textures and sounds.
        public Texture2D sprPoint;
        private Texture2D sprSpriteBlock;
        private Texture2D sprLineHor, sprLineVer;
        private Texture2D sprTitlePlay, sprTitleInfo, sprTitleOps;
        private Texture2D sprTitleLogo, sprTitleSnakeSelector;
        private Texture2D sprTitlePlayPlayers, sprTitlePlayPlayersOne, sprTitlePlayPlayersTwo;
        private Texture2D sprTitlePlayPlayersThree, sprTitlePlayPlayersFour;
        private Texture2D sprTitleGameStart, sprTitleGameStartBlue, sprTitleGameStartRed;
        private Texture2D sprTitleGameStartGreen, sprTitleGameStartYellow;
        private Texture2D sprTitleGamePaused, sprTitleInfoBack, sprTitleInfoHowToPlay;
        private Texture2D sprTitleOpsGridSmall, sprTitleOpsGridMedium, sprTitleOpsGridLarge;
        private Texture2D sprTitleOpsToggleGrid, sprTitleOpsToggleSfx;
        private SoundEffect sfxGamePoint, sfxGameEnd, sfxMenuClick, sfxMenuSwitch;
        private SoundEffect sfxMusic01, sfxMusic02, sfxMusic03, sfxMusic04;

        //Gamepad and keyboard controls.
        private GamePadState gpState1, gpState1Old, gpState2, gpState2Old;
        private GamePadState gpState3, gpState3Old, gpState4, gpState4Old;
        private KeyboardState kbState, kbStateOld;

        /// <summary>
        /// The user-highlighted button in menus.
        /// </summary>
        public MenuButtons activeMenuButton;

        /// <summary>
        /// The dimensions of the grid and default update speed.
        /// </summary>
        public GridSize gridSize;

        /// <summary>
        /// The number of update calls to skip before updating the game.
        /// </summary>
        private float numUpdatesToRefresh;

        /// <summary>
        /// The current number of update calls since the last actual update.
        /// </summary>
        private float numUpdates;

        /// <summary>
        /// Used to create randomness.
        /// </summary>
        private Random rng;

        /// <summary>
        /// The width of the screen.
        /// </summary>
        private int scrWidth;

        /// <summary>
        /// The height of the screen.
        /// </summary>
        private int scrHeight;

        /// <summary>
        /// Contains a list of all blocks occupied by each player.
        /// </summary>
        private List<Queue<Vector2>> playerSnakes;

        /// <summary>
        /// Contains a list of all previous player positions last update.
        /// </summary>
        private List<Vector2> playerDeletedBlocks;

        /// <summary>
        /// Stores all player directions.
        /// </summary>
        private List<PlayerDir> playerDirections;

        /// <summary>
        /// Contains a list of all points on the map.
        /// </summary>
        private List<GamePoint> gamePoints;

        /// <summary>
        /// Contains a list of all points queued to be deleted.
        /// </summary>
        private List<GamePoint> gamePointsToDelete;

        /// <summary>
        /// Tracks the list of all players that have lost.
        /// </summary>
        private ObservableCollection<PlayerNum> losers;

        /// <summary>
        /// Tracks the players that have lost during this update. If the
        /// game ties, only these players are eligible to break it.
        /// </summary>
        private List<PlayerNum> losersThisUpdate;

        /// <summary>
        /// Tracks the resulting winner of a game.
        /// </summary>
        private PlayerNum winner;

        /// <summary>
        /// Whether or not the game is paused.
        /// </summary>
        private bool isPaused;

        /// <summary>
        /// Whether or not sound is muted.
        /// </summary>
        private bool isSoundEnabled;

        /// <summary>
        /// Whether or not to draw the grid during gameplay.
        /// </summary>
        private bool doDrawGrid;

        /// <summary>
        /// The music player.
        /// </summary>
        private MusicPlayer musicList;

        /// <summary>
        /// The number of active players.
        /// </summary>
        private int numPlayers;

        /// <summary>
        /// The state of the game, e.g. menus or gameplay.
        /// </summary>
        private GameState gameState;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes the game.
        /// </summary>
        public MainLoop()
        {
            //Sets the default graphics and content management.
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            //Sets the default active button in the menu.
            activeMenuButton = MenuButtons.Play;

            //Sets the default size of the grid.
            gridSize = GridSize.Medium;

            //The number of updates towards the next refresh starts at 0.
            numUpdates = 0;

            //Sets the random number generator.
            rng = new Random();

            //Populates the snake block lists and player direction.
            int numPlayersExpected = 4;
            playerSnakes = new List<Queue<Vector2>>(numPlayersExpected);
            playerDeletedBlocks = new List<Vector2>(numPlayersExpected);
            playerDirections = new List<PlayerDir>(numPlayersExpected);

            for (int i = 0; i < numPlayersExpected; i++)
            {
                playerSnakes.Add(new Queue<Vector2>());
                playerDeletedBlocks.Add(Vector2.Zero);
                playerDirections.Add(PlayerDir.None);
            }

            //Creates a list of points (and a deletion list).
            gamePoints = new List<GamePoint>();
            gamePointsToDelete = new List<GamePoint>();

            //Sets default win/lose statuses.
            losersThisUpdate = new List<PlayerNum>();
            losers = new ObservableCollection<PlayerNum>();
            winner = PlayerNum.All;

            //Sets defaults for game options.
            isPaused = false;
            isSoundEnabled = true;
            doDrawGrid = false;

            //Sets miscellaneous details.
            numPlayers = 1;
            gameState = GameState.Menu;

            //Sets the game to start in the center of the screen.
            Window.Position = Window.ClientBounds.Center;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Loads all textures and sounds.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            fntDefault = Content.Load<SpriteFont>("fntDefault");
            fntSmall = Content.Load<SpriteFont>("fntSmall");
            fntTiny = Content.Load<SpriteFont>("fntTiny");

            sfxGamePoint = Content.Load<SoundEffect>("sfxGamePoint");
            sfxGameEnd = Content.Load<SoundEffect>("sfxGameEnd");
            sfxMenuClick = Content.Load<SoundEffect>("sfxMenuClick");
            sfxMenuSwitch = Content.Load<SoundEffect>("sfxMenuSwitch");
            sfxMusic01 = Content.Load<SoundEffect>("sfxMusic01");
            sfxMusic02 = Content.Load<SoundEffect>("sfxMusic02");
            sfxMusic03 = Content.Load<SoundEffect>("sfxMusic03");
            sfxMusic04 = Content.Load<SoundEffect>("sfxMusic04");

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
            sprTitlePlayPlayersThree = Content.Load<Texture2D>("sprTitlePlayPlayersThree");
            sprTitlePlayPlayersFour = Content.Load<Texture2D>("sprTitlePlayPlayersFour");
            sprTitleSnakeSelector = Content.Load<Texture2D>("sprTitleSnakeSelector");
            sprTitleGameStart = Content.Load<Texture2D>("sprTitleGameStart");
            sprTitleGameStartRed = Content.Load<Texture2D>("sprTitleGameStartRed");
            sprTitleGameStartBlue = Content.Load<Texture2D>("sprTitleGameStartBlue");
            sprTitleGameStartGreen = Content.Load<Texture2D>("sprTitleGameStartGreen");
            sprTitleGameStartYellow = Content.Load<Texture2D>("sprTitleGameStartYellow");
            sprTitleGamePaused = Content.Load<Texture2D>("sprTitleGamePaused");
            sprTitleOpsGridSmall = Content.Load<Texture2D>("sprTitleSizeSmall");
            sprTitleOpsGridMedium = Content.Load<Texture2D>("sprTitleSizeMedium");
            sprTitleOpsGridLarge = Content.Load<Texture2D>("sprTitleSizeLarge");
            sprTitleInfoBack = Content.Load<Texture2D>("sprTitleInfoBack");
            sprTitleInfoHowToPlay = Content.Load<Texture2D>("sprTitleInfoHowToPlay");
            sprTitleOpsToggleGrid = Content.Load<Texture2D>("sprTitleOpsToggleGrid");
            sprTitleOpsToggleSfx = Content.Load<Texture2D>("sprTitleOpsToggleSfx");
            sprPoint = Content.Load<Texture2D>("sprPoint");

            //Sets the height and width of the screen.
            scrWidth = GraphicsDevice.Viewport.Width;
            scrHeight = GraphicsDevice.Viewport.Height;

            //Sets up the music player.
            musicList = new MusicPlayer(sfxMusic01, sfxMusic02, sfxMusic03, sfxMusic04);
            musicList.NextSoundRandom();

            //Sets the default gamepad and keyboard states.
            gpState1 = GamePad.GetState(PlayerIndex.One);
            gpState2 = GamePad.GetState(PlayerIndex.Two);
            gpState3 = GamePad.GetState(PlayerIndex.Three);
            gpState4 = GamePad.GetState(PlayerIndex.Four);
            kbState = Keyboard.GetState();
        }

        /// <summary>
        /// Handles gameplay interaction in all rooms.
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            //Sets the old gamepad and keyboard states.
            gpState1Old = gpState1;
            gpState2Old = gpState2;
            gpState3Old = gpState3;
            gpState4Old = gpState4;
            kbStateOld = kbState;

            //Gets the current gamepad and keyboard states.
            gpState1 = GamePad.GetState(PlayerIndex.One);
            gpState2 = GamePad.GetState(PlayerIndex.Two);
            gpState3 = GamePad.GetState(PlayerIndex.Three);
            gpState4 = GamePad.GetState(PlayerIndex.Four);
            kbState = Keyboard.GetState();

            //Updates the music if sound is enabled.
            if (isSoundEnabled)
            {
                musicList.Update();
            }

            //Toggles music.
            if ((kbState.IsKeyDown(Keys.Y) && kbStateOld.IsKeyUp(Keys.Y)) ||
                gpState1.IsButtonDown(Buttons.Y) && gpState1Old.IsButtonUp(Buttons.Y) ||
                gpState2.IsButtonDown(Buttons.Y) && gpState2Old.IsButtonUp(Buttons.Y) ||
                gpState3.IsButtonDown(Buttons.Y) && gpState3Old.IsButtonUp(Buttons.Y) ||
                gpState4.IsButtonDown(Buttons.Y) && gpState4Old.IsButtonUp(Buttons.Y))
            {
                isSoundEnabled = !isSoundEnabled;

                if (isSoundEnabled)
                {
                    //Starts playing music again when turned back on.
                    musicList.NextSoundRandom();
                }
                else
                {
                    //Stops music abruptly when turned off.
                    musicList.sound.Stop();
                }
            }

            //If the game state is the menu.
            if (gameState == GameState.Menu)
            {
                //Moving upwards
                if ((kbState.IsKeyDown(Keys.Up) && kbStateOld.IsKeyUp(Keys.Up)) ||
                    gpState1.IsButtonDown(Buttons.LeftThumbstickUp) && gpState1Old.IsButtonUp(Buttons.LeftThumbstickUp) ||
                    gpState1.IsButtonDown(Buttons.DPadUp) && gpState1Old.IsButtonUp(Buttons.DPadUp) ||
                    gpState2.IsButtonDown(Buttons.LeftThumbstickUp) && gpState2Old.IsButtonUp(Buttons.LeftThumbstickUp) ||
                    gpState2.IsButtonDown(Buttons.DPadUp) && gpState2Old.IsButtonUp(Buttons.DPadUp) ||
                    gpState3.IsButtonDown(Buttons.LeftThumbstickUp) && gpState3Old.IsButtonUp(Buttons.LeftThumbstickUp) ||
                    gpState3.IsButtonDown(Buttons.DPadUp) && gpState3Old.IsButtonUp(Buttons.DPadUp) ||
                    gpState4.IsButtonDown(Buttons.LeftThumbstickUp) && gpState4Old.IsButtonUp(Buttons.LeftThumbstickUp) ||
                    gpState4.IsButtonDown(Buttons.DPadUp) && gpState4Old.IsButtonUp(Buttons.DPadUp))
                {
                    if (isSoundEnabled)
                    {
                        sfxMenuSwitch.Play();
                    }

                    switch (activeMenuButton)
                    {
                        case (MenuButtons.Play):
                            activeMenuButton = MenuButtons.Ops;
                            break;
                        case (MenuButtons.Info):
                            activeMenuButton = MenuButtons.Play;
                            break;
                        case (MenuButtons.Ops):
                            activeMenuButton = MenuButtons.Info;
                            break;
                    }
                }

                //Moving downwards
                if (kbState.IsKeyDown(Keys.Down) && kbStateOld.IsKeyUp(Keys.Down) ||
                    gpState1.IsButtonDown(Buttons.LeftThumbstickDown) && gpState1Old.IsButtonUp(Buttons.LeftThumbstickDown) ||
                    gpState1.IsButtonDown(Buttons.DPadDown) && gpState1Old.IsButtonUp(Buttons.DPadDown) ||
                    gpState2.IsButtonDown(Buttons.LeftThumbstickDown) && gpState2Old.IsButtonUp(Buttons.LeftThumbstickDown) ||
                    gpState2.IsButtonDown(Buttons.DPadDown) && gpState2Old.IsButtonUp(Buttons.DPadDown) ||
                    gpState3.IsButtonDown(Buttons.LeftThumbstickDown) && gpState3Old.IsButtonUp(Buttons.LeftThumbstickDown) ||
                    gpState3.IsButtonDown(Buttons.DPadDown) && gpState3Old.IsButtonUp(Buttons.DPadDown) ||
                    gpState4.IsButtonDown(Buttons.LeftThumbstickDown) && gpState4Old.IsButtonUp(Buttons.LeftThumbstickDown) ||
                    gpState4.IsButtonDown(Buttons.DPadDown) && gpState4Old.IsButtonUp(Buttons.DPadDown))
                {
                    if (isSoundEnabled)
                    {
                        sfxMenuSwitch.Play();
                    }

                    switch (activeMenuButton)
                    {
                        case (MenuButtons.Play):
                            activeMenuButton = MenuButtons.Info;
                            break;
                        case (MenuButtons.Info):
                            activeMenuButton = MenuButtons.Ops;
                            break;
                        case (MenuButtons.Ops):
                            activeMenuButton = MenuButtons.Play;
                            break;
                    }
                }
                
                //What happens when the buttons are clicked.
                if (kbState.IsKeyDown(Keys.Enter) && kbStateOld.IsKeyUp(Keys.Enter) ||
                    gpState1.IsButtonDown(Buttons.A) && gpState1Old.IsButtonUp(Buttons.A) ||
                    gpState2.IsButtonDown(Buttons.A) && gpState2Old.IsButtonUp(Buttons.A) ||
                    gpState3.IsButtonDown(Buttons.A) && gpState3Old.IsButtonUp(Buttons.A) ||
                    gpState4.IsButtonDown(Buttons.A) && gpState4Old.IsButtonUp(Buttons.A))
                {
                    if (isSoundEnabled)
                    {
                        sfxMenuClick.Play();
                    }

                    switch (activeMenuButton)
                    {
                        case (MenuButtons.Play):
                            activeMenuButton = MenuButtons.OnePlayer;
                            gameState = GameState.MenuPlay;
                            break;
                        case (MenuButtons.Info):
                            activeMenuButton = MenuButtons.Info;
                            gameState = GameState.MenuInfo;
                            break;
                        case (MenuButtons.Ops):
                            activeMenuButton = MenuButtons.SmallGrid;
                            gameState = GameState.MenuOptions;
                            break;
                    }
                }
            }
            
            //If the game state is the play submenu.
            else if (gameState == GameState.MenuPlay)
            {
                //Switches between the number of active players (up).
                if (kbState.IsKeyDown(Keys.Up) && kbStateOld.IsKeyUp(Keys.Up) ||
                    gpState1.IsButtonDown(Buttons.LeftThumbstickUp) && gpState1Old.IsButtonUp(Buttons.LeftThumbstickUp) ||
                    gpState1.IsButtonDown(Buttons.DPadUp) && gpState1Old.IsButtonUp(Buttons.DPadUp) ||
                    gpState2.IsButtonDown(Buttons.LeftThumbstickUp) && gpState2Old.IsButtonUp(Buttons.LeftThumbstickUp) ||
                    gpState2.IsButtonDown(Buttons.DPadUp) && gpState2Old.IsButtonUp(Buttons.DPadUp) ||
                    gpState3.IsButtonDown(Buttons.LeftThumbstickUp) && gpState3Old.IsButtonUp(Buttons.LeftThumbstickUp) ||
                    gpState3.IsButtonDown(Buttons.DPadUp) && gpState3Old.IsButtonUp(Buttons.DPadUp) ||
                    gpState4.IsButtonDown(Buttons.LeftThumbstickUp) && gpState4Old.IsButtonUp(Buttons.LeftThumbstickUp) ||
                    gpState4.IsButtonDown(Buttons.DPadUp) && gpState4Old.IsButtonUp(Buttons.DPadUp))
                {
                    if (isSoundEnabled)
                    {
                        sfxMenuSwitch.Play();
                    }

                    switch (activeMenuButton)
                    {
                        case (MenuButtons.OnePlayer):
                            activeMenuButton = MenuButtons.Back;
                            break;
                        case (MenuButtons.TwoPlayer):
                            activeMenuButton = MenuButtons.OnePlayer;
                            break;
                        case (MenuButtons.ThreePlayer):
                            activeMenuButton = MenuButtons.TwoPlayer;
                            break;
                        case (MenuButtons.FourPlayer):
                            activeMenuButton = MenuButtons.ThreePlayer;
                            break;
                        case (MenuButtons.Back):
                            activeMenuButton = MenuButtons.FourPlayer;
                            break;
                    }
                }

                //Switches between the number of active players (down).
                if (kbState.IsKeyDown(Keys.Down) && kbStateOld.IsKeyUp(Keys.Down) ||
                    gpState1.IsButtonDown(Buttons.LeftThumbstickDown) && gpState1Old.IsButtonUp(Buttons.LeftThumbstickDown) ||
                    gpState1.IsButtonDown(Buttons.DPadDown) && gpState1Old.IsButtonUp(Buttons.DPadDown) ||
                    gpState2.IsButtonDown(Buttons.LeftThumbstickDown) && gpState2Old.IsButtonUp(Buttons.LeftThumbstickDown) ||
                    gpState2.IsButtonDown(Buttons.DPadDown) && gpState2Old.IsButtonUp(Buttons.DPadDown) ||
                    gpState3.IsButtonDown(Buttons.LeftThumbstickDown) && gpState3Old.IsButtonUp(Buttons.LeftThumbstickDown) ||
                    gpState3.IsButtonDown(Buttons.DPadDown) && gpState3Old.IsButtonUp(Buttons.DPadDown) ||
                    gpState4.IsButtonDown(Buttons.LeftThumbstickDown) && gpState4Old.IsButtonUp(Buttons.LeftThumbstickDown) ||
                    gpState4.IsButtonDown(Buttons.DPadDown) && gpState4Old.IsButtonUp(Buttons.DPadDown))
                {
                    if (isSoundEnabled)
                    {
                        sfxMenuSwitch.Play();
                    }

                    switch (activeMenuButton)
                    {
                        case (MenuButtons.OnePlayer):
                            activeMenuButton = MenuButtons.TwoPlayer;
                            break;
                        case (MenuButtons.TwoPlayer):
                            activeMenuButton = MenuButtons.ThreePlayer;
                            break;
                        case (MenuButtons.ThreePlayer):
                            activeMenuButton = MenuButtons.FourPlayer;
                            break;
                        case (MenuButtons.FourPlayer):
                            activeMenuButton = MenuButtons.Back;
                            break;
                        case (MenuButtons.Back):
                            activeMenuButton = MenuButtons.OnePlayer;
                            break;
                    }
                }

                //What happens when the buttons are clicked.
                if (kbState.IsKeyDown(Keys.Enter) && kbStateOld.IsKeyUp(Keys.Enter) ||
                    gpState1.IsButtonDown(Buttons.A) && gpState1Old.IsButtonUp(Buttons.A) ||
                    gpState2.IsButtonDown(Buttons.A) && gpState2Old.IsButtonUp(Buttons.A) ||
                    gpState3.IsButtonDown(Buttons.A) && gpState3Old.IsButtonUp(Buttons.A) ||
                    gpState4.IsButtonDown(Buttons.A) && gpState4Old.IsButtonUp(Buttons.A))
                {
                    if (isSoundEnabled)
                    {
                        sfxMenuClick.Play();
                    }

                    //Goes back if pressed.
                    if (activeMenuButton == MenuButtons.Back)
                    {
                        gameState = GameState.Menu;
                        activeMenuButton = MenuButtons.Play;
                    }

                    //Prepares for gameplay if number of players is selected.
                    if (activeMenuButton == MenuButtons.OnePlayer ||
                        activeMenuButton == MenuButtons.TwoPlayer ||
                        activeMenuButton == MenuButtons.ThreePlayer ||
                        activeMenuButton == MenuButtons.FourPlayer)
                    {
                        //Clears the list of blocks owned by each snake.
                        playerSnakes.ForEach(o => o.Clear());

                        //Removes dots from the screen and the dot deletion list.
                        gamePoints = new List<GamePoint>();
                        gamePointsToDelete = new List<GamePoint>();

                        //Resets gameplay refresh speed based on grid size.
                        numUpdatesToRefresh = GetGridUpdateSpeed(gridSize);

                        //Resets time until game refreshes.
                        numUpdates = 0;

                        //Resets snake directions.
                        for (int i = 0; i < playerDirections.Count; i++)
                        {
                            playerDirections[i] = PlayerDir.None;
                        }

                        //Sets the number of active players.
                        if (activeMenuButton == MenuButtons.OnePlayer)
                        {
                            numPlayers = 1;
                        }
                        else if (activeMenuButton == MenuButtons.TwoPlayer)
                        {
                            numPlayers = 2;
                        }
                        else if (activeMenuButton == MenuButtons.ThreePlayer)
                        {
                            numPlayers = 3;
                        }
                        else if (activeMenuButton == MenuButtons.FourPlayer)
                        {
                            numPlayers = 4;
                        }

                        //Positions each snake randomly so none are overlapping.
                        List<Vector2> snakePositions = new List<Vector2>();

                        Vector2 tempPos = new Vector2(
                            (int)(rng.Next(0, GetGridCols(gridSize)) * GetGridCellSizes(gridSize).X),
                            (int)(rng.Next(0, GetGridRows(gridSize)) * GetGridCellSizes(gridSize).Y));

                        for (int i = 0; i < numPlayers; i++)
                        {
                            while (snakePositions.Where(o => o.Equals(tempPos)).Count() > 0)
                            {
                                tempPos = new Vector2(
                                    (int)(rng.Next(0, GetGridCols(gridSize)) * GetGridCellSizes(gridSize).X),
                                    (int)(rng.Next(0, GetGridRows(gridSize)) * GetGridCellSizes(gridSize).Y));
                            }

                            snakePositions.Add(tempPos);
                        }

                        //Sets each position.
                        for (int i = 0; i < numPlayers; i++)
                        {
                            playerSnakes[i].Enqueue(snakePositions[i]);
                        }

                        //Sets the button to 'play' if the player returns to the main menu.
                        activeMenuButton = MenuButtons.Play;

                        //Sets the pixels in the grid and begins gameplay.                        
                        gameState = GameState.Gameplay;
                    }
                }
            }
            
            //If the game state is the info submenu.
            else if (gameState == GameState.MenuInfo)
            {
                //Exits back to the main menu.
                if (kbState.IsKeyDown(Keys.Enter) && kbStateOld.IsKeyUp(Keys.Enter) ||
                    gpState1.IsButtonDown(Buttons.A) && gpState1Old.IsButtonUp(Buttons.A) ||
                    gpState2.IsButtonDown(Buttons.A) && gpState2Old.IsButtonUp(Buttons.A) ||
                    gpState3.IsButtonDown(Buttons.A) && gpState3Old.IsButtonUp(Buttons.A) ||
                    gpState4.IsButtonDown(Buttons.A) && gpState4Old.IsButtonUp(Buttons.A))
                {
                    if (isSoundEnabled)
                    {
                        sfxMenuClick.Play();
                    }
                    activeMenuButton = MenuButtons.Play;
                    gameState = GameState.Menu;
                }
            }
            
            //If the game sate is the options submenu.
            else if (gameState == GameState.MenuOptions)
            {
                //Moving upwards
                if (kbState.IsKeyDown(Keys.Up) && kbStateOld.IsKeyUp(Keys.Up) ||
                    gpState1.IsButtonDown(Buttons.LeftThumbstickUp) && gpState1Old.IsButtonUp(Buttons.LeftThumbstickUp) ||
                    gpState1.IsButtonDown(Buttons.DPadUp) && gpState1Old.IsButtonUp(Buttons.DPadUp) ||
                    gpState2.IsButtonDown(Buttons.LeftThumbstickUp) && gpState2Old.IsButtonUp(Buttons.LeftThumbstickUp) ||
                    gpState2.IsButtonDown(Buttons.DPadUp) && gpState2Old.IsButtonUp(Buttons.DPadUp) ||
                    gpState3.IsButtonDown(Buttons.LeftThumbstickUp) && gpState3Old.IsButtonUp(Buttons.LeftThumbstickUp) ||
                    gpState3.IsButtonDown(Buttons.DPadUp) && gpState3Old.IsButtonUp(Buttons.DPadUp) ||
                    gpState4.IsButtonDown(Buttons.LeftThumbstickUp) && gpState4Old.IsButtonUp(Buttons.LeftThumbstickUp) ||
                    gpState4.IsButtonDown(Buttons.DPadUp) && gpState4Old.IsButtonUp(Buttons.DPadUp))
                {
                    if (isSoundEnabled)
                    {
                        sfxMenuSwitch.Play();
                    }

                    switch (activeMenuButton)
                    {
                        case (MenuButtons.SmallGrid):
                            activeMenuButton = MenuButtons.Back;
                            break;
                        case (MenuButtons.MediumGrid):
                            activeMenuButton = MenuButtons.SmallGrid;
                            break;
                        case (MenuButtons.LargeGrid):
                            activeMenuButton = MenuButtons.MediumGrid;
                            break;
                        case (MenuButtons.ToggleGrid):
                            activeMenuButton = MenuButtons.LargeGrid;
                            break;
                        case (MenuButtons.ToggleSfx):
                            activeMenuButton = MenuButtons.ToggleGrid;
                            break;
                        case (MenuButtons.Back):
                            activeMenuButton = MenuButtons.ToggleSfx;
                            break;
                    }
                }

                //Moving downwards
                if (kbState.IsKeyDown(Keys.Down) && kbStateOld.IsKeyUp(Keys.Down) ||
                    gpState1.IsButtonDown(Buttons.LeftThumbstickDown) && gpState1Old.IsButtonUp(Buttons.LeftThumbstickDown) ||
                    gpState1.IsButtonDown(Buttons.DPadDown) && gpState1Old.IsButtonUp(Buttons.DPadDown) ||
                    gpState2.IsButtonDown(Buttons.LeftThumbstickDown) && gpState2Old.IsButtonUp(Buttons.LeftThumbstickDown) ||
                    gpState2.IsButtonDown(Buttons.DPadDown) && gpState2Old.IsButtonUp(Buttons.DPadDown) ||
                    gpState3.IsButtonDown(Buttons.LeftThumbstickDown) && gpState3Old.IsButtonUp(Buttons.LeftThumbstickDown) ||
                    gpState3.IsButtonDown(Buttons.DPadDown) && gpState3Old.IsButtonUp(Buttons.DPadDown) ||
                    gpState4.IsButtonDown(Buttons.LeftThumbstickDown) && gpState4Old.IsButtonUp(Buttons.LeftThumbstickDown) ||
                    gpState4.IsButtonDown(Buttons.DPadDown) && gpState4Old.IsButtonUp(Buttons.DPadDown))
                {
                    if (isSoundEnabled)
                    {
                        sfxMenuSwitch.Play();
                     }

                    switch (activeMenuButton)
                    {
                        case (MenuButtons.SmallGrid):
                            activeMenuButton = MenuButtons.MediumGrid;
                            break;
                        case (MenuButtons.MediumGrid):
                            activeMenuButton = MenuButtons.LargeGrid;
                            break;
                        case (MenuButtons.LargeGrid):
                            activeMenuButton = MenuButtons.ToggleGrid;
                            break;
                        case (MenuButtons.ToggleGrid):
                            activeMenuButton = MenuButtons.ToggleSfx;
                            break;
                        case (MenuButtons.ToggleSfx):
                            activeMenuButton = MenuButtons.Back;
                            break;
                        case (MenuButtons.Back):
                            activeMenuButton = MenuButtons.SmallGrid;
                            break;
                    }
                }

                //Defines what happens when each button is pressed.
                if (kbState.IsKeyDown(Keys.Enter) && kbStateOld.IsKeyUp(Keys.Enter) ||
                    gpState1.IsButtonDown(Buttons.A) && gpState1Old.IsButtonUp(Buttons.A) ||
                    gpState2.IsButtonDown(Buttons.A) && gpState2Old.IsButtonUp(Buttons.A) ||
                    gpState3.IsButtonDown(Buttons.A) && gpState3Old.IsButtonUp(Buttons.A) ||
                    gpState4.IsButtonDown(Buttons.A) && gpState4Old.IsButtonUp(Buttons.A))
                {
                    if (isSoundEnabled)
                    {
                        sfxMenuClick.Play();
                    }

                    switch (activeMenuButton)
                    {
                        case (MenuButtons.SmallGrid):
                            gridSize = GridSize.Small;
                            break;
                        case (MenuButtons.MediumGrid):
                            gridSize = GridSize.Medium;
                            break;
                        case (MenuButtons.LargeGrid):
                            gridSize = GridSize.Large;
                            break;
                        case (MenuButtons.ToggleGrid):
                            doDrawGrid = !doDrawGrid;
                            break;
                        case (MenuButtons.ToggleSfx):
                            isSoundEnabled = !isSoundEnabled;                        
                            if (isSoundEnabled)
                            {
                                musicList.NextSoundRandom();
                            }
                            else
                            {
                                musicList.sound.Stop();
                            }
                            break;
                        case (MenuButtons.Back):
                            gameState = GameState.Menu;
                            activeMenuButton = MenuButtons.Play;
                            break;
                    }
                }
            }
            
            //If the game state is gameplay.
            else if (gameState == GameState.Gameplay)
            {
                //Checks if a player has won.
                if (winner != PlayerNum.All)
                {
                    //Returns to the menu if enter or start is pressed.
                    if (kbState.IsKeyUp(Keys.Enter) && kbStateOld.IsKeyDown(Keys.Enter) ||
                        gpState1.IsButtonUp(Buttons.Start) && gpState1Old.IsButtonDown(Buttons.Start) ||
                        gpState2.IsButtonUp(Buttons.Start) && gpState2Old.IsButtonDown(Buttons.Start) ||
                        gpState3.IsButtonUp(Buttons.Start) && gpState3Old.IsButtonDown(Buttons.Start) ||
                        gpState4.IsButtonUp(Buttons.Start) && gpState4Old.IsButtonDown(Buttons.Start))
                    {
                        if (isSoundEnabled)
                        {
                            sfxMenuClick.Play();
                        }

                        //Resets win/loss statuses.
                        losers.Clear();
                        winner = PlayerNum.All;

                        //Changes the room to the menu.
                        gameState = GameState.Menu;
                    }

                    return;
                }

                //Gets whether or not the game is paused.
                if (kbState.IsKeyDown(Keys.P) && kbStateOld.IsKeyUp(Keys.P) ||
                    gpState1.IsButtonDown(Buttons.B) && gpState1Old.IsButtonUp(Buttons.B) ||
                    gpState2.IsButtonDown(Buttons.B) && gpState2Old.IsButtonUp(Buttons.B) ||
                    gpState3.IsButtonDown(Buttons.B) && gpState3Old.IsButtonUp(Buttons.B) ||
                    gpState4.IsButtonDown(Buttons.B) && gpState4Old.IsButtonUp(Buttons.B))
                {
                    if (isPaused && winner == PlayerNum.All)
                    {
                        isPaused = false;
                    }
                    else if (!isPaused && winner == PlayerNum.All)
                    {
                        isPaused = true;
                    }
                }

                //Suspends functionality while paused.
                if (isPaused)
                {
                    return;
                }

                //Clear the list of players that lost since last update.
                losersThisUpdate.Clear();

                //Processes gamepad input for player 1.
                if (!losers.Contains(PlayerNum.One))
                {
                    if ((gpState1.ThumbSticks.Left.X > 0 &&
                        gpState1.ThumbSticks.Left.X > Math.Abs(gpState1.ThumbSticks.Left.Y)) ||
                        (gpState1.IsButtonDown(Buttons.DPadRight) && gpState1Old.IsButtonUp(Buttons.DPadRight)))
                    {
                        //Prevents moving backwards into yourself with 2+ blocks.
                        if (playerSnakes.Count < 2 ||
                            !IsOppositeDirection(playerDirections[0], PlayerDir.Right))
                        {
                            playerDirections[0] = PlayerDir.Right;
                        }
                    }
                    else if ((gpState1.ThumbSticks.Left.X < 0 &&
                        gpState1.ThumbSticks.Left.X < -Math.Abs(gpState1.ThumbSticks.Left.Y)) ||
                        (gpState1.IsButtonDown(Buttons.DPadLeft) && gpState1Old.IsButtonUp(Buttons.DPadLeft)))
                    {
                        if (playerSnakes.Count < 2 ||
                            !IsOppositeDirection(playerDirections[0], PlayerDir.Left))
                        {
                            playerDirections[0] = PlayerDir.Left;
                        }
                    }
                    else if ((gpState1.ThumbSticks.Left.Y < 0 &&
                        gpState1.ThumbSticks.Left.Y < Math.Abs(gpState1.ThumbSticks.Left.X)) ||
                        (gpState1.IsButtonDown(Buttons.DPadDown) && gpState1Old.IsButtonUp(Buttons.DPadDown)))
                    {
                        if (playerSnakes.Count < 2 ||
                            !IsOppositeDirection(playerDirections[0], PlayerDir.Down))
                        {
                            playerDirections[0] = PlayerDir.Down;
                        }
                    }
                    else if ((gpState1.ThumbSticks.Left.Y > 0 &&
                        gpState1.ThumbSticks.Left.Y > Math.Abs(gpState1.ThumbSticks.Left.X)) ||
                        (gpState1.IsButtonDown(Buttons.DPadUp) && gpState1Old.IsButtonUp(Buttons.DPadUp)))
                    {
                        if (playerSnakes.Count < 2 ||
                            !IsOppositeDirection(playerDirections[0], PlayerDir.Up))
                        {
                            playerDirections[0] = PlayerDir.Up;
                        }
                    }
                }
                //Processes gamepad input for player 2.
                if (!losers.Contains(PlayerNum.Two))
                {
                    if ((gpState2.ThumbSticks.Left.X > 0 &&
                        gpState2.ThumbSticks.Left.X > Math.Abs(gpState2.ThumbSticks.Left.Y)) ||
                        (gpState2.IsButtonDown(Buttons.DPadRight) && gpState2Old.IsButtonUp(Buttons.DPadRight)))
                    {
                        if (playerSnakes.Count < 2 ||
                            !IsOppositeDirection(playerDirections[1], PlayerDir.Right))
                        {
                            playerDirections[1] = PlayerDir.Right;
                        }
                    }
                    else if ((gpState2.ThumbSticks.Left.X < 0 &&
                        gpState2.ThumbSticks.Left.X < -Math.Abs(gpState2.ThumbSticks.Left.Y)) ||
                        (gpState2.IsButtonDown(Buttons.DPadLeft) && gpState2Old.IsButtonUp(Buttons.DPadLeft)))
                    {
                        if (playerSnakes.Count < 2 ||
                            !IsOppositeDirection(playerDirections[1], PlayerDir.Left))
                        {
                            playerDirections[1] = PlayerDir.Left;
                        }
                    }
                    else if ((gpState2.ThumbSticks.Left.Y < 0 &&
                        gpState2.ThumbSticks.Left.Y < Math.Abs(gpState2.ThumbSticks.Left.X)) ||
                        (gpState2.IsButtonDown(Buttons.DPadDown) && gpState2Old.IsButtonUp(Buttons.DPadDown)))
                    {
                        if (playerSnakes.Count < 2 ||
                            !IsOppositeDirection(playerDirections[1], PlayerDir.Down))
                        {
                            playerDirections[1] = PlayerDir.Down;
                        }
                    }
                    else if ((gpState2.ThumbSticks.Left.Y > 0 &&
                        gpState2.ThumbSticks.Left.Y > Math.Abs(gpState2.ThumbSticks.Left.X)) ||
                        (gpState2.IsButtonDown(Buttons.DPadUp) && gpState2Old.IsButtonUp(Buttons.DPadUp)))
                    {
                        if (playerSnakes.Count < 2 ||
                            !IsOppositeDirection(playerDirections[1], PlayerDir.Up))
                        {
                            playerDirections[1] = PlayerDir.Up;
                        }
                    }
                }
                //Processes gamepad input for player 3.
                if (!losers.Contains(PlayerNum.Three))
                {
                    if ((gpState3.ThumbSticks.Left.X > 0 &&
                        gpState3.ThumbSticks.Left.X > Math.Abs(gpState3.ThumbSticks.Left.Y)) ||
                        (gpState3.IsButtonDown(Buttons.DPadRight) && gpState3Old.IsButtonUp(Buttons.DPadRight)))
                    {
                        if (playerSnakes.Count < 2 ||
                            !IsOppositeDirection(playerDirections[2], PlayerDir.Right))
                        {
                            playerDirections[2] = PlayerDir.Right;
                        }
                    }
                    else if ((gpState3.ThumbSticks.Left.X < 0 &&
                        gpState3.ThumbSticks.Left.X < -Math.Abs(gpState3.ThumbSticks.Left.Y)) ||
                        (gpState3.IsButtonDown(Buttons.DPadLeft) && gpState3Old.IsButtonUp(Buttons.DPadLeft)))
                    {
                        if (playerSnakes.Count < 2 ||
                            !IsOppositeDirection(playerDirections[2], PlayerDir.Left))
                        {
                            playerDirections[2] = PlayerDir.Left;
                        }
                    }
                    else if ((gpState3.ThumbSticks.Left.Y < 0 &&
                        gpState3.ThumbSticks.Left.Y < Math.Abs(gpState3.ThumbSticks.Left.X)) ||
                        (gpState3.IsButtonDown(Buttons.DPadDown) && gpState3Old.IsButtonUp(Buttons.DPadDown)))
                    {
                        if (playerSnakes.Count < 2 ||
                            !IsOppositeDirection(playerDirections[2], PlayerDir.Down))
                        {
                            playerDirections[2] = PlayerDir.Down;
                        }
                    }
                    else if ((gpState3.ThumbSticks.Left.Y > 0 &&
                        gpState3.ThumbSticks.Left.Y > Math.Abs(gpState3.ThumbSticks.Left.X)) ||
                        (gpState3.IsButtonDown(Buttons.DPadUp) && gpState3Old.IsButtonUp(Buttons.DPadUp)))
                    {
                        if (playerSnakes.Count < 2 ||
                            !IsOppositeDirection(playerDirections[2], PlayerDir.Up))
                        {
                            playerDirections[2] = PlayerDir.Up;
                        }
                    }
                }
                //Processes gamepad input for player 4.
                if (!losers.Contains(PlayerNum.Four))
                {
                    if ((gpState4.ThumbSticks.Left.X > 0 &&
                        gpState4.ThumbSticks.Left.X > Math.Abs(gpState4.ThumbSticks.Left.Y)) ||
                        (gpState4.IsButtonDown(Buttons.DPadRight) && gpState4Old.IsButtonUp(Buttons.DPadRight)))
                    {
                        if (playerSnakes.Count < 2 ||
                            !IsOppositeDirection(playerDirections[3], PlayerDir.Right))
                        {
                            playerDirections[3] = PlayerDir.Right;
                        }
                    }
                    else if ((gpState4.ThumbSticks.Left.X < 0 &&
                        gpState4.ThumbSticks.Left.X < -Math.Abs(gpState4.ThumbSticks.Left.Y)) ||
                        (gpState4.IsButtonDown(Buttons.DPadLeft) && gpState4Old.IsButtonUp(Buttons.DPadLeft)))
                    {
                        if (playerSnakes.Count < 2 ||
                            !IsOppositeDirection(playerDirections[3], PlayerDir.Left))
                        {
                            playerDirections[3] = PlayerDir.Left;
                        }
                    }
                    else if ((gpState4.ThumbSticks.Left.Y < 0 &&
                        gpState4.ThumbSticks.Left.Y < Math.Abs(gpState4.ThumbSticks.Left.X)) ||
                        (gpState4.IsButtonDown(Buttons.DPadDown) && gpState4Old.IsButtonUp(Buttons.DPadDown)))
                    {
                        if (playerSnakes.Count < 2 ||
                            !IsOppositeDirection(playerDirections[3], PlayerDir.Down))
                        {
                            playerDirections[3] = PlayerDir.Down;
                        }
                    }
                    else if ((gpState4.ThumbSticks.Left.Y > 0 &&
                        gpState4.ThumbSticks.Left.Y > Math.Abs(gpState4.ThumbSticks.Left.X)) ||
                        (gpState4.IsButtonDown(Buttons.DPadUp) && gpState4Old.IsButtonUp(Buttons.DPadUp)))
                    {
                        if (playerSnakes.Count < 2 ||
                        !IsOppositeDirection(playerDirections[3], PlayerDir.Up))
                        {
                            playerDirections[3] = PlayerDir.Up;
                        }
                    }
                }

                //Updates the keyboard for player 1.
                if (!losers.Contains(PlayerNum.One))
                {
                    if (kbState.IsKeyDown(Keys.Right) && (playerSnakes[0].Count < 2 ||
                            !IsOppositeDirection(playerDirections[0], PlayerDir.Right)))
                    {
                        playerDirections[0] = PlayerDir.Right;
                    }
                    if (kbState.IsKeyDown(Keys.Up) && (playerSnakes[0].Count < 2 ||
                            !IsOppositeDirection(playerDirections[0], PlayerDir.Up)))
                    {
                        playerDirections[0] = PlayerDir.Up;
                    }
                    if (kbState.IsKeyDown(Keys.Left) && (playerSnakes[0].Count < 2 ||
                            !IsOppositeDirection(playerDirections[0], PlayerDir.Left)))
                    {
                        playerDirections[0] = PlayerDir.Left;
                    }
                    if (kbState.IsKeyDown(Keys.Down) && (playerSnakes[0].Count < 2 ||
                            !IsOppositeDirection(playerDirections[0], PlayerDir.Down)))
                    {
                        playerDirections[0] = PlayerDir.Down;
                    }
                }

                //Updates the keyboard for player 2.
                if (!losers.Contains(PlayerNum.Two) && numPlayers >= 2)
                {
                    if (kbState.IsKeyDown(Keys.D) && (playerSnakes[1].Count < 2 ||
                            !IsOppositeDirection(playerDirections[1], PlayerDir.Right)))
                    {
                        playerDirections[1] = PlayerDir.Right;
                    }
                    if (kbState.IsKeyDown(Keys.W) && (playerSnakes[1].Count < 2 ||
                            !IsOppositeDirection(playerDirections[1], PlayerDir.Up)))
                    {
                        playerDirections[1] = PlayerDir.Up;
                    }
                    if (kbState.IsKeyDown(Keys.A) && (playerSnakes[1].Count < 2 ||
                            !IsOppositeDirection(playerDirections[1], PlayerDir.Left)))
                    {
                        playerDirections[1] = PlayerDir.Left;
                    }
                    if (kbState.IsKeyDown(Keys.S) && (playerSnakes[1].Count < 2 ||
                            !IsOppositeDirection(playerDirections[1], PlayerDir.Down)))
                    {
                        playerDirections[1] = PlayerDir.Down;
                    }
                }

                //Updates the keyboard for player 3.
                if (!losers.Contains(PlayerNum.Three) && numPlayers >= 3)
                {
                    if (kbState.IsKeyDown(Keys.L) && (playerSnakes[2].Count < 2 ||
                            !IsOppositeDirection(playerDirections[2], PlayerDir.Right)))
                    {
                        playerDirections[2] = PlayerDir.Right;
                    }
                    if (kbState.IsKeyDown(Keys.I) && (playerSnakes[2].Count < 2 ||
                            !IsOppositeDirection(playerDirections[2], PlayerDir.Up)))
                    {
                        playerDirections[2] = PlayerDir.Up;
                    }
                    if (kbState.IsKeyDown(Keys.J) && (playerSnakes[2].Count < 2 ||
                            !IsOppositeDirection(playerDirections[2], PlayerDir.Left)))
                    {
                        playerDirections[2] = PlayerDir.Left;
                    }
                    if (kbState.IsKeyDown(Keys.K) && (playerSnakes[2].Count < 2 ||
                            !IsOppositeDirection(playerDirections[2], PlayerDir.Down)))
                    {
                        playerDirections[2] = PlayerDir.Down;
                    }
                }

                //Updates the keyboard for player 4.
                if (!losers.Contains(PlayerNum.Four) && numPlayers >= 4)
                {
                    if (kbState.IsKeyDown(Keys.H) && (playerSnakes[3].Count < 2 ||
                            !IsOppositeDirection(playerDirections[3], PlayerDir.Right)))
                    {
                        playerDirections[3] = PlayerDir.Right;
                    }
                    if (kbState.IsKeyDown(Keys.T) && (playerSnakes[3].Count < 2 ||
                            !IsOppositeDirection(playerDirections[3], PlayerDir.Up)))
                    {
                        playerDirections[3] = PlayerDir.Up;
                    }
                    if (kbState.IsKeyDown(Keys.F) && (playerSnakes[3].Count < 2 ||
                            !IsOppositeDirection(playerDirections[3], PlayerDir.Left)))
                    {
                        playerDirections[3] = PlayerDir.Left;
                    }
                    if (kbState.IsKeyDown(Keys.G) && (playerSnakes[3].Count < 2 ||
                            !IsOppositeDirection(playerDirections[3], PlayerDir.Down)))
                    {
                        playerDirections[3] = PlayerDir.Down;
                    }
                }

                //Doesn't start the game until all players have a direction.
                if (playerDirections.Take(numPlayers).Any(o => o == PlayerDir.None))
                {
                    return;
                }

                //Updates the timer that affects the overall update.
                numUpdates--;
                if (numUpdates <= 0)
                {
                    numUpdates = numUpdatesToRefresh;
                    UpdateMovement();
                    UpdateCollision();
                    UpdateWinStatus();
                }

                //Spawns points randomly and spawns a point if there are none.
                if ((gamePoints.Count < 2 && rng.Next(0, 1000) == 1) ||
                    gamePoints.Count == 0)
                {
                    //temporary variables to store the position on the screen.
                    Vector2 point = Vector2.Zero;
                    bool isValid = false;

                    //As long as there is open space on the screen.
                    if (playerSnakes.Sum(o => o.Count) < GetGridCols(gridSize) * GetGridRows(gridSize))
                    {
                        //If tempvalid is false.
                        while (!isValid)
                        {
                            isValid = true;

                            //Gets new values for the positions.
                            point = new Vector2(
                                (int)(rng.Next(0, GetGridCols(gridSize)) * GetGridCellSizes(gridSize).X),
                                (int)(rng.Next(0, GetGridRows(gridSize)) * GetGridCellSizes(gridSize).Y));

                            //The point cannot spawn in a block occupied by a player.
                            for (int i = 0; i < playerSnakes.Count; i++)
                            {
                                for (int j = 0; j < playerSnakes[i].Count; j++)
                                {
                                    if (playerSnakes[i].ElementAt(j).Equals(point))
                                    {
                                        isValid = false;
                                    }
                                }
                            }
                        }

                        gamePoints.Add(new GamePoint(this, point));
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
                        if (isSoundEnabled)
                        {
                            sfxGamePoint.Play();
                        }

                        gamePointsToDelete.Add(gamePoints[i]);

                        //Reverses the capturing snake to add a block to it.
                        if (gamePoints[i].playerWhoCaptured != PlayerNum.All &&
                            gamePoints[i].playerWhoCaptured != PlayerNum.None)
                        {
                            //Increases the game speed based on grid size.
                            if (gridSize == GridSize.Small)
                            {
                                if (numUpdatesToRefresh > 10)
                                {
                                    numUpdatesToRefresh -= 0.5f;
                                }
                                else if (numUpdatesToRefresh > 5)
                                {
                                    numUpdatesToRefresh -= 0.5f;
                                }
                                else if (numUpdatesToRefresh > 2)
                                {
                                    numUpdatesToRefresh -= 0.25f;
                                }
                            }
                            else if (gridSize == GridSize.Medium)
                            {
                                if (numUpdatesToRefresh > 10)
                                {
                                    numUpdatesToRefresh -= 0.75f;
                                }
                                else if (numUpdatesToRefresh > 5)
                                {
                                    numUpdatesToRefresh -= 0.5f;
                                }
                                else if (numUpdatesToRefresh > 2)
                                {
                                    numUpdatesToRefresh -= 0.25f;
                                }
                            }
                            else if (gridSize == GridSize.Large)
                            {
                                if (numUpdatesToRefresh > 10)
                                {
                                    numUpdatesToRefresh -= 1;
                                }
                                else if (numUpdatesToRefresh > 3)
                                {
                                    numUpdatesToRefresh -= 0.5f;
                                }
                                else if (numUpdatesToRefresh > 1)
                                {
                                    numUpdatesToRefresh -= 0.25f;
                                }
                            }

                            int playerIndex = (int)gamePoints[i].playerWhoCaptured;

                            //Adds a block to the opposite end of the queue.
                            playerSnakes[playerIndex] = new Queue<Vector2>(playerSnakes[playerIndex].Reverse());
                            playerSnakes[playerIndex].Enqueue(playerDeletedBlocks[playerIndex]);
                            playerSnakes[playerIndex] = new Queue<Vector2>(playerSnakes[playerIndex].Reverse());
                        }
                    }
                }

                //Goes through the deletion list.
                for (int i = 0; i < gamePointsToDelete.Count; i++)
                {
                    //Removes items from the gamePoints array.
                    gamePoints.Remove(gamePointsToDelete[i]);
                }

                //Clears the deletion array.
                gamePointsToDelete.Clear();
            }

            base.Update(gameTime);
        }

        /// <summary>
        /// Draws the game in all rooms.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);
            
            //Makes it so that there is no interpolation for stretching (nearest-neighbor algorithm).
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);

            //If the game state is the menu.
            if (gameState == GameState.Menu)
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

                //Draws the copyright.
                string copyright1 = "Copyright Joshua Lamusga 2014 | Music Copyright airtone";
                string copyright2 = "Music licensed under a Creative Commons Attribution Noncommercial 3.0 license";

                spriteBatch.DrawString(fntTiny,
                    copyright1,
                    new Vector2(scrWidth / 2 - fntTiny.MeasureString(copyright1).X/2, scrHeight / 5 + 332),
                    Color.Gray);

                spriteBatch.DrawString(fntTiny,
                    copyright2,
                    new Vector2(scrWidth / 2 - fntTiny.MeasureString(copyright2).X/2, scrHeight / 5 + 344),
                    Color.Gray);

                //Sets up a temporary y-coordinate for the selector image.
                //Based on the currently active button.
                int tempYPos = 0;
                
                switch (activeMenuButton)
                {
                    case (MenuButtons.Play):                        
                        tempYPos = scrHeight / 5 + 96;
                        break;
                    case (MenuButtons.Info):
                        tempYPos = scrHeight / 5 + 192;
                        break;
                    case (MenuButtons.Ops):
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

            //If the game state is the info room.
            else if (gameState == GameState.MenuInfo)
            {
                //Draws the instructions.
                spriteBatch.Draw(sprTitleInfoHowToPlay,
                    new Vector2(70, 20),
                    Color.White);

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

            //If the game state is in the options room.
            else if (gameState == GameState.MenuOptions)
            {
                //Draws the small button.
                spriteBatch.Draw(sprTitleOpsGridSmall,
                    new Rectangle(scrWidth / 2, scrHeight / 5, sprTitleOpsGridSmall.Width, sprTitleOpsGridSmall.Height),
                    sprTitleOpsGridSmall.Bounds,
                    Color.White,
                    0,
                    new Vector2(sprTitleOpsGridSmall.Width / 2, sprTitleOpsGridSmall.Height / 2),
                    SpriteEffects.None,
                    0);

                //Draws the medium button.
                spriteBatch.Draw(sprTitleOpsGridMedium,
                    new Rectangle(scrWidth / 2, scrHeight / 5 + 64, sprTitleOpsGridMedium.Width, sprTitleOpsGridMedium.Height),
                    sprTitleOpsGridMedium.Bounds,
                    Color.White,
                    0,
                    new Vector2(sprTitleOpsGridMedium.Width / 2, sprTitleOpsGridMedium.Height / 2),
                    SpriteEffects.None,
                    0);

                //Draws the large button.
                spriteBatch.Draw(sprTitleOpsGridLarge,
                    new Rectangle(scrWidth / 2, scrHeight / 5 + 128, sprTitleOpsGridLarge.Width, sprTitleOpsGridLarge.Height),
                    sprTitleOpsGridLarge.Bounds,
                    Color.White,
                    0,
                    new Vector2(sprTitleOpsGridLarge.Width / 2, sprTitleOpsGridLarge.Height / 2),
                    SpriteEffects.None,
                    0);

                //Draws the toggle grid button.
                spriteBatch.Draw(sprTitleOpsToggleGrid,
                    new Rectangle(scrWidth / 2, scrHeight / 5 + 192, sprTitleOpsToggleGrid.Width, sprTitleOpsToggleGrid.Height),
                    sprTitleOpsToggleGrid.Bounds,
                    Color.White,
                    0,
                    new Vector2(sprTitleOpsToggleGrid.Width / 2, sprTitleOpsToggleGrid.Height / 2),
                    SpriteEffects.None,
                    0);

                //Draws the toggle sfx button.
                spriteBatch.Draw(sprTitleOpsToggleSfx,
                    new Rectangle(scrWidth / 2, scrHeight / 5 + 256, sprTitleOpsToggleGrid.Width, sprTitleOpsToggleGrid.Height),
                    sprTitleOpsToggleGrid.Bounds,
                    Color.White,
                    0,
                    new Vector2(sprTitleOpsToggleSfx.Width / 2, sprTitleOpsToggleSfx.Height / 2),
                    SpriteEffects.None,
                    0);

                //Draws the back button.
                spriteBatch.Draw(sprTitleInfoBack,
                    new Rectangle(scrWidth / 2, scrHeight / 5 + 320, sprTitleInfoBack.Width, sprTitleInfoBack.Height),
                    sprTitleInfoBack.Bounds,
                    Color.White,
                    0,
                    new Vector2(sprTitleInfoBack.Width / 2, sprTitleInfoBack.Height / 2),
                    SpriteEffects.None,
                    0);

                //Sets up a temporary y-coordinate for the selector image.
                //Based on the currently active button.
                int tempYPos = 0;

                switch (activeMenuButton)
                {
                    case (MenuButtons.SmallGrid):
                        tempYPos = scrHeight / 5;
                        break;
                    case (MenuButtons.MediumGrid):
                        tempYPos = scrHeight / 5 + 64;
                        break;
                    case (MenuButtons.LargeGrid):
                        tempYPos = scrHeight / 5 + 128;
                        break;
                    case (MenuButtons.ToggleGrid):
                        tempYPos = scrHeight / 5 + 192;
                        break;
                    case (MenuButtons.ToggleSfx):
                        tempYPos = scrHeight / 5 + 256;
                        break;
                    case (MenuButtons.Back):
                        tempYPos = scrHeight / 5 + 320;
                        break;
                }

                //Draws the selector image.
                spriteBatch.Draw(sprTitleSnakeSelector,
                    new Rectangle(scrWidth / 2 - 196, tempYPos, sprTitleSnakeSelector.Width, sprTitleSnakeSelector.Height),
                    sprTitleSnakeSelector.Bounds,
                    Color.White,
                    0,
                    new Vector2(sprTitleSnakeSelector.Width / 2, sprTitleSnakeSelector.Height / 2),
                    SpriteEffects.None,
                    0);
            }

            //if the game state is the play menu. 
            else if (gameState == GameState.MenuPlay)
            {
                //Draws the 'players' text.
                spriteBatch.Draw(sprTitlePlayPlayers,
                    new Rectangle(scrWidth / 2, scrHeight / 5, sprTitlePlayPlayers.Width, sprTitlePlayPlayers.Height),
                    sprTitlePlayPlayers.Bounds,
                    Color.White,
                    0,
                    new Vector2(sprTitlePlayPlayers.Width / 2, sprTitlePlayPlayers.Height / 2),
                    SpriteEffects.None,
                    0);

                //Draws the one button.
                spriteBatch.Draw(sprTitlePlayPlayersOne,
                    new Rectangle(scrWidth / 2, scrHeight / 5 + 64, sprTitlePlayPlayersOne.Width, sprTitlePlayPlayersOne.Height),
                    sprTitlePlayPlayersOne.Bounds,
                    Color.White,
                    0,
                    new Vector2(sprTitlePlayPlayersOne.Width / 2, sprTitlePlayPlayersOne.Height / 2),
                    SpriteEffects.None,
                    0);

                //Draws the two button.
                spriteBatch.Draw(sprTitlePlayPlayersTwo,
                    new Rectangle(scrWidth / 2, scrHeight / 5 + 128, sprTitlePlayPlayersTwo.Width, sprTitlePlayPlayersTwo.Height),
                    sprTitlePlayPlayersTwo.Bounds,
                    Color.White,
                    0,
                    new Vector2(sprTitlePlayPlayersTwo.Width / 2, sprTitlePlayPlayersTwo.Height / 2),
                    SpriteEffects.None,
                    0);

                //Draws the three button.
                spriteBatch.Draw(sprTitlePlayPlayersThree,
                    new Rectangle(scrWidth / 2, scrHeight / 5 + 192, sprTitlePlayPlayersThree.Width, sprTitlePlayPlayersThree.Height),
                    sprTitlePlayPlayersThree.Bounds,
                    Color.White,
                    0,
                    new Vector2(sprTitlePlayPlayersThree.Width / 2, sprTitlePlayPlayersThree.Height / 2),
                    SpriteEffects.None,
                    0);

                //Draws the four button.
                spriteBatch.Draw(sprTitlePlayPlayersFour,
                    new Rectangle(scrWidth / 2, scrHeight / 5 + 256, sprTitlePlayPlayersFour.Width, sprTitlePlayPlayersFour.Height),
                    sprTitlePlayPlayersFour.Bounds,
                    Color.White,
                    0,
                    new Vector2(sprTitlePlayPlayersFour.Width / 2, sprTitlePlayPlayersFour.Height / 2),
                    SpriteEffects.None,
                    0);

                //Draws the back button.
                spriteBatch.Draw(sprTitleInfoBack,
                    new Rectangle(scrWidth / 2, scrHeight / 5 + 320, sprTitleInfoBack.Width, sprTitleInfoBack.Height),
                    sprTitleInfoBack.Bounds,
                    Color.White,
                    0,
                    new Vector2(sprTitleInfoBack.Width / 2, sprTitleInfoBack.Height / 2),
                    SpriteEffects.None,
                    0);

                //Sets up a temporary y-coordinate for the selector image.
                //Based on the currently active button.
                int tempYPos = 0;

                switch (activeMenuButton)
                {
                    case (MenuButtons.OnePlayer):
                        tempYPos = scrHeight / 5 + 64;
                        break;
                    case (MenuButtons.TwoPlayer):
                        tempYPos = scrHeight / 5 + 128;
                        break;
                    case (MenuButtons.ThreePlayer):
                        tempYPos = scrHeight / 5 + 192;
                        break;
                    case (MenuButtons.FourPlayer):
                        tempYPos = scrHeight / 5 + 256;
                        break;
                    case (MenuButtons.Back):
                        tempYPos = scrHeight / 5 + 320;
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
            
            //If the game state is the game.
            else if (gameState == GameState.Gameplay)
            {
                if (doDrawGrid)
                {
                    //Draws the grid to the screen.
                    //Draws all vertical rows.
                    for (float i = 0; i < scrHeight; i += GetGridCellSizes(gridSize).Y)
                    {
                        spriteBatch.Draw(sprLineHor, new Rectangle(0, (int)i, scrWidth, 1), Color.Black);
                    }

                    //Draws all horizontal rows.
                    for (float i = 0; i < scrWidth; i += GetGridCellSizes(gridSize).X)
                    {
                        spriteBatch.Draw(sprLineVer, new Rectangle((int)i, 0, 1, scrHeight), Color.Black);
                    }
                }

                //Draws all points.
                for (int i = 0; i < gamePoints.Count; i++)
                {
                    gamePoints[i].Draw(spriteBatch);
                }

                //Draws all players.
                List<Color> playerColors = new List<Color>()
                    { Color.Red, Color.Blue, Color.Green, Color.DarkGoldenrod };

                for (int i = 0; i < playerSnakes.Count; i++)
                {
                    //Players' snakes that lost are drawn in black.
                    if ((i == 0 && losers.Contains(PlayerNum.One)) ||
                        (i == 1 && losers.Contains(PlayerNum.Two)) ||
                        (i == 2 && losers.Contains(PlayerNum.Three)) ||
                        (i == 3 && losers.Contains(PlayerNum.Four)))
                    {
                        playerColors[i] = Color.Black;
                    }

                    //Draws each block.
                    for (int j = 0; j < playerSnakes[i].Count; j++)
                    {
                        spriteBatch.Draw(sprSpriteBlock, new Rectangle(
                            (int)playerSnakes[i].ElementAt(j).X,
                            (int)playerSnakes[i].ElementAt(j).Y,
                            (int)GetGridCellSizes(gridSize).X,
                            (int)GetGridCellSizes(gridSize).Y),
                            playerColors[i]);
                    }
                }

                //If the game hasn't yet started...
                if (playerDirections[0] == PlayerDir.None ||
                    (numPlayers >= 2 && playerDirections[1] == PlayerDir.None) ||
                    (numPlayers >= 3 && playerDirections[2] == PlayerDir.None) ||
                    (numPlayers >= 4 && playerDirections[3] == PlayerDir.None))
                {
                    //If no player has taken an initial direction.
                    if (playerDirections[0] == PlayerDir.None &&
                        (numPlayers < 2 || playerDirections[1] == PlayerDir.None) &&
                        (numPlayers < 3 || playerDirections[2] == PlayerDir.None) &&
                        (numPlayers < 4 || playerDirections[3] == PlayerDir.None))
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

                    //Displays which players haven't chosen a direction.
                    else
                    {
                        if (playerDirections[0] == PlayerDir.None)
                        {
                            spriteBatch.Draw(sprTitleGameStartRed,
                                new Rectangle(scrWidth / 2, scrHeight / 2, sprTitleGameStartRed.Width, sprTitleGameStartRed.Height),
                                sprTitleGameStartRed.Bounds,
                                Color.White * 0.5f,
                                0,
                                new Vector2(sprTitleGameStartRed.Width / 2, sprTitleGameStartRed.Height / 2),
                                SpriteEffects.None,
                                0);
                        }
                        if (numPlayers >= 2 && playerDirections[1] == PlayerDir.None)
                        {
                            spriteBatch.Draw(sprTitleGameStartBlue,
                                new Rectangle(scrWidth / 2, scrHeight / 2 + 64, sprTitleGameStartBlue.Width, sprTitleGameStartBlue.Height),
                                sprTitleGameStartBlue.Bounds,
                                Color.White * 0.5f,
                                0,
                                new Vector2(sprTitleGameStartBlue.Width / 2, sprTitleGameStartBlue.Height / 2),
                                SpriteEffects.None,
                                0);
                        }
                        if (numPlayers >= 3 && playerDirections[2] == PlayerDir.None)
                        {
                            spriteBatch.Draw(sprTitleGameStartGreen,
                                new Rectangle(scrWidth / 2, scrHeight / 2 + 128, sprTitleGameStartGreen.Width, sprTitleGameStartGreen.Height),
                                sprTitleGameStartGreen.Bounds,
                                Color.White * 0.5f,
                                0,
                                new Vector2(sprTitleGameStartGreen.Width / 2, sprTitleGameStartGreen.Height / 2),
                                SpriteEffects.None,
                                0);
                        }
                        if (numPlayers >= 4 && playerDirections[3] == PlayerDir.None)
                        {
                            spriteBatch.Draw(sprTitleGameStartYellow,
                                new Rectangle(scrWidth / 2, scrHeight / 2 + 192, sprTitleGameStartYellow.Width, sprTitleGameStartYellow.Height),
                                sprTitleGameStartYellow.Bounds,
                                Color.White * 0.5f,
                                0,
                                new Vector2(sprTitleGameStartYellow.Width / 2, sprTitleGameStartYellow.Height / 2),
                                SpriteEffects.None,
                                0);
                        }
                    }
                }

                //Draws the word 'paused' when the game is paused
                if (isPaused)
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

                //End of game messages in multiplayer.
                if (winner != PlayerNum.All && numPlayers >= 2)
                {
                    if (winner == PlayerNum.One)
                    {
                        spriteBatch.DrawString(
                            fntDefault,
                            "Red wins!",
                            new Vector2(scrWidth / 2 - (fntDefault.MeasureString("Red wins!").X / 2), scrHeight / 2),
                            Color.Red);
                    }
                    else if (winner == PlayerNum.Two)
                    {
                        spriteBatch.DrawString(
                            fntDefault,
                            "Blue wins!",
                            new Vector2(scrWidth / 2 - (fntDefault.MeasureString("Blue wins!").X / 2), scrHeight / 2),
                            Color.Blue);
                    }
                    else if (winner == PlayerNum.Three)
                    {
                        spriteBatch.DrawString(
                            fntDefault,
                            "Green wins!",
                            new Vector2(scrWidth / 2 - (fntDefault.MeasureString("Green wins!").X / 2), scrHeight / 2),
                            Color.Green);
                    }
                    else if (winner == PlayerNum.Four)
                    {
                        spriteBatch.DrawString(
                            fntDefault,
                            "Yellow wins!",
                            new Vector2(scrWidth / 2 - (fntDefault.MeasureString("Yellow wins!").X / 2), scrHeight / 2),
                            Color.DarkGoldenrod);
                    }
                    else if (winner == PlayerNum.None)
                    {
                        spriteBatch.DrawString(
                            fntDefault,
                            "Tie game",
                            new Vector2(scrWidth / 2 - (fntDefault.MeasureString("Tie game").X / 2), scrHeight / 2),
                            Color.Black);
                    }

                    //Displays the sizes of each snake on-screen.
                    string snakeSizes = "Red: " + playerSnakes[0].Count;
                    if (numPlayers >= 2) { snakeSizes += "  Blue: " + playerSnakes[1].Count; }
                    if (numPlayers >= 3) { snakeSizes += "  Green: " + playerSnakes[2].Count; }
                    if (numPlayers >= 4) { snakeSizes += "  Yellow " + playerSnakes[3].Count; }

                    spriteBatch.DrawString(
                        fntDefault,
                        snakeSizes,
                        new Vector2(scrWidth / 2 -
                            (fntDefault.MeasureString(snakeSizes).X / 2),
                            scrHeight / 2 + 64),
                        Color.DarkGray);

                    spriteBatch.DrawString(
                        fntDefault,
                        "Press Enter or Start to exit to menu.",
                        new Vector2(scrWidth / 2 - (fntDefault.MeasureString("Press Enter or Start to exit to menu.").X / 2), scrHeight / 2 + 160),
                        Color.Purple);
                }

                //End of game messages in single player.
                else if (losers.Count > 0 && numPlayers == 1)
                {
                    //Displays the size of the snake on-screen.
                    string snakeSize = "size: " + playerSnakes[0].Count;

                    spriteBatch.DrawString(
                        fntDefault,
                        snakeSize,
                        new Vector2(scrWidth / 2 - (fntDefault.MeasureString(snakeSize).X / 2),
                            scrHeight / 2 + 64),
                        Color.DarkGray);

                    spriteBatch.DrawString(
                        fntDefault,
                        "Press Enter or Start to exit to menu.",
                        new Vector2(scrWidth / 2 - (fntDefault.MeasureString("Press Enter or Start to exit to menu.").X / 2), scrHeight / 2 + 160),
                        Color.Purple);
                }
            }

            spriteBatch.End();
            base.Draw(gameTime);
        }

        /// <summary>
        /// Updates the snake positions based on direction.
        /// </summary>
        private void UpdateMovement()
        {
            //Keeps track of the player's direction.
            List<Vector2> tempPlayerDirections = new List<Vector2>();

            //Gets the next position of each player.
            for (int i = 0; i < numPlayers && i < playerDirections.Count; i++)
            {
                if (playerDirections[i] == PlayerDir.Right)
                {
                    tempPlayerDirections.Add(new Vector2(GetGridCellSizes(gridSize).X, 0));
                }
                else if (playerDirections[i] == PlayerDir.Down)
                {
                    tempPlayerDirections.Add(new Vector2(0, GetGridCellSizes(gridSize).Y));
                }
                else if (playerDirections[i] == PlayerDir.Left)
                {
                    tempPlayerDirections.Add(new Vector2(-GetGridCellSizes(gridSize).X, 0));
                }
                else if (playerDirections[i] == PlayerDir.Up)
                {
                    tempPlayerDirections.Add(new Vector2(0, -GetGridCellSizes(gridSize).Y));
                }

                //Players that haven't lost can move.
                if (!losers.Contains((PlayerNum)i))
                {
                    //Adds the new block so the player moves forward.
                    playerSnakes[i].Enqueue(new Vector2(
                        playerSnakes[i].Last().X + tempPlayerDirections[i].X,
                        playerSnakes[i].Last().Y + tempPlayerDirections[i].Y));

                    //Players out-of-bounds lose.
                    if (playerSnakes[i].Last().X >= scrWidth ||
                        playerSnakes[i].Last().Y >= scrHeight ||
                        playerSnakes[i].Last().X < 0 ||
                        playerSnakes[i].Last().Y < 0)
                    {
                        if (isSoundEnabled)
                        {
                            sfxGameEnd.Play();
                        }

                        losersThisUpdate.Add((PlayerNum)i);
                        losers.Add((PlayerNum)i);
                    }

                    //Removes the old block.
                    playerDeletedBlocks[i] = playerSnakes[i].Dequeue();
                }
            }
        }

        /// <summary>
        /// Checks for collisions between the snakes and points.
        /// </summary>
        private void UpdateCollision()
        {
            //Iterates through each player.
            for (int i = 0; i < playerSnakes.Count && i < numPlayers; i++)
            {
                //Skips players who lost.
                if (losers.Contains((PlayerNum)i))
                {
                    continue;
                }

                //Checks for collisions with each player.
                for (int j = 0; j < playerSnakes.Count && j < numPlayers; j++)
                {
                    //Checks for self-collisions.
                    if (i == j)
                    {
                        if (playerSnakes[i].Take(playerSnakes[i].Count - 1).Any(o =>
                        o.X == playerSnakes[i].Last().X &&
                        o.Y == playerSnakes[i].Last().Y))
                        {
                            if (isSoundEnabled)
                            {
                                sfxGameEnd.Play();
                            }

                            losersThisUpdate.Add((PlayerNum)i);
                            losers.Add((PlayerNum)i);
                        }
                    }

                    //Checks for collisions with other players.
                    else if (playerSnakes[j].Any(o => o.Equals(playerSnakes[i].Last())))
                    {
                        if (isSoundEnabled)
                        {
                            sfxGameEnd.Play();
                        }

                        losersThisUpdate.Add((PlayerNum)i);
                        losers.Add((PlayerNum)i);
                    }

                    //Checks for collisions between players of size 1.
                    else if (playerSnakes[i].Count == 1 && playerSnakes[j].Count == 1 &&
                        IsOppositeDirection(playerDirections[i], playerDirections[j]) &&
                        playerSnakes[i].Last().Equals(playerDeletedBlocks[j]))
                    {
                        losersThisUpdate.Add((PlayerNum)i);
                        losers.Add((PlayerNum)i);
                    }
                }
            }

            //Updates the point array.
            for (int i = 0; i < gamePoints.Count; i++)
            {
                if (!gamePoints[i].markedForDeletion)
                {
                    //Iterates through each player to see who collected it.
                    for (int j = 0; j < numPlayers && j < playerSnakes.Count; j++)
                    {
                        if (!losers.Contains((PlayerNum)j) &&
                            playerSnakes[j].Last().Equals(gamePoints[i].position))
                        {
                            gamePoints[i].playerWhoCaptured = (PlayerNum)j;
                            gamePoints[i].markedForDeletion = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks to see if any player has won and, if so, which player.
        /// </summary>
        private void UpdateWinStatus()
        {
            //In multiplayer, declares a winner with one player left.
            if (numPlayers != 1 && numPlayers - losers.Count == 1)
            {
                if (isSoundEnabled)
                {
                    sfxGameEnd.Play();
                }

                if (!losers.Contains(PlayerNum.One))
                {
                    winner = PlayerNum.One;
                }
                else if (!losers.Contains(PlayerNum.Two))
                {
                    winner = PlayerNum.Two;
                }
                else if (!losers.Contains(PlayerNum.Three))
                {
                    winner = PlayerNum.Three;
                }
                else if (!losers.Contains(PlayerNum.Four))
                {
                    winner = PlayerNum.Four;
                }
            }

            //If all players have lost, the game is a tie.
            else if (numPlayers == losers.Count)
            {
                if (isSoundEnabled)
                {
                    sfxGameEnd.Play();
                }

                //Only snakes that just lost can break a tie.
                bool tieBreakFound = false;
                for (int i = 0; i < losersThisUpdate.Count; i++)
                {
                    bool isLargestSnake = true;
                    int thisSnake = (int)losersThisUpdate[i];

                    //A player can only break a tie if they're the largest.
                    for (int j = 0; j < losersThisUpdate.Count; j++)
                    {
                        //Doesn't compare players to themselves.
                        if (i == j)
                        {
                            continue;
                        }

                        int otherSnake = (int)losersThisUpdate[j];

                        if (playerSnakes[thisSnake].Count <=
                            playerSnakes[otherSnake].Count)
                        {
                            isLargestSnake = false;
                            break;
                        }
                    }

                    //Otherwise, the player wins.
                    if (isLargestSnake)
                    {
                        tieBreakFound = true;
                        winner = (PlayerNum)thisSnake;
                    }
                }

                //If a player couldn't break the tie, it's a tie.
                if (!tieBreakFound)
                {
                    winner = PlayerNum.None;
                }
            }
        }

        /// <summary>
        /// Returns true if the first direction is opposite to the second.
        /// </summary>
        private bool IsOppositeDirection(PlayerDir dir1, PlayerDir dir2)
        {
            if ((dir1 == PlayerDir.Left && dir2 == PlayerDir.Right) ||
                (dir1 == PlayerDir.Right && dir2 == PlayerDir.Left) ||
                (dir1 == PlayerDir.Up && dir2 == PlayerDir.Down) ||
                (dir1 == PlayerDir.Down && dir2 == PlayerDir.Up) ||
                (dir2 == PlayerDir.Left && dir1 == PlayerDir.Right) ||
                (dir2 == PlayerDir.Right && dir1 == PlayerDir.Left) ||
                (dir2 == PlayerDir.Up && dir1 == PlayerDir.Down) ||
                (dir2 == PlayerDir.Down && dir1 == PlayerDir.Up))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns the number of columns based on grid size.
        /// </summary>
        private int GetGridCols(GridSize size)
        {
            switch (size)
            {
                case (GridSize.Small):
                    return 20;
                case (GridSize.Medium):
                    return 40;
                case (GridSize.Large):
                    return 80;
                default:
                    return 40;
            }
        }

        /// <summary>
        /// Returns the number of rows based on grid size.
        /// </summary>
        private int GetGridRows(GridSize size)
        {
            switch (size)
            {
                case (GridSize.Small):
                    return 15;
                case (GridSize.Medium):
                    return 30;
                case (GridSize.Large):
                    return 60;
                default:
                    return 30;
            }
        }

        /// <summary>
        /// Returns the default update speed based on grid size.
        /// </summary>
        private int GetGridUpdateSpeed(GridSize size)
        {
            switch (size)
            {
                case (GridSize.Small):
                    return 12;
                case (GridSize.Medium):
                    return 10;
                case (GridSize.Large):
                    return 8;
                default:
                    return 10;
            }
        }

        /// <summary>
        /// Returns the pixel width and height of a cell.
        /// </summary>
        public Vector2 GetGridCellSizes(GridSize size)
        {
            return new Vector2(
                scrWidth / GetGridCols(size),
                scrHeight / GetGridRows(size));
        }
        #endregion
    }
}