using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Monika.Shared;

namespace Monika.Shimeji {
    public partial class MainWindow : Window {// Физические константы
        private double velocityX = 0;
        private double velocityY = 0;
        private double gravity = 0.4;  // Уменьшили (было 0.8)
        private double friction = 0.99; // Почти не тормозит в воздухе
        private double bounce = 0.5;    // Сильнее гасим удар при отскоке (было 0.7)

        private DispatcherTimer timer;
        private bool isDragging = false;
        private Point lastMousePos;

        private double targetAngle = 0;
        private double currentAngle = 0;

        private double targetScaleX = 1.0;
        private double targetScaleY = 1.0;
        private double currentScaleX = 1.0;
        private double currentScaleY = 1.0;
        private enum State { Idle, Walking, Physics }
        private State currentState = State.Idle;

        private Random rnd = new Random();
        private double walkTimer = 0;
        private int walkDirection = 1; // 1 - вправо, -1 - влево
                                       // Внутри класса ShimejiWindow
        private int _clickCount = 0;
        private DispatcherTimer _clickTimer;
        public MainWindow() {
            InitializeComponent();

            // Создаем таймер
            _clickTimer = new DispatcherTimer();
            // Интервал ожидания (400 миллисекунд)
            _clickTimer.Interval = TimeSpan.FromMilliseconds(400);
            // Привязываем событие
            _clickTimer.Tick += OnClickTimerTick;

            Loaded += async (e, s) => {
                //Variable to hold the handle for the form
                var helper = new WindowInteropHelper(this).Handle;
                //Performing some magic to hide the form from Alt+Tab
                SetWindowLong(helper, GWL_EXSTYLE, (GetWindowLong(helper, GWL_EXSTYLE) | WS_EX_TOOLWINDOW) & ~WS_EX_APPWINDOW);

                var lastTime = DateTime.Now;
                while (true) {
                    var currentTime = DateTime.Now;
                    double deltaTime = (currentTime - lastTime).TotalSeconds;
                    lastTime = currentTime;

                    UpdatePhysics(deltaTime); // Передавай время в расчеты

                    await Task.Delay(5); // Минимальная задержка, чтобы не вешать поток
                }

            };

            // 1. Create the ContextMenuStrip
            ContextMenu contextMenuStrip1 = new ContextMenu();

            contextMenuStrip1.Background = new SolidColorBrush(Color.FromRgb(235, 230, 244));
            contextMenuStrip1.Foreground = new SolidColorBrush(Color.FromRgb(187, 85, 153));
            contextMenuStrip1.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 189, 225));
            contextMenuStrip1.BorderThickness = new Thickness(4);
            contextMenuStrip1.FontFamily = new FontFamily("My Font");
            contextMenuStrip1.FontSize = 15;

            MenuItem quitItem = new MenuItem() { Header = "Quit" };

            contextMenuStrip1.Items.Add(quitItem);

            quitItem.Click += (s, e) => {
                this.Close();
            };

            this.ContextMenu = contextMenuStrip1;


        }
        private void UpdatePhysics(double deltaTime) {
            // 1. Сначала определяем состояние (ИИ)
            HandleAI(deltaTime);

            if (isDragging) {
                currentState = State.Physics; // Если схватили, прерываем ходьбу
                targetAngle = velocityX * 2.0;
                if (targetAngle > 30) targetAngle = 30;
                if (targetAngle < -30) targetAngle = -30;
            } else if (currentState == State.Walking) {
                // Имитируем покачивание при ходьбе
                targetAngle = Math.Sin(DateTime.Now.Ticks / 1000000.0 * 10) * 5 * walkDirection;
            } else {
                targetAngle = 0;
                if (Math.Abs(velocityX) > 5) targetAngle = velocityX * 0.5;
            }

            // Плавный поворот
            currentAngle += (targetAngle - currentAngle) * 0.1;
            BodyRotate.Angle = currentAngle;

            if (isDragging) {
                Point currentPos = PointToScreen(Mouse.GetPosition(this));
                velocityX = currentPos.X - lastMousePos.X;
                velocityY = currentPos.Y - lastMousePos.Y;
                lastMousePos = currentPos;
                return;
            }

            // --- ФИЗИКА ---
            velocityY += gravity * deltaTime * 100;

            // Если она идет, трение воздуха не должно её останавливать полностью
            double currentFriction = (currentState == State.Walking) ? 0.95 : friction;
            velocityX *= currentFriction;
            velocityY *= friction;

            this.Left += velocityX;
            this.Top += velocityY;

            ApplyBoundaries(); // Вынес границы в метод ниже для чистоты
                               // В конце UpdatePhysics, после ApplyBoundaries()
            if (!isDragging) {
                bool isOnGround = this.Top + this.ActualHeight >= SystemParameters.PrimaryScreenHeight - 1;
                bool isStopped = Math.Abs(velocityX) < 0.2 && Math.Abs(velocityY) < 0.2;

                if (isOnGround && isStopped) {
                    // Если мы всё еще в состоянии Physics, значит мы только что упали и замерли
                    if (currentState == State.Physics) {
                        currentState = State.Idle;
                        // Ставим таймер на следующее действие (например, через 2-5 секунд)
                        walkTimer = rnd.Next(2, 5);

                        // Сбрасываем скорости в честный ноль, чтобы не было микро-дрейфа
                        velocityX = 0;
                        velocityY = 0;
                    }
                } else if (!isOnGround) {
                    // Если мы в воздухе (нас подкинули или мы падаем), принудительно возвращаем физику
                    currentState = State.Physics;
                }
            }
            // Пружина масштаба
            currentScaleX += (targetScaleX - currentScaleX) * 0.15;
            currentScaleY += (targetScaleY - currentScaleY) * 0.15;
            BodyScale.ScaleX = currentScaleX;
            BodyScale.ScaleY = currentScaleY;
        }

        private void HandleAI(double deltaTime) {
            if (isDragging) return;

            // Моника может ходить только если она на полу и почти не двигается
            bool isOnGround = this.Top + this.ActualHeight >= SystemParameters.PrimaryScreenHeight - 5;
            bool isStill = Math.Abs(velocityY) < 0.2;

            if (isOnGround && isStill) {
                walkTimer -= deltaTime;

                if (walkTimer <= 0) {
                    if (currentState == State.Idle) {
                        // Решаем: пойти погулять или постоять еще?
                        if (rnd.Next(0, 10) > 4) // 60% шанс, что пойдет
                        {
                            currentState = State.Walking;
                            walkDirection = rnd.Next(0, 2) == 0 ? 1 : -1;
                            walkTimer = rnd.Next(2, 5); // Идет 2-5 секунд
                        } else {
                            walkTimer = rnd.Next(3, 7); // Просто стоит 3-7 секунд
                        }
                    } else if (currentState == State.Walking) {
                        // Закончила идти
                        currentState = State.Idle;
                        walkTimer = rnd.Next(3, 10); // Отдыхает после прогулки
                        velocityX = 0;
                    }
                }

                // Если сейчас состояние ходьбы — задаем скорость
                if (currentState == State.Walking) {
                    velocityX = walkDirection * 1; // Скорость ходьбы

                    // Проверка, чтобы не ушла за экран во время ходьбы
                    if (this.Left < 10 && walkDirection == -1) walkDirection = 1;
                    if (this.Left + this.ActualWidth > SystemParameters.PrimaryScreenWidth - 10 && walkDirection == 1) walkDirection = -1;
                    MainScale.ScaleX = walkDirection * -1;
                }
            } else if (!isOnGround) {
                currentState = State.Physics; // В воздухе всегда физика
            }
        }

        private void ApplyBoundaries() {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;

            // Пол
            if (this.Top + this.ActualHeight >= screenHeight) {
                double impactPower = Math.Abs(velocityY);

                // Гасим скорость только если не идем специально
                if (currentState != State.Walking) velocityX *= 0.8;

                if (impactPower > 1.5) {
                    double squashAmount = Math.Min(impactPower * 0.08, 0.6);
                    currentScaleY = 1.0 - squashAmount;
                    currentScaleX = 1.0 + squashAmount * 0.5;
                    if (currentScaleY < 0.7) currentScaleY = 0.7;
                    if (currentScaleX > 1.2) currentScaleX = 1.2;
                }

                this.Top = screenHeight - this.ActualHeight;
                velocityY = -velocityY * bounce;
                if (Math.Abs(velocityY) < 0.5) velocityY = 0;
            }

            // Стены
            if (this.Left <= -200) { this.Left = -200; velocityX = -velocityX * bounce; walkDirection = 1; } else if (this.Left + this.ActualWidth / 2 >= screenWidth) { this.Left = screenWidth - this.ActualWidth / 2; velocityX = -velocityX * bounce; walkDirection = -1; }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {

            isDragging = true;
            lastMousePos = PointToScreen(Mouse.GetPosition(this));
            this.DragMove(); // Позволяет перемещать окно
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            isDragging = false;
        }
        private int clickCount = 0;
        private DateTime lastClickTime = DateTime.Now;
        private void Window_MouseDown(object sender, MouseButtonEventArgs e) {
            if (e.LeftButton == MouseButtonState.Released) {
                MonikaSprite.Source = new BitmapImage(new Uri("pack://application:,,,/images/m_sticker_2.png"));
                _clickCount++;

                // Перезапускаем таймер
                _clickTimer.Stop();
                _clickTimer.Start();

            }
        }

        private void Window_MouseUp(object sender, MouseButtonEventArgs e) {

        }
        private void OnClickTimerTick(object sender, EventArgs e) {
            MonikaSprite.Source = new BitmapImage(new Uri("pack://application:,,,/images/m_sticker_1.png"));
            // Таймер сработал — значит, пользователь закончил серию кликов
            _clickTimer.Stop();

            if (_clickCount >= 10) {
                PipeManager.SendMessage("MonikaInteractionPipe", "ANNOYED");
            } else if (_clickCount > 0) {
                PipeManager.SendMessage("MonikaInteractionPipe", "SINGLE_CLICK");
            }

            _clickCount = 0; // Сброс для следующего раза
        }


        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_APPWINDOW = 0x00040000, WS_EX_TOOLWINDOW = 0x00000080;

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    }
}