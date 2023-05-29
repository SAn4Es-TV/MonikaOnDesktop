using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Pong
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        //Timer
        private DispatcherTimer PlayerMovementTimer = new DispatcherTimer();
        private DispatcherTimer AnimationTimer = new DispatcherTimer();

        //BallSpeed
        private double BeginSpeedX = 140;
        private double BeginSpeedY = 220;
        private double SpeedX = 5;
        private double SpeedY = 5;

        //Ball Pos
        double BallX = 400;
        double BallY = 192;

        //Player Score
        public int PlayerScore = 0;
        public int MonikaScore = 0;

        //Ball Starting Position
        private double BeginBallX = 200;
        private double BeginBallY = 150;

        //Cooldown to prevent recollision with paddle
        private bool NavX = false;
        private bool NavY = false;

        public MainWindow()
        {
            InitializeComponent();
            //Player Movement Timer Initialization
            PlayerMovementTimer.Interval = TimeSpan.FromMilliseconds(10);
            PlayerMovementTimer.Tick += MovePlayer;
            PlayerMovementTimer.Start();

            //Animation Timer Initialization
            AnimationTimer.Interval = TimeSpan.FromMilliseconds(10);
            AnimationTimer.Tick += Animate;
            AnimationTimer.Start();
        }

        private void Animate(object sender, EventArgs e)
        {
            double[] playerRange = new double[2] { myRocket.Margin.Top, myRocket.Margin.Top + myRocket.Height };
            double[] monikaRange = new double[2] { monikaRocket.Margin.Top, monikaRocket.Margin.Top + monikaRocket.Height };

            if (BallX <= 10)
            {   
                if(BallY >= playerRange[0] && BallY <= playerRange[1])
                    NavX = true;
                else
                    BallX = 400;
            }

            if (BallX >= 790)
            {
                if(BallY >= monikaRange[0] && BallY <= monikaRange[1])
                    NavX = false;
                else
                    BallX = 400;
            }


            //if (BallX <= 0)
            //    NavX = true;
            //else if (BallX >= 800 - ball.Width / 2)
            //    NavX = false;

            if (BallY <= 0)
                NavY = true;
            else if (BallY >= 384.04 - ball.Height/2)
                NavY = false;


            if (NavX)
                BallX += SpeedX;
            else
                BallX -= SpeedX;

            if(NavY)
                BallY += SpeedY;
            else
                BallY -= SpeedY;

            ball.Margin = new Thickness(BallX, BallY, 0, 0);

            SpeedX *= 1.001;
            SpeedY *= 1.001;
        }

        private void MovePlayer(object sender, EventArgs e)
        {
            //Rocket Positions
            double mousePosY = Mouse.GetPosition(board).Y;
            double ballPosY = ball.Margin.Top;

            myRocket.Margin = new Thickness(10, mousePosY - myRocket.Height/2, 0, 0);
            monikaRocket.Margin = new Thickness(0, ballPosY - monikaRocket.Height / 2, 10, 0);
        }
    }
}
