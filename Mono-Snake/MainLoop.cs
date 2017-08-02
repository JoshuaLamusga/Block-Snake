using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Snake
{
    /// <summary>
    /// Handles execution of the game.
    /// </summary>
    public class MainLoop : Game
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
        private Texture2D sprTitlePlayPlayersThree, sprTitlePlayPlayersFour;
        private Texture2D sprTitleGameStart, sprTitleGameStartBlue, sprTitleGameStartRed;
        private Texture2D sprTitleGameStartGreen, sprTitleGameStartYellow;
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
        private Random rng;

        //The height and width of the screen; see LoadContent.
        private int scrWidth, scrHeight;

        //Stacks of Vector2s controlling the positions of occupied squares; see constructor.
        private Queue<Vector2> playerOneSnake, playerTwoSnake, playerThreeSnake, playerFourSnake;

        //The last positions occupied by the players, used for growth.
        Vector2 playerOneDeleted, playerTwoDeleted, playerThreeDeleted, playerFourDeleted;

        //A single list controlling the positions of points; see constructor.
        private List<Point> gamePoints, gamePointsDelete;

        //Gets the directions of the players.
        private Direction playerOneDir = Direction.None, playerTwoDir = Direction.None,
            playerThreeDir = Direction.None, playerFourDir = Direction.None;

        //Who won, if any.
        private ObservableCollection<Player> losers = new ObservableCollection<Player>();
        private Player winner = Player.All;

        //The current controls.
        private GamePadState gpState1, gpState1Old, gpState2, gpState2Old;
        private GamePadState gpState3, gpState3Old, gpState4, gpState4Old;
        private KeyboardState kbState, kbStateOld;

        //Whether or not the game is paused.
        private bool paused = false;

        private bool soundEnabled = true; //Whether or not sound is being played.
        private bool gridEnabled = false; //Whether or not a grid is drawn.

        //The music player instance; see LoadContent().
        MusicPlayer musicList;

        //How many players are involved.
        private int numPlayers = 1;

        //The current room.
        private RoomIndex room = RoomIndex.rmMenu;

        public MainLoop()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            //Sets up the random instance.
            rng = new Random();

            //Creates stacks of Vector2s controlling the positions of occupied squares.
            playerOneSnake = new Queue<Vector2>();
            playerTwoSnake = new Queue<Vector2>();
            playerThreeSnake = new Queue<Vector2>();
            playerFourSnake = new Queue<Vector2>();

            //Creates a list of points (and a deletion list).
            gamePoints = new List<Point>();
            gamePointsDelete = new List<Point>();

            //Checks the win status when a player loses.
            losers.CollectionChanged += (a, b) => { UpdateWinStatus(); };
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
            sprTitlePlayPlayersThree = Content.Load<Texture2D>("sprTitlePlayPlayersThree");
            sprTitlePlayPlayersFour = Content.Load<Texture2D>("sprTitlePlayPlayersFour");
            sprTitleSnakeSelector = Content.Load<Texture2D>("sprTitleSnakeSelector");
            sprTitleGameStart = Content.Load<Texture2D>("sprTitleGameStart");
            sprTitleGameStartRed = Content.Load<Texture2D>("sprTitleGameStartRed");
            sprTitleGameStartBlue = Content.Load<Texture2D>("sprTitleGameStartBlue");
            sprTitleGameStartGreen = Content.Load<Texture2D>("sprTitleGameStartGreen");
            sprTitleGameStartYellow = Content.Load<Texture2D>("sprTitleGameStartYellow");
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
            gpState3 = GamePad.GetState(PlayerIndex.Three);
            gpState4 = GamePad.GetState(PlayerIndex.Four);
            kbState = Keyboard.GetState();
        }

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
            if (soundEnabled)
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
                soundEnabled = !soundEnabled;

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
            }

            //If the room is the menu.
            if (room == RoomIndex.rmMenu)
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
                if (kbState.IsKeyDown(Keys.Enter) && kbStateOld.IsKeyUp(Keys.Enter) ||
                    gpState1.IsButtonDown(Buttons.A) && gpState1Old.IsButtonUp(Buttons.A) ||
                    gpState2.IsButtonDown(Buttons.A) && gpState2Old.IsButtonUp(Buttons.A) ||
                    gpState3.IsButtonDown(Buttons.A) && gpState3Old.IsButtonUp(Buttons.A) ||
                    gpState4.IsButtonDown(Buttons.A) && gpState4Old.IsButtonUp(Buttons.A))
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
                    if (soundEnabled)
                    {
                        sfxMenuSwitch.Play();
                    }

                    switch (button)
                    {
                        case (MenuButtons.onePlayer):
                            button = MenuButtons.fourPlayer;
                            break;
                        case (MenuButtons.twoPlayer):
                            button = MenuButtons.onePlayer;
                            break;
                        case (MenuButtons.threePlayer):
                            button = MenuButtons.twoPlayer;
                            break;
                        case (MenuButtons.fourPlayer):
                            button = MenuButtons.threePlayer;
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
                            button = MenuButtons.threePlayer;
                            break;
                        case (MenuButtons.threePlayer):
                            button = MenuButtons.fourPlayer;
                            break;
                        case (MenuButtons.fourPlayer):
                            button = MenuButtons.onePlayer;
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
                    if (soundEnabled)
                    {
                        sfxMenuClick.Play();
                    }

                    //If a number-of-players button is pressed.
                    if (button == MenuButtons.onePlayer ||
                        button == MenuButtons.twoPlayer ||
                        button == MenuButtons.threePlayer ||
                        button == MenuButtons.fourPlayer)
                    {
                        //Clears the list of blocks owned by each snake.
                        playerOneSnake.Clear();
                        playerTwoSnake.Clear();
                        playerThreeSnake.Clear();
                        playerFourSnake.Clear();

                        //Removes dots from the screen and the dot deletion list.
                        gamePoints = new List<Point>();
                        gamePointsDelete = new List<Point>();

                        //Resets gameplay refresh speed based on grid size.
                        if (gridXSize == 20)
                        {
                            tickDelay = 12;
                        }
                        else if (gridXSize == 40)
                        {
                            tickDelay = 10;
                        }
                        else if (gridXSize == 80)
                        {
                            tickDelay = 8;
                        }

                        //Resets time until game refreshes.
                        tick = 0;

                        //Resets snake directions.
                        playerOneDir = Direction.None;
                        playerTwoDir = Direction.None;
                        playerThreeDir = Direction.None;
                        playerFourDir = Direction.None;

                        //Initializes the size of the grid in pixels.
                        gridXPixels = scrWidth / gridXSize;
                        gridYPixels = scrHeight / gridYSize;

                        //Sets the number of active players.
                        if (button == MenuButtons.onePlayer)
                        {
                            numPlayers = 1;
                        }
                        else if (button == MenuButtons.twoPlayer)
                        {
                            numPlayers = 2;
                        }
                        else if (button == MenuButtons.threePlayer)
                        {
                            numPlayers = 3;
                        }
                        else if (button == MenuButtons.fourPlayer)
                        {
                            numPlayers = 4;
                        }

                        //Positions each snake randomly so none are overlapping.
                        List<Vector2> snakePositions = new List<Vector2>();

                        Vector2 tempPos = new Vector2(
                            (int)(rng.Next(0, gridXSize) * gridXPixels),
                            (int)(rng.Next(0, gridYSize) * gridYPixels));

                        for (int i = 0; i < numPlayers; i++)
                        {
                            while (snakePositions.Where(o => o.Equals(tempPos)).Count() > 0)
                            {
                                tempPos = new Vector2(
                                    (int)(rng.Next(0, gridXSize) * gridXPixels),
                                    (int)(rng.Next(0, gridYSize) * gridYPixels));
                            }

                            snakePositions.Add(tempPos);
                        }

                        //Sets each position.
                        playerOneSnake.Enqueue(snakePositions[0]);
                        if (numPlayers >= 2) { playerTwoSnake.Enqueue(snakePositions[1]); }
                        if (numPlayers >= 3) { playerThreeSnake.Enqueue(snakePositions[2]); }
                        if (numPlayers >= 4) { playerFourSnake.Enqueue(snakePositions[3]); }

                        //Sets the button to 'play' if the player returns to the main menu.
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
                if (kbState.IsKeyDown(Keys.Enter) && kbStateOld.IsKeyUp(Keys.Enter) ||
                    gpState1.IsButtonDown(Buttons.A) && gpState1Old.IsButtonUp(Buttons.A) ||
                    gpState2.IsButtonDown(Buttons.A) && gpState2Old.IsButtonUp(Buttons.A) ||
                    gpState3.IsButtonDown(Buttons.A) && gpState3Old.IsButtonUp(Buttons.A) ||
                    gpState4.IsButtonDown(Buttons.A) && gpState4Old.IsButtonUp(Buttons.A))
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
                if (kbState.IsKeyDown(Keys.Enter) && kbStateOld.IsKeyUp(Keys.Enter) ||
                    gpState1.IsButtonDown(Buttons.A) && gpState1Old.IsButtonUp(Buttons.A) ||
                    gpState2.IsButtonDown(Buttons.A) && gpState2Old.IsButtonUp(Buttons.A) ||
                    gpState3.IsButtonDown(Buttons.A) && gpState3Old.IsButtonUp(Buttons.A) ||
                    gpState4.IsButtonDown(Buttons.A) && gpState4Old.IsButtonUp(Buttons.A))
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
                        gridEnabled = !gridEnabled;
                        break;
                    case (MenuButtons.toggleSfx):
                        soundEnabled = !soundEnabled;

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
                if (winner != Player.All)
                {
                    //Returns to the menu if enter or start is pressed.
                    if (kbState.IsKeyUp(Keys.Enter) && kbStateOld.IsKeyDown(Keys.Enter) ||
                        gpState1.IsButtonUp(Buttons.Start) && gpState1Old.IsButtonDown(Buttons.Start) ||
                        gpState2.IsButtonUp(Buttons.Start) && gpState2Old.IsButtonDown(Buttons.Start) ||
                        gpState3.IsButtonUp(Buttons.Start) && gpState3Old.IsButtonDown(Buttons.Start) ||
                        gpState4.IsButtonUp(Buttons.Start) && gpState4Old.IsButtonDown(Buttons.Start))
                    {
                        if (soundEnabled)
                        {
                            sfxMenuClick.Play();
                        }

                        //Resets win/loss statuses.
                        losers.Clear();
                        winner = Player.All;

                        //Changes the room to the menu.
                        room = RoomIndex.rmMenu;
                    }

                    //Suspends functionality when a player has won (like pausing).
                    return;
                }

                //Gets whether or not the game is paused.
                if (kbState.IsKeyDown(Keys.P) && kbStateOld.IsKeyUp(Keys.P) ||
                    gpState1.IsButtonDown(Buttons.B) && gpState1Old.IsButtonUp(Buttons.B) ||
                    gpState2.IsButtonDown(Buttons.B) && gpState2Old.IsButtonUp(Buttons.B) ||
                    gpState3.IsButtonDown(Buttons.B) && gpState3Old.IsButtonUp(Buttons.B) ||
                    gpState4.IsButtonDown(Buttons.B) && gpState4Old.IsButtonUp(Buttons.B))
                {
                    if (paused && winner == Player.All)
                    {
                        paused = false;
                    }
                    else if (!paused && winner == Player.All)
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
                if (!losers.Contains(Player.One))
                {
                    if ((gpState1.ThumbSticks.Left.X > 0 &&
                        gpState1.ThumbSticks.Left.X > Math.Abs(gpState1.ThumbSticks.Left.Y)) ||
                        (gpState1.IsButtonDown(Buttons.DPadRight) && gpState1Old.IsButtonUp(Buttons.DPadRight)))
                    {
                        playerOneDir = Direction.Right;
                    }
                    else if ((gpState1.ThumbSticks.Left.X < 0 &&
                        gpState1.ThumbSticks.Left.X < -Math.Abs(gpState1.ThumbSticks.Left.Y)) ||
                        (gpState1.IsButtonDown(Buttons.DPadLeft) && gpState1Old.IsButtonUp(Buttons.DPadLeft)))
                    {
                        playerOneDir = Direction.Left;
                    }
                    else if ((gpState1.ThumbSticks.Left.Y < 0 &&
                        gpState1.ThumbSticks.Left.Y < Math.Abs(gpState1.ThumbSticks.Left.X)) ||
                        (gpState1.IsButtonDown(Buttons.DPadDown) && gpState1Old.IsButtonUp(Buttons.DPadDown)))
                    {
                        playerOneDir = Direction.Down;
                    }
                    else if ((gpState1.ThumbSticks.Left.Y > 0 &&
                        gpState1.ThumbSticks.Left.Y > Math.Abs(gpState1.ThumbSticks.Left.X)) ||
                        (gpState1.IsButtonDown(Buttons.DPadUp) && gpState1Old.IsButtonUp(Buttons.DPadUp)))
                    {
                        playerOneDir = Direction.Up;
                    }
                }
                //Processes gamepad input for player 2.
                if (!losers.Contains(Player.Two))
                {
                    if ((gpState2.ThumbSticks.Left.X > 0 &&
                        gpState2.ThumbSticks.Left.X > Math.Abs(gpState2.ThumbSticks.Left.Y)) ||
                        (gpState2.IsButtonDown(Buttons.DPadRight) && gpState2Old.IsButtonUp(Buttons.DPadRight)))
                    {
                        playerTwoDir = Direction.Right;
                    }
                    else if ((gpState2.ThumbSticks.Left.X < 0 &&
                        gpState2.ThumbSticks.Left.X < -Math.Abs(gpState2.ThumbSticks.Left.Y)) ||
                        (gpState2.IsButtonDown(Buttons.DPadLeft) && gpState2Old.IsButtonUp(Buttons.DPadLeft)))
                    {
                        playerTwoDir = Direction.Left;
                    }
                    else if ((gpState2.ThumbSticks.Left.Y < 0 &&
                        gpState2.ThumbSticks.Left.Y < Math.Abs(gpState2.ThumbSticks.Left.X)) ||
                        (gpState2.IsButtonDown(Buttons.DPadDown) && gpState2Old.IsButtonUp(Buttons.DPadDown)))
                    {
                        playerTwoDir = Direction.Down;
                    }
                    else if ((gpState2.ThumbSticks.Left.Y > 0 &&
                        gpState2.ThumbSticks.Left.Y > Math.Abs(gpState2.ThumbSticks.Left.X)) ||
                        (gpState2.IsButtonDown(Buttons.DPadUp) && gpState2Old.IsButtonUp(Buttons.DPadUp)))
                    {
                        playerTwoDir = Direction.Up;
                    }
                }
                //Processes gamepad input for player 3.
                if (!losers.Contains(Player.Three))
                {
                    if ((gpState3.ThumbSticks.Left.X > 0 &&
                        gpState3.ThumbSticks.Left.X > Math.Abs(gpState3.ThumbSticks.Left.Y)) ||
                        (gpState3.IsButtonDown(Buttons.DPadRight) && gpState3Old.IsButtonUp(Buttons.DPadRight)))
                    {
                        playerThreeDir = Direction.Right;
                    }
                    else if ((gpState3.ThumbSticks.Left.X < 0 &&
                        gpState3.ThumbSticks.Left.X < -Math.Abs(gpState3.ThumbSticks.Left.Y)) ||
                        (gpState3.IsButtonDown(Buttons.DPadLeft) && gpState3Old.IsButtonUp(Buttons.DPadLeft)))
                    {
                        playerThreeDir = Direction.Left;
                    }
                    else if ((gpState3.ThumbSticks.Left.Y < 0 &&
                        gpState3.ThumbSticks.Left.Y < Math.Abs(gpState3.ThumbSticks.Left.X)) ||
                        (gpState3.IsButtonDown(Buttons.DPadDown) && gpState3Old.IsButtonUp(Buttons.DPadDown)))
                    {
                        playerThreeDir = Direction.Down;
                    }
                    else if ((gpState3.ThumbSticks.Left.Y > 0 &&
                        gpState3.ThumbSticks.Left.Y > Math.Abs(gpState3.ThumbSticks.Left.X)) ||
                        (gpState3.IsButtonDown(Buttons.DPadUp) && gpState3Old.IsButtonUp(Buttons.DPadUp)))
                    {
                        playerThreeDir = Direction.Up;
                    }
                }
                //Processes gamepad input for player 4.
                if (!losers.Contains(Player.Four))
                {
                    if ((gpState4.ThumbSticks.Left.X > 0 &&
                        gpState4.ThumbSticks.Left.X > Math.Abs(gpState4.ThumbSticks.Left.Y)) ||
                        (gpState4.IsButtonDown(Buttons.DPadRight) && gpState4Old.IsButtonUp(Buttons.DPadRight)))
                    {
                        playerFourDir = Direction.Right;
                    }
                    else if ((gpState4.ThumbSticks.Left.X < 0 &&
                        gpState4.ThumbSticks.Left.X < -Math.Abs(gpState4.ThumbSticks.Left.Y)) ||
                        (gpState4.IsButtonDown(Buttons.DPadLeft) && gpState4Old.IsButtonUp(Buttons.DPadLeft)))
                    {
                        playerFourDir = Direction.Left;
                    }
                    else if ((gpState4.ThumbSticks.Left.Y < 0 &&
                        gpState4.ThumbSticks.Left.Y < Math.Abs(gpState4.ThumbSticks.Left.X)) ||
                        (gpState4.IsButtonDown(Buttons.DPadDown) && gpState4Old.IsButtonUp(Buttons.DPadDown)))
                    {
                        playerFourDir = Direction.Down;
                    }
                    else if ((gpState4.ThumbSticks.Left.Y > 0 &&
                        gpState4.ThumbSticks.Left.Y > Math.Abs(gpState4.ThumbSticks.Left.X)) ||
                        (gpState4.IsButtonDown(Buttons.DPadUp) && gpState4Old.IsButtonUp(Buttons.DPadUp)))
                    {
                        playerFourDir = Direction.Up;
                    }
                }
                //Updates the keyboard for player 1.
                if (!losers.Contains(Player.One))
                {
                    if (kbState.IsKeyDown(Keys.D))
                    {
                        playerOneDir = Direction.Right;
                    }
                    if (kbState.IsKeyDown(Keys.W))
                    {
                        playerOneDir = Direction.Up;
                    }
                    if (kbState.IsKeyDown(Keys.A))
                    {
                        playerOneDir = Direction.Left;
                    }
                    if (kbState.IsKeyDown(Keys.S))
                    {
                        playerOneDir = Direction.Down;
                    }
                }
                if (!losers.Contains(Player.Two) && numPlayers >= 2)
                {
                    //Updates the keyboard for player 2.
                    if (kbState.IsKeyDown(Keys.Right))
                    {
                        playerTwoDir = Direction.Right;
                    }
                    if (kbState.IsKeyDown(Keys.Up))
                    {
                        playerTwoDir = Direction.Up;
                    }
                    if (kbState.IsKeyDown(Keys.Left))
                    {
                        playerTwoDir = Direction.Left;
                    }
                    if (kbState.IsKeyDown(Keys.Down))
                    {
                        playerTwoDir = Direction.Down;
                    }
                }
                if (!losers.Contains(Player.Three) && numPlayers >= 3)
                {
                    //Updates the keyboard for player 3.
                    if (kbState.IsKeyDown(Keys.L))
                    {
                        playerThreeDir = Direction.Right;
                    }
                    if (kbState.IsKeyDown(Keys.I))
                    {
                        playerThreeDir = Direction.Up;
                    }
                    if (kbState.IsKeyDown(Keys.J))
                    {
                        playerThreeDir = Direction.Left;
                    }
                    if (kbState.IsKeyDown(Keys.K))
                    {
                        playerThreeDir = Direction.Down;
                    }
                }
                if (!losers.Contains(Player.Four) && numPlayers >= 4)
                {
                    //Updates the keyboard for player 4.
                    if (kbState.IsKeyDown(Keys.H))
                    {
                        playerFourDir = Direction.Right;
                    }
                    if (kbState.IsKeyDown(Keys.T))
                    {
                        playerFourDir = Direction.Up;
                    }
                    if (kbState.IsKeyDown(Keys.F))
                    {
                        playerFourDir = Direction.Left;
                    }
                    if (kbState.IsKeyDown(Keys.G))
                    {
                        playerFourDir = Direction.Down;
                    }
                }

                //Doesn't start the game until all players have a direction.
                if ((numPlayers >= 1 && playerOneDir == Direction.None) ||
                    (numPlayers >= 2 && playerTwoDir == Direction.None) ||
                    (numPlayers >= 3 && playerThreeDir == Direction.None) ||
                    (numPlayers >= 4 && playerFourDir == Direction.None))
                {
                    return;
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
                if ((gamePoints.Count < 2 && rng.Next(0, 1000) == 1) ||
                    gamePoints.Count == 0)
                {
                    //temporary variables to store the position on the screen.
                    int tempPosX, tempPosY;
                    bool tempvalid = false;

                    //As long as there is open space on the screen.
                    if (playerOneSnake.Count + playerTwoSnake.Count +
                        playerThreeSnake.Count + playerFourSnake.Count < gridXSize * gridYSize)
                    {
                        //If tempvalid is false.
                        while (!tempvalid)
                        {
                            //Gets new values for the positions.
                            tempPosX = (int)(rng.Next(0, gridXSize) * gridXPixels);
                            tempPosY = (int)(rng.Next(0, gridYSize) * gridYPixels);

                            //Checks to see if those positions are occupied.
                            //If so, moves to another spot on the screen.
                            for (int i = 0; i < playerOneSnake.Count - 1; i++)
                            {
                                if (new Vector2(tempPosX, tempPosY).Equals(playerOneSnake.ElementAt(i)))
                                {
                                    continue;
                                }
                            }
                            if (numPlayers >= 2)
                            {
                                for (int i = 0; i < playerTwoSnake.Count - 1; i++)
                                {
                                    if (new Vector2(tempPosX, tempPosY).Equals(playerTwoSnake.ElementAt(i)))
                                    {
                                        continue;
                                    }
                                }
                            }
                            if (numPlayers >= 3)
                            {
                                for (int i = 0; i < playerThreeSnake.Count - 1; i++)
                                {
                                    if (new Vector2(tempPosX, tempPosY).Equals(playerThreeSnake.ElementAt(i)))
                                    {
                                        continue;
                                    }
                                }
                            }
                            if (numPlayers >= 4)
                            {
                                for (int i = 0; i < playerFourSnake.Count - 1; i++)
                                {
                                    if (new Vector2(tempPosX, tempPosY).Equals(playerFourSnake.ElementAt(i)))
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
                        float tempXPlayer3 = 0, tempYPlayer3 = 0;
                        float tempXPlayer4 = 0, tempYPlayer4 = 0;

                        //Tries to determine the movement of each player.
                        switch (playerOneDir)
                        {
                            case (Direction.Right):
                                tempXPlayer1 = gridXPixels;
                                break;
                            case (Direction.Down):
                                tempYPlayer1 = gridYPixels;
                                break;
                            case (Direction.Left):
                                tempXPlayer1 = -gridXPixels;
                                break;
                            case (Direction.Up):
                                tempYPlayer1 = -gridYPixels;
                                break;
                        }
                        switch (playerTwoDir)
                        {
                            case (Direction.Right):
                                tempXPlayer2 = gridXPixels;
                                break;
                            case (Direction.Down):
                                tempYPlayer2 = gridYPixels;
                                break;
                            case (Direction.Left):
                                tempXPlayer2 = -gridXPixels;
                                break;
                            case (Direction.Up):
                                tempYPlayer2 = -gridYPixels;
                                break;
                        }
                        switch (playerThreeDir)
                        {
                            case (Direction.Right):
                                tempXPlayer3 = gridXPixels;
                                break;
                            case (Direction.Down):
                                tempYPlayer3 = gridYPixels;
                                break;
                            case (Direction.Left):
                                tempXPlayer3 = -gridXPixels;
                                break;
                            case (Direction.Up):
                                tempYPlayer3 = -gridYPixels;
                                break;
                        }
                        switch (playerFourDir)
                        {
                            case (Direction.Right):
                                tempXPlayer4 = gridXPixels;
                                break;
                            case (Direction.Down):
                                tempYPlayer4 = gridYPixels;
                                break;
                            case (Direction.Left):
                                tempXPlayer4 = -gridXPixels;
                                break;
                            case (Direction.Up):
                                tempYPlayer4 = -gridYPixels;
                                break;
                        }

                        //Increases the game speed based on grid size.
                        if (gridXSize == 20)
                        {
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
                        else if (gridXSize == 40)
                        {
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
                        else if (gridXSize == 80)
                        {
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

                        //Reverses the capturing snake to add a block to it.
                        if (gamePoints[i].playerWhoCaptured == Player.One)
                        {
                            playerOneSnake = new Queue<Vector2>(playerOneSnake.Reverse());
                            playerOneSnake.Enqueue(playerOneDeleted);
                            playerOneSnake = new Queue<Vector2>(playerOneSnake.Reverse());
                        }
                        else if (gamePoints[i].playerWhoCaptured == Player.Two)
                        {
                            playerTwoSnake = new Queue<Vector2>(playerTwoSnake.Reverse());
                            playerTwoSnake.Enqueue(playerTwoDeleted);
                            playerTwoSnake = new Queue<Vector2>(playerTwoSnake.Reverse());
                        }
                        else if (gamePoints[i].playerWhoCaptured == Player.Three)
                        {
                            playerThreeSnake = new Queue<Vector2>(playerThreeSnake.Reverse());
                            playerThreeSnake.Enqueue(playerThreeDeleted);
                            playerThreeSnake = new Queue<Vector2>(playerThreeSnake.Reverse());
                        }
                        else if (gamePoints[i].playerWhoCaptured == Player.Four)
                        {
                            playerFourSnake = new Queue<Vector2>(playerFourSnake.Reverse());
                            playerFourSnake.Enqueue(playerFourDeleted);
                            playerFourSnake = new Queue<Vector2>(playerFourSnake.Reverse());
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
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullCounterClockwise);

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
                    "left thumbstick for controllers, WASD for player one, ",
                    new Vector2(scrWidth / 2
                        - (fntSmall.MeasureString("left thumbstick for controllers, WASD for player one, ").X / 2),
                        scrHeight / 5 + 120),
                    Color.Black);

                spriteBatch.DrawString(fntSmall,
                     "arrowkeys for player two, JIKL for player three, and ",
                     new Vector2(scrWidth / 2
                         - (fntSmall.MeasureString("arrowkeys for player two, JIKL for player three, and ").X / 2),
                         scrHeight / 5 + 144),
                     Color.Black);

                spriteBatch.DrawString(fntSmall,
                     "FTGH for player four.  Don't run into yourself or another ",
                     new Vector2(scrWidth / 2
                         - (fntSmall.MeasureString("FTGH for player four.  Don't run into yourself or another ").X / 2),
                         scrHeight / 5 + 168),
                     Color.Black);

                spriteBatch.DrawString(fntSmall,
                     "player. Cut off another player to make them lose.",
                     new Vector2(scrWidth / 2
                         - (fntSmall.MeasureString("player. Cut off another player to make them lose.").X / 2),
                         scrHeight / 5 + 192),
                     Color.Black);

                spriteBatch.DrawString(fntSmall,
                     "P (keyboard) or B (gamepad) to pause. Y toggles music.",
                     new Vector2(scrWidth / 2
                         - (fntSmall.MeasureString("P (keyboard) or B (gamepad) to pause. Y toggles music.").X / 2),
                         scrHeight / 5 + 216),
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

            //If the room is in the options room.
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
                    new Rectangle(scrWidth / 2, scrHeight / 5 + 72, sprTitlePlayPlayers.Width, sprTitlePlayPlayers.Height),
                    sprTitlePlayPlayers.Bounds,
                    Color.White,
                    0,
                    new Vector2(sprTitlePlayPlayers.Width / 2, sprTitlePlayPlayers.Height / 2),
                    SpriteEffects.None,
                    0);

                //Draws the one button.
                spriteBatch.Draw(sprTitlePlayPlayersOne,
                    new Rectangle(scrWidth / 2, scrHeight / 5 + 160, sprTitlePlayPlayersOne.Width, sprTitlePlayPlayersOne.Height),
                    sprTitlePlayPlayersOne.Bounds,
                    Color.White,
                    0,
                    new Vector2(sprTitlePlayPlayersOne.Width / 2, sprTitlePlayPlayersOne.Height / 2),
                    SpriteEffects.None,
                    0);

                //Draws the two button.
                spriteBatch.Draw(sprTitlePlayPlayersTwo,
                    new Rectangle(scrWidth / 2, scrHeight / 5 + 224, sprTitlePlayPlayersTwo.Width, sprTitlePlayPlayersTwo.Height),
                    sprTitlePlayPlayersTwo.Bounds,
                    Color.White,
                    0,
                    new Vector2(sprTitlePlayPlayersTwo.Width / 2, sprTitlePlayPlayersTwo.Height / 2),
                    SpriteEffects.None,
                    0);

                //Draws the three button.
                spriteBatch.Draw(sprTitlePlayPlayersThree,
                    new Rectangle(scrWidth / 2, scrHeight / 5 + 288, sprTitlePlayPlayersThree.Width, sprTitlePlayPlayersThree.Height),
                    sprTitlePlayPlayersThree.Bounds,
                    Color.White,
                    0,
                    new Vector2(sprTitlePlayPlayersThree.Width / 2, sprTitlePlayPlayersThree.Height / 2),
                    SpriteEffects.None,
                    0);

                //Draws the four button.
                spriteBatch.Draw(sprTitlePlayPlayersFour,
                    new Rectangle(scrWidth / 2, scrHeight / 5 + 352, sprTitlePlayPlayersFour.Width, sprTitlePlayPlayersFour.Height),
                    sprTitlePlayPlayersFour.Bounds,
                    Color.White,
                    0,
                    new Vector2(sprTitlePlayPlayersFour.Width / 2, sprTitlePlayPlayersFour.Height / 2),
                    SpriteEffects.None,
                    0);

                //Sets up a temporary y-coordinate for the selector image.
                //Based on the currently active button.
                int tempYPos = 0;

                switch (button)
                {
                    case (MenuButtons.onePlayer):
                        tempYPos = scrHeight / 5 + 160;
                        break;
                    case (MenuButtons.twoPlayer):
                        tempYPos = scrHeight / 5 + 224;
                        break;
                    case (MenuButtons.threePlayer):
                        tempYPos = scrHeight / 5 + 288;
                        break;
                    case (MenuButtons.fourPlayer):
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

                //Draws first player.
                Color player1Color = Color.Red;
                if (losers.Contains(Player.One)) { player1Color = Color.Black; }

                for (int i = 0; i < playerOneSnake.Count; i++)
                {
                    spriteBatch.Draw(sprSpriteBlock, new Rectangle(
                        (int)playerOneSnake.ElementAt(i).X,
                        (int)playerOneSnake.ElementAt(i).Y,
                        (int)gridXPixels,
                        (int)gridYPixels),
                        player1Color);
                }
                if (numPlayers >= 2) //Draws second player.
                {
                    Color player2Color = Color.Blue;
                    if (losers.Contains(Player.Two)) { player2Color = Color.Black; }

                    for (int i = 0; i < playerTwoSnake.Count; i++)
                    {
                        spriteBatch.Draw(sprSpriteBlock, new Rectangle(
                            (int)playerTwoSnake.ElementAt(i).X,
                            (int)playerTwoSnake.ElementAt(i).Y,
                            (int)gridXPixels,
                            (int)gridYPixels),
                            player2Color);
                    }
                }
                if (numPlayers >= 3) //Draws third player.
                {
                    Color player3Color = Color.Green;
                    if (losers.Contains(Player.Three)) { player3Color = Color.Black; }

                    for (int i = 0; i < playerThreeSnake.Count; i++)
                    {
                        spriteBatch.Draw(sprSpriteBlock, new Rectangle(
                            (int)playerThreeSnake.ElementAt(i).X,
                            (int)playerThreeSnake.ElementAt(i).Y,
                            (int)gridXPixels,
                            (int)gridYPixels),
                            player3Color);
                    }
                }
                if (numPlayers >= 4) //Draws fourth player.
                {
                    Color player4Color = Color.DarkGoldenrod;
                    if (losers.Contains(Player.Four)) { player4Color = Color.Black; }

                    for (int i = 0; i < playerFourSnake.Count; i++)
                    {
                        spriteBatch.Draw(sprSpriteBlock, new Rectangle(
                            (int)playerFourSnake.ElementAt(i).X,
                            (int)playerFourSnake.ElementAt(i).Y,
                            (int)gridXPixels,
                            (int)gridYPixels),
                            player4Color);
                    }
                }

                //If the game hasn't yet started...
                if (playerOneDir == Direction.None ||
                    (numPlayers >= 2 && playerTwoDir == Direction.None) ||
                    (numPlayers >= 3 && playerThreeDir == Direction.None) ||
                    (numPlayers >= 4 && playerFourDir == Direction.None))
                {
                    //If no player has taken an initial direction.
                    if (playerOneDir == Direction.None &&
                        (numPlayers >= 2 && playerTwoDir == Direction.None) &&
                        (numPlayers >= 3 && playerThreeDir == Direction.None) &&
                        (numPlayers >= 4 && playerFourDir == Direction.None))
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
                    else if (playerOneDir == Direction.None)
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
                    else if (numPlayers >= 2 && playerTwoDir == Direction.None)
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
                    else if (numPlayers >= 3 && playerThreeDir == Direction.None)
                    {
                        spriteBatch.Draw(sprTitleGameStartGreen,
                            new Rectangle(scrWidth / 2, scrHeight / 2, sprTitleGameStartGreen.Width, sprTitleGameStartGreen.Height),
                            sprTitleGameStartGreen.Bounds,
                            Color.White,
                            0,
                            new Vector2(sprTitleGameStartGreen.Width / 2, sprTitleGameStartGreen.Height / 2),
                            SpriteEffects.None,
                            0);
                    }
                    else if (numPlayers >= 4 && playerFourDir == Direction.None)
                    {
                        spriteBatch.Draw(sprTitleGameStartYellow,
                            new Rectangle(scrWidth / 2, scrHeight / 2, sprTitleGameStartYellow.Width, sprTitleGameStartYellow.Height),
                            sprTitleGameStartYellow.Bounds,
                            Color.White,
                            0,
                            new Vector2(sprTitleGameStartYellow.Width / 2, sprTitleGameStartYellow.Height / 2),
                            SpriteEffects.None,
                            0);
                    }
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

                //End of game messages in multiplayer.
                if (winner != Player.All && numPlayers >= 2)
                {
                    if (winner == Player.One)
                    {
                        spriteBatch.DrawString(
                            fntDefault,
                            "Red wins!",
                            new Vector2(scrWidth / 2 - (fntDefault.MeasureString("Red wins!").X / 2), scrHeight / 2),
                            Color.Red);
                    }
                    else if (winner == Player.Two)
                    {
                        spriteBatch.DrawString(
                            fntDefault,
                            "Blue wins!",
                            new Vector2(scrWidth / 2 - (fntDefault.MeasureString("Blue wins!").X / 2), scrHeight / 2),
                            Color.Blue);
                    }
                    else if (winner == Player.Three)
                    {
                        spriteBatch.DrawString(
                            fntDefault,
                            "Green wins!",
                            new Vector2(scrWidth / 2 - (fntDefault.MeasureString("Green wins!").X / 2), scrHeight / 2),
                            Color.Green);
                    }
                    else if (winner == Player.Four)
                    {
                        spriteBatch.DrawString(
                            fntDefault,
                            "Yellow wins!",
                            new Vector2(scrWidth / 2 - (fntDefault.MeasureString("Yellow wins!").X / 2), scrHeight / 2),
                            Color.DarkGoldenrod);
                    }
                    else if (winner == Player.None)
                    {
                        spriteBatch.DrawString(
                            fntDefault,
                            "Tie game",
                            new Vector2(scrWidth / 2 - (fntDefault.MeasureString("Tie game").X / 2), scrHeight / 2),
                            Color.Black);
                    }

                    //Displays the sizes of each snake on-screen.
                    string snakeSizes = "Red: " + playerOneSnake.Count;
                    if (numPlayers >= 2) { snakeSizes += "  Blue: " + playerTwoSnake.Count; }
                    if (numPlayers >= 3) { snakeSizes += "  Green: " + playerThreeSnake.Count; }
                    if (numPlayers >= 4) { snakeSizes += "  Yellow " + playerFourSnake.Count; }

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
                        Color.Black);
                }

                //End of game messages in single player.
                else if (losers.Count > 0 && numPlayers == 1)
                {
                    //Displays the size of the snake on-screen.
                    string snakeSize = "size: " + playerOneSnake.Count;

                    spriteBatch.DrawString(
                        fntDefault,
                        snakeSize,
                        new Vector2(scrWidth / 2 - (fntDefault.MeasureString(snakeSize).X / 2),
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
            float tempXPlayer3 = 0, tempYPlayer3 = 0;
            float tempXPlayer4 = 0, tempYPlayer4 = 0;

            //Affects the movement of player one.
            if (playerOneDir == Direction.Right)
            {
                tempXPlayer1 = gridXPixels;
            }
            else if (playerOneDir == Direction.Down)
            {
                tempYPlayer1 = gridYPixels;
            }
            else if (playerOneDir == Direction.Left)
            {
                tempXPlayer1 = -gridXPixels;
            }
            else if (playerOneDir == Direction.Up)
            {
                tempYPlayer1 = -gridYPixels;
            }

            if (numPlayers >= 2) //Moves second player.
            {
                if (playerTwoDir == Direction.Right)
                {
                    tempXPlayer2 = gridXPixels;
                }
                else if (playerTwoDir == Direction.Down)
                {
                    tempYPlayer2 = gridYPixels;
                }
                else if (playerTwoDir == Direction.Left)
                {
                    tempXPlayer2 = -gridXPixels;
                }
                else if (playerTwoDir == Direction.Up)
                {
                    tempYPlayer2 = -gridYPixels;
                }
            }

            if (numPlayers >= 3) //Moves third player.
            {
                if (playerThreeDir == Direction.Right)
                {
                    tempXPlayer3 = gridXPixels;
                }
                else if (playerThreeDir == Direction.Down)
                {
                    tempYPlayer3 = gridYPixels;
                }
                else if (playerThreeDir == Direction.Left)
                {
                    tempXPlayer3 = -gridXPixels;
                }
                else if (playerThreeDir == Direction.Up)
                {
                    tempYPlayer3 = -gridYPixels;
                }
            }

            if (numPlayers >= 4) //Moves fourth player.
            {
                if (playerFourDir == Direction.Right)
                {
                    tempXPlayer4 = gridXPixels;
                }
                else if (playerFourDir == Direction.Down)
                {
                    tempYPlayer4 = gridYPixels;
                }
                else if (playerFourDir == Direction.Left)
                {
                    tempXPlayer4 = -gridXPixels;
                }
                else if (playerFourDir == Direction.Up)
                {
                    tempYPlayer4 = -gridYPixels;
                }
            }

            //Adds a new block to players based on their direction.
            //This just simulates movement of blocks.
            if (!losers.Contains(Player.One))
            {
                playerOneSnake.Enqueue(new Vector2(
                    playerOneSnake.ElementAt(playerOneSnake.Count - 1).X + tempXPlayer1,
                    playerOneSnake.ElementAt(playerOneSnake.Count - 1).Y + tempYPlayer1));
            }
            if (!losers.Contains(Player.Two) && numPlayers >= 2)
            {
                playerTwoSnake.Enqueue(new Vector2(
                    playerTwoSnake.ElementAt(playerTwoSnake.Count - 1).X + tempXPlayer2,
                    playerTwoSnake.ElementAt(playerTwoSnake.Count - 1).Y + tempYPlayer2));
            }
            if (!losers.Contains(Player.Three) && numPlayers >= 3)
            {
                playerThreeSnake.Enqueue(new Vector2(
                    playerThreeSnake.ElementAt(playerThreeSnake.Count - 1).X + tempXPlayer3,
                    playerThreeSnake.ElementAt(playerThreeSnake.Count - 1).Y + tempYPlayer3));
            }
            if (!losers.Contains(Player.Four) && numPlayers >= 4)
            {
                playerFourSnake.Enqueue(new Vector2(
                    playerFourSnake.ElementAt(playerFourSnake.Count - 1).X + tempXPlayer4,
                    playerFourSnake.ElementAt(playerFourSnake.Count - 1).Y + tempYPlayer4));
            }

            //Removes the first block from the stack.
            if (!losers.Contains(Player.One))
            {
                playerOneDeleted = playerOneSnake.Dequeue();
            }
            if (!losers.Contains(Player.Two) && numPlayers >= 2)
            {
                playerTwoDeleted = playerTwoSnake.Dequeue();
            }
            if (!losers.Contains(Player.Three) && numPlayers >= 3)
            {
                playerThreeDeleted = playerThreeSnake.Dequeue();
            }
            if (!losers.Contains(Player.Four) && numPlayers >= 4)
            {
                playerFourDeleted = playerFourSnake.Dequeue();
            }

            //Checks to see if the new block is outside of the screen.
            if (!losers.Contains(Player.One))
            {
                if (playerOneSnake.Last().X >= scrWidth ||
                    playerOneSnake.Last().X < 0 ||
                    playerOneSnake.Last().Y >= scrHeight ||
                    playerOneSnake.Last().Y < 0)
                {
                    if (soundEnabled)
                    {
                        sfxGameEnd.Play();
                    }

                    losers.Add(Player.One);
                }
            }
            if (numPlayers >= 2 && !losers.Contains(Player.Two))
            {
                if (playerTwoSnake.Last().X >= scrWidth ||
                    playerTwoSnake.Last().X < 0 ||
                    playerTwoSnake.Last().Y >= scrHeight ||
                    playerTwoSnake.Last().Y < 0)
                {

                    if (soundEnabled)
                    {
                        sfxGameEnd.Play();
                    }

                    losers.Add(Player.Two);
                }
            }
            if (numPlayers >= 3 && !losers.Contains(Player.Three))
            {
                if (playerThreeSnake.Last().X >= scrWidth ||
                    playerThreeSnake.Last().X < 0 ||
                    playerThreeSnake.Last().Y >= scrHeight ||
                    playerThreeSnake.Last().Y < 0)
                {

                    if (soundEnabled)
                    {
                        sfxGameEnd.Play();
                    }

                    losers.Add(Player.Three);
                }
            }
            if (numPlayers >= 4 && !losers.Contains(Player.Four))
            {
                if (playerFourSnake.Last().X >= scrWidth ||
                    playerFourSnake.Last().X < 0 ||
                    playerFourSnake.Last().Y >= scrHeight ||
                    playerFourSnake.Last().Y < 0)
                {

                    if (soundEnabled)
                    {
                        sfxGameEnd.Play();
                    }

                    losers.Add(Player.Four);
                }
            }
        }

        /// <summary>
        /// Checks for collisions between the snakes and points.
        /// </summary>
        public void UpdateCollision()
        {
            //Checks for collision of each snake to itself.
            if (!losers.Contains(Player.One))
            {
                for (int i = 0; i < playerOneSnake.Count - 1; i++)
                {
                    if ((playerOneSnake.Count - 1) != 0)
                    {
                        if (playerOneSnake.Last().X == playerOneSnake.ElementAt(i).X &&
                            playerOneSnake.Last().Y == playerOneSnake.ElementAt(i).Y)
                        {
                            if (soundEnabled)
                            {
                                sfxGameEnd.Play();
                            }

                            losers.Add(Player.One);
                        }
                    }
                }
            }
            if (!losers.Contains(Player.Two))
            {
                for (int i = 0; i < playerTwoSnake.Count - 1; i++)
                {
                    if ((playerTwoSnake.Count - 1) != 0)
                    {
                        if (playerTwoSnake.Last().X == playerTwoSnake.ElementAt(i).X &&
                            playerTwoSnake.Last().Y == playerTwoSnake.ElementAt(i).Y)
                        {
                            if (soundEnabled)
                            {
                                sfxGameEnd.Play();
                            }

                            losers.Add(Player.Two);
                        }
                    }
                }
            }
            if (!losers.Contains(Player.Three))
            {
                for (int i = 0; i < playerThreeSnake.Count - 1; i++)
                {
                    if ((playerThreeSnake.Count - 1) != 0)
                    {
                        if (playerThreeSnake.Last().X == playerThreeSnake.ElementAt(i).X &&
                            playerThreeSnake.Last().Y == playerThreeSnake.ElementAt(i).Y)
                        {
                            if (soundEnabled)
                            {
                                sfxGameEnd.Play();
                            }

                            losers.Add(Player.Three);
                        }
                    }
                }
            }
            if (!losers.Contains(Player.Four))
            {
                for (int i = 0; i < playerFourSnake.Count - 1; i++)
                {
                    if ((playerFourSnake.Count - 1) != 0)
                    {
                        if (playerFourSnake.Last().X == playerFourSnake.ElementAt(i).X &&
                            playerFourSnake.Last().Y == playerFourSnake.ElementAt(i).Y)
                        {
                            if (soundEnabled)
                            {
                                sfxGameEnd.Play();
                            }

                            losers.Add(Player.Four);
                        }
                    }
                }
            }

            //Checks if the first player hits another snake.
            if (!losers.Contains(Player.One))
            {
                for (int i = 0; i < playerTwoSnake.Count; i++)
                {
                    if (numPlayers < 2)
                    {
                        break;
                    }

                    if (playerOneSnake.Last().X == playerTwoSnake.ElementAt(i).X &&
                        playerOneSnake.Last().Y == playerTwoSnake.ElementAt(i).Y)
                    {
                        if (soundEnabled)
                        {
                            sfxGameEnd.Play();
                        }

                        losers.Add(Player.One);
                    }
                }
                for (int i = 0; i < playerThreeSnake.Count; i++)
                {
                    if (numPlayers < 3)
                    {
                        break;
                    }

                    if (playerOneSnake.Last().X == playerThreeSnake.ElementAt(i).X &&
                        playerOneSnake.Last().Y == playerThreeSnake.ElementAt(i).Y)
                    {
                        if (soundEnabled)
                        {
                            sfxGameEnd.Play();
                        }

                        losers.Add(Player.One);
                    }
                }
                for (int i = 0; i < playerFourSnake.Count; i++)
                {
                    if (numPlayers < 4)
                    {
                        break;
                    }

                    if (playerOneSnake.Last().X == playerFourSnake.ElementAt(i).X &&
                        playerOneSnake.Last().Y == playerFourSnake.ElementAt(i).Y)
                    {
                        if (soundEnabled)
                        {
                            sfxGameEnd.Play();
                        }

                        losers.Add(Player.One);
                    }
                }
            }

            //Checks if the second player hits another snake.
            if (!losers.Contains(Player.Two))
            {
                for (int i = 0; i < playerOneSnake.Count; i++)
                {
                    if (numPlayers < 2)
                    {
                        break;
                    }

                    if (playerTwoSnake.Last().X == playerOneSnake.ElementAt(i).X &&
                        playerTwoSnake.Last().Y == playerOneSnake.ElementAt(i).Y)
                    {
                        if (soundEnabled)
                        {
                            sfxGameEnd.Play();
                        }

                        losers.Add(Player.Two);
                    }
                }
                for (int i = 0; i < playerThreeSnake.Count; i++)
                {
                    if (numPlayers < 3)
                    {
                        break;
                    }

                    if (playerTwoSnake.Last().X == playerThreeSnake.ElementAt(i).X &&
                        playerTwoSnake.Last().Y == playerThreeSnake.ElementAt(i).Y)
                    {
                        if (soundEnabled)
                        {
                            sfxGameEnd.Play();
                        }

                        losers.Add(Player.Two);
                    }
                }
                for (int i = 0; i < playerFourSnake.Count; i++)
                {
                    if (numPlayers < 4)
                    {
                        break;
                    }

                    if (playerTwoSnake.Last().X == playerFourSnake.ElementAt(i).X &&
                        playerTwoSnake.Last().Y == playerFourSnake.ElementAt(i).Y)
                    {
                        if (soundEnabled)
                        {
                            sfxGameEnd.Play();
                        }

                        losers.Add(Player.Two);
                    }
                }
            }

            //Checks if the third player hits another snake.
            if (!losers.Contains(Player.Three))
            {
                for (int i = 0; i < playerOneSnake.Count; i++)
                {
                    if (numPlayers < 3)
                    {
                        break;
                    }

                    if (playerThreeSnake.Last().X == playerOneSnake.ElementAt(i).X &&
                        playerThreeSnake.Last().Y == playerOneSnake.ElementAt(i).Y)
                    {
                        if (soundEnabled)
                        {
                            sfxGameEnd.Play();
                        }

                        losers.Add(Player.Three);
                    }
                }
                for (int i = 0; i < playerTwoSnake.Count; i++)
                {
                    if (numPlayers < 3)
                    {
                        break;
                    }

                    if (playerThreeSnake.Last().X == playerTwoSnake.ElementAt(i).X &&
                        playerThreeSnake.Last().Y == playerTwoSnake.ElementAt(i).Y)
                    {
                        if (soundEnabled)
                        {
                            sfxGameEnd.Play();
                        }

                        losers.Add(Player.Three);
                    }
                }
                for (int i = 0; i < playerFourSnake.Count; i++)
                {
                    if (numPlayers < 4)
                    {
                        break;
                    }

                    if (playerThreeSnake.Last().X == playerFourSnake.ElementAt(i).X &&
                        playerThreeSnake.Last().Y == playerFourSnake.ElementAt(i).Y)
                    {
                        if (soundEnabled)
                        {
                            sfxGameEnd.Play();
                        }

                        losers.Add(Player.Three);
                    }
                }
            }

            //Checks if the fourth player hits another snake.
            if (!losers.Contains(Player.Four) && numPlayers >= 4)
            {
                for (int i = 0; i < playerOneSnake.Count; i++)
                {
                    if (playerFourSnake.Last().X == playerOneSnake.ElementAt(i).X &&
                        playerFourSnake.Last().Y == playerOneSnake.ElementAt(i).Y)
                    {
                        if (soundEnabled)
                        {
                            sfxGameEnd.Play();
                        }

                        losers.Add(Player.Four);
                    }
                }
                for (int i = 0; i < playerTwoSnake.Count; i++)
                {
                    if (playerFourSnake.Last().X == playerTwoSnake.ElementAt(i).X &&
                        playerFourSnake.Last().Y == playerTwoSnake.ElementAt(i).Y)
                    {
                        if (soundEnabled)
                        {
                            sfxGameEnd.Play();
                        }

                        losers.Add(Player.Four);
                    }
                }
                for (int i = 0; i < playerThreeSnake.Count; i++)
                {
                    if (playerFourSnake.Last().X == playerThreeSnake.ElementAt(i).X &&
                        playerFourSnake.Last().Y == playerThreeSnake.ElementAt(i).Y)
                    {
                        if (soundEnabled)
                        {
                            sfxGameEnd.Play();
                        }

                        losers.Add(Player.Four);
                    }
                }
            }

            //Checks if the first player collides head-on with another.
            if (numPlayers >= 2 &&
                playerOneSnake.Last().X == playerTwoSnake.Last().X &&
                playerOneSnake.Last().Y == playerTwoSnake.Last().Y)
            {
                losers.Add(Player.One);
                losers.Add(Player.Two);
            }

            //Updates the point array.
            for (int i = 0; i < gamePoints.Count; i++)
            {
                if (!gamePoints[i].markedForDeletion)
                {
                    //If there's a direct collision between player one and a point.
                    if (!losers.Contains(Player.One) &&
                        playerOneSnake.Last().X == gamePoints[i].xPos &&
                        playerOneSnake.Last().Y == gamePoints[i].yPos)
                    {
                        gamePoints[i].playerWhoCaptured = Player.One;
                        gamePoints[i].markedForDeletion = true;
                    }

                    //If there's a direct collision between player two and a point.
                    if (!losers.Contains(Player.Two) && numPlayers >= 2 &&
                        playerTwoSnake.Last().X == gamePoints[i].xPos &&
                        playerTwoSnake.Last().Y == gamePoints[i].yPos)
                    {
                        gamePoints[i].playerWhoCaptured = Player.Two;
                        gamePoints[i].markedForDeletion = true;
                    }

                    //If there's a direct collision between player three and a point.
                    if (!losers.Contains(Player.Three) && numPlayers >= 3 &&
                        playerThreeSnake.Last().X == gamePoints[i].xPos &&
                        playerThreeSnake.Last().Y == gamePoints[i].yPos)
                    {
                        gamePoints[i].playerWhoCaptured = Player.Three;
                        gamePoints[i].markedForDeletion = true;
                    }

                    //If there's a direct collision between player four and a point.
                    if (!losers.Contains(Player.Four) && numPlayers >= 4 &&
                        playerFourSnake.Last().X == gamePoints[i].xPos &&
                        playerFourSnake.Last().Y == gamePoints[i].yPos)
                    {
                        gamePoints[i].playerWhoCaptured = Player.Four;
                        gamePoints[i].markedForDeletion = true;
                    }
                }
            }
        }

        /// <summary>
        /// Checks to see if any player has won and, if so, which player.
        /// </summary>
        public void UpdateWinStatus()
        {
            //If only one player hasn't lost, then they've won.
            if (numPlayers - losers.Count == 1)
            {
                if (soundEnabled)
                {
                    sfxGameEnd.Play();
                }

                if (!losers.Contains(Player.One))
                {
                    winner = Player.One;
                }
                else if (!losers.Contains(Player.Two))
                {
                    winner = Player.Two;
                }
                else if (!losers.Contains(Player.Three))
                {
                    winner = Player.Three;
                }
                else if (!losers.Contains(Player.Four))
                {
                    winner = Player.Four;
                }
            }

            //If all players have lost, the game is a tie.
            else if (numPlayers == losers.Count)
            {
                if (soundEnabled)
                {
                    sfxGameEnd.Play();
                }

                if (playerOneSnake.Count > playerTwoSnake.Count &&
                    playerOneSnake.Count > playerThreeSnake.Count &&
                    playerOneSnake.Count > playerFourSnake.Count)
                {
                    winner = Player.One;
                }
                else if (playerTwoSnake.Count > playerOneSnake.Count &&
                    playerTwoSnake.Count > playerThreeSnake.Count &&
                    playerTwoSnake.Count > playerFourSnake.Count)
                {
                    winner = Player.Two;
                }
                else if (playerThreeSnake.Count > playerOneSnake.Count &&
                    playerThreeSnake.Count > playerTwoSnake.Count &&
                    playerThreeSnake.Count > playerFourSnake.Count)
                {
                    winner = Player.Three;
                }
                else if (playerFourSnake.Count > playerOneSnake.Count &&
                    playerFourSnake.Count > playerTwoSnake.Count &&
                    playerFourSnake.Count > playerThreeSnake.Count)
                {
                    winner = Player.Four;
                }
                else
                {
                    winner = Player.None;
                }
            }
        }
    }
}